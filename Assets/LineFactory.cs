using System.Collections.Generic;
using UnityEngine;

public class LineFactory : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab; 
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();

    private List<Vector3> _points;
    private void Start()
    {
        _points = new List<Vector3>();

        foreach (var a in FrameworkStorage.GlobalData.MeshGen.navPoints)
        {
            _points.Add(a.position+Vector3.up * 0.1f);
        }

    

    //    GameObject lineObject = Instantiate(linePrefab);
    //      LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

        //        lineRenderer.positionCount = _points.Count;

        //       lineRenderer.SetPositions(_points.ToArray());

        CreateCurvedLine(_points, 600);
        CreateCurvedLineWithBorders(_points, 600);

        //        lineRenderer.material.color = Color.red;

        //  _lineRenderers.Add(lineRenderer);
    }
    public void CreateCurvedLineWithBorders(List<Vector3> points, int segments = 40)
    {
        CreateCurvedLine(points, segments); // Рисуем основную кривую

        // Создаем бортики
        for (int i = 0; i < 2; i++)
        {
            CreateBorders(points, segments, i == 0 ? 4 : -4);
        }
    }

    private void CreateBorders(List<Vector3> points, int segments, float offset)
    {
        GameObject lineObject = Instantiate(linePrefab);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments; // Нормализованный параметр
            Vector3 point = GetCatmullRomPosition(t, points); // Точка на курве
            Vector3 tangent = GetCatmullRomTangent(t, points).normalized; // Получаем тангенциальный вектор

            // Смещаем точку вдоль тангенциального вектора
            Vector3 borderPoint = point + new Vector3(-tangent.z, 0, tangent.x) * offset; // Учитываем смещение перпендикулярно тангенциальному вектору

            lineRenderer.SetPosition(i, borderPoint);
        }

        _lineRenderers.Add(lineRenderer);
    }

    private Vector3 GetCatmullRomTangent(float t, List<Vector3> points)
    {
        // Используется для получения тангенциального вектора
        int numSections = points.Count - 1;
        int p1 = Mathf.Clamp(Mathf.FloorToInt(t * numSections), 0, numSections);
        int p0 = Mathf.Clamp(p1 - 1, 0, numSections);
        int p2 = Mathf.Clamp(p1 + 1, 0, numSections);

        float tLocal = (t * numSections) - p1; // Вычисляем t для интерполяции в пределах сегмента

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



    public void CreateCurvedLine(List<Vector3> points, int segments = 40)
    {
        GameObject lineObject = Instantiate(linePrefab);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;

        for (int i = 0; i < segments + 1; i++)
        {
            float t = i / (float)segments; // Нормализованный параметр
            Vector3 point = GetCatmullRomPosition(t, points);
            lineRenderer.SetPosition(i, point);
        }

        _lineRenderers.Add(lineRenderer);
    }

    private Vector3 GetCatmullRomPosition(float t, List<Vector3> points)
    {
        // Определяем индекс точки, вокруг которой будем делать интерполяцию
        int numSections = points.Count - 1;
        int p0, p1, p2, p3;

        if (numSections == 0)
        {
            // Не хватает точек для построения сплайна
            return points[0];
        }

        // Делим t по сегментам
        float tScaled = t * numSections;
        int segmentIndex = Mathf.FloorToInt(tScaled);
        t = tScaled - segmentIndex; // Вычисляем t для интерполяции в пределах сегмента

        // Берем соответствующие контрольные точки
        p0 = Mathf.Clamp(segmentIndex - 1, 0, numSections); // Предыдущая точка (или первая, если на краю)
        p1 = Mathf.Clamp(segmentIndex, 0, numSections);     // Текущая точка
        p2 = Mathf.Clamp(segmentIndex + 1, 0, numSections); // Следующая точка
        p3 = Mathf.Clamp(segmentIndex + 2, 0, numSections); // Следующая после следующей (возможно, вылезет за пределы)

        // Используем формулу Catmull-Rom
        Vector3 position = 0.5f * (
            (2f * points[p1]) +
            (-points[p0] + points[p2]) * t +
            (2f * points[p0] - 5f * points[p1] + 4f * points[p2] - points[p3]) * t * t +
            (-points[p0] + 3f * points[p1] - 3f * points[p2] + points[p3]) * t * t * t
        );

        return position;
    }





    void DrawArc(float radius, bool leftTurn)
    {
        List<Vector3> points = new List<Vector3>();

        Vector3 center =
            transform.position +
            transform.right * (leftTurn ? -radius : radius);

        float startAngle =
            Mathf.Atan2(
                transform.position.z - center.z,
                transform.position.x - center.x
            );

        Vector3 previous = transform.position;

        for (int i = 1; i <= 10; i++)
        {
            float angleStep = Mathf.PI / 60;

            float angle =
                startAngle +
                (leftTurn ? angleStep * i : -angleStep * i);

            Vector3 next = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                transform.position.y,
                center.z + Mathf.Sin(angle) * radius
            );
            points.Add(previous);
            points.Add(next);

            previous = next;
        }
        
    }

    public void ClearLines()
    {
        foreach (LineRenderer lineRenderer in _lineRenderers)
        {
      //      Destroy(lineRenderer.gameObject);
        }

     //   _lineRenderers.Clear();
    }
}

