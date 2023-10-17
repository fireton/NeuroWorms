using System;
using System.Collections.Generic;

namespace NeuroWorms.Core
{
    public class StupidRandomBrain : WormBrain
    {
        private readonly Random random = new();
        private int keepGoing = 3;

        public override WormBrain Clone()
        {
           return new StupidRandomBrain();
        }

        public override MoveDirection GetNextMove(Field field, Worm worm)
        {
            if (keepGoing > 0)
            {
                keepGoing--;
                return worm.CurrentDirection;
            }
            
            var possibleDirections = new List<MoveDirection>();
            if (worm.CurrentDirection != MoveDirection.Down)
            {
                possibleDirections.Add(MoveDirection.Up);
            }
            if (worm.CurrentDirection != MoveDirection.Up)
            {
                possibleDirections.Add(MoveDirection.Down);
            }
            if (worm.CurrentDirection != MoveDirection.Right)
            {
                possibleDirections.Add(MoveDirection.Left);
            }
            if (worm.CurrentDirection != MoveDirection.Left)
            {
                possibleDirections.Add(MoveDirection.Right);
            }

            keepGoing = 3;
            return possibleDirections[random.Next(possibleDirections.Count)];
        }

        public override void Init()
        {
            // nothing to init in this brain
        }
    }
}
