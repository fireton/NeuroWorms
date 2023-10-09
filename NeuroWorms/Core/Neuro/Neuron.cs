using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuroWorms.Core.Neuro
{
    public class Neuron : BasicNeuron
    {
        public List<Synapse> Synapses = new List<Synapse>();
        public double Bias { get; set; }
        
        protected override double Activate()
        {
            var sum = Synapses.Sum(s => s.GetValue());
            return Math.Tanh(sum + Bias);
        }
    }
}
