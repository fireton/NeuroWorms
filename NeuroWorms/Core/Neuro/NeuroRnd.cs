using System;

namespace NeuroWorms.Core.Neuro;

internal static class NeuroRnd
{
    private static readonly Random random = new();
    public static double Next(double strength = 1.0)
    {
        return random.NextDouble() * 2 * strength - strength;
    }

    public static int Next(int from, int to)
    {
        return random.Next(from, to);
    }

    public static double Jitter(double value, double strength)
    {
        return Math.Min(1.0, Math.Max(-1, value + Next(strength)));
    }

    public static double GaussianJitter(double value, double stdDev = 0.1)
    {
        // Box-Muller transform: создаёт нормально распределённое значение с μ=0, σ=1
        double u1 = 1.0 - random.NextDouble(); // гарантирует, что не будет лог(0)
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        // смещаем value на randStdNormal * stdDev
        double result = value + randStdNormal * stdDev;

        // обрезаем до [-1, 1]
        return Math.Max(-1.0, Math.Min(1.0, result));
    }

    public static double NextDouble()
    {
        return random.NextDouble();
    }
}
