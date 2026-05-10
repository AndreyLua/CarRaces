using UnityEngine;

public static class VectorExtensions
{
    public static Vector2 Round(this Vector2 vector, float step)
    {
        return new Vector2(vector.x.Round(step), vector.y.Round(step));
    }

    public static Vector3 DirectionTo(this Vector3 pos1, Vector3 pos2)
    {
        return (pos2 - pos1).normalized;
    }

    public static Vector3 Round(this Vector3 vector, float step)
    {
        return new Vector3(vector.x.Round(step), vector.y.Round(step), vector.z.Round(step));
    }

    public static Vector2Int Round(this Vector2Int vector, int step)
    {
        return new Vector2Int(vector.x.Round(step), vector.y.Round(step));
    }

    public static Vector3Int Round(this Vector3Int vector, int step)
    {
        return new Vector3Int(vector.x.Round(step), vector.y.Round(step), vector.z.Round(step));
    }

    public static Vector3Int ToInt(this Vector3 vector)
    {
        return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
    }

    public static Vector2Int ToInt(this Vector2 vector)
    {
        return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
    }

    public static Vector3 Divide(this Vector3 vector1, Vector3 vector2)
    {
        return new Vector3(vector1.x / vector2.x, vector1.y / vector2.y, vector1.z / vector2.z);
    }

    public static Vector3 LerpByStep(this Vector3 vector, Vector3 target, float step)
    {
        Vector3 targetOffcet = target - vector;
        Vector3 offcet = Vector3.ClampMagnitude(targetOffcet, step);
        return vector + offcet;
    }

    public static Vector3 XY(this Vector3 vector) => new Vector3(vector.x, vector.y, 0f);
    public static Vector3 XZ(this Vector3 vector) => new Vector3(vector.x, 0f, vector.z);
    public static Vector3 YZ(this Vector3 vector) => new Vector3(0f, vector.y, vector.z);
    public static Vector3 X(this Vector3 vector) => new Vector3(vector.x, 0f, 0f);
    public static Vector3 Y(this Vector3 vector) => new Vector3(0f, vector.y, 0f);
    public static Vector3 Z(this Vector3 vector) => new Vector3(0f, 0f, vector.z);

    public static Vector2 X(this Vector2 vector) => new Vector2(vector.x, 0f);
    public static Vector2 Y(this Vector2 vector) => new Vector2(0f, vector.y);

    public static Vector3 SetX(this Vector3 vector, float x) => new Vector3(x, vector.y, vector.z);
    public static Vector3 SetY(this Vector3 vector, float y) => new Vector3(vector.x, y, vector.z);
    public static Vector3 SetZ(this Vector3 vector, float z) => new Vector3(vector.x, vector.y, z);

    public static Vector3 Abs(this Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }

    public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
    {
        return new Vector3(Mathf.Clamp(vector.x, min.x, max.x), Mathf.Clamp(vector.y, min.y, max.y), Mathf.Clamp(vector.z, min.z, max.z));
    }

    public static Vector3 Clamp(this Vector3 vector, float min, float max)
    {
        return new Vector3(Mathf.Clamp(vector.x, min, max), Mathf.Clamp(vector.y, min, max), Mathf.Clamp(vector.z, min, max));
    }

    public static Vector2 Clamp(this Vector2 vector, float min, float max)
    {
        return new Vector3(Mathf.Clamp(vector.x, min, max), Mathf.Clamp(vector.y, min, max));
    }

    public static Rect SetX(this Rect rect, float x) => new Rect(x, rect.y, rect.width, rect.height);
    public static Rect SetY(this Rect rect, float y) => new Rect(rect.x, y, rect.width, rect.height);
    public static Rect SetWidth(this Rect rect, float width) => new Rect(rect.x, rect.y, width, rect.height);

    public static float HorizontalDistace(this Vector3 vector1, Vector3 vector2)
    {
        return Vector2.Distance(new Vector2(vector1.x, vector1.z), new Vector2(vector2.x, vector2.z));
    }

    public static bool NextHorizontalVectorLess(this Vector3 vector1, Vector3 next)
    {
        float xDif = vector1.x - next.x;
        float zDif = vector1.z - next.z;

        if (xDif >= 0f && zDif >= 0f) 
            return true;

        if (xDif < 0f && zDif < 0f)
            return false;

        if (xDif < 0f)
            return zDif.Abs() > xDif.Abs();

        return xDif.Abs() > zDif.Abs();
    }
}
