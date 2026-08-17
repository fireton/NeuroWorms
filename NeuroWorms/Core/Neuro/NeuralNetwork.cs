using System;
using System.Collections.Generic;
using System.IO;
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
            // food detection
            AddNeuron(new EyeObjectDetectionSensor(NeuroConstants.FoodPresenceSensorId, eyeSight, ObjectType.Food), 0);
            AddNeuron(new EyeAngleSensor(NeuroConstants.FoodAngleSensorId, eyeSight, ObjectType.Food), 0);
            AddNeuron(new EyeDistanceSensor(NeuroConstants.FoodDistanceSensorId, eyeSight, ObjectType.Food), 0);
            // worm detection
            AddNeuron(new EyeObjectDetectionSensor(NeuroConstants.WormPresenceSensorId, eyeSight, ObjectType.Worm), 0);
            AddNeuron(new EyeAngleSensor(NeuroConstants.WormAngleSensorId, eyeSight, ObjectType.Worm), 0);
            AddNeuron(new EyeDistanceSensor(NeuroConstants.WormDistanceSensorId, eyeSight, ObjectType.Worm), 0);
            // wall detection
            AddNeuron(new EyeObjectDetectionSensor(NeuroConstants.WallPresenceSensorId, eyeSight, ObjectType.Wall), 0);
            AddNeuron(new EyeAngleSensor(NeuroConstants.WallAngleSensorId, eyeSight, ObjectType.Wall), 0);
            AddNeuron(new EyeDistanceSensor(NeuroConstants.WallDistanceSensorId, eyeSight, ObjectType.Wall), 0);
            // obstacle detection
            AddNeuron(new ObstacleAtLeftSensor(), 0);
            AddNeuron(new ObstacleAheadSensor(), 0);
            AddNeuron(new ObstacleAtRightSensor(), 0);
            // worm self-awareness
            AddNeuron(new LengthSensor(), 0);
            AddNeuron(new HungerSensor(), 0);
            AddNeuron(new CollisionStreakSensor(), 0);
            // and then motor neuron
            MotorNeuron = new MotorNeuron(0.0);
            AddNeuron(MotorNeuron, 3);

            var sensorCount = GetNeuronsInLayer(0).Count();
            if (sensorCount != NeuroConstants.SensorCount)
            {
                throw new InvalidOperationException(
                    $"The network contains {sensorCount} sensors; expected {NeuroConstants.SensorCount}.");
            }
        }

        public void AddNeuron(BasicNeuron neuron, int layer)
        {
            if (Neurons.Any(existingNeuron => existingNeuron.Id == neuron.Id))
            {
                throw new InvalidOperationException($"A neuron with ID {neuron.Id} already exists in the network.");
            }

            neuron.Layer = layer;
            Neurons.Add(neuron);
        }

        public IBasicNeuron GetNeuron(Guid id)
        {
            return Neurons.Single(n => n.Id == id);
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
                else
                {
                    throw new InvalidOperationException("Unknown neuron type!");
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

            clone.MotorNeuron.Bias = MotorNeuron.Bias;
            
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

        public BrainGenome ExportGenome()
        {
            var evolvingNeurons = GetEvolvingNeurons().ToList();
            return new BrainGenome
            {
                Biases = evolvingNeurons.Select(neuron => neuron.Bias).ToList(),
                Weights = evolvingNeurons
                    .SelectMany(neuron => neuron.Synapses)
                    .Select(synapse => synapse.Weight)
                    .ToList(),
            };
        }

        public void ImportGenome(BrainGenome genome)
        {
            ArgumentNullException.ThrowIfNull(genome);

            if (genome.Biases is null || genome.Weights is null)
            {
                throw new InvalidDataException("The genome must contain both biases and weights.");
            }

            var evolvingNeurons = GetEvolvingNeurons().ToList();
            var expectedWeightCount = evolvingNeurons.Sum(neuron => neuron.Synapses.Count);

            if (genome.Biases.Count != evolvingNeurons.Count)
            {
                throw new InvalidDataException(
                    $"The genome contains {genome.Biases.Count} biases; expected {evolvingNeurons.Count}.");
            }

            if (genome.Weights.Count != expectedWeightCount)
            {
                throw new InvalidDataException(
                    $"The genome contains {genome.Weights.Count} weights; expected {expectedWeightCount}.");
            }

            if (genome.Biases.Any(value => !double.IsFinite(value)) ||
                genome.Weights.Any(value => !double.IsFinite(value)))
            {
                throw new InvalidDataException("The genome contains a non-finite parameter.");
            }

            for (var neuronIndex = 0; neuronIndex < evolvingNeurons.Count; neuronIndex++)
            {
                evolvingNeurons[neuronIndex].Bias = genome.Biases[neuronIndex];
            }

            var weightIndex = 0;
            foreach (var neuron in evolvingNeurons)
            {
                foreach (var synapse in neuron.Synapses)
                {
                    synapse.Weight = genome.Weights[weightIndex++];
                }
            }
        }

        public void Mutate(MutationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var mutableNeurons = GetEvolvingNeurons().Count();
            var neuronsToMutate = (int)Math.Max(1, Math.Round(mutableNeurons * settings.PercentOfNeurons / 100.0));
            var neurons = GetRandomNeuronsWithSynapses(neuronsToMutate);
            foreach (var neuron in neurons)
            {
                if (NeuroRnd.NextDouble() < 0.4)
                    MutateBias(neuron, settings.Strength);

                if (NeuroRnd.NextDouble() < 0.6)
                    MutateSynapses(neuron, settings.Strength);
            }
        }

        private static void MutateSynapses(INeuronWithSynapses neuron, double mutationStrength)
        {
            if (neuron.Synapses.Count == 0) return;

            int toMutate = Math.Max(1, neuron.Synapses.Count / 3); // 33%
            foreach (var synapse in neuron.Synapses.OrderBy(_ => NeuroRnd.NextDouble()).Take(toMutate))
            {
                synapse.Weight = NeuroRnd.GaussianJitter(synapse.Weight, mutationStrength);
            }
        }

        private static void MutateBias(INeuronWithSynapses neuron, double mutationStrength)
        {
            neuron.Bias = NeuroRnd.GaussianJitter(neuron.Bias, mutationStrength);
        }

        private IEnumerable<INeuronWithSynapses> GetRandomNeuronsWithSynapses(int count)
        {
            var neuronsWithSynapses = Neurons.FindAll(n => n is INeuronWithSynapses).ConvertAll(n => (INeuronWithSynapses)n);

            if (neuronsWithSynapses.Count < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Not enough neurons with synapses to select from.");
            }

            // select number of unique random neurons
            return neuronsWithSynapses.OrderBy(_ => NeuroRnd.NextDouble()).Take(count);
        }

        private IEnumerable<INeuronWithSynapses> GetEvolvingNeurons()
        {
            return Neurons
                .Where(neuron => neuron is INeuronWithSynapses)
                .OrderBy(neuron => neuron.Layer)
                .Cast<INeuronWithSynapses>();
        }
    }
}
