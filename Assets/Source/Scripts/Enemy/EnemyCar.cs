using EasyChart.Samples;
using KrisDevelopment.ERMG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCar : MonoBehaviour
{
    [SerializeField] private SteeringAlgorithm  _steeringAlgorithm;
    [SerializeField] private JsonInjectionExample _jsonInjectionExample;
    [SerializeField] private JsonInjectionExample _jsonInjectionExample2;
    [SerializeField] private ERMeshGen           _meshGen;
    [SerializeField] private EngineConfig        _engineConfig;
    [SerializeField] private NavMeshAgent        _navmeshAgent;

    private const int   FinishPointIndex = 34;
    private const float RouteTargetSwitchDistance = 15f;
    private const float BrakeEntryAngle  = 25f;
    private const float BrakeExitAngle   = 11f;
    private const float BrakeEntrySpeed  = 7f;
    private const float BrakeForceSpeed  = 17f;
    private const float SteeringDamping  = 0.6f;
    private const float SpeedFactorMax   = 45f;
    private const float DebugLineMaxDist = 6f;

    private SpeedUIScreen _speedUI;
    private TimeUIScreen  _timeUI;

    private Transmission       _transmission;
    private Rigidbody          _body;
    private Engine             _engine;
    private BrakeLights        _brakeLights;
    private List<Vector3>      _points;
    private RouteNavigator     _routeNavigator;
    private SteeringControllerBase _steeringController;
    private VehicleTelemetry   _telemetry;

    private Coroutine _telemetryRoutine;
    private bool      _shouldBrake;
    private bool      _isFinished;
    private float     _timer;
    private float     _distanceToTrace;
    private float     _accumulatedDeviation;
    private long      _steeringTotalTicks;
    private long      _steeringMaxTicks;
    private int       _steeringCallCount;

    private void Awake()
    {
        _points = new List<Vector3>();
        foreach (var point in _meshGen.navPoints)
            _points.Add(point.position);

        _body         = GetComponent<Rigidbody>();
        _brakeLights  = GetComponentInChildren<BrakeLights>();
        _transmission = GetComponentInChildren<Transmission>();
        _engine       = new Engine(_engineConfig, 5);
        _telemetry    = new VehicleTelemetry();

        _body.centerOfMass += new Vector3(0, -0.8f, 0);
        _routeNavigator     = new RouteNavigator(_points, transform.position, transform.forward);
        _navmeshAgent.enabled = false;
    }

    private void Start()
    {
        _speedUI = UIScreenRepository.GetScreen<SpeedUIScreen>();
        _timeUI  = UIScreenRepository.GetScreen<TimeUIScreen>();

        _steeringController = SteeringControllerFactory.Create(
            _steeringAlgorithm,
            FrameworkStorage.GlobalData.LineFactory,
            _points,
            _navmeshAgent,
            FinishPointIndex);

        _telemetryRoutine = StartCoroutine(TelemetryRoutine());
    }

    private void Update()
    {
        if (_isFinished)
            return;

        if (_routeNavigator.CurrentTargetIndex == FinishPointIndex)
        {
            OnFinish();
            return;
        }

        _timer += Time.deltaTime;

        UpdateRouteTarget();

        NavigationContext context = BuildContext();

        long t0             = System.Diagnostics.Stopwatch.GetTimestamp();
        float angleToTarget = _steeringController.GetAngleToTurn(transform, context);
        long elapsed        = System.Diagnostics.Stopwatch.GetTimestamp() - t0;

        _steeringTotalTicks += elapsed;
        _steeringCallCount++;
        if (elapsed > _steeringMaxTicks) _steeringMaxTicks = elapsed;
        float currentSpeed  = _body.velocity.magnitude;

        UpdateBraking(angleToTarget, currentSpeed);
        UpdateDriving(angleToTarget, currentSpeed);
        UpdateDebugLine();
        UpdateUI();
    }

    private void OnFinish()
    {
        _isFinished = true;

        if (_telemetryRoutine != null)
            StopCoroutine(_telemetryRoutine);

        _transmission.OnBrakingActiveChange(true, 100);
        _engine.OnMotorPullingChange(false);

        double usPerTick = 1_000_000.0 / System.Diagnostics.Stopwatch.Frequency;
        double avgUs     = _steeringCallCount > 0 ? _steeringTotalTicks * usPerTick / _steeringCallCount : 0;
        double maxUs     = _steeringMaxTicks * usPerTick;
        double totalMs   = _steeringTotalTicks * usPerTick / 1000.0;

        Debug.Log(
            $"METRIC [{_steeringAlgorithm}] | " +
            $"Time={_timer:F2}s | " +
            $"RMS={_telemetry.CalculateRmsAcceleration():F3} | " +
            $"Deviation={_accumulatedDeviation:F1}m | " +
            $"Steering avg={avgUs:F1}µs max={maxUs:F0}µs total={totalMs:F0}ms calls={_steeringCallCount}");
    }

    private void UpdateRouteTarget()
    {
        Vector3 currentTarget = _points[_routeNavigator.CurrentTargetIndex];
        if (Vector3.Distance(currentTarget, transform.position) < RouteTargetSwitchDistance)
            _routeNavigator.CurrentTargetIndex = _routeNavigator.GetNextPoint(_routeNavigator.CurrentTargetIndex);
    }

    private NavigationContext BuildContext()
    {
        int nextIndex = _routeNavigator.GetNextPoint(_routeNavigator.CurrentTargetIndex);
        return new NavigationContext
        {
            TargetPoint     = _routeNavigator.CurrentTarget,
            TargetIndex     = _routeNavigator.CurrentTargetIndex,
            NextTargetIndex = nextIndex,
            NextTargetPoint = _points[nextIndex],
            Route           = _points,
            CurrentSpeed    = _body.velocity.magnitude
        };
    }

    private void UpdateBraking(float angleToTarget, float currentSpeed)
    {
        float absAngle = Mathf.Abs(angleToTarget);

        _shouldBrake = _shouldBrake
            ? absAngle > BrakeExitAngle  && currentSpeed > 1f
            : absAngle > BrakeEntryAngle && currentSpeed > BrakeEntrySpeed;

        _brakeLights.TurnLight(_shouldBrake);

        if (_shouldBrake)
        {
            _transmission.OnBrakingActiveChange(true, Mathf.Clamp01(currentSpeed / BrakeForceSpeed));
            _transmission.OffMoment();
        }
    }

    private void UpdateDriving(float angleToTarget, float currentSpeed)
    {
        TransmissionAngleState angleState = angleToTarget switch
        {
            > 0f => TransmissionAngleState.TurnRight,
            < 0f => TransmissionAngleState.TurnLeft,
            _    => TransmissionAngleState.Forward
        };

        float steeringAmount = Mathf.Clamp01(Mathf.Abs(angleToTarget) / _transmission.MaxEversionAngle) * SteeringDamping;

        if (!_shouldBrake)
        {
            float speedFactor         = Mathf.Clamp01(currentSpeed / SpeedFactorMax);
            float speedReductionBlend = 1f - Mathf.Clamp01(Mathf.Abs(angleToTarget) / 35f);
            steeringAmount *= Mathf.Lerp(1f, 1f - speedFactor, speedReductionBlend);

            _engine.OnMotorPullingChange(true);
            _transmission.TransferPowerToWheels(_engine.Force);
        }
        else
        {
            _engine.OnMotorPullingChange(false);
        }

        _transmission.OnTransmissionAngleStateChange(angleState, steeringAmount, CarDirection.Forward);
    }

    private void UpdateDebugLine()
    {
        Vector3 closest = PathGeometry.FindClosestPoint(transform.position, _points);
        _distanceToTrace = Vector3.Distance(transform.position, closest);

        float t     = Mathf.Clamp01(_distanceToTrace / DebugLineMaxDist);
        Color color = Color.Lerp(Color.green, Color.red, t);

        FrameworkStorage.GlobalData.LineFactory.CreateLine(
            transform.position,
            closest + Vector3.up * 0.1f,
            color);
    }

    private void UpdateUI()
    {
        _speedUI.SetSpeed((_body.velocity.magnitude * 5).ToInt());
        _speedUI.SetGear(_engine.CurrentGear);
        _timeUI.SeTime(_timer);
    }

    private IEnumerator TelemetryRoutine()
    {
        while (true)
        {
            float speed = _body.velocity.magnitude * 5;
            _accumulatedDeviation += _distanceToTrace;

            _jsonInjectionExample.UpdateSpeed(speed.ToInt());
            _jsonInjectionExample2.UpdateSpeed(_accumulatedDeviation.ToInt());

            _telemetry.Record(_timer, speed);

            yield return new WaitForSeconds(0.1f);
        }
    }
}
