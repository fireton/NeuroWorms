namespace NeuroWorms.Core.Neuro
{
    internal class EyeAngleSensor : EyeSensor
    {
        public EyeAngleSensor(EyeSight eyeSight) : base(NeuroConstants.EyeAngleSensorId, eyeSight)
        {
        }

        protected override double Activate()
        {
            EyeSight.DetectNearestObject(Worm, Field);
            return EyeSight.NearestAngle;
        }
    }
}
