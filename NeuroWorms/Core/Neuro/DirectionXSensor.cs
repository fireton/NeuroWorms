using System;
using NeuroWorms.Core.Helpers;

namespace NeuroWorms.Core.Neuro;

public class DirectionXSensor() : BasicNeuron(NeuroConstants.DirXNeuronId), IWormResettable
{
    private double value = 0.0;

    public void Reset(Worm worm)
    {
        value = Math.Cos(worm.CurrentDirection.AngleRad());
    }

    protected override double Activate()
    {
        return value;
    }
}
