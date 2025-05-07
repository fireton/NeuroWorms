namespace NeuroWorms.Core
{
    public static class Constants
    {
        public const int FieldWidth = 180;
        public const int FieldHeight = 180;
        public const int WormStartLength = 3;
        public const int FoodNutrition = 1;
        public const int MaxFoodCount = 40;
        public const int StartWormCount = 25;

        public const int NumberOfParents = 5;
        public const int MaxGenerationTicks   = 5000;
        public const int ChildrenPerGeneration = 5;
        public const double ViewAngle = 120.0;
        public const double ViewDistance = 15.0;

        public const double MutationChance = 0.5;
        public const double MutationStrength = 0.15;

        public const int NeuronsInHiddenLayer1 = 12;
        public const int NeuronsInHiddenLayer2 = 6;
        public const int PercentOfNeuronsToMutate = 30;
    }
}
