using System.Collections.Generic;
using System.Diagnostics;
using NeuroWorms.Core.Helpers;

namespace NeuroWorms.Core.Neuro
{
    // This class solves the problem of finding nearest non-empty cells in the direction of the worm's head.
    // It uses given field of view angle and maximum distance.
    // It is used in the EyeSensors class.
    internal class EyeSight(double viewAngle, double viewDistance)
    {
        public Dictionary<ObjectType, FoundInfo> Found { get; private set; } = [];

        private bool isCalculated = false;

        public void DetectObjects(Worm worm, Field field)
        {
            if (isCalculated) return;
            isCalculated = true;

            var lookDirection = worm.CurrentDirection;
            var lookDirectionAngle = lookDirection.AngleRad();
            var startAngle = MathHelper.NormalizeAngle(MathHelper.ToRadians(lookDirection.Angle() - viewAngle / 2));
            var endAngle = MathHelper.NormalizeAngle(MathHelper.ToRadians(lookDirection.Angle() + viewAngle / 2));

            Found.Clear();

            bool AllFound() => Found.ContainsKey(ObjectType.Food) && Found.ContainsKey(ObjectType.Worm) && Found.ContainsKey(ObjectType.Wall);

            for (int curDistance = 1; curDistance <= viewDistance && !AllFound(); curDistance++)
            {
                var forwardPos = worm.Head.Move(lookDirection, curDistance);
                CheckPosition(forwardPos, lookDirectionAngle, curDistance);

                var scanSideDistance = 1;
                var scanLeftDir = lookDirection.TurnLeft();
                var scanRightDir = lookDirection.TurnRight();

                while (true)
                {
                    var leftPos = forwardPos.Move(scanLeftDir, scanSideDistance);
                    var (leftAngle, leftDist) = MathHelper.AngleAndDistance(worm.Head, leftPos);

                    var rightPos = forwardPos.Move(scanRightDir, scanSideDistance);
                    var (rightAngle, rightDist) = MathHelper.AngleAndDistance(worm.Head, rightPos);

                    if (leftDist > viewDistance && rightDist > viewDistance)
                        break;

                    CheckPosition(rightPos, rightAngle, rightDist);
                    CheckPosition(leftPos, leftAngle, leftDist);

                    if (AllFound()) break;

                    scanSideDistance++;
                }
            }

            void CheckPosition(Position pos, double angle, double dist)
            {

                var type = field[pos];
                if (type == CellType.Empty) return;

                if (AllFound()) return;

                if (!MathHelper.IsAngleBetween(angle, startAngle, endAngle)) return;

                if (type == CellType.Food && !Found.ContainsKey(ObjectType.Food))
                    Found[ObjectType.Food] = CreateFoundInfo(angle, dist);
                else if (type == CellType.Wall && !Found.ContainsKey(ObjectType.Wall))
                    Found[ObjectType.Wall] = CreateFoundInfo(angle, dist);
                else if ((type == CellType.WormHead || type == CellType.WormBody) && !Found.ContainsKey(ObjectType.Worm))
                    Found[ObjectType.Worm] = CreateFoundInfo(angle, dist);
            }

            FoundInfo CreateFoundInfo(double angle, double dist)
            {
                return new FoundInfo(dist / viewDistance * 2 - 1, MathHelper.MapAngle(angle, startAngle, endAngle));
            }
        }

        public void Reset()
        {
            isCalculated = false;
        }

        public EyeSight Clone()
        {
            return new EyeSight(viewAngle, viewDistance);
        }

        internal void PrintDebug()
        {
            foreach (var found in Found)
            {
                Debug.WriteLine($"Found {found.Key} at angle {found.Value.AngleValue} and distance {found.Value.DistanceValue}");
            }
        }
    }

    public record FoundInfo(double AngleValue, double DistanceValue);

    public enum ObjectType
    {
        Food,
        Worm,
        Wall
    }
}
