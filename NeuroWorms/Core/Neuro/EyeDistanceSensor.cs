using System;

namespace NeuroWorms.Core.Neuro;

internal class EyeDistanceSensor(Guid id, EyeSight eyeSight, ObjectType objectType) : EyeSensor(id, eyeSight)
{
    protected override double Activate()
    {
        EyeSight.DetectObjects(Worm, Field);
        return EyeSight.Found.TryGetValue(objectType, out var info) ? info.DistanceValue : 0.0;
    }
}
