namespace NeuroWorms.Core.Neuro;

internal sealed class OwnBodyPressureSensor(BodySense bodySense)
    : BodySenseSensor(NeuroConstants.OwnBodyPressureSensorId, bodySense)
{
    protected override double Activate()
    {
        return BodySense.Pressure;
    }
}
