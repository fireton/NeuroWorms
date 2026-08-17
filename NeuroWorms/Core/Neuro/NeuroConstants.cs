using System;

namespace NeuroWorms.Core.Neuro
{
    public static class NeuroConstants
    {
        public static readonly Guid FoodAngleSensorId = new("de059de7-1e33-49e1-b349-87978ab29f06");
        public static readonly Guid FoodDistanceSensorId = new("cb734dcb-32fc-4177-8eb8-659dca46881f");
        public static readonly Guid FoodPresenceSensorId = new("0116743f-2803-4d25-a5f0-22c6e22ca729");
        public static readonly Guid WormAngleSensorId = new("A780A9C8-4D58-4798-A6B5-9CA417BFC026");
        public static readonly Guid WormDistanceSensorId = new("3AECDF4A-FC60-4E0D-B640-E1C665A798A3");
        public static readonly Guid WormPresenceSensorId = new("BDD23085-3026-4C9A-8E0A-3C176EE2E80E");
        public static readonly Guid WallAngleSensorId = new("CCA5E0AF-BDF7-47F9-A7A6-F96FB37ECDD7");
        public static readonly Guid WallDistanceSensorId = new("FC1D91E6-25F1-41B7-8CB8-9D514FA57871");
        public static readonly Guid WallPresenceSensorId = new("EBFAF543-CCD2-4257-AFB6-1C18C333AF23");
        public static readonly Guid LengthSensorId = new("02C916E0-2F10-4EF9-9C98-FFDCA43F6F02");
        public static readonly Guid MotorNeuronId = new("1192BFDE-F516-41FE-BE8C-8EAC91B538AC");
        public static readonly Guid DirXNeuronId = new("AD46D9E7-34B8-4413-8032-43422413047B");
        public static readonly Guid DirYNeuronId = new("92FCF541-9529-4E34-BE95-5B52D5878371");
        public static readonly Guid HungerSensorId = new("C0A2E4D1-3F5B-4E8F-9A7C-6D1B2F5A0E7D");
        public static readonly Guid ObstacleAtLeftSensorId = new("2F97F73E-9474-4283-B0B8-EEE7D074888E");
        public static readonly Guid ObstacleAtRightSensorId = new("FC77A6A0-F5FA-48C5-99C8-51076459F5B3");

        public const double MutationStrength = 0.075;
        public const int NeuronsInHiddenLayer1 = 25;
        public const int NeuronsInHiddenLayer2 = 10;
        public const int PercentOfNeuronsToMutate = 15;
    }
}
