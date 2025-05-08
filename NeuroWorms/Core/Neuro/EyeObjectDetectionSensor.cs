using System;

namespace NeuroWorms.Core.Neuro;

internal class EyeObjectDetectionSensor(Guid id, EyeSight eyeSight, ObjectType objectType) : EyeSensor(id, eyeSight)
{
    protected override double Activate()
    {
        EyeSight.DetectObjects(Worm, Field);
        return EyeSight.Found.ContainsKey(objectType) ? 1.0 : 0.0;
    }
}
