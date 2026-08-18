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
    int SelfBodyCollisions,
    int OtherWormCollisions,
    double AverageCollisions,
    int HungerDeaths,
    int WallDeaths,
    int SelfBodyDeaths,
    int OtherWormDeaths,
    int Survivors,
    int ChampionAge,
    int ChampionFoodEaten,
    int ChampionLength,
    double ChampionFitness,
    int ChampionWallCollisions,
    int ChampionSelfBodyCollisions,
    int ChampionOtherWormCollisions,
    DeathReason ChampionDeathReason)
{
    public int WormBodyCollisions => SelfBodyCollisions + OtherWormCollisions;
    public int WormBodyDeaths => SelfBodyDeaths + OtherWormDeaths;
    public int ChampionTotalCollisions =>
        ChampionWallCollisions + ChampionSelfBodyCollisions + ChampionOtherWormCollisions;
}
