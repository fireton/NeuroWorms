using NeuroWorms.Core;
using NeuroWorms.Core.Neuro;

namespace NeuroWorms.Tests;

public class SensorTests
{
    [Theory]
    [InlineData(MoveDirection.Up, 5, 3)]
    [InlineData(MoveDirection.Down, 5, 7)]
    [InlineData(MoveDirection.Left, 3, 5)]
    [InlineData(MoveDirection.Right, 7, 5)]
    public void EyeSightDetectsObjectsStraightAheadInEveryDirection(
        MoveDirection direction,
        int foodX,
        int foodY)
    {
        var field = new Field(11, 11);
        var worm = new Worm(new Position(5, 5), [], new StupidRandomBrain())
        {
            CurrentDirection = direction,
        };
        var eyeSight = new EyeSight(viewAngle: 180.0, viewDistance: 4.0);
        field[foodX, foodY] = CellType.Food;

        eyeSight.DetectObjects(worm, field);

        var food = Assert.Contains(ObjectType.Food, eyeSight.Found);
        Assert.Equal(0.0, food.AngleValue, 10);
        Assert.Equal(0.0, food.DistanceValue, 10);
    }

    [Theory]
    [InlineData(MoveDirection.Up)]
    [InlineData(MoveDirection.Down)]
    [InlineData(MoveDirection.Left)]
    [InlineData(MoveDirection.Right)]
    public void EyeSightScansEveryCellInTheFrontHalfDisk(MoveDirection direction)
    {
        const int radius = 3;
        var head = new Position(5, 5);

        for (var deltaX = -radius; deltaX <= radius; deltaX++)
        {
            for (var deltaY = -radius; deltaY <= radius; deltaY++)
            {
                var distanceSquared = deltaX * deltaX + deltaY * deltaY;
                if (distanceSquared == 0 || distanceSquared > radius * radius)
                {
                    continue;
                }

                var forwardDistance = direction switch
                {
                    MoveDirection.Right => deltaX,
                    MoveDirection.Up => -deltaY,
                    MoveDirection.Left => -deltaX,
                    MoveDirection.Down => deltaY,
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
                };
                var shouldBeVisible = forwardDistance >= 0;
                var field = new Field(11, 11);
                field[head.X + deltaX, head.Y + deltaY] = CellType.Food;
                var worm = new Worm(head, [], new StupidRandomBrain())
                {
                    CurrentDirection = direction,
                };
                var eyeSight = new EyeSight(viewAngle: 180.0, viewDistance: radius);

                eyeSight.DetectObjects(worm, field);

                Assert.Equal(shouldBeVisible, eyeSight.Found.ContainsKey(ObjectType.Food));
            }
        }
    }

    [Fact]
    public void EyeSightReturnsNearestObjectInsteadOfFirstScanLineMatch()
    {
        var field = new Field(151, 151);
        var worm = new Worm(new Position(75, 75), [], new StupidRandomBrain())
        {
            CurrentDirection = MoveDirection.Right,
        };
        var eyeSight = new EyeSight(viewAngle: 180.0, viewDistance: 70.0);
        field[76, 135] = CellType.Food;
        field[77, 75] = CellType.Food;

        eyeSight.DetectObjects(worm, field);

        var food = Assert.Contains(ObjectType.Food, eyeSight.Found);
        Assert.Equal(0.0, food.AngleValue, 10);
        Assert.Equal(2.0 / 70.0 * 2.0 - 1.0, food.DistanceValue, 10);
    }

    [Fact]
    public void ProductionEyeSightScanPatternHasNoHolesOrDuplicates()
    {
        var eyeSight = new EyeSight(Constants.ViewAngle, Constants.ViewDistance);
        var actualOffsets = eyeSight.ScanCellOffsets.ToList();
        var actualSet = actualOffsets.ToHashSet();
        var expectedSet = new HashSet<(int Forward, int Left)>();
        var radius = (int)Constants.ViewDistance;
        var maxDistanceSquared = Constants.ViewDistance * Constants.ViewDistance;

        for (var forward = 0; forward <= radius; forward++)
        {
            for (var left = -radius; left <= radius; left++)
            {
                var distanceSquared = forward * forward + left * left;
                if (distanceSquared > 0 && distanceSquared <= maxDistanceSquared)
                {
                    expectedSet.Add((forward, left));
                }
            }
        }

        Assert.Equal(actualSet.Count, actualOffsets.Count);
        Assert.True(
            expectedSet.SetEquals(actualSet),
            $"Expected {expectedSet.Count} scan cells, but found {actualSet.Count}.");
    }

    [Fact]
    public void DirectionSensorsRefreshAfterReset()
    {
        var worm = CreateWorm();
        var xSensor = new DirectionXSensor();
        var ySensor = new DirectionYSensor();

        worm.CurrentDirection = MoveDirection.Right;
        xSensor.Reset(worm);
        ySensor.Reset(worm);
        Assert.Equal(1.0, xSensor.GetValue(), 10);
        Assert.Equal(0.0, ySensor.GetValue(), 10);

        worm.CurrentDirection = MoveDirection.Up;
        xSensor.Reset(worm);
        ySensor.Reset(worm);
        Assert.Equal(0.0, xSensor.GetValue(), 10);
        Assert.Equal(1.0, ySensor.GetValue(), 10);
    }

    [Fact]
    public void RightObstacleSensorLooksRightAndRefreshesAfterReset()
    {
        var field = new Field(5, 5);
        var worm = CreateWorm();
        var sensor = new ObstacleAtRightSensor();

        field[3, 2] = CellType.Wall;
        sensor.Reset(worm, field);
        Assert.Equal(1.0, sensor.GetValue());

        field[3, 2] = CellType.Empty;
        field[1, 2] = CellType.Wall;
        sensor.Reset(worm, field);
        Assert.Equal(0.0, sensor.GetValue());
    }

    [Fact]
    public void LeftObstacleSensorRefreshesAfterReset()
    {
        var field = new Field(5, 5);
        var worm = CreateWorm();
        var sensor = new ObstacleAtLeftSensor();

        field[1, 2] = CellType.Wall;
        sensor.Reset(worm, field);
        Assert.Equal(1.0, sensor.GetValue());

        field[1, 2] = CellType.Empty;
        sensor.Reset(worm, field);
        Assert.Equal(0.0, sensor.GetValue());
    }

    private static Worm CreateWorm()
    {
        return new Worm(new Position(2, 2), [], new StupidRandomBrain())
        {
            CurrentDirection = MoveDirection.Up,
        };
    }
}
