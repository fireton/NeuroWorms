using NeuroWorms.Core;
using NeuroWorms.Core.Evolution;

namespace NeuroWorms.Tests;

public class FitnessFunctionTests
{
    [Fact]
    public void WeightedFitnessRewardsAgeAndFoodAndPenalizesEveryCollision()
    {
        var worm = new Worm(
            new Position(2, 2),
            [new Position(1, 2)],
            new StupidRandomBrain())
        {
            Age = 100,
        };
        worm.Eat();
        worm.Eat();
        worm.RegisterCollision(DeathReason.Wall);
        worm.RegisterCollision(DeathReason.OtherWorm);
        var fitness = new WeightedAgeFoodCollisionFitness();

        var result = fitness.Evaluate(worm);

        Assert.Equal(202.0, result);
    }
}
