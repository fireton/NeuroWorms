namespace NeuroWorms.Core.Neuro
{
    public abstract class SensorNeuron : BasicNeuron
    {
        protected Worm Worm { get; set; }
        protected Field Field { get; set; }
        
        public void Reset(Worm worm, Field field)
        {
            Worm = worm;
            Field = field;
            base.Reset();
        }
    }
}
