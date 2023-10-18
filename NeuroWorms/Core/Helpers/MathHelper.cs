using System;

namespace NeuroWorms.Core.Helpers
{
    internal static class MathHelper
    {
        public static readonly double PI2 = Math.PI * 2;
        public static readonly double PIdiv2 = Math.PI / 2.0;

        private static readonly double PIdiv180 = Math.PI / 180.0;

        public static double NormalizeAngle(double angle)
        {
            while (angle < 0)
            {
                angle += PI2;
            }
            while (angle > PI2)
            {
                angle -= PI2;
            }
            return angle;
        }

        public static double ToRadians(double degrees)
        {
            return degrees * PIdiv180;
        }

        public static double ToDegrees(double radians)
        {
            return radians / PIdiv180;
        }

        public static double AngleBetween(Position a, Position b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var angle = Math.Atan2(dy, dx);
            return NormalizeAngle(angle);
        }

        public static double Distance(Position a, Position b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static (double, double) AngleAndDistance(Position a, Position b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var angle = Math.Atan2(dy, dx) + PIdiv2;
            return (NormalizeAngle(angle), Math.Sqrt(dx * dx + dy * dy));
        }

        public static bool IsAngleBetween(
            double angle, 
            double startAngle, 
            double endAngle)
        {
            if (startAngle < endAngle)
            {
                return angle >= startAngle && angle <= endAngle;
            }
            else
            {
                return angle >= startAngle || angle <= endAngle;
            }
        }

        // Function maps angle between two angles to [-1,1] range.
        // Assumes that angle is always between startAngle and endAngle.
        public static double MapAngle(double angle, double startAngle, double endAngle)
        {
            if (startAngle < endAngle)
            {
                return (angle - startAngle) / (endAngle - startAngle) * 2 - 1;
            }
            else
            {
                if (angle > endAngle)
                {
                    return (angle - startAngle) / (endAngle + PI2 - startAngle) * 2 - 1;
                }
                else
                {
                    return (angle + PI2 - startAngle) / (endAngle + PI2 - startAngle) * 2 - 1;
                }
            }
        }
    }
}
