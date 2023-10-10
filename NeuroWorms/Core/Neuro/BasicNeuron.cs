namespace NeuroWorms.Core.Neuro
{
    public abstract class BasicNeuron
    {
        private double activatedValue;
        private bool isActivated;
        
        // This Activate should return value between -1 and 1
        // as we using tanh as activation function.
        // This is especially important for SensorNeurons.
        protected abstract double Activate();

        public double GetValue()
        {
            if (!isActivated)
            {
                activatedValue = Activate();
            }

            return activatedValue;
        }

        public void Reset()
        {
            isActivated = false;
        }

    }
}
