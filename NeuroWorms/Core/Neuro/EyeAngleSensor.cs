namespace NeuroWorms.Core.Neuro
{
    internal class EyeAngleSensor(EyeSight eyeSight) : EyeSensor(NeuroConstants.EyeAngleSensorId, eyeSight)
    {
        protected override double Activate()
        {
            EyeSight.DetectNearestObject(Worm, Field);
            return EyeSight.NearestAngle;
        }
    }
}
