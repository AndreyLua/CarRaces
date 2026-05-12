using UnityEngine;

public class BrakeLights : MonoBehaviour
{
    [SerializeField] private Light[] _lights;

    private void Awake()
    {
        _lights = gameObject.GetComponentsInChildren<Light>();
        TurnOff();
    }

    public void TurnLight(bool isTurnOn)
    {
        foreach (Light light in _lights)
            light.intensity = isTurnOn? 1:0;
    }

    public void TurnOn()
    {
        foreach (Light light in _lights)
            light.intensity = 1;
    }

    public void TurnOff()
    {
        foreach (Light light in _lights)
            light.intensity = 0;
    }
}
