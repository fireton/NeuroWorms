using System;
using System.Linq;

namespace NeuroWorms.Core.Neuro
{
    internal class WormNeuroBrain : WormBrain
    {
        private readonly EyeSight eyeSight;
        private readonly NeuralNetwork neuralNetwork;

        public WormNeuroBrain(EyeSight eyeSight = null, NeuralNetwork neuralNetwork = null)
        {
            eyeSight ??= new EyeSight(Constants.ViewAngle, Constants.ViewDistance);
            neuralNetwork ??= new NeuralNetwork(eyeSight);
            this.eyeSight = eyeSight;
            this.neuralNetwork = neuralNetwork;
        }
        
        public override WormBrain Clone()
        {
            var cloneEyeSight = eyeSight.Clone();
            var cloneNeuralNetwork = neuralNetwork.Clone(cloneEyeSight);  
            var clone = new WormNeuroBrain(cloneEyeSight, cloneNeuralNetwork);
            return clone;
        }

        public override MoveDirection GetNextMove(Field field, Worm worm)
        {
            eyeSight.Reset();
            neuralNetwork.Reset(worm, field);
            return neuralNetwork.MotorNeuron.GetDirection();
        }

        public override void Init()
        {
            // add hidden neurons, four neurons per layer
            for (var i = 0; i < Constants.NeuronsPerHiddenLayer; i++)
            {
                neuralNetwork.AddNeuron(new Neuron(Guid.NewGuid(), NeuroRnd.Next()), 1);
                neuralNetwork.AddNeuron(new Neuron(Guid.NewGuid(), NeuroRnd.Next()), 2);
            }
            // now we connect all neurons in sensor layer with all neurons in first hidden layer
            var sensorNeurons = neuralNetwork.GetNeuronsInLayer(0);
            var hiddenNeurons1 = neuralNetwork.GetNeuronWithSynapsesInLayer(1);
            foreach (var sensorNeuron in sensorNeurons)
            {
                foreach (var hiddenNeuron in hiddenNeurons1)
                {
                    hiddenNeuron.Synapses.Add(new Synapse(NeuroRnd.Next(), sensorNeuron));
                }
            }
            // now we connect all neurons in second hidden layer with motor neuron
            var hiddenNeurons2 = neuralNetwork.GetNeuronWithSynapsesInLayer(2);
            var motorNeuron = neuralNetwork.GetNeuronWithSynapsesInLayer(3).FirstOrDefault() ?? throw new InvalidOperationException("Motor neuron not found!"); // it's only one neuron in motor layer
            foreach (var hiddenNeuron in hiddenNeurons2)
            {
                motorNeuron.Synapses.Add(new Synapse(NeuroRnd.Next(), hiddenNeuron));
            }

            // now we connect all neurons in first hidden layer with all neurons in second hidden layer
            foreach (var hiddenNeuron1 in hiddenNeurons1)
            {
                foreach (var hiddenNeuron2 in hiddenNeurons2)
                {
                    hiddenNeuron2.Synapses.Add(new Synapse(NeuroRnd.Next(), hiddenNeuron1));
                }
            }
        }

        public override void Mutate()
        {
            neuralNetwork.Mutate();
        }
    }
}
