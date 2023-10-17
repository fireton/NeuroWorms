namespace NeuroWorms.Core
{
    public abstract class WormBrain
    {
        public abstract void Init();
        public abstract WormBrain Clone();
        public abstract MoveDirection GetNextMove(Field field, Worm worm);
    }
}