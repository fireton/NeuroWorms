namespace NeuroWorms.Core.Neuro
{
    public class Synapse
    {
        public double Weight { get; set; }
        public BasicNeuron From { get; set; }

        public Synapse(double weight, BasicNeuron from)
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
