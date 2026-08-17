using System;

namespace NeuroWorms.Core.Neuro;

internal sealed record MutationSettings
{
    public double Strength { get; }
    public int PercentOfNeurons { get; }

    public MutationSettings(double strength, int percentOfNeurons)
    {
        if (strength <= 0.0 || strength > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(strength), "Mutation strength must be in (0, 1].");
        }

        if (percentOfNeurons <= 0 || percentOfNeurons > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(percentOfNeurons),
                "The percentage of neurons to mutate must be in [1, 100].");
        }

        Strength = strength;
        PercentOfNeurons = percentOfNeurons;
    }

    public static MutationSettings Default { get; } = new(
        NeuroConstants.MutationStrength,
        NeuroConstants.PercentOfNeuronsToMutate);
}
