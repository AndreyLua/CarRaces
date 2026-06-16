using System.Collections.Generic;
using UnityEngine;

public class RouteNavigator
{
    private readonly List<Vector3> _points;

    public int CurrentTargetIndex { get; set; }

    public Vector3 CurrentTarget => _points[CurrentTargetIndex];

    public RouteNavigator(List<Vector3> points)
    {
        _points = points;
    }

    public RouteNavigator(List<Vector3> points, Vector3 startPosition, Vector3 forward)
    {
        _points = points;
        SetCurrentTargetIndex(startPosition, forward);
    }

    private void SetCurrentTargetIndex(Vector3 startPosition, Vector3 forward)
    {
        CurrentTargetIndex = FindClosestPointAhead(startPosition, forward);
    }

    public void UpdateTarget(Vector3 currentPosition, float switchDistance)
    {
        if (Vector3.Distance(CurrentTarget, currentPosition) < switchDistance)
        {
            CurrentTargetIndex = GetNextPoint(CurrentTargetIndex);
        }
    }

    public int GetNextPoint(int indexPoint)
    {
        if (indexPoint - 1 < 0)
        {
            return _points.Count - 1;
        }

        return indexPoint - 1;
    }

    public int GetPreviousPoint(int indexPoint)
    {
        if (indexPoint + 1 >= _points.Count)
        {
            return 0;
        }

        return indexPoint + 1;
    }

    private int FindClosestPointAhead(Vector3 carPosition, Vector3 carDirection)
    {
        int closestPointIndex = -1;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < _points.Count; i++)
        {
            Vector3 directionToPoint = _points[i] - carPosition;

            float forwardDot =
                Vector3.Dot(
                    carDirection,
                    directionToPoint.normalized);

            if (forwardDot > 0)
            {
                float distance =
                    Vector3.Distance(
                        carPosition,
                        _points[i]);

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPointIndex = i;
                }
            }
        }

        return closestPointIndex;
    }
}
