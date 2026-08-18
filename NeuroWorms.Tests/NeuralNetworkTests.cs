using NeuroWorms.Core;
using NeuroWorms.Core.Neuro;

namespace NeuroWorms.Tests;

public class NeuralNetworkTests
{
    [Fact]
    public void AllSensorIdsAreUnique()
    {
        var sensorIds = new[]
        {
            NeuroConstants.FoodAngleSensorId,
            NeuroConstants.FoodDistanceSensorId,
            NeuroConstants.FoodPresenceSensorId,
            NeuroConstants.OtherWormAngleSensorId,
            NeuroConstants.OtherWormDistanceSensorId,
            NeuroConstants.OtherWormPresenceSensorId,
            NeuroConstants.OwnBodyAngleSensorId,
            NeuroConstants.OwnBodyDistanceSensorId,
            NeuroConstants.OwnBodyPresenceSensorId,
            NeuroConstants.WallAngleSensorId,
            NeuroConstants.WallDistanceSensorId,
            NeuroConstants.WallPresenceSensorId,
            NeuroConstants.ObstacleAtLeftSensorId,
            NeuroConstants.ObstacleAheadSensorId,
            NeuroConstants.ObstacleAtRightSensorId,
            NeuroConstants.LengthSensorId,
            NeuroConstants.HungerSensorId,
            NeuroConstants.CollisionStreakSensorId,
            NeuroConstants.OwnBodyAvoidanceForwardSensorId,
            NeuroConstants.OwnBodyAvoidanceRightSensorId,
            NeuroConstants.OwnBodyPressureSensorId,
        };

        Assert.Equal(NeuroConstants.SensorCount, sensorIds.Length);
        Assert.Equal(sensorIds.Length, sensorIds.Distinct().Count());
    }

    [Fact]
    public void InitializedBrainHasCompactTopologyAndXavierWeights()
    {
        var brain = new WormNeuroBrain();

        brain.Init();
        var genome = brain.ExportGenome();

        Assert.Equal(19, genome.Biases.Count);
        Assert.Equal(330, genome.Weights.Count);
        Assert.All(genome.Biases, bias => Assert.Equal(0.0, bias));

        AssertWeightsWithinXavierLimit(
            genome.Weights.Take(252),
            fanIn: NeuroConstants.SensorCount,
            fanOut: NeuroConstants.NeuronsInHiddenLayer1);
        AssertWeightsWithinXavierLimit(
            genome.Weights.Skip(252).Take(72),
            fanIn: NeuroConstants.NeuronsInHiddenLayer1,
            fanOut: NeuroConstants.NeuronsInHiddenLayer2);
        AssertWeightsWithinXavierLimit(
            genome.Weights.Skip(324).Take(6),
            fanIn: NeuroConstants.NeuronsInHiddenLayer2,
            fanOut: 1);
    }

    [Theory]
    [InlineData(-10.0, MoveDirection.Left)]
    [InlineData(0.0, MoveDirection.Up)]
    [InlineData(10.0, MoveDirection.Right)]
    public void MotorUsesNegativeForLeftAndPositiveForRight(
        double bias,
        MoveDirection expectedDirection)
    {
        var motor = new MotorNeuron(bias);
        var worm = new Worm(new Position(2, 2), [], new StupidRandomBrain())
        {
            CurrentDirection = MoveDirection.Up,
        };

        motor.Reset(worm);

        Assert.Equal(expectedDirection, motor.GetDirection());
    }

    [Fact]
    public void AddNeuronRejectsDuplicateIds()
    {
        var network = new NeuralNetwork(new EyeSight(10.0, 70.0));
        var duplicate = new Neuron(NeuroConstants.MotorNeuronId, 0.0);

        Assert.Throws<InvalidOperationException>(() => network.AddNeuron(duplicate, 1));
    }

    [Fact]
    public void ClonePreservesNeuronParametersAndConnections()
    {
        var network = new NeuralNetwork(new EyeSight(10.0, 70.0));
        var hiddenNeuron1 = new Neuron(Guid.NewGuid(), 0.25);
        var hiddenNeuron2 = new Neuron(Guid.NewGuid(), -0.5);
        network.AddNeuron(hiddenNeuron1, 1);
        network.AddNeuron(hiddenNeuron2, 2);

        var weight = -0.9;
        foreach (var sensor in network.GetNeuronsInLayer(0))
        {
            hiddenNeuron1.Synapses.Add(new Synapse(weight, sensor));
            weight += 0.1;
        }

        hiddenNeuron2.Synapses.Add(new Synapse(0.35, hiddenNeuron1));
        network.MotorNeuron.Bias = 0.75;
        network.MotorNeuron.Synapses.Add(new Synapse(-0.65, hiddenNeuron2));

        var clone = network.Clone(new EyeSight(10.0, 70.0));

        Assert.NotSame(network, clone);
        Assert.Equal(network.Neurons.Count, clone.Neurons.Count);

        foreach (var originalNeuron in network.Neurons)
        {
            var clonedNeuron = clone.GetNeuron(originalNeuron.Id);
            Assert.NotSame(originalNeuron, clonedNeuron);
            Assert.Equal(originalNeuron.Layer, clonedNeuron.Layer);

            if (originalNeuron is not INeuronWithSynapses originalWithSynapses)
            {
                continue;
            }

            var clonedWithSynapses = Assert.IsAssignableFrom<INeuronWithSynapses>(clonedNeuron);
            Assert.Equal(originalWithSynapses.Bias, clonedWithSynapses.Bias);
            Assert.Equal(originalWithSynapses.Synapses.Count, clonedWithSynapses.Synapses.Count);

            for (var i = 0; i < originalWithSynapses.Synapses.Count; i++)
            {
                var originalSynapse = originalWithSynapses.Synapses[i];
                var clonedSynapse = clonedWithSynapses.Synapses[i];
                Assert.Equal(originalSynapse.Weight, clonedSynapse.Weight);
                Assert.Equal(originalSynapse.From.Id, clonedSynapse.From.Id);
                Assert.NotSame(originalSynapse.From, clonedSynapse.From);
            }
        }
    }

    [Fact]
    public void GetValueCachesActivationUntilReset()
    {
        var neuron = new CountingNeuron();

        Assert.Equal(1.0, neuron.GetValue());
        Assert.Equal(1.0, neuron.GetValue());
        Assert.Equal(1, neuron.ActivationCount);

        neuron.Reset();

        Assert.Equal(2.0, neuron.GetValue());
        Assert.Equal(2, neuron.ActivationCount);
    }

    private sealed class CountingNeuron() : BasicNeuron(Guid.NewGuid())
    {
        public int ActivationCount { get; private set; }

        protected override double Activate()
        {
            ActivationCount++;
            return ActivationCount;
        }
    }

    private static void AssertWeightsWithinXavierLimit(
        IEnumerable<double> weights,
        int fanIn,
        int fanOut)
    {
        var limit = Math.Sqrt(6.0 / (fanIn + fanOut));
        Assert.All(weights, weight => Assert.InRange(weight, -limit, limit));
    }
}
