using System.Collections.Generic;
using UnityEngine;

public class NavigationContext
{
    public Vector3 TargetPoint;
    public Vector3 NextTargetPoint;

    public int TargetIndex;
    public int NextTargetIndex;

    public List<Vector3> Route;

    public float CurrentSpeed;
}