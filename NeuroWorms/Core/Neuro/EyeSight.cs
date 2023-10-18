using NeuroWorms.Core.Helpers;

namespace NeuroWorms.Core.Neuro
{
    // This class solves the problem of finding nearest non-empty cell in the direction of the worm's head.
    // It uses given field of view angle and maximum distance.
    // It is used in the EyeSensor class.
    internal class EyeSight
    {
        public double ViewAngle { get; protected set; }
        public double ViewDistance { get; }

        public double NearestDistance { get; private set; } // relative to ViewDistance, mapped to [-1,1]
        public double NearestAngle { get; private set; } // relative to look direction, mapped to [-1,1]
        public ObjectType NearestType { get; private set; }

        // object type mapped to [-1,1]
        public double NearestTypeValue => NearestType switch
        {
            ObjectType.Nothing => -1,
            ObjectType.OtherWorm => -0.5,
            ObjectType.SelfWorm => 0,
            ObjectType.Food => 0.5,
            ObjectType.Wall => 1,
            _ => -1
        };

        private bool isCalculated = false;

        public EyeSight(double viewAngle, double viewDistance)
        {
            ViewAngle = viewAngle;
            ViewDistance = viewDistance;
        }

        public void DetectNearestObject(Worm worm, Field field)
        {
            // this method is called only once per tick
            if (isCalculated)
            {
                return;
            }
            isCalculated = true;
            var lookDirection = worm.CurrentDirection;
            var startAngle = MathHelper.NormalizeAngle(MathHelper.ToRadians(lookDirection.Angle() - ViewAngle / 2));
            var endAngle = MathHelper.NormalizeAngle(MathHelper.ToRadians(lookDirection.Angle() + ViewAngle / 2));

            // start scan
            var curDistance = 1;
            while (curDistance <= ViewDistance)
            {
                var scanForwardPos = worm.Head.Move(lookDirection, curDistance);
                if (field[scanForwardPos] != CellType.Empty)
                {
                    NearestDistance = curDistance / ViewDistance * 2 - 1; // relative to ViewDistance
                    NearestAngle = 0.0; // relative to lookDirection
                    NearestType = GetObjectType(field[scanForwardPos], worm, scanForwardPos);
                    return;
                }
                /*
                var scanLeftDirection = lookDirection.TurnLeft();
                var scanRightDirection = lookDirection.TurnRight();
                var scanSideDistance = 1;

                while (true)
                {
                    var scanLeftPos = scanForwardPos.Move(scanLeftDirection, scanSideDistance);
                    (var scanLeftAngle, var scanPosDistance) = MathHelper.AngleAndDistance(worm.Head, scanLeftPos);
                    if (scanPosDistance <= ViewDistance && MathHelper.IsAngleBetween(scanLeftAngle, startAngle, endAngle))
                    {
                        if (field[scanLeftPos] != CellType.Empty)
                        {
                            NearestDistance = scanPosDistance / ViewDistance * 2 - 1;
                            NearestAngle = MathHelper.MapAngle(scanLeftAngle, startAngle, endAngle); // relative to lookDirection
                            NearestType = GetObjectType(field[scanLeftPos], worm, scanLeftPos);
                            return;
                        }
                    }
                    else
                    {
                        break;
                    }

                    var scanRightPos = scanForwardPos.Move(scanRightDirection, scanSideDistance);
                    var scanRightAngle = MathHelper.AngleBetween(worm.Head, scanRightPos);

                    // Since left and right scans are symmetrical, we can skip the check if the angle is between start and end
                    // Also the distance is the same and we know that scanPosDistance <= ViewDistance
                    if (field[scanRightPos] != CellType.Empty)
                    {
                        NearestDistance = scanPosDistance / ViewDistance * 2 - 1;
                        NearestAngle = MathHelper.MapAngle(scanRightAngle, startAngle, endAngle); // relative to lookDirection
                        NearestType = GetObjectType(field[scanRightPos], worm, scanRightPos);
                        return;
                    }
                    scanSideDistance++;
                }
                */

                curDistance++;
            }

            // No objects found
            NearestDistance = 1;
            NearestAngle = 0;
            NearestType = ObjectType.Nothing;
        }

        public void Reset()
        {
            isCalculated = false;
        }

        public EyeSight Clone()
        {
            return new EyeSight(ViewAngle, ViewDistance);
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
