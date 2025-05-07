using System;
using NeuroWorms.Core.Helpers;

namespace NeuroWorms.Core.Neuro;

public class DirectionYSensor() : BasicNeuron(NeuroConstants.DirYNeuronId), IWormResettable
{
    private double value = 0.0;

    public void Reset(Worm worm)
    {
        var angle = MathHelper.ToRadians(worm.CurrentDirection.Angle());
        value = Math.Sin(angle);
    }

    protected override double Activate()
    {
        return value;
    }
}
