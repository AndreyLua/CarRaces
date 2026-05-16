using System;
using UnityEngine;

public class Car : MonoBehaviour, ITriggerable
{
    private Camera _camera;
    private Engine _engine;


    [SerializeField] private BrakeLights _brakeLights;
    [SerializeField] private EngineConfig _engineConfig;
    private Transmission _transmission;
    [SerializeField] private UserInputCarControl _userInputCarControl;

    private Rigidbody _body;
    private CarDirection _carDirection;
    private SpeedUIScreen _speedUI;
    public event Action<ITriggerable> OnTriggerRemoveForced;

    public Rigidbody Body => _body;
    public Transform Transform => transform;
    public Vector3 Velocity => _body.velocity;

    private void Awake()
    {
        _engine = new Engine(_engineConfig, 5);
        _transmission = gameObject.GetComponentInChildren<Transmission>();
        _speedUI = UIScreenRepository.GetScreen<SpeedUIScreen>();
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
        _transmission.Restart();
    }

    private void Update()
    {
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
