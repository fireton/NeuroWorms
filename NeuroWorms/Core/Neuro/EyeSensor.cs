using System;

namespace NeuroWorms.Core.Neuro
{
    internal abstract class EyeSensor : BasicNeuron, IWormFieldResettable
    {
        protected Worm Worm { get; private set; }
        protected Field Field { get; private set; }


        protected EyeSight EyeSight { get; }

        protected EyeSensor(Guid id, EyeSight eyeSight) : base(id) 
        {
            EyeSight = eyeSight;
        }

        public void Reset(Worm worm, Field field)
        {
            Worm = worm;
            Field = field;
            Reset();
        }
    }
}
