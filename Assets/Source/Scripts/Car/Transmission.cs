using System.Collections.Generic;
using UnityEngine;

public class Transmission : MonoBehaviour
{
    [SerializeField] private Wheel _frontWheel;
    [SerializeField] private Wheel _rearWheel;
    [SerializeField] private float _width;
    [SerializeField] private float _length;
    [SerializeField] private float _suspensionWheelDistance = 0;
    [SerializeField] private float _maxEversionAngle = 30;
    [SerializeField] private float _weightWheel=1;
    [SerializeField] private float _brakeTorque = 100;
    [SerializeField] private float _brakeTime = 0;
    [SerializeField] private Rigidbody _body;

    private TransmissionAngleState _state;
    private float _eversionAngle =0;
    private float _frontFriction;
    private float _rearFriction;
    private Vector3 _frontFrictionForce;
    private Vector3 _rearFrictionForce;
    private bool _isBraking;
    private Dictionary<WheelsQuadPositionId, Wheel> _wheels;
    private float _brakeTimer = 0;
    private Vector3 _brakeSpeed;
    private float _wheelRotationTimer = 0;
    private float _eversionRealAngle = 0;

    private CarDirection _carDirection = CarDirection.Forward;


    public float Length => _length;
    public float Width => _width;
    public float EversionAngle => _eversionAngle;
    public Vector3 FrontFrictionForce => _frontFrictionForce;
    public Vector3 RearFrictionForce => _rearFrictionForce;
   
    private void Awake()
    {
        InitializingAxes();
        _frontFriction = _wheels[WheelsQuadPositionId.FrontLeft].Friction + _wheels[WheelsQuadPositionId.FrontRight].Friction;
        _rearFriction = _wheels[WheelsQuadPositionId.RearLeft].Friction + _wheels[WheelsQuadPositionId.RearRight].Friction;
        _state = TransmissionAngleState.Forward;
    }

    private void InitializingAxes()
    {
        _wheels = new Dictionary<WheelsQuadPositionId, Wheel>();
        Vector3 widthWheelOffset = new Vector3(_width / 2, 0, 0);
        Vector3 lengthWheelOffset = new Vector3(0, 0, _length / 2);
        Vector3 transmissionUpOffset = new Vector3(0, _suspensionWheelDistance,0);  

        Quaternion reverse = Quaternion.Euler(new Vector3(0, 0, 180));

        _wheels[WheelsQuadPositionId.FrontLeft] = Instantiate<Wheel>(_frontWheel,
        gameObject.transform.position - widthWheelOffset + lengthWheelOffset + transmissionUpOffset, reverse);
        _wheels[WheelsQuadPositionId.RearLeft] = Instantiate<Wheel>(_frontWheel,
        gameObject.transform.position - widthWheelOffset - lengthWheelOffset + transmissionUpOffset, reverse);

        _wheels[WheelsQuadPositionId.FrontRight] = Instantiate<Wheel>(_frontWheel,
        gameObject.transform.position + widthWheelOffset + lengthWheelOffset+ transmissionUpOffset, Quaternion.identity);
        _wheels[WheelsQuadPositionId.RearRight] = Instantiate<Wheel>(_frontWheel,
        gameObject.transform.position + widthWheelOffset - lengthWheelOffset+ transmissionUpOffset, Quaternion.identity);

        foreach (var wheel in _wheels)
        {
            wheel.Value.transform.parent = gameObject.transform;
            wheel.Value.WheelCollider.suspensionDistance = _suspensionWheelDistance;
        }
    }

    private void Update()
    {
        if (_isBraking)
        {
            _body.velocity = Vector3.Lerp(_brakeSpeed, Vector3.zero, _brakeTimer / _brakeTime);
            _brakeTimer += Time.deltaTime;
        }
        else
        {
            _brakeTimer = 0;
        }

        if (_wheelRotationTimer<1)
        {
            _wheelRotationTimer += Time.deltaTime;
            _eversionRealAngle = Mathf.Lerp(_eversionRealAngle, _eversionAngle, _wheelRotationTimer);
            RotateFrontWheels();
        }
        UpdateWheelVisualization(_wheels[WheelsQuadPositionId.FrontRight].WheelCollider, _wheels[WheelsQuadPositionId.FrontRight].Skin, false);
        UpdateWheelVisualization(_wheels[WheelsQuadPositionId.FrontLeft].WheelCollider, _wheels[WheelsQuadPositionId.FrontLeft].Skin, true);
        UpdateWheelVisualization(_wheels[WheelsQuadPositionId.RearRight].WheelCollider, _wheels[WheelsQuadPositionId.RearRight].Skin, false);
        UpdateWheelVisualization(_wheels[WheelsQuadPositionId.RearLeft].WheelCollider, _wheels[WheelsQuadPositionId.RearLeft].Skin, true);
    }

