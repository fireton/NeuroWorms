namespace NeuroWorms.Core.Neuro;

internal sealed class ObstacleAheadSensor()
    : BasicNeuron(NeuroConstants.ObstacleAheadSensorId), IWormFieldResettable
{
    private double value;

    public void Reset(Worm worm, Field field)
    {
        var cellTypeAhead = field[worm.Head.Move(worm.CurrentDirection)];
        value = cellTypeAhead switch
        {
            CellType.Wall or CellType.WormBody or CellType.WormHead => 1.0,
            CellType.Food => -1.0,
            _ => 0.0,
        };
        base.Reset();
    }

    protected override double Activate()
    {
        return value;
    }
}
