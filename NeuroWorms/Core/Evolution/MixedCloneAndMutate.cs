using System;
using System.Collections.Generic;
using System.Linq;
using NeuroWorms.Core.Neuro;

namespace NeuroWorms.Core.Evolution;

internal sealed class MixedCloneAndMutate : GenerationMutator
{
    public int ParentCount { get; } = 6;
    public int ExactChildrenPerParent { get; } = 3;
    public int FineTuningChildrenPerParent { get; } = 3;
    public int LegacyChildrenPerParent { get; } = 2;
    public int NewBloodCount { get; } = 2;

    public MutationSettings FineTuningMutation { get; } = new(
        strength: 0.075,
        percentOfNeurons: 15);

    public MutationSettings LegacyMutation { get; } = new(
        strength: 0.15,
        percentOfNeurons: 25);

    public override IReadOnlyList<WormBrain> CreateNextGeneration(IReadOnlyList<Worm> rankedPopulation)
    {
        ArgumentNullException.ThrowIfNull(rankedPopulation);

        if (rankedPopulation.Count < ParentCount)
        {
            throw new ArgumentException(
                $"At least {ParentCount} ranked worms are required.",
                nameof(rankedPopulation));
        }

        var brains = new List<WormBrain>(Constants.StartWormCount);
        foreach (var parent in rankedPopulation.Take(ParentCount))
        {
            AddExactChildren(brains, parent);
            AddMutatedChildren(
                brains,
                parent,
                FineTuningChildrenPerParent,
                FineTuningMutation);
            AddMutatedChildren(
                brains,
                parent,
                LegacyChildrenPerParent,
                LegacyMutation);
        }

        for (var index = 0; index < NewBloodCount; index++)
        {
            var brain = new WormNeuroBrain();
            brain.Init();
            brains.Add(brain);
        }

        if (brains.Count != Constants.StartWormCount)
        {
            throw new InvalidOperationException(
                $"The strategy created {brains.Count} brains; expected {Constants.StartWormCount}.");
        }

        return brains.OrderBy(_ => NeuroRnd.NextDouble()).ToList();
    }

    private void AddExactChildren(List<WormBrain> brains, Worm parent)
    {
        for (var index = 0; index < ExactChildrenPerParent; index++)
        {
            brains.Add(parent.Brain.Clone());
        }
    }

    private static void AddMutatedChildren(
        List<WormBrain> brains,
        Worm parent,
        int count,
        MutationSettings mutationSettings)
    {
        for (var index = 0; index < count; index++)
        {
            var brain = parent.Brain.Clone();
            brain.Mutate(mutationSettings);
            brains.Add(brain);
        }
    }
}
