using System;

public class Health
{
    private BarStorage _barStorage;

    public BarStorage BarStorage => _barStorage;
    public bool IsAlive => _barStorage.Value > 0;
    public float Value => _barStorage.Value;

    public event Action Died;
    public event Action Changed;

    public Health(float value, float maxValue)
    {
        _barStorage = new BarStorage(value, maxValue);
        _barStorage.ChangeValue += InvokeChanged;
        _barStorage.StorageEmpty += InvokeDie;
    }

    public void Init(float value)
    {
        _barStorage.SetMaxValue(value);
        _barStorage.Add(value);
    }

    private void InvokeDie()
    {
        Died?.Invoke();
    }

    private void InvokeChanged()
    {
        Changed?.Invoke();
    }

    public void TakeDamage(float value)
    {
        _barStorage.Take(value);
    }

    public void Heal(float value)
    {
        _barStorage.Add(value);
    }

    public void HealFull()
    {
        _barStorage.Add(_barStorage.MaxValue);
    }

    public void Kill()
    {
        _barStorage.Clear();
    }
}
