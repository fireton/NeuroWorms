namespace NeuroWorms.Core.Neuro
{
    internal class EyeObjectSensor : EyeSensor
    {
        public EyeObjectSensor(EyeSight eyeSight) : base(NeuroConstants.EyeObjectSensorId, eyeSight)
        {
        }

        protected override double Activate()
        {
            EyeSight.DetectNearestObject(Worm, Field);
            return EyeSight.NearestTypeValue;
        }
    }
}
