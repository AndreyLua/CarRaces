using EasyChart.Samples;
using KrisDevelopment.ERMG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCar : MonoBehaviour
{
    [SerializeField] private JsonInjectionExample _jsonInjectionExample;
    [SerializeField] private JsonInjectionExample _jsonInjectionExample2;



    [SerializeField] private ERMeshGen _meshGen;
    [SerializeField] private EngineConfig _engineConfig;

    private SpeedUIScreen _speedUI;
    private TimeUIScreen _timeUI;

    private Transmission _transmission;
    private Rigidbody _body;
    private List<Vector3> _points;
    private int _targetPointIndex;
    private Engine _engine;
    private BrakeLights _brakeLights;
    private bool _shouldBrake;

    private EngineMoveCoreBase _engineMoveCore;

    private EngineWayPoint2tMoveCore _engineWayPoint2TMoveCore;
    private Coroutine _sendSpeedCoroutine;

    private float _pointClosely = 0;
    private float _distanceToTrace = 0;

    private float _timer = 0;

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

        _targetPointIndex = FindClosestPointAhead(transform.position, transform.forward);  
    }

    private void Start()
    {
        _speedUI = UIScreenRepository.GetScreen<SpeedUIScreen>();
        _timeUI = UIScreenRepository.GetScreen<TimeUIScreen>();

        _engineWayPoint2TMoveCore = new EngineWayPoint2tMoveCore(FrameworkStorage.GlobalData.LineFactory);
       // _engineMoveCore = new EnginePurePursuitMoveCore(FrameworkStorage.GlobalData.LineFactory);
        _engineMoveCore = new EngineWayPointtMoveCore(FrameworkStorage.GlobalData.LineFactory);


        StartSendingSpeed();

      //  _jsonInjectionExample.UpdateSpeed((_body.velocity.magnitude * 5).ToInt());
    }

    private int FindClosestPointAhead(Vector3 carPosition, Vector3 carDirection)
    {
        int closestPointIndex = -1;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < _points.Count; i++)
        {
            Vector3 directionToPoint = _points[i] - carPosition;
            float forwardDot = Vector3.Dot(carDirection, directionToPoint.normalized);

            if (forwardDot > 0)
            {
                float distance = Vector3.Distance(carPosition, _points[i]);

            
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPointIndex = i;
                }
            }
        }

        return closestPointIndex;
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

    public int GetNextPoint(int indexPoint)
    {
        if (indexPoint - 1 < 0)
        {
            return _points.Count-1;
        }
        return indexPoint - 1;
    }

    public int GetPreviousPoint(int indexPoint)
    {
        if (indexPoint + 1 >= _points.Count)
        {
            return 0;
        }
        return indexPoint + 1;
    }


    public void Update()
    {
        if (_targetPointIndex == 34)
        {
            _transmission.OnBrakingActiveChange(true, 100);
            _engine.OnMotorPullingChange(false);
            return;
        }

        _timer += Time.deltaTime;

        //  _jsonInjectionExample.UpdateSpeed((_body.velocity.magnitude).ToInt());

        Vector3 closestPoint = _points[_targetPointIndex];

        if (Vector3.Distance(closestPoint, transform.position) < 15)
        {
            _targetPointIndex = GetNextPoint(_targetPointIndex);
   
        }
  
        TransmissionAngleState transmissionAngleState = TransmissionAngleState.Forward;

       // float angleToTarget = _engineWayPoint2TMoveCore.GetAngleToTurn(transform, _targetPointIndex, _points, _body.velocity.magnitude);//_engineMoveCore.GetAngleToTurn(transform, closestPoint);

        float angleToTarget = _engineMoveCore.GetAngleToTurn(transform, closestPoint);

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

        Vector3 closest = FindPerpendicularPointOnLine(_points[GetPreviousPoint(FindClosestPointAhead(transform.position, transform.forward)+1)], _points[FindClosestPointAhead(transform.position, transform.forward)], transform.position);

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

    Vector3 FindPerpendicularPointOnLine(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 lineDir = end - start;
        Vector3 pointDir = point - start;
        float t = Vector3.Dot(pointDir, lineDir) / Vector3.Dot(lineDir, lineDir);
        Vector3 closestPointOnLine = start + t * lineDir;
        return closestPointOnLine;
    }

}


