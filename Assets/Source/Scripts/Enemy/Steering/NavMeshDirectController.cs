using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshDirectController : SteeringControllerBase
{
    private readonly LineFactory   _lineFactory;
    private readonly NavMeshAgent  _agent;
    private readonly List<Vector3> _route;
    private readonly int           _finishIndex;

    private Vector3[] _corners = System.Array.Empty<Vector3>();
    private int       _cornerIndex;
    private int       _segment;

    private int[] _segmentTargetIndices;

    private const float SwitchDistance = 8f;

    public NavMeshDirectController(LineFactory lineFactory, NavMeshAgent agent, List<Vector3> route, int finishIndex)
    {
        _lineFactory = lineFactory;
        _agent       = agent;
        _route       = route;
        _finishIndex = finishIndex;
    }

    public override float GetAngleToTurn(Transform carTransform, NavigationContext context)
    {
        _lineFactory.ClearLines();

        if (_segmentTargetIndices == null)
            BuildSegmentTargets(context.TargetIndex);

        if (_corners.Length == 0)
            CalculateNextPath(carTransform.position);

        if (_corners.Length == 0)
            return 0f;

        while (_cornerIndex < _corners.Length - 1 && IsCornerReached(carTransform, _corners[_cornerIndex]))
            _cornerIndex++;

        if (_cornerIndex == _corners.Length - 1 && IsCornerReached(carTransform, _corners[_cornerIndex]))
        {
            _segment++;
            CalculateNextPath(carTransform.position);

            if (_corners.Length == 0)
                return 0f;
        }

        Vector3 target    = _corners[_cornerIndex];
        Vector3 direction = (target - carTransform.position).normalized;

        //DrawDebugPath(carTransform.position, target);

        return Vector3.SignedAngle(carTransform.forward, direction, Vector3.up);
    }

    // Compute 3 waypoint indices by stepping forward along the route
    // so each target is guaranteed to be in the forward direction for NavMesh
    private void BuildSegmentTargets(int startRouteIndex)
    {
        int totalSteps = CountStepsToFinish(startRouteIndex);

        _segmentTargetIndices = new int[3]
        {
            StepForward(startRouteIndex, totalSteps / 3),
            StepForward(startRouteIndex, totalSteps * 2 / 3),
            _finishIndex
        };

        Debug.Log($"NavMeshDirect targets: {_segmentTargetIndices[0]}, {_segmentTargetIndices[1]}, {_segmentTargetIndices[2]} (total steps: {totalSteps})");
    }

    private int CountStepsToFinish(int fromIndex)
    {
        int steps   = 0;
        int current = fromIndex;
        int limit   = _route.Count * 2;

        while (current != _finishIndex && steps < limit)
        {
            current = NextIndex(current);
            steps++;
        }

        return steps;
    }

    private int StepForward(int fromIndex, int steps)
    {
        int current = fromIndex;
        for (int i = 0; i < steps; i++)
            current = NextIndex(current);
        return current;
    }

    private int NextIndex(int index) => index - 1 < 0 ? _route.Count - 1 : index - 1;

    private void CalculateNextPath(Vector3 fromPosition)
    {
        if (_segment >= 3)
        {
            _corners = System.Array.Empty<Vector3>();
            return;
        }

        int targetIndex = _segmentTargetIndices[_segment];

        if (!NavMesh.SamplePosition(fromPosition, out NavMeshHit fromHit, 5f, NavMesh.AllAreas))
        {
            Debug.LogWarning("NavMeshDirect: cannot snap start position");
            _corners = System.Array.Empty<Vector3>();
            return;
        }

        if (!NavMesh.SamplePosition(_route[targetIndex], out NavMeshHit toHit, 5f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"NavMeshDirect: cannot snap target[{targetIndex}]");
            _segment++;
            CalculateNextPath(fromPosition);
            return;
        }

        NavMeshPath path = new NavMeshPath();

        if (NavMesh.CalculatePath(fromHit.position, toHit.position, NavMesh.AllAreas, path) && path.corners.Length > 0)
        {
            _corners     = path.corners;
            _cornerIndex = 0;
            Debug.Log($"NavMeshDirect: segment {_segment} → route[{targetIndex}], {_corners.Length} corners");
        }
        else
        {
            Debug.LogWarning($"NavMeshDirect: CalculatePath failed for segment {_segment}");
            _segment++;
            CalculateNextPath(fromPosition);
        }
    }

    private bool IsCornerReached(Transform car, Vector3 corner)
    {
        Vector3 toCorner = corner - car.position;
        return toCorner.magnitude < SwitchDistance
            || Vector3.Dot(car.forward, toCorner.normalized) < 0f;
    }

    private void DrawDebugPath(Vector3 carPosition, Vector3 target)
    {
        for (int i = 0; i < _corners.Length - 1; i++)
            _lineFactory.CreateLine(
                _corners[i]     + Vector3.up * 0.2f,
                _corners[i + 1] + Vector3.up * 0.2f,
                Color.yellow);

        _lineFactory.CreateLine(carPosition, target, Color.blue);

        Vector3 dest = _corners[^1];
        _lineFactory.CreateLine(dest, dest + Vector3.up * 5f, Color.magenta);
    }
}
