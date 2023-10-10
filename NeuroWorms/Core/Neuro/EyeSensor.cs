using System;

namespace NeuroWorms.Core.Neuro
{
    internal class EyeSensor : SensorNeuron
    {
        public EyeSensor(Guid id) : base(id) { }

        protected override double Activate()
        {
            throw new NotImplementedException();
        }
    }
}
