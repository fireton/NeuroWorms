using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroWorms.Core.Neuro
{
    // This neuron returns new direction for a worm. It should take in account
    // current direction of a worm as worm can't turn more than 90 degrees per
    // turn.
    public class MotorNeuron : Neuron
    {
        private MoveDirection currentDirection;
        public void Reset(MoveDirection currentDirection)
        {
            this.currentDirection = currentDirection;
            base.Reset();
        }

        public MoveDirection GetDirection()
        {
            // We take GetValue and map it into MoveDirection.
            // If we move Left, possible directions are Left, Up, Down. And so on.
            // It meant that [-1,1] maps to 180 degrees.
            var value = GetValue();
            if (value < -0.3333)
            {
                return currentDirection.TurnLeft();
            }
            else if (value > 0.3333)
            {
                return currentDirection.TurnRight();
            }
            else
            {
                return currentDirection;
            }
            
        }
    }
}
