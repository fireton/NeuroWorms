using NeuroWorms.Core;
using NeuroWorms.Core.Evolution;

namespace NeuroWorms.Tests;

public class SimulationEngineTests
{
    [Fact]
    public async Task TickLimitStartsCleanGenerationWithoutMovingOldWorms()
    {
        var strategy = new FineTuningCloneAndMutate();
        var engine = new SimulationEngine(saveFilePath: null, strategy);
        var previousGeneration = engine.Worms.ToArray();
        SetCurrentTick(engine, Constants.MaxGenerationTicks);

        await engine.NextMove();

        Assert.Equal(1, engine.CurrentGeneration);
        Assert.Equal(0, engine.CurrentTick);
        Assert.All(engine.Worms, worm => Assert.Equal(0, worm.Age));
        Assert.Empty(engine.Worms.Intersect(previousGeneration));
        Assert.Equal(0, engine.LastGenerationResult.Generation);
        Assert.Equal(Constants.MaxGenerationTicks, engine.LastGenerationResult.Ticks);
        Assert.Equal(0, engine.LastGenerationResult.BestAge);
        Assert.Equal(0.0, engine.LastGenerationResult.AverageAge);
        Assert.Equal(0, engine.LastGenerationResult.BestFoodEaten);
        Assert.Equal(0.0, engine.LastGenerationResult.AverageFoodEaten);
        Assert.Equal(0, engine.LastGenerationResult.HungerDeaths);
        Assert.Equal(0, engine.LastGenerationResult.WallDeaths);
        Assert.Equal(0, engine.LastGenerationResult.WormBodyDeaths);
        Assert.Equal(Constants.StartWormCount, engine.LastGenerationResult.Survivors);
    }

    [Fact]
    public async Task GenerationResultCountsEveryDeathReason()
    {
        var engine = new SimulationEngine(saveFilePath: null);
        SetDeathReason(engine.Worms.Take(20), DeathReason.Hunger);
        SetDeathReason(engine.Worms.Skip(20).Take(10), DeathReason.Wall);
        SetDeathReason(engine.Worms.Skip(30).Take(5), DeathReason.WormBody);
        SetCurrentTick(engine, Constants.MaxGenerationTicks);

        await engine.NextMove();

        Assert.Equal(20, engine.LastGenerationResult.HungerDeaths);
        Assert.Equal(10, engine.LastGenerationResult.WallDeaths);
        Assert.Equal(5, engine.LastGenerationResult.WormBodyDeaths);
        Assert.Equal(15, engine.LastGenerationResult.Survivors);
    }

    [Fact]
    public async Task ParentSelectionPrioritizesLengthAndUsesAgeToBreakTies()
    {
        var strategy = new MixedCloneAndMutate();
        var engine = new SimulationEngine(saveFilePath: null);
        engine.Worms.Clear();

        var lengthTen = CreateCandidate("length-10", bodyLength: 10, age: 100);
        var lengthNine = CreateCandidate("length-9", bodyLength: 9, age: 100);
        var lengthEight = CreateCandidate("length-8", bodyLength: 8, age: 100);
        var lengthSeven = CreateCandidate("length-7", bodyLength: 7, age: 100);
        var lengthSix = CreateCandidate("length-6", bodyLength: 6, age: 100);
        var olderLengthFive = CreateCandidate("length-5-older", bodyLength: 5, age: 200);
        var youngerLengthFive = CreateCandidate("length-5-younger", bodyLength: 5, age: 100);
        var ancientButShort = CreateCandidate("length-4-ancient", bodyLength: 4, age: 5_000);
        engine.Worms.AddRange([
            ancientButShort,
            youngerLengthFive,
            olderLengthFive,
            lengthSix,
            lengthSeven,
            lengthEight,
            lengthNine,
            lengthTen,
        ]);

        SetCurrentTick(engine, Constants.MaxGenerationTicks);
        await engine.NextMove();

        var inheritedBrains = engine.Worms
            .Select(worm => worm.Brain)
            .OfType<TrackingBrain>()
            .ToList();
        var childrenPerParent = strategy.ExactChildrenPerParent
            + strategy.FineTuningChildrenPerParent
            + strategy.LegacyChildrenPerParent;

        Assert.Equal(strategy.ParentCount * childrenPerParent, inheritedBrains.Count);
        Assert.Equal(18, inheritedBrains.Count(brain => brain.MutationCount == 0));
        Assert.Equal(30, inheritedBrains.Count(brain => brain.MutationCount == 1));
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "length-10"));
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "length-9"));
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "length-8"));
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "length-7"));
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "length-6"));
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "length-5-older"));
        Assert.DoesNotContain(inheritedBrains, brain => brain.Marker == "length-5-younger");
        Assert.DoesNotContain(inheritedBrains, brain => brain.Marker == "length-4-ancient");
    }

    private static Worm CreateCandidate(string marker, int bodyLength, int age)
    {
        var body = Enumerable.Range(1, bodyLength - 1)
            .Select(index => new Position(index, 0))
            .ToList();

        return new Worm(new Position(0, 0), body, new TrackingBrain(marker))
        {
            Age = age,
            CurrentDirection = MoveDirection.Right,
        };
    }

    private static void SetCurrentTick(SimulationEngine engine, int value)
    {
        var setter = typeof(SimulationEngine)
            .GetProperty(nameof(SimulationEngine.CurrentTick))!
            .GetSetMethod(nonPublic: true)!;
        setter.Invoke(engine, [value]);
    }

    private static void SetDeathReason(IEnumerable<Worm> worms, DeathReason deathReason)
    {
        foreach (var worm in worms)
        {
            worm.DeathReason = deathReason;
        }
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
