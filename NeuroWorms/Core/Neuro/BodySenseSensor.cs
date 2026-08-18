using System;

namespace NeuroWorms.Core.Neuro;

internal abstract class BodySenseSensor(Guid id, BodySense bodySense)
    : BasicNeuron(id), IWormResettable
{
    protected BodySense BodySense { get; } = bodySense
        ?? throw new ArgumentNullException(nameof(bodySense));

    public void Reset(Worm worm)
    {
        BodySense.Reset(worm);
        base.Reset();
    }
}
