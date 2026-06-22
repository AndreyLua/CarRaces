using System.Collections.Generic;

public class VehicleTelemetry
{
    private readonly List<double> _times  = new();
    private readonly List<double> _speeds = new();

    public void Record(float time, float speed)
    {
        _times.Add(time);
        _speeds.Add(speed);
    }

    public double CalculateRmsAcceleration() =>
        MotionMetrics.CalculateRmsAcceleration(_times, _speeds);

    public void Clear()
    {
        _times.Clear();
        _speeds.Clear();
    }
}
