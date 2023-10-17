using System;

namespace NeuroWorms.Core.Neuro
{
    internal static class NeuroRnd
    {
        private static readonly Random random = new();
        public static double Next()
        {
            return random.NextDouble() * 2 - 1;
        }

        public static double Jitter(double value)
        {
            return Math.Min(1.0, Math.Max(-1, value + Next()));
        }
    }
}
