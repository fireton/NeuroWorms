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

    [Theory]
    [InlineData(7, 3, -0.5)]
    [InlineData(7, 7, 0.5)]
    public void EyeSightAngleUsesSameLeftRightSignAsMotor(int foodX, int foodY, double expected)
    {
        var field = new Field(11, 11);
        var worm = new Worm(new Position(5, 5), [], new StupidRandomBrain())
        {
            CurrentDirection = MoveDirection.Right,
        };
        var eyeSight = new EyeSight(viewAngle: 180.0, viewDistance: 4.0);
        field[foodX, foodY] = CellType.Food;

        eyeSight.DetectObjects(worm, field);

        Assert.Equal(expected, eyeSight.Found[ObjectType.Food].AngleValue, 10);
    }

    [Fact]
    public void EyeSightSeparatesOwnBodyFromOtherWorm()
    {
        var field = new Field(11, 11);
        var worm = new Worm(
            new Position(5, 5),
            [new Position(6, 5)],
            new StupidRandomBrain())
        {
            CurrentDirection = MoveDirection.Right,
        };
        var otherWorm = new Worm(
            new Position(7, 5),
            [],
            new StupidRandomBrain());
        worm.RenderToField(field);
        otherWorm.RenderToField(field);
        var eyeSight = new EyeSight(viewAngle: 180.0, viewDistance: 4.0);

        eyeSight.DetectObjects(worm, field);

        var own = Assert.Contains(ObjectType.OwnBody, eyeSight.Found);
        Assert.Equal(0.0, own.AngleValue, 10);
        Assert.Equal(-0.5, own.DistanceValue, 10);
        var other = Assert.Contains(ObjectType.OtherWorm, eyeSight.Found);
        Assert.Equal(0.0, other.AngleValue, 10);
        Assert.Equal(0.0, other.DistanceValue, 10);
    }

    [Fact]
    public void EyeSightReportsOwnBodyWithoutReportingAnotherWorm()
    {
        var field = new Field(11, 11);
        var worm = new Worm(
            new Position(5, 5),
            [new Position(6, 5), new Position(7, 5)],
            new StupidRandomBrain())
        {
            CurrentDirection = MoveDirection.Right,
        };
        worm.RenderToField(field);
        var eyeSight = new EyeSight(viewAngle: 180.0, viewDistance: 4.0);

        eyeSight.DetectObjects(worm, field);

        var own = Assert.Contains(ObjectType.OwnBody, eyeSight.Found);
        Assert.Equal(0.0, own.AngleValue, 10);
        Assert.Equal(-0.5, own.DistanceValue, 10);
        Assert.DoesNotContain(ObjectType.OtherWorm, eyeSight.Found);
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

    [Fact]
    public void AheadObstacleSensorDetectsObstacleFoodAndEmptyCell()
    {
        var field = new Field(5, 5);
        var worm = CreateWorm();
        var sensor = new ObstacleAheadSensor();

        field[2, 1] = CellType.WormBody;
        sensor.Reset(worm, field);
        Assert.Equal(1.0, sensor.GetValue());

        field[2, 1] = CellType.Food;
        sensor.Reset(worm, field);
        Assert.Equal(-1.0, sensor.GetValue());

        field[2, 1] = CellType.Empty;
        sensor.Reset(worm, field);
        Assert.Equal(0.0, sensor.GetValue());
    }

    [Fact]
    public void CollisionStreakSensorReportsThreeDecisionStates()
    {
        var worm = CreateWorm();
        var sensor = new CollisionStreakSensor();

        sensor.Reset(worm);
        Assert.Equal(-1.0, sensor.GetValue());

        worm.RegisterCollision(DeathReason.Wall);
        sensor.Reset(worm);
        Assert.Equal(0.0, sensor.GetValue());

        worm.RegisterCollision(DeathReason.SelfBody);
        sensor.Reset(worm);
        Assert.Equal(1.0, sensor.GetValue());
    }

    [Theory]
    [InlineData(10, 8, -0.4, 0.0)]
    [InlineData(10, 12, 0.4, 0.0)]
    [InlineData(12, 10, 0.0, -0.4)]
    [InlineData(8, 10, 0.0, 0.4)]
    public void BodySenseProducesRelativeAvoidanceVector(
        int segmentX,
        int segmentY,
        double expectedForward,
        double expectedRight)
    {
        var worm = CreateBodySenseWorm([new Position(segmentX, segmentY)]);
        var bodySense = new BodySense();
        var forwardSensor = new OwnBodyAvoidanceForwardSensor(bodySense);
        var rightSensor = new OwnBodyAvoidanceRightSensor(bodySense);
        var pressureSensor = new OwnBodyPressureSensor(bodySense);

        forwardSensor.Reset(worm);
        rightSensor.Reset(worm);
        pressureSensor.Reset(worm);

        Assert.Equal(expectedForward, forwardSensor.GetValue(), 10);
        Assert.Equal(expectedRight, rightSensor.GetValue(), 10);
        Assert.Equal(0.2, pressureSensor.GetValue(), 10);
    }

    [Theory]
    [InlineData(MoveDirection.Up, 12, 10)]
    [InlineData(MoveDirection.Right, 10, 12)]
    [InlineData(MoveDirection.Down, 8, 10)]
    [InlineData(MoveDirection.Left, 10, 8)]
    public void BodySenseRightSideIsRelativeToEveryDirection(
        MoveDirection direction,
        int segmentX,
        int segmentY)
    {
        var worm = CreateBodySenseWorm([new Position(segmentX, segmentY)]);
        worm.CurrentDirection = direction;
        var bodySense = new BodySense();
        var rightSensor = new OwnBodyAvoidanceRightSensor(bodySense);

        rightSensor.Reset(worm);

        Assert.Equal(-0.4, rightSensor.GetValue(), 10);
    }

    [Fact]
    public void BodySenseIgnoresThreeSegmentsNearestTheHead()
    {
        var worm = CreateBodySenseWorm([]);
        var bodySense = new BodySense();
        var forwardSensor = new OwnBodyAvoidanceForwardSensor(bodySense);
        var rightSensor = new OwnBodyAvoidanceRightSensor(bodySense);
        var pressureSensor = new OwnBodyPressureSensor(bodySense);

        forwardSensor.Reset(worm);
        rightSensor.Reset(worm);
        pressureSensor.Reset(worm);

        Assert.Equal(0.0, forwardSensor.GetValue());
        Assert.Equal(0.0, rightSensor.GetValue());
        Assert.Equal(0.0, pressureSensor.GetValue());
    }

    [Fact]
    public void BodyPressureRemainsWhenOppositeAvoidanceVectorsCancel()
    {
        var worm = CreateBodySenseWorm([
            new Position(8, 10),
            new Position(12, 10),
        ]);
        var bodySense = new BodySense();
        var rightSensor = new OwnBodyAvoidanceRightSensor(bodySense);
        var pressureSensor = new OwnBodyPressureSensor(bodySense);

        rightSensor.Reset(worm);
        pressureSensor.Reset(worm);

        Assert.Equal(0.0, rightSensor.GetValue(), 10);
        Assert.Equal(0.4, pressureSensor.GetValue(), 10);
    }

    [Fact]
    public void FieldTracksOwnerThroughRenderingMovementAndRemoval()
    {
        var field = new Field(10, 10);
        var worm = new Worm(
            new Position(3, 3),
            [new Position(2, 3), new Position(1, 3)],
            new StupidRandomBrain())
        {
            CurrentDirection = MoveDirection.Right,
        };

        worm.RenderToField(field);

        Assert.Equal(worm.OwnerId, field.GetOwnerId(new Position(3, 3)));
        Assert.Equal(worm.OwnerId, field.GetOwnerId(new Position(2, 3)));
        Assert.Equal(worm.OwnerId, field.GetOwnerId(new Position(1, 3)));

        worm.Move(MoveDirection.Right, field);

        Assert.Equal(worm.OwnerId, field.GetOwnerId(new Position(4, 3)));
        Assert.Equal(worm.OwnerId, field.GetOwnerId(new Position(3, 3)));
        Assert.Equal(worm.OwnerId, field.GetOwnerId(new Position(2, 3)));
        Assert.Equal(Field.NoOwnerId, field.GetOwnerId(new Position(1, 3)));

        worm.RemoveFromField(field);

        Assert.Equal(Field.NoOwnerId, field.GetOwnerId(new Position(4, 3)));
        Assert.Equal(Field.NoOwnerId, field.GetOwnerId(new Position(3, 3)));
        Assert.Equal(Field.NoOwnerId, field.GetOwnerId(new Position(2, 3)));
    }

    [Fact]
    public void DifferentWormsReceiveDifferentOwnerIds()
    {
        var first = CreateWorm();
        var second = CreateWorm();

        Assert.NotEqual(Field.NoOwnerId, first.OwnerId);
        Assert.NotEqual(first.OwnerId, second.OwnerId);
    }

    private static Worm CreateWorm()
    {
        return new Worm(new Position(2, 2), [], new StupidRandomBrain())
        {
            CurrentDirection = MoveDirection.Up,
        };
    }

    private static Worm CreateBodySenseWorm(IReadOnlyList<Position> sensedSegments)
    {
        var body = new List<Position>
        {
            new(10, 11),
            new(10, 12),
            new(10, 13),
        };
        body.AddRange(sensedSegments);

        return new Worm(new Position(10, 10), body, new StupidRandomBrain())
        {
            CurrentDirection = MoveDirection.Up,
        };
    }
}
