using UnityEngine;

public class Engine
{
    private float _startForce;
    private float _force;
    private float _forceAcceleration;

    private bool _isPull;

    public float Force => _force;
    public float StartForce => _startForce;

    public Engine(EngineConfig engineConfig)
    {
        _startForce = engineConfig.StartForce;
        _forceAcceleration = engineConfig.ForceAcceleration;

    }

    public void Update()
    {
        _force += _force * _forceAcceleration * Time.deltaTime;
    }

    public void OnMotorPullingChange(bool state)
    {
        _isPull = state;
        if (_isPull)
            _force = _startForce;
        else
        {
            _force = 0;
        }
    }

    public void StopMotor()
    {
        _force = 0;
    }
}