using System;
using System.Collections.Generic;

namespace NeuroWorms.Core.Neuro
{
    public interface ISimpleResettable
    {
        void Reset();
    }

    public interface IWormResettable
    {
        void Reset(Worm worm);
    }

    public interface IWormFieldResettable
    {
        void Reset(Worm worm, Field field);
    }

    public interface IBasicNeuron
    {
        Guid Id { get; }
        int Layer { get; set; }
        double GetValue();
    }

    public interface INeuronWithSynapses : IBasicNeuron
    {
        double Bias { get; set; }
        List<Synapse> Synapses { get; }
        bool IsConnectedWith(IBasicNeuron neuron);
    }
}
