using System;

namespace NeuroWorms.Core.Evolution;

internal abstract class FitnessFunction
{
    public abstract double Evaluate(Worm worm);
}

internal sealed class WeightedAgeFoodCollisionFitness : FitnessFunction
{
    public double AgeWeight { get; }
    public double FoodWeight { get; }
    public double CollisionPenalty { get; }

    public WeightedAgeFoodCollisionFitness(
        double ageWeight = Constants.FitnessAgeWeight,
        double foodWeight = Constants.FitnessFoodWeight,
        double collisionPenalty = Constants.FitnessCollisionPenalty)
    {
        if (!double.IsFinite(ageWeight) || ageWeight < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(ageWeight));
        }

        if (!double.IsFinite(foodWeight) || foodWeight < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(foodWeight));
        }

        if (!double.IsFinite(collisionPenalty) || collisionPenalty < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(collisionPenalty));
        }

        AgeWeight = ageWeight;
        FoodWeight = foodWeight;
        CollisionPenalty = collisionPenalty;
    }

    public override double Evaluate(Worm worm)
    {
        ArgumentNullException.ThrowIfNull(worm);

        return worm.Age * AgeWeight
            + worm.FoodEaten * FoodWeight
            - worm.TotalCollisions * CollisionPenalty;
    }
}
