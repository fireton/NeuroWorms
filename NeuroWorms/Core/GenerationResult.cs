namespace NeuroWorms.Core;

public sealed record GenerationResult(
    int Generation,
    int Ticks,
    int BestAge,
    double AverageAge,
    int BestFoodEaten,
    double AverageFoodEaten,
    int HungerDeaths,
    int WallDeaths,
    int WormBodyDeaths,
    int Survivors);
