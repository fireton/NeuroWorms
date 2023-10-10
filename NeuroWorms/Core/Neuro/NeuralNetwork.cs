namespace NeuroWorms.Core.Neuro
{
    internal class NeuralNetwork
    {
        private readonly NeuronLayer[] layers = new NeuronLayer[4];

        public NeuralNetwork()
        {
            layers[0] = new NeuronLayer(LayerType.Sensor);
            layers[1] = new NeuronLayer(LayerType.Hidden);
            layers[2] = new NeuronLayer(LayerType.Hidden);
            layers[3] = new NeuronLayer(LayerType.Motor);
        }

        public void AddNeuron(BasicNeuron neuron, int layer)
        {
            layers[layer].AddNeuron(neuron);
        }

        public void Reset(Worm worm, Field field)
        {
            foreach (var layer in layers)
            {
                layer.Reset(worm, field);
            }
        }
    }
}
