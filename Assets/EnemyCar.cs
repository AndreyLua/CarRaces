using EasyChart.Samples;
using KrisDevelopment.ERMG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCar : MonoBehaviour
{
    [SerializeField] private JsonInjectionExample _jsonInjectionExample;
    [SerializeField] private JsonInjectionExample _jsonInjectionExample2;
    [SerializeField] private ERMeshGen _meshGen;
    [SerializeField] private EngineConfig _engineConfig;
    [SerializeField] private NavMeshAgent _navmeshAgent;

    private SpeedUIScreen _speedUI;
    private TimeUIScreen _timeUI;

    private Transmission _transmission;
    private Rigidbody _body;
    private List<Vector3> _points;
    private Engine _engine;
    private BrakeLights _brakeLights;
    private bool _shouldBrake;

    private EngineMoveCoreBase _engineMoveCore;

    private Coroutine _sendSpeedCoroutine;

    private float _pointClosely = 0;
    private float _distanceToTrace = 0;

    private float _timer = 0;

    private List<double> _times = new List<double>();
    private List<double> _speeds = new List<double>();


    private Vector3 _navmeshTargetPosition;
    private RouteNavigator _routeNavigator;


    private void Awake()
    {
        _points = new List<Vector3>();

        foreach (var a in _meshGen.navPoints)
        {
            _points.Add(a.position);
        }

        _engine = new Engine(_engineConfig, 5);
        _body = gameObject.GetComponent<Rigidbody>();
        _brakeLights = gameObject.GetComponentInChildren<BrakeLights>();
        _transmission = gameObject.GetComponentInChildren<Transmission>();
        _body.centerOfMass += new Vector3(0, -0.8f, 0);

        _routeNavigator = new RouteNavigator(_points, transform.position, transform.forward);
        
    }

    private void Start()
    {
        _speedUI = UIScreenRepository.GetScreen<SpeedUIScreen>();
        _timeUI = UIScreenRepository.GetScreen<TimeUIScreen>();

        _engineMoveCore = new EngineWayPoint2tMoveCore(FrameworkStorage.GlobalData.LineFactory);
      //  _engineMoveCore = new EnginePurePursuitMoveCore(FrameworkStorage.GlobalData.LineFactory);
        //_engineMoveCore = new EngineWayPointtMoveCore(FrameworkStorage.GlobalData.LineFactory);


        StartSendingSpeed();
        _navmeshAgent.isStopped = true;


        //  _jsonInjectionExample.UpdateSpeed((_body.velocity.magnitude * 5).ToInt());
    }


    private IEnumerator SendSpeedEveryTwoSeconds()
    {
        while (true) // Бесконечный цикл, который будет выполняться до остановки
        {
            // Получаем скорость (мagnitude вектора скорости)
            int speed = ((_body.velocity.magnitude * 5).ToInt()); // Приведение значения к int
            _jsonInjectionExample.UpdateSpeed(speed); // Отправляем скорость

            _jsonInjectionExample2.UpdateSpeed(_pointClosely.ToInt());

            _pointClosely += _distanceToTrace;
            _times.Add(_timer);
            _speeds.Add((_body.velocity.magnitude * 5));

            yield return new WaitForSeconds(0.1f); // Ждём 2 секунды перед следующим отправлением
        }
    }

    private void StartSendingSpeed()
    {
        if (_sendSpeedCoroutine != null)
        {
            StopCoroutine(_sendSpeedCoroutine);
        }

        _sendSpeedCoroutine = StartCoroutine(SendSpeedEveryTwoSeconds());
    }

    public void Update()
    {

/*       NavMeshHit hit;
        if (NavMesh.SamplePosition(_points[15], out hit, 1.0f, NavMesh.AllAreas))
        {
            _navmeshTargetPosition = hit.position;
            Debug.Log(_navmeshTargetPosition);
        }
        if (_navmeshTargetPosition == null )
        {
            _navmeshTargetPosition = transform.position;
        }*/

        if (_routeNavigator.CurrentTargetIndex == 34)
        {
            StopCoroutine(_sendSpeedCoroutine);
            _transmission.OnBrakingActiveChange(true, 100);
            _engine.OnMotorPullingChange(false);
            
            Debug.Log("METRIC: "+ MotionMetrics.CalculateRmsAcceleration(_times,_speeds));
            return;
        }

        _timer += Time.deltaTime;

        //  _jsonInjectionExample.UpdateSpeed((_body.velocity.magnitude).ToInt());

        Vector3 closestPoint = _points[_routeNavigator.CurrentTargetIndex];

        if (Vector3.Distance(closestPoint, transform.position) < 15)
        {
        //    _routeNavigator.UpdateTarget()
            _routeNavigator.CurrentTargetIndex = _routeNavigator.GetNextPoint(_routeNavigator.CurrentTargetIndex);
   
        }
  
        TransmissionAngleState transmissionAngleState = TransmissionAngleState.Forward;


        NavigationContext context =
            new NavigationContext()
            {
                TargetPoint =
                    _routeNavigator.CurrentTarget,

                TargetIndex =
                    _routeNavigator.CurrentTargetIndex,

                NextTargetIndex =
                    _routeNavigator.GetNextPoint(_routeNavigator.CurrentTargetIndex),

                NextTargetPoint =
                    _points[_routeNavigator.GetNextPoint(_routeNavigator.CurrentTargetIndex)],
                Route =
                    _points,

                CurrentSpeed =
                    _body.velocity.magnitude
            };

        //  float angleToTarget = _engineWayPoint2TMoveCore.GetAngleToTurn(transform, _targetPointIndex, _points, _body.velocity.magnitude);
        //_engineMoveCore.GetAngleToTurn(transform, closestPoint);

        float angleToTarget = _engineMoveCore.GetAngleToTurn(transform, context);

      //  float angleToTarget = _engineMoveCore.GetAngleToTurn(transform, _navmeshTargetPosition);

        float currentSpeed = _body.velocity.magnitude;

        _shouldBrake = _shouldBrake ? (Mathf.Abs(angleToTarget) > 20f && currentSpeed > 1f) : (Mathf.Abs(angleToTarget) > 20f && currentSpeed > 6f);

        _brakeLights.TurnLight(_shouldBrake);
        if (_shouldBrake)
        {
            _transmission.OnBrakingActiveChange(_shouldBrake, Mathf.Clamp01(currentSpeed / 13f));
            _transmission.OffMoment();
        }

        if (angleToTarget > 0)
        {
            transmissionAngleState = TransmissionAngleState.TurnRight;
        }
        else if (angleToTarget < 0)
        {
            transmissionAngleState = TransmissionAngleState.TurnLeft;
        }
        else
        {
            transmissionAngleState = TransmissionAngleState.Forward;
        }

        float speedFactor = Mathf.Clamp01(_body.velocity.magnitude / 45f);

        float steeringAmount = Mathf.Clamp01(Mathf.Abs(angleToTarget) / _transmission.MaxEversionAngle);


        steeringAmount *= (1 - speedFactor);


        if (!_shouldBrake)
        {
            _engine.OnMotorPullingChange(true);
            _transmission.TransferPowerToWheels(_engine.Force);
        }
        else
        {
            _engine.OnMotorPullingChange(false);
        }

        _transmission.OnTransmissionAngleStateChange(transmissionAngleState, steeringAmount, CarDirection.Forward);

        Vector3 closest = PathGeometry.FindClosestPoint(transform.position, _points);

        _distanceToTrace = Vector3.Distance(transform.position, closest);

        float maxDistance = 6f;

        float t = Mathf.Clamp01(_distanceToTrace / maxDistance);

     
        Color color = Color.Lerp(Color.green, Color.red, t);

        FrameworkStorage.GlobalData.LineFactory.CreateLine(
            transform.position,
            closest + Vector3.up * 0.1f,
            color);

        _speedUI.SetSpeed((_body.velocity.magnitude * 5).ToInt());
        _speedUI.SetGear(_engine.CurrentGear);

        _timeUI.SeTime(_timer);
    }
}


