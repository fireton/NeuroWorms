using System;

namespace NeuroWorms.Core.Neuro
{
    public abstract class SensorNeuron : BasicNeuron
    {
        protected SensorNeuron(Guid id) : base(id) { }

        protected Worm Worm { get; private set; }
        protected Field Field { get; private set; }
        
        public void Reset(Worm worm, Field field)
        {
            Worm = worm;
            Field = field;
            Reset();
        }
    }
}
