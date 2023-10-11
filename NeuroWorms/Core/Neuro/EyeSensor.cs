using System;

namespace NeuroWorms.Core.Neuro
{
    internal abstract class EyeSensor : SensorNeuron
    {
        protected EyeSight EyeSight { get; }

        public EyeSensor(Guid id, EyeSight eyeSight) : base(id) 
        {
            EyeSight = eyeSight;
        }
    }
}
