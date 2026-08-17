using NeuroWorms.Core.Neuro;

namespace NeuroWorms.Core.Evolution;

internal sealed class FineTuningCloneAndMutate : CloneAndMutateGenerationMutator
{
    public FineTuningCloneAndMutate()
        : base(
            parentCount: 6,
            childrenPerParent: 8,
            newBloodCount: 2,
            mutationChance: 0.5,
            new MutationSettings(
                strength: 0.075,
                percentOfNeurons: 15))
    {
    }
}
