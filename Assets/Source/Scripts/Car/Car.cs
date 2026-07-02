using System;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour, ITriggerable
{
    private Camera _camera;
    private Engine _engine;


    [SerializeField] private BrakeLights _brakeLights;
    [SerializeField] private EngineConfig _engineConfig;
    private Transmission _transmission;
    [SerializeField] private UserInputCarControl _userInputCarControl;

    private TargetArrowUIScreen _targetArrowUIScreen;

    private Rigidbody _body;
    private CarDirection _carDirection;
    private SpeedUIScreen _speedUI;
    public event Action<ITriggerable> OnTriggerRemoveForced;

    public Rigidbody Body => _body;
    public Transform Transform => transform;
    public Vector3 Velocity => _body.velocity;

    private List<Vector3> _points;
    private int _point = 0;

    private void Awake()
    {
        _engine = new Engine(_engineConfig, 5);
        _transmission = gameObject.GetComponentInChildren<Transmission>();
        _speedUI = UIScreenRepository.GetScreen<SpeedUIScreen>();
        _targetArrowUIScreen = UIScreenRepository.GetScreen<TargetArrowUIScreen>();
    }


    private void Start()
    {
        _camera = Camera.main;
        _body = gameObject.GetComponent<Rigidbody>();
        _body.centerOfMass += new Vector3(0, -0.6f, 0);


        _points = new List<Vector3>();
       
        foreach (var a in FrameworkStorage.GlobalData.MeshGen.navPoints)
        {
            _points.Add(a.position);
        }
        _point = FindClosestPointAhead(transform.position, transform.forward);

    }

    public void ResetMe()
    {
        _body.velocity = Vector3.zero;
        _body.angularVelocity = Vector3.zero;
        _engine.StopMotor();
        _transmission.Restart();
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

    private void Update()
    {
      
        Vector3 directionToTarget = _points[_point] - transform.position;
        if (directionToTarget != Vector3.zero)
        {
         //   Debug.DrawLine(transform.position, _points[FindClosestPointAhead(transform.position, transform.forward)], Color.red);

            //Debug.DrawLine(transform.position, transform.position+ directionToTarget*100, Color.red);
        }
        float angle = Mathf.Atan2(directionToTarget.z, directionToTarget.x) * Mathf.Rad2Deg ;


  //      Debug.Log(directionToTarget);
     //   Debug.Log(directionToTarget.x);
    //    Debug.Log(directionToTarget.y);
    //    Debug.Log(directionToTarget.z);


        _targetArrowUIScreen.SetTargetAngle(angle);
        Move();
        _speedUI.SetSpeed((_body.velocity.magnitude*5).ToInt());
        _speedUI.SetGear(_engine.CurrentGear);
    }

    private void Move()
    {
        Vector2 input = FrameworkStorage.GlobalData.UserInput.JoystickOffcet;

        TransmissionAngleState transmissionAngleState = TransmissionAngleState.Forward;

        float norm = Vector3.Dot(_camera.transform.forward.XZ(), _body.velocity.XZ().normalized);

        bool? isForward = null;

        if (norm > 0.99f)
        {
            isForward = true;
        }
        else if (norm < -0.99f)
        {
            isForward = false;
        }

        if (input.y != 0)
        {
            _engine.OnMotorPullingChange(true);
            _transmission.TransferPowerToWheels(_engine.Force * (int)_carDirection);
        }
        else
        {
            _engine.OnMotorPullingChange(false);
            _transmission.OffMoment();
        }

        if (input.y > 0)
        {
            if (isForward == null || isForward == true)
            {
                _carDirection = CarDirection.Forward;
            }
        }

        if (input.y < 0)
        {
            if (isForward == null || isForward == false)
            {
                _carDirection = CarDirection.Back;
            }
        }


        if (input.x > 0)
            transmissionAngleState = TransmissionAngleState.TurnRight;
        if (input.x < 0)
            transmissionAngleState = TransmissionAngleState.TurnLeft;

        if (input.x == 0)
        {
            transmissionAngleState = TransmissionAngleState.Forward;
        }

        float speedFactor = Mathf.Clamp01(_body.velocity.magnitude / 40f);

        float procent = Mathf.Lerp(Math.Abs(input.x), 0, speedFactor);

        _transmission.OnTransmissionAngleStateChange(transmissionAngleState, procent, _carDirection);

        _transmission.OnBrakingActiveChange(FrameworkStorage.GlobalData.UserInput.IsBraking, 1);
        _brakeLights.TurnLight(FrameworkStorage.GlobalData.UserInput.IsBraking);
        if (FrameworkStorage.GlobalData.UserInput.IsBraking)
        {
            _transmission.OffMoment();
        }
    }
}
