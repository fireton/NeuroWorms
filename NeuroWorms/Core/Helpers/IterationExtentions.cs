using System;

namespace NeuroWorms.Core.Helpers;

public static class IntExtensions
{
    public static void Times(this int count, Action action)
    {
        for (int i = 0; i < count; i++) action();
    }
}