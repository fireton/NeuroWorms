namespace NeuroWorms.Core.Neuro;

internal sealed class OwnBodyAvoidanceForwardSensor(BodySense bodySense)
    : BodySenseSensor(NeuroConstants.OwnBodyAvoidanceForwardSensorId, bodySense)
{
    protected override double Activate()
    {
        return BodySense.AvoidanceForward;
    }
}