    private void ChangeTransmissionAngle()
    {
        _wheelRotationTimer = 0;
        switch (_state)
        {
            case TransmissionAngleState.Forward:
                _eversionAngle = 0;
                _frontFrictionForce = Vector3.zero;
                _rearFrictionForce = Vector3.zero;  
                break;
            case TransmissionAngleState.TurnLeft:
                _eversionAngle = -_maxEversionAngle;
                _frontFrictionForce = -transform.right * _frontFriction * _weightWheel;
                _rearFrictionForce = transform.right * _rearFriction * _weightWheel;
                break;
            case TransmissionAngleState.TurnRight:
                _eversionAngle = _maxEversionAngle;
                _frontFrictionForce = transform.right * _frontFriction * _weightWheel;
                _rearFrictionForce = -transform.right * _rearFriction * _weightWheel;
                break;
        }
    }

    private void UpdateAng(float procent)
    {
 
        float newAngle = _maxEversionAngle * procent;
        switch (_state)
        {
            case TransmissionAngleState.Forward:
                _eversionAngle = 0;
                break;
            case TransmissionAngleState.TurnLeft:
                _eversionAngle = -newAngle;

                break;
            case TransmissionAngleState.TurnRight:
                _eversionAngle = newAngle;
 
                break;
        }
        RotateFrontWheels();
    }

    private void RotateFrontWheels()
    {
        _wheels[WheelsQuadPositionId.FrontRight].WheelCollider.steerAngle =_eversionRealAngle;
        _wheels[WheelsQuadPositionId.FrontLeft].WheelCollider.steerAngle = _eversionRealAngle;
    }

    private void UpdateWheelVisualization(WheelCollider collider, Transform visualization, bool isLeftWheel)
    {
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        visualization.position = position;
        visualization.rotation = rotation;
        if (isLeftWheel)
            visualization.Rotate(0, 0, 180);
    }

    public void ResetMe()
    {
        _wheels[WheelsQuadPositionId.FrontLeft].WheelCollider.motorTorque = 0;
       _wheels[WheelsQuadPositionId.FrontRight].WheelCollider.motorTorque = 0;
        _wheels[WheelsQuadPositionId.RearLeft].WheelCollider.motorTorque = 0;
        _wheels[WheelsQuadPositionId.RearRight].WheelCollider.motorTorque = 0;

        _frontFriction = _wheels[WheelsQuadPositionId.FrontLeft].Friction + _wheels[WheelsQuadPositionId.FrontRight].Friction;
        _rearFriction = _wheels[WheelsQuadPositionId.RearLeft].Friction + _wheels[WheelsQuadPositionId.RearRight].Friction;
        _state = TransmissionAngleState.Forward;
    }

    private void Braking()
    {
        if (_isBraking)
        {
            _brakeSpeed = _body.velocity;
            _brakeTimer += Time.deltaTime;
            _wheels[WheelsQuadPositionId.FrontLeft].WheelCollider.motorTorque = 0;
            _wheels[WheelsQuadPositionId.FrontRight].WheelCollider.motorTorque = 0;
            _wheels[WheelsQuadPositionId.RearLeft].WheelCollider.motorTorque = 0;
            _wheels[WheelsQuadPositionId.RearRight].WheelCollider.motorTorque = 0;


            _wheels[WheelsQuadPositionId.FrontLeft].WheelCollider.brakeTorque = 100000;
            _wheels[WheelsQuadPositionId.FrontRight].WheelCollider.brakeTorque = 100000;
            _wheels[WheelsQuadPositionId.RearLeft].WheelCollider.brakeTorque = 100000;
            _wheels[WheelsQuadPositionId.RearRight].WheelCollider.brakeTorque = 100000;

        }
        else
        {
            _brakeTimer = 0;
            ResetBrakingState();
        }
    }

    public void OffMoment()
    {
        _wheels[WheelsQuadPositionId.FrontLeft].WheelCollider.motorTorque = 0;
        _wheels[WheelsQuadPositionId.FrontRight].WheelCollider.motorTorque = 0;
        _wheels[WheelsQuadPositionId.RearLeft].WheelCollider.motorTorque = 0;
        _wheels[WheelsQuadPositionId.RearRight].WheelCollider.motorTorque = 0;
    }

    public void ResetBrakingState()
    {
        _brakeTimer = 0;
        _isBraking = false;
        _wheels[WheelsQuadPositionId.FrontLeft].WheelCollider.brakeTorque = 0;
        _wheels[WheelsQuadPositionId.FrontRight].WheelCollider.brakeTorque = 0;
        _wheels[WheelsQuadPositionId.RearLeft].WheelCollider.brakeTorque = 0;
        _wheels[WheelsQuadPositionId.RearRight].WheelCollider.brakeTorque = 0;
    }

    public void TransferPowerToWheels(float power)
    {
        _wheels[WheelsQuadPositionId.FrontLeft].WheelCollider.motorTorque = power;
        _wheels[WheelsQuadPositionId.FrontRight].WheelCollider.motorTorque = power;
    }

    public void OnTransmissionAngleStateChange(TransmissionAngleState state, float procent, CarDirection carDirection)
    {
        if (state != _state || _carDirection!= carDirection)
        {
            _carDirection = carDirection;
            _state = state;
            ChangeTransmissionAngle();
        }
        UpdateAng(procent);
    }

    public void OnBrakingActiveChange(bool isBraking)
    {
        if (_isBraking != isBraking)
        {
            _isBraking = isBraking;
            Braking();
        }
    }
}
