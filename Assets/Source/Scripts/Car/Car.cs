using System;
using UnityEngine;

public class Car : MonoBehaviour, ITriggerable
{
    private Camera _camera;
    private Engine _engine;


    [SerializeField] private BrakeLights _brakeLights;
    [SerializeField] private EngineConfig _engineConfig;
    [SerializeField] private Transmission _transmission;
    [SerializeField] private UserInputCarControl _userInputCarControl;

    private Rigidbody _body;
    private CarDirection _carDirection;
    public event Action<ITriggerable> OnTriggerRemoveForced;

    public Rigidbody Body => _body;
    public Transform Transform => transform;
    public Vector3 Velocity => _body.velocity;

    private void Awake()
    {
        _engine = new Engine(_engineConfig, 5);
    }

    private void Start()
    {
        _camera = Camera.main;
        _body = gameObject.GetComponent<Rigidbody>();
        _body.centerOfMass += new Vector3(0, -0.6f, 0);
    }

    public void ResetMe()
    {
        _body.velocity = Vector3.zero;
        _body.angularVelocity = Vector3.zero;
        _engine.StopMotor();
        _transmission.Reset();
    }

    private void Update()
    {
        Move();
    }

    private void EnginePull(bool kek)
    {
        _engine.OnMotorPullingChange(kek);
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
            EnginePull(true);
        }
        else
        {
            EnginePull(false);
            _body.velocity *= 1-0.5f*Time.deltaTime;
            _transmission.OffMoment();
        }

        if (input.y > 0)
        {
            if (isForward == null || isForward == true)
            {
                _carDirection = CarDirection.Forward;
            }
            else
            {
            //    _transmission.OnBrakingActiveChange(true);
      
            }
        }

        if (input.y < 0)
        {
            if (isForward == null || isForward == false)
            {
                _carDirection = CarDirection.Back;
            }
            else
            {
           //     _transmission.OnBrakingActiveChange(true);
               
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

        float procent = Math.Abs(input.x);

        _transmission.OnTransmissionAngleStateChange(transmissionAngleState, procent, _carDirection);

        _transmission.TransferPowerToWheels(_engine.Force* (int)_carDirection);    

        _transmission.OnBrakingActiveChange(FrameworkStorage.GlobalData.UserInput.IsBraking);

    }
}
