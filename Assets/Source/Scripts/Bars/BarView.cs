using UnityEngine;
using DG.Tweening;

public class BarView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _bar;
    [SerializeField] private SpriteRenderer _dynamicBar;

    private float _updateValueDuration = 0.2f;
    private float _updateValueDynamicRatio =4;

    private BarStorage _storage;

    public void Init(BarStorage storage)
    {
        _storage = storage;
        storage.ChangeValue += UpdateBar;
    }

    public virtual void UpdateBar()
    {
        float storageRatio = _storage.Value / _storage.MaxValue;
        _bar.transform.DOScaleX(_storage.Value / _storage.MaxValue, _updateValueDuration);
        _dynamicBar.transform.DOScaleX(_storage.Value / _storage.MaxValue, _updateValueDuration* _updateValueDynamicRatio);
    }
}
