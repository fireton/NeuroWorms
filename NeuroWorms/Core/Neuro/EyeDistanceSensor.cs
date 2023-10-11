namespace NeuroWorms.Core.Neuro
{
    internal class EyeDistanceSensor : EyeSensor
    {
        public EyeDistanceSensor(EyeSight eyeSight) : base(NeuroConstants.EyeDistanceSensorId, eyeSight)
        {
        }

        protected override double Activate()
        {
            EyeSight.DetectNearestObject(Worm, Field);
            return EyeSight.NearestDistance;
        }
    }
}
