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

        public Position Move(MoveDirection direction, int distance = 1)
        {
            switch (direction)
            {
                case MoveDirection.Up:
                    return new Position(X, Y - distance);
                case MoveDirection.Down:
                    return new Position(X, Y + distance);
                case MoveDirection.Left:
                    return new Position(X - distance, Y);
                case MoveDirection.Right:
                    return new Position(X + distance, Y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}