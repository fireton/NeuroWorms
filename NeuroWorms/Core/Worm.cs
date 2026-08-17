using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeuroWorms.Core;

public class Worm(Position head, List<Position> body, WormBrain brain)
{
    private int growCount = 0;

    public DeathReason DeathReason { get; set; } = DeathReason.None;
    public bool IsAlive => DeathReason == DeathReason.None;
    public Position Head { get; private set; } = head;
    public List<Position> Body { get; } = body;
    public WormBrain Brain { get; } = brain;
    public int Age { get; set; } = 0;
    public int Hunger { get; set; } = 0;
    public int FoodEaten { get; private set; } = 0;
    public int ConsecutiveCollisions { get; private set; } = 0;
    public int WallCollisions { get; private set; } = 0;
    public int WormBodyCollisions { get; private set; } = 0;
    public int TotalCollisions => WallCollisions + WormBodyCollisions;
    public int Length => Body.Count + 1;

    public readonly Guid Id = Guid.NewGuid();

    public MoveDirection CurrentDirection { get; set; }

    public void Move(MoveDirection direction, Field field)
    {
        var newHead = field.RoundUp(Head.Move(direction));
        field[newHead.X, newHead.Y] = CellType.WormHead;
        field[Head.X, Head.Y] = CellType.WormBody;
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
            case DeathReason.WormBody:
                WormBodyCollisions++;
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
        field[Head.X, Head.Y] = CellType.WormHead;
        foreach (var bodyPart in Body)
        {
            field[bodyPart.X, bodyPart.Y] = CellType.WormBody;
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
        Hunger++;
    }
}

public enum DeathReason
{
    None = 0,
    Hunger = 1,
    WormBody = 2,
    Wall = 3,
}
