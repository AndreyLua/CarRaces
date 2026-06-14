using System;
using System.Collections.Generic;

public static class MotionMetrics
{
    public static double CalculateRmsAcceleration(
        IReadOnlyList<double> times,
        IReadOnlyList<double> speeds)
    {
        if (times == null || speeds == null)
            throw new ArgumentNullException();

        if (times.Count != speeds.Count)
            throw new ArgumentException("times и speeds должны иметь одинаковую длину");

        if (times.Count < 2)
            return 0.0;

        double sumSquares = 0.0;
        int accelerationCount = 0;

        for (int i = 1; i < times.Count; i++)
        {
            double dt = times[i] - times[i - 1];

            if (dt <= 0)
                throw new ArgumentException("Время должно строго возрастать");

            double dv = speeds[i] - speeds[i - 1];
            double acceleration = dv / dt;

            sumSquares += acceleration * acceleration;
            accelerationCount++;
        }

        return Math.Sqrt(sumSquares / accelerationCount);
    }
}
