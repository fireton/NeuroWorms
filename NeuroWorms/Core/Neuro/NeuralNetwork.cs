using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuroWorms.Core.Neuro
{
    internal class NeuralNetwork
    {
        public readonly List<BasicNeuron> Neurons = new();
        public MotorNeuron MotorNeuron { get; private set; }

        public NeuralNetwork(EyeSight eyeSight)
        {
            AddSensorsAndMotor(eyeSight);
        }

        private void AddSensorsAndMotor(EyeSight eyeSight)
        {
            // first we add sensor neurons
            AddNeuron(new EyeAngleSensor(eyeSight), 0);
            AddNeuron(new EyeDistanceSensor(eyeSight), 0);
            AddNeuron(new EyeObjectSensor(eyeSight), 0);
            AddNeuron(new LengthSensor(), 0);
            // and motor neuron
            MotorNeuron = new MotorNeuron(NeuroRnd.Next());
            AddNeuron(MotorNeuron, 3);
        }

        public void AddNeuron(BasicNeuron neuron, int layer)
        {
            neuron.Layer = layer;
            Neurons.Add(neuron);
        }

        public IBasicNeuron GetNeuron(Guid id)
        {
            return Neurons.Find(n => n.Id == id);
        }

        public IEnumerable<IBasicNeuron> GetNeuronsInLayer(int layer)
        {
            return Neurons.FindAll(n => n.Layer == layer);
        }

        public IEnumerable<INeuronWithSynapses> GetNeuronWithSynapsesInLayer(int layer)
        {
            return Neurons.FindAll(n => n.Layer == layer && n is INeuronWithSynapses).ConvertAll(n => (INeuronWithSynapses)n);
        }

        public void Reset(Worm worm, Field field)
        {
            Neurons.ForEach(n =>
            {
                if (n is IWormFieldResettable wormFieldResettable)
                {
                    wormFieldResettable.Reset(worm, field);
                }
                else if (n is IWormResettable wormResettable)
                {
                    wormResettable.Reset(worm);
                }
                else if (n is ISimpleResettable simpleResettable)
                {
                    simpleResettable.Reset();
                }
            });
        }

        public NeuralNetwork Clone(EyeSight eyeSight)
        {
            int[] hiddenLayers = { 1, 2 };
            var clone = new NeuralNetwork(eyeSight);
            // now we need to copy all hidden neurons and synapses
            var hiddenNeurons = Neurons.FindAll(n => hiddenLayers.Contains(n.Layer)).ConvertAll(n => (INeuronWithSynapses)n);
            foreach (var hiddenNeuron in hiddenNeurons)
            {
                clone.AddNeuron(new Neuron(hiddenNeuron.Id, hiddenNeuron.Bias), hiddenNeuron.Layer);
            }
            
            var neuronsWithSynapsesToCopy = Neurons.FindAll(n => n is INeuronWithSynapses).ConvertAll(n => (INeuronWithSynapses)n);
            foreach (var neuronWithSynapsesToCopy in neuronsWithSynapsesToCopy)
            {
                var cloneNeuron = (INeuronWithSynapses)clone.GetNeuron(neuronWithSynapsesToCopy.Id);
                foreach (var synapse in neuronWithSynapsesToCopy.Synapses)
                {
                    cloneNeuron.Synapses.Add(new Synapse(synapse.Weight, clone.GetNeuron(synapse.From.Id)));
                }
            }
            return clone;
        }

        public void Mutate()
        {
            var neuron = GetRandomNeuronWithSynapses();
            if (NeuroRnd.NextDouble() > 0.5)
            {
                MutateBias(neuron);
            }
            else
            {
                MutateSynapse(neuron);
            }
        }

        private void MutateSynapse(INeuronWithSynapses neuron)
        {
            var synapse = neuron.Synapses.Count > 0 ? neuron.Synapses[NeuroRnd.Next(0, neuron.Synapses.Count)] : null;
            if (synapse != null)
            {
                synapse.Weight = NeuroRnd.Jitter(synapse.Weight);
            }
        }

        private void MutateBias(INeuronWithSynapses neuron)
        {
            neuron.Bias = NeuroRnd.Jitter(neuron.Bias);
        }

        private INeuronWithSynapses GetRandomNeuronWithSynapses()
        {
            var neuronsWithSynapses = Neurons.FindAll(n => n is INeuronWithSynapses).ConvertAll(n => (INeuronWithSynapses)n);
            return neuronsWithSynapses[NeuroRnd.Next(0, neuronsWithSynapses.Count)];
        }
    }
}
