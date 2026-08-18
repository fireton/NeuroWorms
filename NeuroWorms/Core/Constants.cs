namespace NeuroWorms.Core
{
    public static class Constants
    {
        public const int FieldWidth = 180;
        public const int FieldHeight = 180;
        public const int WormStartLength = 3;
        public const int FoodNutrition = 1;
        public const int StartWormCount = 50;
        public const int StartFoodCount = StartWormCount;

        public const int MaxGenerationTicks   = 5000;
        
        public const double ViewAngle = 180.0;
        public const double ViewDistance = 70.0;
        public const int MaxHunger = 300;
        public const double BaseHungerPerTick = 1.0;
        public const double HungerPerExtraSegment = 1.0 / 50.0;
        public const int MaxConsecutiveCollisions = 3;

        public const double FitnessAgeWeight = 1.0;
        public const double FitnessFoodWeight = 100.0;
        public const double FitnessCollisionPenalty = 50.0;

    }
}
