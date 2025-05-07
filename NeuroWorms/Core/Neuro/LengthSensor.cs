namespace NeuroWorms.Core.Neuro
{
    public class LengthSensor : BasicNeuron, IWormResettable
    {
        private readonly static double matureLength = 50.0;
        private double lengthValue;

        public LengthSensor() : base(NeuroConstants.LengthSensorId)
        {
        }

        public void Reset(Worm worm)
        {
            lengthValue = ((worm.Body.Count + 1) / matureLength) * 2.0 - 1.0;
            base.Reset();
        }

        protected override double Activate()
        {
            return lengthValue;
        }
    }
}
