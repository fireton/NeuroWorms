using NeuroWorms.Core.Helpers;

namespace NeuroWorms.Core.Neuro
{
    internal class ObstacleAtRightSensor() : BasicNeuron(NeuroConstants.ObstacleAtRightSensorId), IWormFieldResettable
    {
        private double value = 0.0;

        public void Reset(Worm worm, Field field)
        {
            var positionAtRight = worm.Head.Move(worm.CurrentDirection.TurnLeft());
            var cellTypeAtRight = field[positionAtRight];
            if (cellTypeAtRight == CellType.Wall || cellTypeAtRight == CellType.WormBody || cellTypeAtRight == CellType.WormHead)
            {
                value = 1.0;
            }
            else if (cellTypeAtRight == CellType.Food)
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
