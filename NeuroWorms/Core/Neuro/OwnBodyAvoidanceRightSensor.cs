namespace NeuroWorms.Core.Neuro;

internal sealed class OwnBodyAvoidanceRightSensor(BodySense bodySense)
    : BodySenseSensor(NeuroConstants.OwnBodyAvoidanceRightSensorId, bodySense)
{
    protected override double Activate()
    {
        return BodySense.AvoidanceRight;
    }
}
