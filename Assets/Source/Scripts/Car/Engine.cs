using UnityEngine;

public class Engine
{
    private float _startForce;
    private float _force;
    private float _forceAcceleration;

    private bool _isPull;
    private int _currentGear;
    private int _maxGears;

    public float Force => _force;
    public float StartForce => _startForce;

    public Engine(EngineConfig engineConfig, int maxGears)
    {
        _startForce = engineConfig.StartForce;
        _forceAcceleration = engineConfig.ForceAcceleration;
        _maxGears = maxGears;
        _currentGear = 1;  
    }

    private void ManageGears()
    {
        float gearMultiplier = 1f + (_currentGear - 1) * _forceAcceleration;
        _force = _startForce * gearMultiplier;

        if (Time.time % 3 < Time.deltaTime) 
        {
            ShiftUp();
        }
    }

    private void ShiftUp()
    {
        if (_currentGear < _maxGears)
        {
            _currentGear++;
        }
    }

    public void OnMotorPullingChange(bool state)
    {
        _isPull = state;
        if (_isPull)
            _force = _startForce; 
        else
        {
            _force = 0;
            _currentGear = 1;  
        }

        ManageGears();
    }

    public void StopMotor()
    {
        _force = 0;
        _currentGear = 1;  
    }
}
