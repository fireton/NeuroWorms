using System.Collections.Generic;

namespace NeuroWorms.Core.Evolution;

internal abstract class GenerationMutator
{
    public abstract IReadOnlyList<WormBrain> CreateNextGeneration(IReadOnlyList<Worm> rankedPopulation);
}
