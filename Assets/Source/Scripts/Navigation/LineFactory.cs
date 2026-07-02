using System.Collections.Generic;
using UnityEngine;

public class LineFactory : MonoBehaviour
{
    [SerializeField] private Material _staticMaterial;
    [SerializeField] private Material _mainMaterial;

    [SerializeField] private GameObject linePrefab; 
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();
    private List<LineRenderer> _staticLineRenderers = new List<LineRenderer>();

    private List<Vector3> _meshGenPoints;

    private void Start()
    {
        _meshGenPoints = new List<Vector3>();

        foreach (var a in FrameworkStorage.GlobalData.MeshGen.navPoints)
        {
            _meshGenPoints.Add(a.position+Vector3.up * 0.1f);
        }
        //CreateCurvedLineWithBorder(_meshGenPoints,4f, 900, true);
    }

    public void CreateCurvedLineWithBorder(List<Vector3> controlPoints, float borderDistance, int segments = 40, bool isStatic = false)
    {
        List<Vector3> curvePoints = new();

        int totalPoints = segments * (controlPoints.Count - 1) + 1;

        for (int j = 0; j < controlPoints.Count - 1; j++)
        {
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments;

                Vector3 point = GetCatmullRomPosition(
                    (j + t) / (controlPoints.Count - 1),
                    controlPoints);

                curvePoints.Add(point);
            }
        }

        curvePoints.Add(controlPoints[^1]);

        LineRenderer borderRenderer1 = CreateParallelLine(curvePoints, borderDistance);
        LineRenderer borderRenderer2 = CreateParallelLine(curvePoints, -borderDistance);


        if (isStatic)
        {
            _staticLineRenderers.Add(borderRenderer1);
            _staticLineRenderers.Add(borderRenderer2);
        }
        else
        {
            _lineRenderers.Add(borderRenderer1);
            _lineRenderers.Add(borderRenderer2);
        }
    }

    private LineRenderer CreateParallelLine(List<Vector3> curvePoints, float borderDistance)
    {
        List<Vector3> borderPoints = new();

        for (int i = 1500; i < curvePoints.Count; i++)
        {
            Vector3 tangent;

            if (i == 0)
            {
                tangent = curvePoints[i + 1] - curvePoints[i];
            }
            else if (i == curvePoints.Count - 1)
            {
                tangent = curvePoints[i] - curvePoints[i - 1];
            }
            else
            {
                tangent = curvePoints[i + 1] - curvePoints[i - 1];
            }

            tangent.Normalize();

            Vector3 normal = Vector3.Cross(Vector3.up, tangent).normalized;

            borderPoints.Add(
                curvePoints[i] + normal * borderDistance);
        }

        GameObject borderObject = Instantiate(linePrefab);

        LineRenderer borderRenderer =
            borderObject.GetComponent<LineRenderer>();

        borderRenderer.positionCount = borderPoints.Count;
        borderRenderer.SetPositions(borderPoints.ToArray());

        return borderRenderer;
    }

    private Vector3 GetCatmullRomTangent(float t, List<Vector3> points)
    {
        int numSections = points.Count - 1;
        int p1 = Mathf.Clamp(Mathf.FloorToInt(t * numSections), 0, numSections);
        int p0 = Mathf.Clamp(p1 - 1, 0, numSections);
        int p2 = Mathf.Clamp(p1 + 1, 0, numSections);

        float tLocal = (t * numSections) - p1; 

        Vector3 tangent = 0.5f * (
            (points[p2] - points[p0]) +
            (2 * points[p1] - 2 * points[p0]) * tLocal +
            (points[p0] - 3 * points[p1] + 3 * points[p2]) * tLocal * tLocal
        );

        return tangent.normalized;
    }

    public void CreateLine(Vector3 start, Vector3 end, Color color, float extensionDistance = 1)
    {
        Vector3 direction = (end - start).normalized;

        Vector3 extendedStart = start - direction * extensionDistance;
        Vector3 extendedEnd = end + direction * extensionDistance; 

        GameObject lineObject = Instantiate(linePrefab);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2; 
        if (extensionDistance == 1)
        {
            extendedStart = start;
            extendedEnd = end;
        }
        lineRenderer.SetPosition(0, extendedStart);
        lineRenderer.SetPosition(1, extendedEnd);
        lineRenderer.material.color = color;

        _lineRenderers.Add(lineRenderer);
    }

    private Vector3 GetCatmullRomPosition(float t, List<Vector3> points)
    {
        int count = points.Count;

        if (count == 0)
            return Vector3.zero;

        if (count == 1)
            return points[0];

        if (t <= 0f)
            return points[0];

        if (t >= 1f)
            return points[count - 1];

        int numSections = count - 1;

        float scaledT = t * numSections;
        int segment = Mathf.FloorToInt(scaledT);

        float localT = scaledT - segment;

        Vector3 p1 = points[segment];
        Vector3 p2 = points[Mathf.Min(segment + 1, count - 1)];

        Vector3 p0;
        if (segment > 0)
            p0 = points[segment - 1];
        else
            p0 = p1 + (p1 - p2);

        Vector3 p3;
        if (segment < count - 2)
            p3 = points[segment + 2];
        else
            p3 = p2 + (p2 - p1);

        float tt = localT * localT;
        float ttt = tt * localT;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * localT +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * tt +
            (-p0 + 3f * p1 - 3f * p2 + p3) * ttt
        );
    }


    public void DrawArc(Transform transformCar, float radius, bool leftTurn)
    {
        List<Vector3> points = new List<Vector3>();

        Vector3 center =
            transformCar.position +
            transformCar.right * (leftTurn ? -radius : radius);

        float startAngle =
            Mathf.Atan2(
                transformCar.position.z - center.z,
                transformCar.position.x - center.x
            );

        Vector3 previous = transformCar.position;

        for (int i = 1; i <= 10; i++)
        {
            float angleStep = Mathf.PI / 60;

            float angle =
                startAngle +
                (leftTurn ? angleStep * i : -angleStep * i);

            Vector3 next = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                transformCar.position.y,
                center.z + Mathf.Sin(angle) * radius
            );
            points.Add(previous);
            points.Add(next);

            previous = next;
        }
        GameObject borderObject = Instantiate(linePrefab);

        LineRenderer borderRenderer =
            borderObject.GetComponent<LineRenderer>();

        borderRenderer.material = _mainMaterial;

        borderRenderer.positionCount = points.Count;
        borderRenderer.SetPositions(points.ToArray());
        _lineRenderers.Add(borderRenderer);

    }

    public void ClearLines()
    {
        foreach (LineRenderer lineRenderer in _lineRenderers)
        {
            Destroy(lineRenderer.gameObject);
        }

        _lineRenderers.Clear();
    }
}

