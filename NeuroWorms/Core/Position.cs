using System;

namespace NeuroWorms.Core
{
    public class Position(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;

        public Position Move(MoveDirection direction, int distance = 1)
        {
            return direction switch
            {
                MoveDirection.Up => new Position(X, Y - distance),
                MoveDirection.Down => new Position(X, Y + distance),
                MoveDirection.Left => new Position(X - distance, Y),
                MoveDirection.Right => new Position(X + distance, Y),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
            };
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}