using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeuroWorms.Core
{
    public class Worm(Position head, List<Position> body, WormBrain brain)
    {
        private int growCount = 0;

        public bool IsAlive { get; private set; } = true;
        public Position Head { get; private set; } = head;
        public List<Position> Body { get; } = body;
        public WormBrain Brain { get; } = brain;
        public int Age { get; set; } = 0;
        public int Hunger { get; set; } = 0;

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
            Age++;
            Hunger++;
        }

        public void Die()
        {
            IsAlive = false;
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
            Debug.WriteLine($"Worm with age {Age} and length {Body.Count} to direction {CurrentDirection}");
            Brain.PrintDebug();
        }
    }
}