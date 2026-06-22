using UnityEngine;

public class DirectWaypointController : SteeringControllerBase
{
    private readonly LineFactory _lineFactory;

    public DirectWaypointController(LineFactory lineFactory)
    {
        _lineFactory = lineFactory;
    }

    public override float GetAngleToTurn(Transform carTransform, NavigationContext context)
    {
        _lineFactory.ClearLines();

        Vector3 direction = (context.TargetPoint - carTransform.position).normalized;
        _lineFactory.CreateLine(carTransform.position, context.TargetPoint, Color.blue);

        return Vector3.SignedAngle(carTransform.forward, direction, Vector3.up);
    }
}
