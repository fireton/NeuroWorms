namespace NeuroWorms.Core.Neuro
{
    public abstract class BasicNeuron
    {
        private double activatedValue;
        private bool isActivated;
        
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
