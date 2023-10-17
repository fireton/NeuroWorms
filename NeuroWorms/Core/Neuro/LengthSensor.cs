namespace NeuroWorms.Core.Neuro
{
    public class LengthSensor : SensorNeuron
    {
        private readonly static double matureLength = 50.0;

        public LengthSensor() : base(NeuroConstants.LengthSensorId)
        {
        }

        protected override double Activate()
        {
            return (Worm.Body.Count + 1) / matureLength * 2 - 1;
        }
    }
}
