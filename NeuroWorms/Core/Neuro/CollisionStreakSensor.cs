namespace NeuroWorms.Core.Neuro;

internal sealed class CollisionStreakSensor()
    : BasicNeuron(NeuroConstants.CollisionStreakSensorId), IWormResettable
{
    private double value;

    public void Reset(Worm worm)
    {
        value = worm.ConsecutiveCollisions switch
        {
            <= 0 => -1.0,
            1 => 0.0,
            _ => 1.0,
        };
        base.Reset();
    }

    protected override double Activate()
    {
        return value;
    }
}
