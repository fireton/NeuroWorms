namespace NeuroWorms.Core
{
    public abstract class WormBrain
    {
        public abstract MoveDirection GetNextMove(Field field, Worm worm);
    }
}