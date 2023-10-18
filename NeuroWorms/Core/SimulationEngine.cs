using NeuroWorms.Core.Helpers;
using NeuroWorms.Core.Neuro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeuroWorms.Core
{
    public class SimulationEngine
    {
        public Field Field { get; }
        public List<Worm> Worms { get; private set; }
        public int CurrentGeneration { get; private set; } = 0;
        public int CurrentTick { get; private set; } = 0;
        public int LongestWorm { get; private set; } = 0;
        public List<Worm> GenerationLongestWorms { get; private set; } = new List<Worm>();

        private readonly Random random = new Random();

        public SimulationEngine()
        {
            Field = new Field(Constants.FieldWidth, Constants.FieldHeight);
            Worms = new List<Worm>();
            InitWorms();
            InitFood();
        }

        public Task NextMove()
        {
            if (Worms.Count == 0 || CurrentTick > Constants.MaxGenerationTicks)
            {
                NextGeneration();
            }

            for (int i = 0; i < Worms.Count; i++)
            {
                var worm = Worms[i];
                var nextMove = worm.Brain.GetNextMove(Field, worm);
                // var nextHead = Field.RoundUp(worm.Head.Move(nextMove));
                var nextHead = worm.Head.Move(nextMove);
                var nexCellType = Field[nextHead.X, nextHead.Y];

                switch (nexCellType)
                {
                    case CellType.Empty:
                        worm.Move(nextMove, Field);
                        break;
                    case CellType.Food:
                        worm.Eat(Constants.FoodNutrition);
                        worm.Move(nextMove, Field);
                        GenerateNewFood();
                        break;
                    case CellType.WormBody:
                    case CellType.WormHead:
                    case CellType.Wall:
                        worm.IsAlive = false;
                        worm.RemoveFromField(Field);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Worms.RemoveAll(w => !w.IsAlive);
            GenerationLongestWorms.AddRange(Worms);
            GenerationLongestWorms.Sort((w1, w2) => w2.Body.Count.CompareTo(w1.Body.Count));
            GenerationLongestWorms = GenerationLongestWorms.GetRange(0, Math.Min(Constants.NumberOfParents, GenerationLongestWorms.Count));
            UpdateLongestWorm();
            CurrentTick++;
            return Task.CompletedTask;
        }

        public async Task RunTillNextGeneration()
        {
            var currentGeneration = CurrentGeneration;
            while (CurrentGeneration == currentGeneration)
            {
                await NextMove();
            }
        }

        private void NextGeneration()
        {
            Field.Clear();
            var newWorms = new List<Worm>();
            var parentsCount = Math.Min(Constants.NumberOfParents, GenerationLongestWorms.Count);
            for (var j = 0; j < parentsCount; j++)
            {
                var parent = GenerationLongestWorms[j];
                for (var i = 0; i < Constants.ChildrenPerGeneration; i++)
                {
                    var brain = parent.Brain.Clone();

                    if (random.NextDouble() < Constants.MutationChance)
                    {
                        brain.Mutate();
                    }

                    var worm = CreateWormOnField(brain);
                    newWorms.Add(worm);
                }
            }
            Worms = newWorms;
            InitFood();
            GenerationLongestWorms.Clear();
            CurrentTick = 0;
            CurrentGeneration++;
        }

        private void InitFood()
        {
            for (var i = 0; i < Constants.MaxFoodCount; i++)
            {
                GenerateNewFood();
            }
        }

        private void GenerateNewFood()
        {
            Position position;
            do
            {
                position = new Position(random.Next(Constants.FieldWidth), random.Next(Constants.FieldHeight));
            } while (Field[position.X, position.Y] != CellType.Empty);

            Field[position.X, position.Y] = CellType.Food;
        }

        private void InitWorms()
        {
            for (var i = 0; i < Constants.StartWormCount; i++)
            {
                var brain = new WormNeuroBrain();
                brain.Init();
                var worm = CreateWormOnField(brain);
                Worms.Add(worm);
            }
        }

        private Worm CreateWormOnField(WormBrain brain)
        {
            Position head;
            List<Position> body;
            MoveDirection buildDirection;

            do
            {
                head = new Position(random.Next(Constants.FieldWidth), random.Next(Constants.FieldHeight));
                buildDirection = (MoveDirection)random.Next(4);

                body = new List<Position>();
                for (var j = 0; j < Constants.WormStartLength; j++)
                {
                    var newPiece = body.Count == 0 ? head.Move(buildDirection) : body[body.Count - 1].Move(buildDirection);
                    body.Add(newPiece);
                }
            } while (Field[head.X, head.Y] != CellType.Empty || body.Exists(p => Field[p.X, p.Y] != CellType.Empty));

            var worm = new Worm(head, body, brain)
            {
                CurrentDirection = buildDirection.Opposite()
            };

            worm.RenderToField(Field);
            return worm;
        }

        private void UpdateLongestWorm()
        {
            foreach (var worm in Worms)
            {
                var wormLength = worm.Body.Count + 1;
                if (wormLength > LongestWorm)
                {
                    LongestWorm = wormLength;
                }
            }
        }
    }
}
