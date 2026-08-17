namespace NeuroWorms.Core;

public sealed record GenerationResult(
    int Generation,
    int Ticks,
    int BestAge,
    double AverageAge,
    int BestFoodEaten,
    double AverageFoodEaten,
    double BestFitness,
    double AverageFitness,
    int WallCollisions,
    int WormBodyCollisions,
    double AverageCollisions,
    int HungerDeaths,
    int WallDeaths,
    int WormBodyDeaths,
    int Survivors);
