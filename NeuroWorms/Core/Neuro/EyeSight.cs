using System;

namespace NeuroWorms.Core.Neuro
{
    // This class solves the problem of finding nearest non-empty cell in the direction of the worm's head.
    // It uses given field of view angle and maximum distance.
    // It is used in the EyeSensor class.
    internal class EyeSight
    {
        public double ViewAngle { get; }
        public double ViewDistance { get; }

        public double NearestDistance { get; private set; }
        public double NearestAngle { get; private set; }
        public ObjectType NearestType { get; private set; }

        public EyeSight(double viewAngle, double viewDistance)
        {
            ViewAngle = viewAngle;
            ViewDistance = viewDistance;
        }

        public void DetectNearestObject(Worm worm, Field field)
        {
            var lookDirection = worm.CurrentDirection;
            var startAngle = lookDirection.Angle() - ViewAngle / 2;
            var endAngle = lookDirection.Angle() + ViewAngle / 2;

            // start scan
            var curDistance = 1;
            while (curDistance <= ViewDistance)
            {
                var checkPos = worm.Head.Move(lookDirection, curDistance);
                if (field[checkPos] != CellType.Empty)
                {
                    NearestDistance = curDistance;
                    NearestAngle = 0.0; // relative to lookDirection
                    NearestType = GetObjectType(field[checkPos], worm, checkPos);
                    return;
                }
                var scanDirection = lookDirection.TurnLeft();
                
                do
                {
                    checkPos = checkPos.Move(scanDirection);
                    var checkPosAngle = ;
                }
            }


        }

        private ObjectType GetObjectType(CellType cellType, Worm worm, Position pos)
        {
            return cellType switch
            {
                CellType.Empty => ObjectType.Nothing,
                CellType.Food => ObjectType.Food,
                CellType.Wall => ObjectType.Wall,
                CellType.WormHead => ObjectType.OtherWorm,
                CellType.WormBody => worm.Body.Contains(pos) ? ObjectType.SelfWorm : ObjectType.OtherWorm,
                _ => ObjectType.Nothing
            };
        }
    }

    public enum ObjectType
    {
        Nothing,
        OtherWorm,
        SelfWorm,
        Food,
        Wall
    }
}
