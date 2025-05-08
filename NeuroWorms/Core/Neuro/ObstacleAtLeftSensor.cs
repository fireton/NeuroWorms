using NeuroWorms.Core.Helpers;

namespace NeuroWorms.Core.Neuro
{
    internal class ObstacleAtLeftSensor() : BasicNeuron(NeuroConstants.ObstacleAtLeftSensorId), IWormFieldResettable
    {
        private double value = 0.0;

        public void Reset(Worm worm, Field field)
        {
            var positionAtLeft = worm.Head.Move(worm.CurrentDirection.TurnLeft());
            var cellTypeAtLeft = field[positionAtLeft];
            if (cellTypeAtLeft == CellType.Wall || cellTypeAtLeft == CellType.WormBody || cellTypeAtLeft == CellType.WormHead)
            {
                value = 1.0;
            }
            else if (cellTypeAtLeft == CellType.Food)
            {
                value = -1.0;
            }
            else
            {
                value = 0.0;
            }
        }

        protected override double Activate()
        {
            return value;
        }
    }
}
