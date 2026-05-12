using System.Collections.Generic;
using UnityEngine;

public class Transmission : MonoBehaviour
{
    [SerializeField] private Rigidbody _body;
    [SerializeField] private TransmissionConfig _transmissionConfig;
    [SerializeField] private WheelbaseConfig _wheelbaseConfig;

    private TransmissionAngleState _state;
    private bool _isBraking;

    private Dictionary<WheelsQuadPositionId, Wheel> _wheels;

    private float _wheelRotationTimer = 0;
    private float _eversionAngle = 0;
    private float _eversionRealAngle = 0;

    private CarDirection _carDirection = CarDirection.Forward;

    private void Awake()
    {
        InitAxes();
        _state = TransmissionAngleState.Forward;
    }

    private void InitAxes()
    {
        _wheels = new Dictionary<WheelsQuadPositionId, Wheel>();
        Vector3 widthWheelOffset = new Vector3(_wheelbaseConfig.Width / 2, 0, 0);
        Vector3 lengthWheelOffset = new Vector3(0, 0, _wheelbaseConfig.Length / 2);

        Vector3 transmissionUpOffset = new Vector3(0, _transmissionConfig.SuspensionWheelDistance, 0);  

        Quaternion reverse = Quaternion.Euler(new Vector3(0, 0, 180));

        _wheels[WheelsQuadPositionId.FrontLeft] = Instantiate<Wheel>(_wheelbaseConfig.FrontWheel,
        gameObject.transform.position - widthWheelOffset + lengthWheelOffset + transmissionUpOffset, reverse);
        _wheels[WheelsQuadPositionId.RearLeft] = Instantiate<Wheel>(_wheelbaseConfig.RearWheel,
        gameObject.transform.position - widthWheelOffset - lengthWheelOffset + transmissionUpOffset, reverse);

        _wheels[WheelsQuadPositionId.FrontRight] = Instantiate<Wheel>(_wheelbaseConfig.FrontWheel,
        gameObject.transform.position + widthWheelOffset + lengthWheelOffset+ transmissionUpOffset, Quaternion.identity);
        _wheels[WheelsQuadPositionId.RearRight] = Instantiate<Wheel>(_wheelbaseConfig.RearWheel,
        gameObject.transform.position + widthWheelOffset - lengthWheelOffset+ transmissionUpOffset, Quaternion.identity);

        foreach (var wheel in _wheels)
        {
            wheel.Value.transform.parent = gameObject.transform;
            wheel.Value.WheelCollider.suspensionDistance = _transmissionConfig.SuspensionWheelDistance;
        }
    }

    private void Update()
    {
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
      
                break;
            case TransmissionAngleState.TurnLeft:
                _eversionAngle = -_transmissionConfig.MaxEversionAngle;
              
                break;
            case TransmissionAngleState.TurnRight:
                _eversionAngle = _transmissionConfig.MaxEversionAngle;
     
                break;
        }
    }

    private void UpdateAng(float procent)
    {
 
        float newAngle = _transmissionConfig.MaxEversionAngle * procent;
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
        _wheels[WheelsQuadPositionId.FrontRight].WheelCollider.steerAngle = _eversionRealAngle;
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

    public void Reset()
    {
        OffMoment();
        _state = TransmissionAngleState.Forward;
    }

    private void Braking()
    {
        if (_isBraking)
        {
            foreach (var wheel in _wheels)
            {
                wheel.Value.WheelCollider.motorTorque = 0;
                wheel.Value.WheelCollider.brakeTorque = _transmissionConfig.BrakeTorque;
            }
        }
        else
        {
            ResetBrakingState();
        }
    }

    public void OffMoment()
    {
        foreach (var wheel in _wheels) {
            wheel.Value.WheelCollider.motorTorque = 0;
            wheel.Value.WheelCollider.rotationSpeed *= 0.6f;
                }
    }

    public void ResetBrakingState()
    {
        _isBraking = false;
        foreach (var wheel in _wheels)
            wheel.Value.WheelCollider.brakeTorque = 0;
    }

    public void TransferPowerToWheels(float power)
    {
        switch (_transmissionConfig.AllWheelDrive)
        {
            case WheelDriveType.Front:
                _wheels[WheelsQuadPositionId.FrontLeft].WheelCollider.motorTorque = power;
                _wheels[WheelsQuadPositionId.FrontRight].WheelCollider.motorTorque = power;
                break;
            case WheelDriveType.Rear:
                _wheels[WheelsQuadPositionId.RearLeft].WheelCollider.motorTorque = power;
                _wheels[WheelsQuadPositionId.RearRight].WheelCollider.motorTorque = power;
                break;
            case WheelDriveType.All:
                foreach (var wheel in _wheels)
                    wheel.Value.WheelCollider.motorTorque = power;
                break;
        }
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
