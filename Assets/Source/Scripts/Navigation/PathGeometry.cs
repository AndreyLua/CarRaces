using System.Collections.Generic;
using UnityEngine;

public static class PathGeometry
{
    public static Vector3 FindClosestPoint(
        Vector3 point,
        List<Vector3> points)
    {
        Vector3 closestPoint = Vector3.zero;
        float closestDistanceSqr = Mathf.Infinity;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 segmentStart = points[i];
            Vector3 segmentEnd = points[i + 1];

            Vector3 nearestPoint =
                GetNearestPointOnSegment(
                    point,
                    segmentStart,
                    segmentEnd);

            float distanceSqr =
                (nearestPoint - point).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestPoint = nearestPoint;
            }
        }

        return closestPoint;
    }

    public static Vector3 GetNearestPointOnSegment(
        Vector3 point,
        Vector3 segmentStart,
        Vector3 segmentEnd)
    {
        Vector3 segment = segmentEnd - segmentStart;
        float segmentLengthSqr = segment.sqrMagnitude;

        if (segmentLengthSqr == 0f)
        {
            return segmentStart;
        }

        float t =
            Vector3.Dot(
                point - segmentStart,
                segment)
            / segmentLengthSqr;

        t = Mathf.Clamp01(t);

        return segmentStart + t * segment;
    }

    public static Vector3 FindPerpendicularPointOnLine(
        Vector3 start,
        Vector3 end,
        Vector3 point)
    {
        Vector3 lineDir = end - start;
        Vector3 pointDir = point - start;

        float t =
            Vector3.Dot(pointDir, lineDir)
            / Vector3.Dot(lineDir, lineDir);

        return start + t * lineDir;
    }

    public static float DeterminePointPositionRelativeToLine(
        Vector3 start,
        Vector3 end,
        Vector3 point)
    {
        Vector3 lineDir = end - start;
        Vector3 pointDir = point - start;

        Vector3 crossProduct =
            Vector3.Cross(lineDir, pointDir);

        return crossProduct.y;
    }
}
