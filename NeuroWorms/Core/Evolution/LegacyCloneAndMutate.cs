using NeuroWorms.Core.Neuro;

namespace NeuroWorms.Core.Evolution;

internal sealed class LegacyCloneAndMutate : CloneAndMutateGenerationMutator
{
    public LegacyCloneAndMutate()
        : base(
            parentCount: 4,
            childrenPerParent: 10,
            newBloodCount: 10,
            mutationChance: 0.5,
            new MutationSettings(
                strength: 0.15,
                percentOfNeurons: 25))
    {
    }
}
