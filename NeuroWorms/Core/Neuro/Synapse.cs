namespace NeuroWorms.Core.Neuro
{
    public class Synapse
    {
        public double Weight { get; set; }
        public IBasicNeuron From { get; set; }

        public Synapse(double weight, IBasicNeuron from)
        {
            Weight = weight;
            From = from;
        }

        public double GetValue()
        {
            return From.GetValue() * Weight;
        }
    }
}
