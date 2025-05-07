namespace NeuroWorms.Core.Neuro
{
    internal class EyeDistanceSensor(EyeSight eyeSight) : EyeSensor(NeuroConstants.EyeDistanceSensorId, eyeSight)
    {
        protected override double Activate()
        {
            EyeSight.DetectNearestObject(Worm, Field);
            return EyeSight.NearestDistance;
        }
    }
}
