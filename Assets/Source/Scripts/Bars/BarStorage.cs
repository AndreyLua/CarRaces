using System;

public class BarStorage
{
    private float _value;
    private float _maxValue;

    public float Value => _value;
    public float MaxValue => _maxValue;

    public event Action ChangeValue;
    public event Action StorageEmpty;
    public event Action<float> StorageOverfilled;

    public BarStorage(float value, float maxValue)
    {
        _maxValue = maxValue;
        _value = value;
    }

    public virtual void SetMaxValue(float maxValue)
    {
        if (maxValue <= 0)
            throw new Exception("Storage max value cant be negative");
        _maxValue = maxValue;
    }

    public void Add(float value)
    {
        if (value < 0)
            throw new Exception("Value cant be negative");

        _value += value;

        if (_value > _maxValue)
        {
            StorageOverfilled?.Invoke(_maxValue - _value);
            OnStorageOverfilled();
        }

        ChangeValue?.Invoke();
    }

    public void Clear()
    {
        _value = 0;
    }

    protected virtual void OnStorageOverfilled()
    {
        _value = _maxValue;
    }

    public void Take(float value)
    {
        if (value < 0)
            throw new Exception("Take value cant be negative");

        _value -= value;

        if (_value <= 0)
        {
            _value = 0;
            StorageEmpty?.Invoke();
        }

        ChangeValue?.Invoke();
    }
}