using System;

namespace NeuroWorms.Core.Helpers
{
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

        public static MoveDirection Opposite(this MoveDirection direction)
        {
            return direction switch
            {
                MoveDirection.Up => MoveDirection.Down,
                MoveDirection.Down => MoveDirection.Up,
                MoveDirection.Left => MoveDirection.Right,
                MoveDirection.Right => MoveDirection.Left,
                _ => MoveDirection.Up,
            };
        }

        public static double Angle(this MoveDirection direction) => direction switch
        {
            MoveDirection.Right => 0.0,
            MoveDirection.Up => 90.0,
            MoveDirection.Left => 180.0,
            MoveDirection.Down => 270.0,
            _ => 0.0,
        };

        public static double AngleRad(this MoveDirection direction) => direction switch
        {
            MoveDirection.Right => 0.0,
            MoveDirection.Up => Math.PI * 0.5,
            MoveDirection.Left => Math.PI,
            MoveDirection.Down => Math.PI * 1.5,
            _ => 0.0,
        };
    }
}
