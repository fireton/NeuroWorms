using System;

namespace NeuroWorms.Core.Neuro;

internal sealed class BodySense
{
    internal const int IgnoredLeadingSegments = 3;

    private Worm worm;
    private bool isCalculated;

    public double AvoidanceForward
    {
        get
        {
            Calculate();
            return fieldAvoidanceForward;
        }
    }

    public double AvoidanceRight
    {
        get
        {
            Calculate();
            return fieldAvoidanceRight;
        }
    }

    public double Pressure
    {
        get
        {
            Calculate();
            return fieldPressure;
        }
    }

    private double fieldAvoidanceForward;
    private double fieldAvoidanceRight;
    private double fieldPressure;

    public void Reset(Worm currentWorm)
    {
        worm = currentWorm ?? throw new ArgumentNullException(nameof(currentWorm));
        isCalculated = false;
    }

    private void Calculate()
    {
        if (isCalculated)
        {
            return;
        }

        if (worm is null)
        {
            throw new InvalidOperationException("BodySense must be reset with a worm before use.");
        }

        var avoidanceForward = 0.0;
        var avoidanceRight = 0.0;
        var pressure = 0.0;

        for (var index = IgnoredLeadingSegments; index < worm.Body.Count; index++)
        {
            var segment = worm.Body[index];
            var deltaX = segment.X - worm.Head.X;
            var deltaY = segment.Y - worm.Head.Y;
            var (forward, right) = ToLocalCoordinates(
                deltaX,
                deltaY,
                worm.CurrentDirection);
            var distanceSquared = forward * forward + right * right;
            if (distanceSquared == 0)
            {
                continue;
            }

            var influence = 1.0 / (1.0 + distanceSquared);
            avoidanceForward -= forward * influence;
            avoidanceRight -= right * influence;
            pressure += influence;
        }

        fieldAvoidanceForward = Math.Clamp(avoidanceForward, -1.0, 1.0);
        fieldAvoidanceRight = Math.Clamp(avoidanceRight, -1.0, 1.0);
        fieldPressure = Math.Clamp(pressure, 0.0, 1.0);
        isCalculated = true;
    }

    private static (int Forward, int Right) ToLocalCoordinates(
        int deltaX,
        int deltaY,
        MoveDirection direction)
    {
        return direction switch
        {
            MoveDirection.Right => (deltaX, deltaY),
            MoveDirection.Up => (-deltaY, deltaX),
            MoveDirection.Left => (-deltaX, -deltaY),
            MoveDirection.Down => (deltaY, -deltaX),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
    }
}
