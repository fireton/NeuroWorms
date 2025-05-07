using System;

namespace NeuroWorms.Core.Neuro;

internal class HungerSensor() : BasicNeuron(NeuroConstants.HungerSensorId), IWormResettable
{
    private double hungerLevel;

    public void Reset(Worm worm)
    {
        hungerLevel = Math.Clamp((double)worm.Hunger / (double)Constants.MaxHunger * 2.0 - 1.0, -1.0, 1.0);
        Reset();
    }

    protected override double Activate()
    {
        return hungerLevel;
    }
}
