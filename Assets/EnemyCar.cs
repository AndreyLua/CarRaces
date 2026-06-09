using KrisDevelopment.ERMG;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCar : MonoBehaviour
{
    [SerializeField] private ERMeshGen _meshGen;
    [SerializeField] private EngineConfig _engineConfig;

    private Transmission _transmission;
    private Rigidbody _body;
    private List<Vector3> _points;
    private int _targetPointIndex;
    private Engine _engine;
    private BrakeLights _brakeLights;
    private bool _shouldBrake;

    private EngineMoveCoreBase _engineMoveCore;

    private EngineWayPoint2tMoveCore _engineWayPoint2TMoveCore;

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
        _engineWayPoint2TMoveCore = new EngineWayPoint2tMoveCore(FrameworkStorage.GlobalData.LineFactory);
        _engineMoveCore = new EnginePurePursuitMoveCore(FrameworkStorage.GlobalData.LineFactory);
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

    public void Update()
    {

        Vector3 closestPoint = _points[_targetPointIndex];

        if (Vector3.Distance(closestPoint, transform.position) < 15)
        {
            _targetPointIndex--;
            if(_targetPointIndex < 0)
            {
                _targetPointIndex = _points.Count - 1;
            }
        }
  

        if (closestPoint != Vector3.zero)
        {
            Debug.DrawLine(transform.position, closestPoint, Color.red);
        }

        TransmissionAngleState transmissionAngleState = TransmissionAngleState.Forward;




        //


       // float angleToTarget = _engineWayPoint2TMoveCore.GetAngleToTurn(transform, _targetPointIndex, _points, _body.velocity.magnitude);//_engineMoveCore.GetAngleToTurn(transform, closestPoint);

        float angleToTarget = _engineMoveCore.GetAngleToTurn(transform, closestPoint);

       // float radius = Mathf.Abs(1f / curvature);

        //  DrawArc(radius, curvature < 0);

        //


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
    }

    void DrawArc(float radius, bool leftTurn)
    {
        Gizmos.color = Color.red;

        Vector3 center =
            transform.position +
            transform.right * (leftTurn ? -radius : radius);

        float startAngle =
            Mathf.Atan2(
                transform.position.z - center.z,
                transform.position.x - center.x
            );

        Vector3 previous = transform.position;

        for (int i = 1; i <= 20; i++)
        {
            float angleStep = Mathf.PI / 60;

            float angle =
                startAngle +
                (leftTurn ? angleStep * i : -angleStep * i);

            Vector3 next = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                transform.position.y,
                center.z + Mathf.Sin(angle) * radius
            );

         //  Debug.Log("ffff "+ next);
            Debug.DrawLine(previous, next);

            previous = next;
        }
    }
}


public abstract class EngineMoveCoreBase
{
    public abstract float GetAngleToTurn(Transform carTransform, Vector3 targetPosition);
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
    public override float GetAngleToTurn(Transform carTransform, Vector3 targetPosition)
    {
        Vector3 directionToClosestPoint = (targetPosition - carTransform.position).normalized;

        return Vector3.SignedAngle(carTransform.forward, directionToClosestPoint, Vector3.up);
    }

}

public class EngineWayPoint2tMoveCore 
{
    private LineFactory _lineFactory;

    public EngineWayPoint2tMoveCore(LineFactory lineFactory)
    {
        _lineFactory = lineFactory;
    }

    public float GetAngleToTurn(Transform carTransform , int index, List<Vector3> points, float currentSpeed)
    {
        _lineFactory.ClearLines();
        Vector3 desiredDirection =
    (points[index] - carTransform.position).normalized;

        float headingError =
            Vector3.SignedAngle(
                carTransform.forward,
                desiredDirection,
                Vector3.up
            ) * Mathf.Rad2Deg;

   
            Vector3 closest = FindPerpendicularPointOnLine(points[index], points[index-1], carTransform.position);

        Debug.DrawLine(carTransform.position, closest, Color.green);
        _lineFactory.CreateLine(carTransform.position, closest+Vector3.up * 0.1f, Color.yellow);

        _lineFactory.CreateLine(points[index]+Vector3.up*0.1f, points[index - 1] + Vector3.up * 0.1f, Color.red, 100);


        float crossTrackError = Vector3.Distance(carTransform.position, closest);


            if (DeterminePointPositionRelativeToLine(points[index], points[index - 1], carTransform.position) > 0)
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

