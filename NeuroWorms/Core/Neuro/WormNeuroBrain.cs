using System;

namespace NeuroWorms.Core.Neuro
{
    internal class WormNeuroBrain : WormBrain
    {
        private readonly EyeSight eyeSight = new EyeSight(Constants.ViewAngle, Constants.ViewDistance);
        private readonly NeuralNetwork neuralNetwork = new NeuralNetwork();

        public override WormBrain Clone()
        {
            throw new NotImplementedException();
        }

        public override MoveDirection GetNextMove(Field field, Worm worm)
        {
            throw new NotImplementedException();
        }

        public override void Init()
        {
            InitSensorsAndMotor();
            // then we add hidden neurons, three neurons per layer
            for (var i = 0; i < 3; i++)
            {
                neuralNetwork.AddNeuron(new Neuron(Guid.NewGuid(), NeuroRnd.Next()), 1);
                neuralNetwork.AddNeuron(new Neuron(Guid.NewGuid(), NeuroRnd.Next()), 2);
            }
            // now we connect all neurons in sensor layer with all neurons in first hidden layer
            for (var i = 0; i < neuralNetwork.Layer; i++)
            {
                neuralNetwork.Connect(0, i, 1, 0);
            }

        }

        private void InitSensorsAndMotor()
        {
            // first we add sensor neurons
            neuralNetwork.AddNeuron(new EyeAngleSensor(eyeSight), 0);
            neuralNetwork.AddNeuron(new EyeDistanceSensor(eyeSight), 0);
            neuralNetwork.AddNeuron(new EyeObjectSensor(eyeSight), 0);
            // and motor neuron
            neuralNetwork.AddNeuron(new MotorNeuron(NeuroRnd.Next()), 3);
        }
    }
}