public abstract class EngineMoveCoreBase
{
    public abstract float GetAngleToTurn(Transform carTransform, Vector3 targetPosition);

    protected Vector3 FindPerpendicularPointOnLine(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 lineDir = end - start;
        Vector3 pointDir = point - start;
        float t = Vector3.Dot(pointDir, lineDir) / Vector3.Dot(lineDir, lineDir);
        Vector3 closestPointOnLine = start + t * lineDir;
        return closestPointOnLine;
    }

}

public class EnginePurePursuitMoveCore: EngineMoveCoreBase
{
    private  LineFactory _lineFactory;

    public EnginePurePursuitMoveCore(LineFactory lineFactory)
    {
        _lineFactory = lineFactory;
    }

    private const float _curvatureK = 20;

    public override float GetAngleToTurn(Transform carTransform, Vector3 targetPosition)
    {
        _lineFactory.ClearLines();
        Vector3 localTarget = carTransform.InverseTransformPoint(targetPosition);
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
    public override float GetAngleToTurn(Transform carTransform, Vector3 targetPosition)
    {
        _lineFactory.ClearLines();

        Vector3 directionToClosestPoint = (targetPosition - carTransform.position).normalized;

        _lineFactory.CreateLine(carTransform.position, targetPosition, Color.blue);


        return Vector3.SignedAngle(carTransform.forward, directionToClosestPoint, Vector3.up);
    }

}

public class EngineWayPoint2tMoveCore 
{
    private LineFactory _lineFactory;
    private List<Vector3> _points;

    public EngineWayPoint2tMoveCore(LineFactory lineFactory)
    {
        _lineFactory = lineFactory;
    }

    public float GetAngleToTurn(Transform carTransform , int index, List<Vector3> points, float currentSpeed)
    {
        _points = points;

        _lineFactory.ClearLines();
        Vector3 desiredDirection =
    (points[index] - carTransform.position).normalized;

        float headingError =
            Vector3.SignedAngle(
                carTransform.forward,
                desiredDirection,
                Vector3.up
            ) * Mathf.Rad2Deg;

   
            Vector3 closest = FindPerpendicularPointOnLine(points[index], points[GetNextPoint(index)], carTransform.position);

        _lineFactory.CreateLine(carTransform.position, closest+Vector3.up * 0.1f, Color.yellow);

        _lineFactory.CreateLine(points[index]+Vector3.up*0.1f, points[GetNextPoint(index)] + Vector3.up * 0.1f, Color.red, 100);


        float crossTrackError = Vector3.Distance(carTransform.position, closest);


            if (DeterminePointPositionRelativeToLine(points[index], points[GetNextPoint(index)], carTransform.position) > 0)
            {
                crossTrackError *= -1;
            }

            float k =1f; // headingError * 2f
        float steering = headingError* 0.01f + Mathf.Atan((k * crossTrackError) / Mathf.Max(currentSpeed*5f, 0.1f)) * Mathf.Rad2Deg;

            steering *=1;
      
        Debug.Log("headingError: " + headingError);
        Debug.Log("crossTrackError: " + crossTrackError);
        Debug.Log("Steering: " + steering);

        return steering;

    }

    public int GetNextPoint(int indexPoint)
    {
        if (indexPoint - 1 < 0)
        {
            return _points.Count - 1;
        }
        return indexPoint - 1;
    }

    public int GetPreviousPoint(int indexPoint)
    {
        if (indexPoint + 1 >= _points.Count)
        {
            return 0;
        }
        return indexPoint + 1;
    }

    Vector3 FindPerpendicularPointOnLine(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 lineDir = end - start;
        Vector3 pointDir = point - start;
        float t = Vector3.Dot(pointDir, lineDir) / Vector3.Dot(lineDir, lineDir);
        Vector3 closestPointOnLine = start + t * lineDir;
        return closestPointOnLine;
    }

    float DeterminePointPositionRelativeToLine(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 lineDir = end - start;
        Vector3 pointDir = point - start;
        Vector3 crossProduct = Vector3.Cross(lineDir, pointDir);
        return crossProduct.y;
    }

}

