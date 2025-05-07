namespace NeuroWorms.Core
{
    public static class Constants
    {
        public const int FieldWidth = 180;
        public const int FieldHeight = 180;
        public const int WormStartLength = 3;
        public const int FoodNutrition = 1;
        public const int StartFoodCount = 100;
        public const int StartWormCount = 50;

        public const int NumberOfParents = 5;
        public const int MaxGenerationTicks   = 5000;
        public const int ChildrenPerGeneration = 10;
        public const double ViewAngle = 120.0;
        public const double ViewDistance = 15.0;
        public const int MaxHunger = 300;
        public const int FoodGenerationTicks = 20;

        public const double MutationChance = 0.5;
    }
}