public abstract class EngineMoveCoreBase
{
    public abstract float GetAngleToTurn(Transform carTransform, NavigationContext context);
}

public class EnginePurePursuitMoveCore: EngineMoveCoreBase
{
    private  LineFactory _lineFactory;

    public EnginePurePursuitMoveCore(LineFactory lineFactory)
    {
        _lineFactory = lineFactory;
    }

    private const float _curvatureK = 20;

    public override float GetAngleToTurn(Transform carTransform, NavigationContext context)
    {
        _lineFactory.ClearLines();
        Vector3 localTarget = carTransform.InverseTransformPoint(context.TargetPoint);
        float L = localTarget.magnitude;
        float curvature = (_curvatureK * localTarget.x) / (L * L);

        float steering = curvature * Mathf.Rad2Deg;

        float angleToTarget = steering;
        float radius = Mathf.Abs(1f / curvature);


        _lineFactory.DrawArc(carTransform, radius, curvature < 0);
        return angleToTarget;
    }
}

public class EngineWayPointtMoveCore : EngineMoveCoreBase
{
    private LineFactory _lineFactory;


    public EngineWayPointtMoveCore(LineFactory lineFactory)
    {
        _lineFactory = lineFactory;
    }

    public override float GetAngleToTurn(Transform carTransform, NavigationContext context)
    {
        _lineFactory.ClearLines();

        Vector3 directionToClosestPoint = (context.TargetPoint - carTransform.position).normalized;

        _lineFactory.CreateLine(carTransform.position, context.TargetPoint, Color.blue);


        return Vector3.SignedAngle(carTransform.forward, directionToClosestPoint, Vector3.up);
    }
}

