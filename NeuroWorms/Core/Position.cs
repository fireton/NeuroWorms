using System;

namespace NeuroWorms.Core
{
    public class Position
    {
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position Move(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Up:
                    return new Position(X, Y - 1);
                case MoveDirection.Down:
                    return new Position(X, Y + 1);
                case MoveDirection.Left:
                    return new Position(X - 1, Y);
                case MoveDirection.Right:
                    return new Position(X + 1, Y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}