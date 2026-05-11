using UnityEngine;

public class Wheel : MonoBehaviour
{
    [SerializeField] private Transform _skin;
    [SerializeField] private WheelCollider _wheelCollider;
    public Transform Skin => _skin;
    public WheelCollider WheelCollider => _wheelCollider;
}
