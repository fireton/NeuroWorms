using System;
using System.Collections.Generic;
using System.Linq;
using NeuroWorms.Core.Neuro;

namespace NeuroWorms.Core.Evolution;

internal class CloneAndMutateGenerationMutator : GenerationMutator
{
    public int ParentCount { get; }
    public int ChildrenPerParent { get; }
    public int NewBloodCount { get; }
    public double MutationChance { get; }
    public MutationSettings MutationSettings { get; }

    public CloneAndMutateGenerationMutator(
        int parentCount,
        int childrenPerParent,
        int newBloodCount,
        double mutationChance,
        MutationSettings mutationSettings)
    {
        if (parentCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentCount));
        }

        if (childrenPerParent <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(childrenPerParent));
        }

        if (newBloodCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newBloodCount));
        }

        if (mutationChance < 0.0 || mutationChance > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(mutationChance));
        }

        if (parentCount * childrenPerParent + newBloodCount != Constants.StartWormCount)
        {
            throw new ArgumentException(
                $"The strategy must produce exactly {Constants.StartWormCount} brains.");
        }

        ParentCount = parentCount;
        ChildrenPerParent = childrenPerParent;
        NewBloodCount = newBloodCount;
        MutationChance = mutationChance;
        MutationSettings = mutationSettings ?? throw new ArgumentNullException(nameof(mutationSettings));
    }

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
            for (var childIndex = 0; childIndex < ChildrenPerParent; childIndex++)
            {
                var brain = parent.Brain.Clone();
                if (NeuroRnd.NextDouble() < MutationChance)
                {
                    brain.Mutate(MutationSettings);
                }

                brains.Add(brain);
            }
        }

        for (var index = 0; index < NewBloodCount; index++)
        {
            var brain = new WormNeuroBrain();
            brain.Init();
            brains.Add(brain);
        }

        // Do not give a lineage a permanent advantage through its position in the turn order.
        return brains.OrderBy(_ => NeuroRnd.NextDouble()).ToList();
    }
}
