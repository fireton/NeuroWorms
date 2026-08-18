using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace NeuroWorms.Core;

public class Worm(Position head, List<Position> body, WormBrain brain)
{
    private static int nextOwnerId;
    private int growCount = 0;

    public DeathReason DeathReason { get; set; } = DeathReason.None;
    public bool IsAlive => DeathReason == DeathReason.None;
    public Position Head { get; private set; } = head;
    public List<Position> Body { get; } = body;
    public WormBrain Brain { get; } = brain;
    public int Age { get; set; } = 0;
    public double Hunger { get; set; } = 0.0;
    public int FoodEaten { get; private set; } = 0;
    public int ConsecutiveCollisions { get; private set; } = 0;
    public int WallCollisions { get; private set; } = 0;
    public int SelfBodyCollisions { get; private set; } = 0;
    public int OtherWormCollisions { get; private set; } = 0;
    public int WormBodyCollisions => SelfBodyCollisions + OtherWormCollisions;
    public int TotalCollisions => WallCollisions + SelfBodyCollisions + OtherWormCollisions;
    public int Length => Body.Count + 1;
    public int OwnerId { get; } = Interlocked.Increment(ref nextOwnerId);

    public readonly Guid Id = Guid.NewGuid();

    public MoveDirection CurrentDirection { get; set; }

    public void Move(MoveDirection direction, Field field)
    {
        var newHead = field.RoundUp(Head.Move(direction));
        field.SetWormCell(newHead, CellType.WormHead, OwnerId);
        field.SetWormCell(Head, CellType.WormBody, OwnerId);
        Body.Insert(0, Head);
        Head = newHead;
        if (growCount == 0)
        {
            field[Body[^1].X, Body[^1].Y] = CellType.Empty;
            Body.RemoveAt(Body.Count - 1);
        }
        else
        {
            growCount--;
        }
        CurrentDirection = direction;
        ConsecutiveCollisions = 0;
        AdvanceTime();
    }

    public void RegisterCollision(DeathReason collisionType)
    {
        switch (collisionType)
        {
            case DeathReason.Wall:
                WallCollisions++;
                break;
            case DeathReason.SelfBody:
                SelfBodyCollisions++;
                break;
            case DeathReason.OtherWorm:
                OtherWormCollisions++;
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(collisionType),
                    collisionType,
                    "A collision must be either a wall or a worm body collision.");
        }

        ConsecutiveCollisions++;
        AdvanceTime();
    }

    public void Die(DeathReason deathReason)
    {
        DeathReason = deathReason;
    }

    public void RemoveFromField(Field field)
    {
        field[Head.X, Head.Y] = CellType.Empty;
        foreach (var bodyPart in Body)
        {
            field[bodyPart.X, bodyPart.Y] = CellType.Empty;
        }
    }

    public void Eat(int nutrition = 1)
    {
        growCount += nutrition;
        Hunger = 0;
        FoodEaten++;
    }

    public void RenderToField(Field field)
    {
        field.SetWormCell(Head, CellType.WormHead, OwnerId);
        foreach (var bodyPart in Body)
        {
            field.SetWormCell(bodyPart, CellType.WormBody, OwnerId);
        }
    }

    public void PrintDebug()
    {
        Debug.WriteLine(
            $"Worm with age {Age}, length {Length}, collisions {TotalCollisions} to direction {CurrentDirection}");
        Brain.PrintDebug();
    }

    private void AdvanceTime()
    {
        Age++;
        Hunger += Constants.BaseHungerPerTick
            + (Length - 1) * Constants.HungerPerExtraSegment;
    }
}

public enum DeathReason
{
    None = 0,
    Hunger = 1,
    SelfBody = 2,
    OtherWorm = 3,
    Wall = 4,
}
