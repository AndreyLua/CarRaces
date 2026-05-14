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

    private void Awake()
    {
        _points = new List<Vector3>();

        foreach (var a in _meshGen.navPoints)
        {
            _points.Add(a.position);
        }

        _engine = new Engine(_engineConfig, 5);
        _body = gameObject.GetComponent<Rigidbody>();
        _transmission = gameObject.GetComponentInChildren<Transmission>();
        _body.centerOfMass += new Vector3(0, -0.8f, 0);

        _targetPointIndex = FindClosestPointAhead(transform.position, transform.forward);

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

        if (Vector3.Distance(closestPoint, transform.position) < 10)
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

        Vector3 directionToClosestPoint = (closestPoint - transform.position).normalized;
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionToClosestPoint, Vector3.up);



        float currentSpeed = _body.velocity.magnitude;

     
        bool shouldBrake = Mathf.Abs(angleToTarget) > 60f && currentSpeed>10;

        if (shouldBrake)
        {
            _transmission.OnBrakingActiveChange(shouldBrake);
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
        float steeringAmount = Mathf.Clamp01(Mathf.Abs(angleToTarget) / _transmission.MaxEversionAngle);

        if (!shouldBrake)
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





    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Устанавливаем цвет для гизмов

        for (int i = 0; i < _points.Count; i++)
        {
            // Отрисовали луч от текущей точки до следующей точки (если она существует)
            Vector3 startPoint = _points[i];
            Gizmos.DrawSphere(startPoint, 0.1f); // Отрисовка сферы в каждой точке

            if (i < _points.Count - 1)
            {
                Vector3 endPoint = _points[i + 1];
                Gizmos.DrawLine(startPoint, endPoint); // Отрисовка линии между точками
            }
        }
    }
}
