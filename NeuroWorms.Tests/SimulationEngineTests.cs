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
        Assert.Equal(1, engine.LastGenerationResult.Generation);
        Assert.Equal(Constants.MaxGenerationTicks, engine.LastGenerationResult.Ticks);
        Assert.Equal(0, engine.LastGenerationResult.BestAge);
        Assert.Equal(0.0, engine.LastGenerationResult.AverageAge);
        Assert.Equal(0, engine.LastGenerationResult.BestFoodEaten);
        Assert.Equal(0.0, engine.LastGenerationResult.AverageFoodEaten);
        Assert.Equal(0.0, engine.LastGenerationResult.BestFitness);
        Assert.Equal(0.0, engine.LastGenerationResult.AverageFitness);
        Assert.Equal(0, engine.LastGenerationResult.WallCollisions);
        Assert.Equal(0, engine.LastGenerationResult.WormBodyCollisions);
        Assert.Equal(0.0, engine.LastGenerationResult.AverageCollisions);
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
    public async Task ParentSelectionUsesWeightedFitnessAndPenalizesCollisions()
    {
        var strategy = new MixedCloneAndMutate();
        var engine = new SimulationEngine(saveFilePath: null, strategy);
        engine.Worms.Clear();

        var old = CreateCandidate("old", age: 5_000);
        var foodTen = CreateCandidate("food-10", age: 0, foodEaten: 10);
        var foodNine = CreateCandidate("food-9", age: 0, foodEaten: 9);
        var foodEight = CreateCandidate("food-8", age: 0, foodEaten: 8);
        var foodSeven = CreateCandidate("food-7", age: 0, foodEaten: 7);
        var collisionFree = CreateCandidate("collision-free", age: 0, foodEaten: 6);
        var collided = CreateCandidate("collided", age: 0, foodEaten: 6, collisions: 1);
        var weak = CreateCandidate("weak", age: 0, foodEaten: 5);
        engine.Worms.AddRange([
            weak,
            collided,
            collisionFree,
            foodSeven,
            foodEight,
            foodNine,
            foodTen,
            old,
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
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "old"));
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "food-10"));
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "food-9"));
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "food-8"));
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "food-7"));
        Assert.Equal(childrenPerParent, inheritedBrains.Count(brain => brain.Marker == "collision-free"));
        Assert.DoesNotContain(inheritedBrains, brain => brain.Marker == "collided");
        Assert.DoesNotContain(inheritedBrains, brain => brain.Marker == "weak");
    }

    [Fact]
    public async Task CollisionStopsWormAndThirdConsecutiveCollisionKillsIt()
    {
        var engine = new SimulationEngine(
            saveFilePath: null,
            new SingleParentGenerationMutator());
        engine.Field.Clear();
        engine.Worms.Clear();
        var worm = new Worm(
            new Position(0, 2),
            [new Position(1, 2), new Position(2, 2), new Position(3, 2)],
            new FixedDirectionBrain(MoveDirection.Left))
        {
            CurrentDirection = MoveDirection.Left,
        };
        engine.Worms.Add(worm);
        worm.RenderToField(engine.Field);

        await engine.NextMove();
        await engine.NextMove();

        Assert.True(worm.IsAlive);
        Assert.Equal(0, worm.Head.X);
        Assert.Equal(2, worm.Head.Y);
        Assert.Equal(2, worm.Age);
        Assert.Equal(2, worm.Hunger);
        Assert.Equal(2, worm.WallCollisions);
        Assert.Equal(2, worm.ConsecutiveCollisions);

        await engine.NextMove();

        Assert.False(worm.IsAlive);
        Assert.Equal(DeathReason.Wall, worm.DeathReason);
        Assert.Equal(3, worm.Age);
        Assert.Equal(3, worm.WallCollisions);
        Assert.Equal(CellType.Empty, engine.Field[0, 2]);

        await engine.NextMove();

        Assert.Equal(1, engine.CurrentGeneration);
        Assert.Equal(3, engine.LastGenerationResult.WallCollisions);
        Assert.Equal(0, engine.LastGenerationResult.WormBodyCollisions);
        Assert.Equal(3.0, engine.LastGenerationResult.AverageCollisions);
        Assert.Equal(1, engine.LastGenerationResult.WallDeaths);
    }

    [Fact]
    public void SuccessfulMoveResetsConsecutiveCollisionStreak()
    {
        var field = new Field(8, 8);
        var worm = new Worm(
            new Position(3, 3),
            [new Position(2, 3)],
            new FixedDirectionBrain(MoveDirection.Right))
        {
            CurrentDirection = MoveDirection.Right,
        };
        worm.RenderToField(field);
        worm.RegisterCollision(DeathReason.Wall);
        worm.RegisterCollision(DeathReason.WormBody);

        worm.Move(MoveDirection.Right, field);

        Assert.Equal(0, worm.ConsecutiveCollisions);
        Assert.Equal(2, worm.TotalCollisions);
        Assert.Equal(3, worm.Age);
        Assert.Equal(3, worm.Hunger);
    }

    private static Worm CreateCandidate(
        string marker,
        int age,
        int foodEaten = 0,
        int collisions = 0)
    {
        var worm = new Worm(
            new Position(0, 0),
            [new Position(1, 0)],
            new TrackingBrain(marker))
        {
            Age = age,
            CurrentDirection = MoveDirection.Right,
        };

        for (var i = 0; i < foodEaten; i++)
        {
            worm.Eat();
        }

        for (var i = 0; i < collisions; i++)
        {
            worm.RegisterCollision(DeathReason.Wall);
        }

        return worm;
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

    private sealed class FixedDirectionBrain(MoveDirection direction) : WormBrain
    {
        public override void Init()
        {
        }

        public override WormBrain Clone()
        {
            return new FixedDirectionBrain(direction);
        }

        public override void Mutate()
        {
        }

        public override MoveDirection GetNextMove(Field field, Worm worm)
        {
            return direction;
        }
    }

    private sealed class SingleParentGenerationMutator : GenerationMutator
    {
        public override IReadOnlyList<WormBrain> CreateNextGeneration(
            IReadOnlyList<Worm> rankedPopulation)
        {
            return Enumerable.Range(0, Constants.StartWormCount)
                .Select(_ => rankedPopulation[0].Brain.Clone())
                .ToList();
        }
    }
}
