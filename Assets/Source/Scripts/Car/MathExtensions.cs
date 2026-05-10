using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public static class MathExtensions
{
    public static float LerpByStep(this float value, float target, float step)
    {
        float targetOffcet = target - value;
        float offcet = Mathf.Min(targetOffcet, step);
        return value + offcet;
    }

    public static float Round(this float value, float step)
    {
        bool isNegative = value < 0;

        if (isNegative)
            value = -value;

        float remains = value % step;

        float result = remains >= step / 2f ? value + (step - remains) : value - remains;

        return isNegative ? -result : result;
    }

    public static float Normalize(this float value, float treshold = 0.01f)
    {
        if (value >= treshold)
            return 1f;
        else if (value <= -treshold)
            return -1f;
        else return 0f;
    }

    public static int Round(this int value, int step)
    {
        bool isNegative = value < 0;

        if (isNegative)
            value = -value; 

        int remains = value % step;

        int result = remains >= step / 2f ? value + (step - remains) : value - remains;

        return isNegative ? -result : result;
    }

    public static int ToInt(this float value) => Convert.ToInt32(value);
    public static float Abs(this float value) => Mathf.Abs(value);

    public static string GetNumberDispayName(this float value)
    {
        if (value < 50)
        {
            return value.ToString("F1");
        }

        if (value < 1000)
        {
            return value.ToString("F0");
        }

        if (value < 100000000)
        {
            return (value / 1000f).ToString("F1") + "K";
        }

        return value.ToString("F0");
    }
}
