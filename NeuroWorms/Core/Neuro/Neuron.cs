using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuroWorms.Core.Neuro
{
    public class Neuron : BasicNeuron, INeuronWithSynapses
    {
        public List<Synapse> Synapses { get; } = new List<Synapse>() ;
        public double Bias { get; set; }

        public Neuron(Guid id, double bias) : base(id) 
        {
            Bias = bias;
        }
        
        protected override double Activate()
        {
            var sum = Synapses.Sum(s => s.GetValue());
            return Math.Tanh(sum + Bias);
        }

        public bool IsConnectedWith(IBasicNeuron neuron)
        {
            return Synapses.Exists(s => s.From.Id == neuron.Id);
        }
    }
}
