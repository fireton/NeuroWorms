namespace NeuroWorms.Core
{
    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class MoveDirectionExtensions
    {
        public static MoveDirection TurnLeft(this MoveDirection direction)
        {
            return direction switch
            {
                MoveDirection.Up => MoveDirection.Left,
                MoveDirection.Down => MoveDirection.Right,
                MoveDirection.Left => MoveDirection.Down,
                MoveDirection.Right => MoveDirection.Up,
                _ => MoveDirection.Up,
            };
        }

        public static MoveDirection TurnRight(this MoveDirection direction)
        {
            return direction switch
            {
                MoveDirection.Up => MoveDirection.Right,
                MoveDirection.Down => MoveDirection.Left,
                MoveDirection.Left => MoveDirection.Up,
                MoveDirection.Right => MoveDirection.Down,
                _ => MoveDirection.Up,
            };
        }
    }
}
