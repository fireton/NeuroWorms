using System.Collections.Generic;

namespace NeuroWorms.Core.Neuro
{
    internal class NeuronLayer
    {
        private readonly List<BasicNeuron> neurons = new();
        public LayerType Type { get; }

        public NeuronLayer(LayerType type)
        {
            Type = type;
        }

        public void AddNeuron(BasicNeuron neuron)
        {
            neurons.Add(neuron);
        }

        public void Reset(Worm worm, Field field)
        {
            switch (Type)
            {
                case LayerType.Sensor:
                    foreach (var neuron in neurons)
                    {
                        ((SensorNeuron)neuron).Reset(worm, field);
                    }
                    break;
                case LayerType.Hidden:
                    foreach (var neuron in neurons)
                    {
                        neuron.Reset();
                    }
                    break;
                case LayerType.Motor:
                    foreach (var neuron in neurons)
                    {
                        ((MotorNeuron)neuron).Reset(worm.CurrentDirection);
                    }
                    break;
            }
        }
    }

    public enum LayerType
    {
        Sensor,
        Hidden,
        Motor
    }
}
