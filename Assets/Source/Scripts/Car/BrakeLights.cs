using UnityEngine;

public class BrakeLights : MonoBehaviour
{
    [SerializeField] private float _intensity = 0.4f;
    private Light[] _lights;

    private void Awake()
    {
        _lights = gameObject.GetComponentsInChildren<Light>();
        TurnOff();
    }

    public void TurnLight(bool isTurnOn)
    {
        foreach (Light light in _lights)
            light.intensity = isTurnOn? _intensity : 0;
    }

    public void TurnOn()
    {
        foreach (Light light in _lights)
            light.intensity = _intensity;
    }

    public void TurnOff()
    {
        foreach (Light light in _lights)
            light.intensity = 0;
    }
}
