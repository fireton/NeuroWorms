namespace NeuroWorms.Core
{
    public abstract class WormBrain
    {
        public abstract void Init();
        public abstract WormBrain Clone();
        public abstract void Mutate();
        public abstract MoveDirection GetNextMove(Field field, Worm worm);
        public virtual void PrintDebug() 
        {
            // does nothing by default
        }
    }
}