public class EngineWayPoint2tMoveCore : EngineMoveCoreBase
{
    private readonly LineFactory _lineFactory;

    private const float HeadingGain = 0.01f;
    private const float CrossTrackGain = 1f;
    private const float SpeedFactor = 6.7f;
    private const float MinSpeed = 0.1f;

    public EngineWayPoint2tMoveCore(LineFactory lineFactory)
    {
        _lineFactory = lineFactory;
    }

    public override float GetAngleToTurn(
        Transform carTransform,
        NavigationContext context)
    {
        _lineFactory.ClearLines();

        Vector3 currentPoint = context.TargetPoint;
        Vector3 nextPoint = context.NextTargetPoint;

        float headingError =
            CalculateHeadingError(
                carTransform,
                currentPoint);

        Vector3 closestPoint =
            PathGeometry.FindPerpendicularPointOnLine(
                currentPoint,
                nextPoint,
                carTransform.position);

        DrawDebug(
            carTransform.position,
            closestPoint,
            currentPoint,
            nextPoint);

        float crossTrackError =
            CalculateCrossTrackError(
                carTransform.position,
                closestPoint,
                currentPoint,
                nextPoint);

        float steering =
            CalculateSteering(
                headingError,
                crossTrackError,
                context.CurrentSpeed);

        Debug.Log($"headingError: {headingError}");
        Debug.Log($"crossTrackError: {crossTrackError}");
        Debug.Log($"Steering: {steering}");

        return steering;
    }

    private float CalculateHeadingError(
        Transform carTransform,
        Vector3 targetPoint)
    {
        Vector3 desiredDirection =
            (targetPoint - carTransform.position).normalized;

        return Vector3.SignedAngle(
                   carTransform.forward,
                   desiredDirection,
                   Vector3.up)
               * Mathf.Rad2Deg;
    }

    private float CalculateCrossTrackError(
        Vector3 carPosition,
        Vector3 closestPoint,
        Vector3 segmentStart,
        Vector3 segmentEnd)
    {
        float error =
            Vector3.Distance(
                carPosition,
                closestPoint);

        bool isLeftSide =
            PathGeometry.DeterminePointPositionRelativeToLine(
                segmentStart,
                segmentEnd,
                carPosition) > 0;

        if (isLeftSide)
        {
            error *= -1;
        }

        return error;
    }

    private float CalculateSteering(
        float headingError,
        float crossTrackError,
        float speed)
    {
        return headingError * HeadingGain +
               Mathf.Atan(
                   (CrossTrackGain * crossTrackError)
                   / Mathf.Max(speed * SpeedFactor, MinSpeed))
               * Mathf.Rad2Deg;
    }

    private void DrawDebug(
        Vector3 carPosition,
        Vector3 closestPoint,
        Vector3 currentPoint,
        Vector3 nextPoint)
    {
        _lineFactory.CreateLine(
            carPosition,
            closestPoint + Vector3.up * 0.1f,
            Color.yellow);

        _lineFactory.CreateLine(
            currentPoint + Vector3.up * 0.1f,
            nextPoint + Vector3.up * 0.1f,
            Color.red,
            100);
    }
}
