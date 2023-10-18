using System.Collections.Generic;

namespace NeuroWorms.Core
{
    public class Worm
    {
        private int growCount = 0;

        public bool IsAlive { get; set; } = true;   
        public Position Head { get; private set; }
        public List<Position> Body { get; }
        public WormBrain Brain { get; }

        public MoveDirection CurrentDirection { get; set; }

        public Worm(Position head, List<Position> body, WormBrain brain)
        {
            Head = head;
            Body = body;
            Brain = brain;
        }

        public void Move(MoveDirection direction, Field field)
        {
            var newHead = field.RoundUp(Head.Move(direction));
            field[newHead.X, newHead.Y] = CellType.WormHead;
            field[Head.X, Head.Y] = CellType.WormBody;
            Body.Insert(0, Head);
            Head = newHead;
            if (growCount == 0)
            {
                field[Body[Body.Count - 1].X, Body[Body.Count - 1].Y] = CellType.Empty;
                Body.RemoveAt(Body.Count - 1);
            }
            else
            {
                growCount--;
            }
            CurrentDirection = direction;
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
        }

        public void RenderToField(Field field)
        {
            field[Head.X, Head.Y] = CellType.WormHead;
            foreach (var bodyPart in Body)
            {
                field[bodyPart.X, bodyPart.Y] = CellType.WormBody;
            }
        }
    }
}