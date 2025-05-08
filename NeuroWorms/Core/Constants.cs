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

        public const int NumberOfParents = 4;
        public const int ChildrenPerParent = 10;
        public const int NewBloodPerGeneration = 10;
        public const int MaxGenerationTicks   = 5000;
        
        public const double ViewAngle = 10.0;
        public const double ViewDistance = 70.0;
        public const int MaxHunger = 300;
        public const int FoodGenerationTicks = 20;

        public const double MutationChance = 0.5;
    }
}
