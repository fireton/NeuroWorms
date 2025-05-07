namespace NeuroWorms.Core.Neuro
{
    internal class EyeObjectSensor(EyeSight eyeSight) : EyeSensor(NeuroConstants.EyeObjectSensorId, eyeSight)
    {
        protected override double Activate()
        {
            EyeSight.DetectNearestObject(Worm, Field);
            return EyeSight.NearestTypeValue;
        }
    }
}
