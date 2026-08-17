using System.Collections.Generic;

namespace NeuroWorms.Core.Neuro;

internal sealed class BrainGenome
{
    public List<double> Biases { get; init; } = [];
    public List<double> Weights { get; init; } = [];
}
