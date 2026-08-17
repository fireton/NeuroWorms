using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeuroWorms.Core.Neuro
{
    // Finds the nearest object of each type inside the worm's field of view.
    // Integer cell offsets are precomputed and ordered in distance shells so
    // every cell in the sector is visited from nearest to farthest.
    internal class EyeSight
    {
        private const int ObjectTypeCount = 3;
        private static readonly ConcurrentDictionary<ScanPattern, ScanOffset[]> ScanPatterns = new();

        private readonly double viewAngle;
        private readonly double viewDistance;
        private readonly ScanOffset[] scanOffsets;

        public EyeSight(double viewAngle, double viewDistance)
        {
            if (!double.IsFinite(viewAngle) || viewAngle <= 0.0 || viewAngle > 360.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viewAngle),
                    "View angle must be greater than 0 and at most 360 degrees.");
            }

            if (!double.IsFinite(viewDistance) || viewDistance <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viewDistance),
                    "View distance must be greater than 0.");
            }

            this.viewAngle = viewAngle;
            this.viewDistance = viewDistance;
            scanOffsets = ScanPatterns.GetOrAdd(
                new ScanPattern(viewAngle, viewDistance),
                static pattern => CreateScanOffsets(pattern.ViewAngle, pattern.ViewDistance));
        }

        public Dictionary<ObjectType, FoundInfo> Found { get; private set; } = [];

        private bool isCalculated;

        public void DetectObjects(Worm worm, Field field)
        {
            if (isCalculated)
            {
                return;
            }

            isCalculated = true;
            Found.Clear();

            foreach (var offset in scanOffsets)
            {
                var position = ApplyOffset(worm.Head, worm.CurrentDirection, offset);
                var type = field[position];

                if (type == CellType.Food && !Found.ContainsKey(ObjectType.Food))
                {
                    Found[ObjectType.Food] = offset.FoundInfo;
                }
                else if (type == CellType.Wall && !Found.ContainsKey(ObjectType.Wall))
                {
                    Found[ObjectType.Wall] = offset.FoundInfo;
                }
                else if ((type == CellType.WormHead || type == CellType.WormBody) &&
                         !Found.ContainsKey(ObjectType.Worm))
                {
                    Found[ObjectType.Worm] = offset.FoundInfo;
                }

                if (Found.Count == ObjectTypeCount)
                {
                    break;
                }
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

        internal IEnumerable<(int Forward, int Left)> ScanCellOffsets =>
            scanOffsets.Select(offset => (offset.Forward, offset.Left));

        internal void PrintDebug()
        {
            foreach (var found in Found)
            {
                Debug.WriteLine(
                    $"Found {found.Key} at angle {found.Value.AngleValue} and distance {found.Value.DistanceValue}");
            }
        }

        private static ScanOffset[] CreateScanOffsets(double viewAngle, double viewDistance)
        {
            var radius = (int)Math.Ceiling(viewDistance);
            var maxDistanceSquared = viewDistance * viewDistance;
            var halfViewAngleRadians = viewAngle * Math.PI / 360.0;
            const double angleTolerance = 1e-12;
            var offsets = new List<ScanOffset>();

            for (var forward = -radius; forward <= radius; forward++)
            {
                for (var left = -radius; left <= radius; left++)
                {
                    var distanceSquared = forward * forward + left * left;
                    if (distanceSquared == 0 || distanceSquared > maxDistanceSquared)
                    {
                        continue;
                    }

                    var relativeAngle = Math.Atan2(left, forward);
                    if (Math.Abs(relativeAngle) > halfViewAngleRadians + angleTolerance)
                    {
                        continue;
                    }

                    var distance = Math.Sqrt(distanceSquared);
                    offsets.Add(new ScanOffset(
                        forward,
                        left,
                        distanceSquared,
                        new FoundInfo(
                            -relativeAngle / halfViewAngleRadians,
                            distance / viewDistance * 2.0 - 1.0)));
                }
            }

            return offsets
                .OrderBy(offset => offset.DistanceSquared)
                .ThenBy(offset => Math.Abs(offset.FoundInfo.AngleValue))
                .ThenBy(offset => offset.FoundInfo.AngleValue)
                .ToArray();
        }

        private static Position ApplyOffset(
            Position head,
            MoveDirection direction,
            ScanOffset offset)
        {
            return direction switch
            {
                MoveDirection.Right => new Position(
                    head.X + offset.Forward,
                    head.Y - offset.Left),
                MoveDirection.Up => new Position(
                    head.X - offset.Left,
                    head.Y - offset.Forward),
                MoveDirection.Left => new Position(
                    head.X - offset.Forward,
                    head.Y + offset.Left),
                MoveDirection.Down => new Position(
                    head.X + offset.Left,
                    head.Y + offset.Forward),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
            };
        }

        private readonly record struct ScanPattern(double ViewAngle, double ViewDistance);

        private readonly record struct ScanOffset(
            int Forward,
            int Left,
            int DistanceSquared,
            FoundInfo FoundInfo);
    }

    public record FoundInfo(double AngleValue, double DistanceValue);

    public enum ObjectType
    {
        Food,
        Worm,
        Wall
    }
}
