using NeuroWorms.Core.Neuro;
using System;
using System.Collections.Generic;

namespace NeuroWorms.Core;

internal sealed class SimulationCheckpoint
{
    public int Version { get; init; }
    public int Generation { get; init; }
    public DateTimeOffset SavedAtUtc { get; init; }
    public List<BrainGenome> Population { get; init; } = [];
}
