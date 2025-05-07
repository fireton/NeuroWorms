//#define NEEDDEBUGWORM
using NeuroWorms.Core.Helpers;
using NeuroWorms.Core.Neuro;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NeuroWorms.Core
{
    public class SimulationEngine
    {
        public Field Field { get; }
        public List<Worm> Worms { get; private set; }
        public int CurrentGeneration { get; private set; } = 0;
        public int CurrentTick { get; private set; } = 0;
        public int LongestWorm { get; private set; } = 0;
        public int LongestAge { get; private set; } = 0;
        public int AliveWormsCount => Worms?.Count(w => w.IsAlive) ?? 0;

        private readonly Random random = new Random();
        #if NEEDDEBUGWORM
        private Worm debugWorm;
        #endif

        private int foodTicks = 0;

        public SimulationEngine()
        {
            Field = new Field(Constants.FieldWidth, Constants.FieldHeight);
            Worms = [];
            InitWorms();
            InitFood();
        }

        public Task NextMove()
        {
            var aliveWorms = Worms.FindAll(w => w.IsAlive);

            if (aliveWorms.Count == 0 || CurrentTick > Constants.MaxGenerationTicks)
            {
                NextGeneration();
            }

            foreach (var worm in aliveWorms)
            {
                var nextMove = worm.Brain.GetNextMove(Field, worm);
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
                        worm.Die();
#if NEEDDEBUGWORM
                        if (worm == debugWorm)
                        {
                            Debug.WriteLine($"Debug worm died of {nexCellType}");
                        }
#endif
                        worm.RemoveFromField(Field);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown cell type");
                }
                if (worm.IsAlive && worm.Hunger > Constants.MaxHunger)
                {
                    worm.Die();
#if NEEDDEBUGWORM
                    if (worm == debugWorm)
                    {
                        Debug.WriteLine($"Debug worm died of hunger");
                    }
#endif
                    worm.RemoveFromField(Field);
                }
                LongestWorm = Math.Max(LongestWorm, worm.Body.Count + 1);
            }
            
            CurrentTick++;
            LongestAge = Math.Max(LongestAge, Worms.Max(w => w.Age));
#if NEEDDEBUGWORM
            if (debugWorm != null && debugWorm.IsAlive)
            {
                debugWorm.PrintDebug();
            }
#endif
            foodTicks++;
            if (foodTicks > Constants.FoodGenerationTicks)
            {
                foodTicks = 0;
                GenerateNewFood();
            }

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
            var parents = Worms
                .OrderByDescending(w => w.Body.Count + 1)
                .ThenByDescending(w => w.Age)
                .Take(Constants.NumberOfParents)
                .ToList();

            foreach (var parent in parents)
            {
                for (var i = 0; i < Constants.ChildrenPerGeneration; i++)
                {
                    var brain = parent.Brain.Clone();

                    if (NeuroRnd.NextDouble() < Constants.MutationChance)
                    {
                       brain.Mutate();
                    }

                    var worm = CreateWormOnField(brain);
                    newWorms.Add(worm);
                }
            }
            Worms = newWorms;
            InitFood();
            CurrentTick = 0;
            foodTicks = 0;
            CurrentGeneration++;
#if NEEDDEBUGWORM
            debugWorm = Worms.First();
#endif
        }

        private void InitFood()
        {
            Constants.StartFoodCount.Times(GenerateNewFood);
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
            Constants.StartWormCount.Times(() =>
            {
                var brain = new WormNeuroBrain();
                brain.Init();
                var worm = CreateWormOnField(brain);
                Worms.Add(worm);
            });
#if NEEDDEBUGWORM
            debugWorm = Worms.First();
#endif
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

                body = [];
                for (var j = 0; j < Constants.WormStartLength; j++)
                {
                    var newPiece = body.Count == 0 ? head.Move(buildDirection) : body[^1].Move(buildDirection);
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
    }
}
