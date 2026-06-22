using UnityEngine;

public class StanleyController : SteeringControllerBase
{
    private readonly LineFactory _lineFactory;

    private const float HeadingGain    = 0.01f;
    private const float CrossTrackGain = 1f;
    private const float SpeedFactor    = 6.7f;
    private const float MinSpeed       = 0.1f;

    public StanleyController(LineFactory lineFactory)
    {
        _lineFactory = lineFactory;
    }

    public override float GetAngleToTurn(Transform carTransform, NavigationContext context)
    {
        _lineFactory.ClearLines();

        Vector3 currentPoint  = context.TargetPoint;
        Vector3 nextPoint     = context.NextTargetPoint;
        Vector3 closestPoint  = PathGeometry.FindPerpendicularPointOnLine(currentPoint, nextPoint, carTransform.position);

        float headingError    = CalculateHeadingError(carTransform, currentPoint);
        float crossTrackError = CalculateCrossTrackError(carTransform.position, closestPoint, currentPoint, nextPoint);

        DrawDebug(carTransform.position, closestPoint, currentPoint, nextPoint);

        return CalculateSteering(headingError, crossTrackError, context.CurrentSpeed);
    }

    private float CalculateHeadingError(Transform carTransform, Vector3 targetPoint)
    {
        Vector3 desiredDirection = (targetPoint - carTransform.position).normalized;
        return Vector3.SignedAngle(carTransform.forward, desiredDirection, Vector3.up) * Mathf.Rad2Deg;
    }

    private float CalculateCrossTrackError(
        Vector3 carPosition, Vector3 closestPoint, Vector3 segmentStart, Vector3 segmentEnd)
    {
        float error   = Vector3.Distance(carPosition, closestPoint);
        bool  isLeft  = PathGeometry.DeterminePointPositionRelativeToLine(segmentStart, segmentEnd, carPosition) > 0;
        return isLeft ? -error : error;
    }

    private float CalculateSteering(float headingError, float crossTrackError, float speed)
    {
        return headingError * HeadingGain
            + Mathf.Atan((CrossTrackGain * crossTrackError) / Mathf.Max(speed * SpeedFactor, MinSpeed))
            * Mathf.Rad2Deg;
    }

    private void DrawDebug(Vector3 carPosition, Vector3 closestPoint, Vector3 currentPoint, Vector3 nextPoint)
    {
        _lineFactory.CreateLine(carPosition, closestPoint + Vector3.up * 0.1f, Color.yellow);
        _lineFactory.CreateLine(currentPoint + Vector3.up * 0.1f, nextPoint + Vector3.up * 0.1f, Color.red, 100);
    }
}
