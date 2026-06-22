using UnityEngine;

public class PurePursuitController : SteeringControllerBase
{
    private readonly LineFactory _lineFactory;
    private const float CurvatureK = 20f;

    public PurePursuitController(LineFactory lineFactory)
    {
        _lineFactory = lineFactory;
    }

    public override float GetAngleToTurn(Transform carTransform, NavigationContext context)
    {
        _lineFactory.ClearLines();

        Vector3 localTarget = carTransform.InverseTransformPoint(context.TargetPoint);
        float L         = localTarget.magnitude;
        float curvature = (CurvatureK * localTarget.x) / (L * L);
        float radius    = Mathf.Abs(1f / curvature);

        _lineFactory.DrawArc(carTransform, radius, curvature < 0);

        return curvature * Mathf.Rad2Deg;
    }
}
