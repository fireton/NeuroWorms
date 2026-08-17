using NeuroWorms.Core;
using NeuroWorms.Core.Evolution;
using NeuroWorms.Core.Neuro;

namespace NeuroWorms.Tests;

public class GenerationMutatorTests
{
    [Fact]
    public void NamedStrategiesPreserveTheirConfigurations()
    {
        var legacy = new LegacyCloneAndMutate();
        var fineTuning = new FineTuningCloneAndMutate();

        AssertStrategy(legacy, 4, 10, 10, 0.5, 0.15, 25);
        AssertStrategy(fineTuning, 6, 8, 2, 0.5, 0.075, 15);
    }

    [Fact]
    public void MixedStrategyCreatesExactFineLegacyAndRandomQuotas()
    {
        var strategy = new MixedCloneAndMutate();
        var rankedPopulation = Enumerable.Range(0, strategy.ParentCount)
            .Select(index => CreateWorm($"parent-{index}"))
            .ToList();

        var brains = strategy.CreateNextGeneration(rankedPopulation);
        var inheritedBrains = brains.OfType<TrackingBrain>().ToList();
        var childrenPerParent = strategy.ExactChildrenPerParent
            + strategy.FineTuningChildrenPerParent
            + strategy.LegacyChildrenPerParent;

        Assert.Equal(Constants.StartWormCount, brains.Count);
        Assert.Equal(48, inheritedBrains.Count);
        Assert.Equal(18, inheritedBrains.Count(brain => brain.MutationCount == 0));
        Assert.Equal(30, inheritedBrains.Count(brain => brain.MutationCount == 1));
        Assert.Equal(2, brains.OfType<WormNeuroBrain>().Count());
        Assert.Equal(0.075, strategy.FineTuningMutation.Strength);
        Assert.Equal(15, strategy.FineTuningMutation.PercentOfNeurons);
        Assert.Equal(0.15, strategy.LegacyMutation.Strength);
        Assert.Equal(25, strategy.LegacyMutation.PercentOfNeurons);

        for (var parentIndex = 0; parentIndex < strategy.ParentCount; parentIndex++)
        {
            var marker = $"parent-{parentIndex}";
            Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == marker));
        }
    }

    [Fact]
    public void CloneAndMutateStrategyCreatesConfiguredPopulation()
    {
        var strategy = new CloneAndMutateGenerationMutator(
            parentCount: 6,
            childrenPerParent: 8,
            newBloodCount: 2,
            mutationChance: 0.0,
            new MutationSettings(strength: 0.075, percentOfNeurons: 15));
        var rankedPopulation = Enumerable.Range(0, 6)
            .Select(index => CreateWorm($"parent-{index}"))
            .ToList();

        var brains = strategy.CreateNextGeneration(rankedPopulation);
        var inheritedBrains = brains.OfType<TrackingBrain>().ToList();

        Assert.Equal(Constants.StartWormCount, brains.Count);
        Assert.Equal(48, inheritedBrains.Count);
        Assert.Equal(2, brains.OfType<WormNeuroBrain>().Count());
        for (var parentIndex = 0; parentIndex < 6; parentIndex++)
        {
            var marker = $"parent-{parentIndex}";
            Assert.Equal(8, inheritedBrains.Count(brain => brain.Marker == marker));
        }
    }

    [Fact]
    public void StrategyRejectsConfigurationWithWrongPopulationSize()
    {
        Assert.Throws<ArgumentException>(() => new CloneAndMutateGenerationMutator(
            parentCount: 4,
            childrenPerParent: 10,
            newBloodCount: 2,
            mutationChance: 0.5,
            MutationSettings.Default));
    }

    [Theory]
    [InlineData(0.0, 15)]
    [InlineData(1.1, 15)]
    [InlineData(0.075, 0)]
    [InlineData(0.075, 101)]
    public void MutationSettingsRejectInvalidValues(double strength, int percentOfNeurons)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new MutationSettings(strength, percentOfNeurons));
    }

    private static Worm CreateWorm(string marker)
    {
        return new Worm(new Position(0, 0), [], new TrackingBrain(marker));
    }

    private static void AssertStrategy(
        CloneAndMutateGenerationMutator strategy,
        int parentCount,
        int childrenPerParent,
        int newBloodCount,
        double mutationChance,
        double mutationStrength,
        int percentOfNeurons)
    {
        Assert.Equal(parentCount, strategy.ParentCount);
        Assert.Equal(childrenPerParent, strategy.ChildrenPerParent);
        Assert.Equal(newBloodCount, strategy.NewBloodCount);
        Assert.Equal(mutationChance, strategy.MutationChance);
        Assert.Equal(mutationStrength, strategy.MutationSettings.Strength);
        Assert.Equal(percentOfNeurons, strategy.MutationSettings.PercentOfNeurons);
    }

    private sealed class TrackingBrain(string marker) : WormBrain
    {
        public string Marker { get; } = marker;
        public int MutationCount { get; private set; }

        public override void Init()
        {
        }

        public override WormBrain Clone()
        {
            return new TrackingBrain(Marker);
        }

        public override void Mutate()
        {
            MutationCount++;
        }

        public override MoveDirection GetNextMove(Field field, Worm worm)
        {
            return worm.CurrentDirection;
        }
    }
}
