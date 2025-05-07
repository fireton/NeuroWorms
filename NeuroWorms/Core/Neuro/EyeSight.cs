using System;
using System.Diagnostics;
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
            ObjectType.Nothing => -1.0,
            ObjectType.OtherWorm => 0.7,
            ObjectType.SelfWorm => 0.7,
            ObjectType.Food => 0,
            ObjectType.Wall => 1.0,
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
            if (isCalculated) return;
            isCalculated = true;

            var lookDirection = worm.CurrentDirection;
            var startAngle = MathHelper.NormalizeAngle(MathHelper.ToRadians(lookDirection.Angle() - ViewAngle / 2));
            var endAngle = MathHelper.NormalizeAngle(MathHelper.ToRadians(lookDirection.Angle() + ViewAngle / 2));

            for (int curDistance = 1; curDistance <= ViewDistance; curDistance++)
            {
                var forwardPos = worm.Head.Move(lookDirection, curDistance);
                if (field[forwardPos] != CellType.Empty)
                {
                    NearestDistance = curDistance / ViewDistance * 2 - 1;
                    NearestAngle = 0.0;
                    NearestType = GetObjectType(field[forwardPos], worm, forwardPos);
                    return;
                }

                var scanSideDistance = 1;
                var scanLeftDir = lookDirection.TurnLeft();
                var scanRightDir = lookDirection.TurnRight();

                while (true)
                {
                    // LEFT
                    var leftPos = forwardPos.Move(scanLeftDir, scanSideDistance);
                    (var leftAngle, var leftDist) = MathHelper.AngleAndDistance(worm.Head, leftPos);
                    if (leftDist <= ViewDistance && MathHelper.IsAngleBetween(leftAngle, startAngle, endAngle))
                    {
                        if (field[leftPos] != CellType.Empty)
                        {
                            NearestDistance = leftDist / ViewDistance * 2 - 1;
                            NearestAngle = MathHelper.MapAngle(leftAngle, startAngle, endAngle);
                            NearestType = GetObjectType(field[leftPos], worm, leftPos);
                            return;
                        }
                    }
                    else break;

                    // RIGHT
                    var rightPos = forwardPos.Move(scanRightDir, scanSideDistance);
                    (var rightAngle, var rightDist) = MathHelper.AngleAndDistance(worm.Head, rightPos);
                    if (rightDist <= ViewDistance && MathHelper.IsAngleBetween(rightAngle, startAngle, endAngle))
                    {
                        if (field[rightPos] != CellType.Empty)
                        {
                            NearestDistance = rightDist / ViewDistance * 2 - 1;
                            NearestAngle = MathHelper.MapAngle(rightAngle, startAngle, endAngle);
                            NearestType = GetObjectType(field[rightPos], worm, rightPos);
                            return;
                        }
                    }
                    else break;

                    scanSideDistance++;
                }
            }

            // Ничего не найдено
            NearestDistance = -1; // дальняя точка зрения
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

        internal void PrintDebug()
        {
            if (NearestType == ObjectType.Nothing)
            {
                Debug.WriteLine("Nothing detected");
            }
            else
            {
                Debug.WriteLine($"Detected {NearestType} at distance {NearestDistance} and angle {NearestAngle}");
            }
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
