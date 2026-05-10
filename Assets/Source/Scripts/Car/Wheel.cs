using UnityEngine;

public class Wheel : MonoBehaviour
{
    [SerializeField] private Transform _skin;
    [SerializeField] private float _friction = 1;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private WheelCollider _wheelCollider;
    public Transform Skin => _skin;
    public float Radius => _radius;
    public float Friction => _friction;
    public WheelCollider WheelCollider => _wheelCollider;
}
