using UnityEngine;

[CreateAssetMenu(menuName = "CarConfig/EngineConfig")]
public class EngineConfig : ScriptableObject
{
    public float StartForce;
    public float ForceAcceleration;
}



[CreateAssetMenu(menuName = "CarConfig/TransmissionConfig")]
public class TransmissionConfig : ScriptableObject
{
    public float SuspensionWheelDistance = 0;
    public float MaxEversionAngle = 30;
    public float WeightWheel = 1;
    public float BrakeTorque = 100;
    public WheelDriveType AllWheelDrive = WheelDriveType.Front;
}

public enum WheelDriveType
{
    Front,
    Rear,
    All
}


