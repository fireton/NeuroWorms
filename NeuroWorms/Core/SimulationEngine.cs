using NeuroWorms.Core.Helpers;
using NeuroWorms.Core.Evolution;
using NeuroWorms.Core.Neuro;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
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
        public GenerationResult LastGenerationResult { get; private set; }

        private readonly Random random = new Random();
        private readonly CheckpointStore checkpointStore;
        private readonly GenerationMutator generationMutator;
        private readonly FitnessFunction fitnessFunction;

        private readonly bool debug = false;

        public static string DefaultSaveFilePath => CheckpointStore.DefaultFilePath;
        public string SaveFilePath => checkpointStore?.FilePath;

        public SimulationEngine() : this(
            CheckpointStore.DefaultFilePath,
            new MixedCloneAndMutate(),
            new WeightedAgeFoodCollisionFitness(),
            loadCheckpoint: true)
        {
        }

        public SimulationEngine(string saveFilePath) : this(
            saveFilePath,
            new MixedCloneAndMutate(),
            new WeightedAgeFoodCollisionFitness(),
            loadCheckpoint: true)
        {
        }

        public SimulationEngine(string saveFilePath, bool loadCheckpoint) : this(
            saveFilePath,
            new MixedCloneAndMutate(),
            new WeightedAgeFoodCollisionFitness(),
            loadCheckpoint)
        {
        }

        internal SimulationEngine(string saveFilePath, GenerationMutator generationMutator) : this(
            saveFilePath,
            generationMutator,
            new WeightedAgeFoodCollisionFitness(),
            loadCheckpoint: true)
        {
        }

        internal SimulationEngine(
            string saveFilePath,
            GenerationMutator generationMutator,
            FitnessFunction fitnessFunction,
            bool loadCheckpoint)
        {
            this.generationMutator = generationMutator
                ?? throw new ArgumentNullException(nameof(generationMutator));
            this.fitnessFunction = fitnessFunction
                ?? throw new ArgumentNullException(nameof(fitnessFunction));
            checkpointStore = saveFilePath is null ? null : new CheckpointStore(saveFilePath);
            Field = new Field(Constants.FieldWidth, Constants.FieldHeight);
            Worms = [];

            if (loadCheckpoint && checkpointStore?.Exists == true)
            {
                try
                {
                    RestoreCheckpoint(checkpointStore.Load());
                }
                catch (InvalidDataException exception)
                {
                    throw new InvalidDataException(
                        $"Cannot restore checkpoint '{checkpointStore.FilePath}': {exception.Message}",
                        exception);
                }
            }
            else
            {
                InitWorms();
                SaveCheckpoint();
            }

            InitFood();
        }

        public Task NextMove()
        {
            var aliveWorms = Worms.FindAll(w => w.IsAlive);

            if (aliveWorms.Count == 0 || CurrentTick >= Constants.MaxGenerationTicks)
            {
                NextGeneration();
                return Task.CompletedTask;
            }

            foreach (var worm in aliveWorms)
            {
                var nextMove = worm.Brain.GetNextMove(Field, worm);
                var nextHead = worm.Head.Move(nextMove);
                var nextCellType = Field[nextHead.X, nextHead.Y];

                switch (nextCellType)
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
                        RegisterCollision(
                            worm,
                            Field.GetOwnerId(nextHead) == worm.OwnerId
                                ? DeathReason.SelfBody
                                : DeathReason.OtherWorm);
                        break;
                    case CellType.Wall:
                        RegisterCollision(worm, DeathReason.Wall);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown cell type");
                }
                if (worm.IsAlive && worm.Hunger > Constants.MaxHunger)
                {
                    KillWorm(worm, DeathReason.Hunger);
                }
                LongestWorm = Math.Max(LongestWorm, worm.Length);
            }
            
            CurrentTick++;
            LongestAge = Math.Max(LongestAge, Worms.Max(w => w.Age));

            return Task.CompletedTask;

            void KillWorm(Worm worm, DeathReason reason)
            {
                worm.Die(reason);
                if (debug)
                {
                    Debug.WriteLine($"Debug worm died of {reason}");
                    worm.PrintDebug();
                    Debug.WriteLine($" --- ");
                }
                worm.RemoveFromField(Field);
            }

            void RegisterCollision(Worm worm, DeathReason reason)
            {
                worm.RegisterCollision(reason);
                if (worm.ConsecutiveCollisions >= Constants.MaxConsecutiveCollisions)
                {
                    KillWorm(worm, reason);
                }
            }

        }

        public void RunTillNextGeneration()
        {
            var currentGeneration = CurrentGeneration;
            while (CurrentGeneration == currentGeneration)
            {
                NextMove();
            }
        }

        private void NextGeneration()
        {
            var scoredPopulation = Worms
                .Select(worm => new
                {
                    Worm = worm,
                    Fitness = fitnessFunction.Evaluate(worm),
                })
                .ToList();
            var rankedCandidates = scoredPopulation
                .OrderByDescending(candidate => candidate.Fitness)
                .ThenByDescending(candidate => candidate.Worm.FoodEaten)
                .ThenByDescending(candidate => candidate.Worm.Age)
                .ThenBy(candidate => candidate.Worm.DeathReason)
                .ToList();
            var champion = rankedCandidates[0];

            LastGenerationResult = new GenerationResult(
                CurrentGeneration + 1,
                CurrentTick,
                Worms.Max(worm => worm.Age),
                Worms.Average(worm => worm.Age),
                Worms.Max(worm => worm.FoodEaten),
                Worms.Average(worm => worm.FoodEaten),
                scoredPopulation.Max(candidate => candidate.Fitness),
                scoredPopulation.Average(candidate => candidate.Fitness),
                Worms.Sum(worm => worm.WallCollisions),
                Worms.Sum(worm => worm.SelfBodyCollisions),
                Worms.Sum(worm => worm.OtherWormCollisions),
                Worms.Average(worm => worm.TotalCollisions),
                Worms.Count(worm => worm.DeathReason == DeathReason.Hunger),
                Worms.Count(worm => worm.DeathReason == DeathReason.Wall),
                Worms.Count(worm => worm.DeathReason == DeathReason.SelfBody),
                Worms.Count(worm => worm.DeathReason == DeathReason.OtherWorm),
                Worms.Count(worm => worm.IsAlive),
                champion.Worm.Age,
                champion.Worm.FoodEaten,
                champion.Worm.Length,
                champion.Fitness,
                champion.Worm.WallCollisions,
                champion.Worm.SelfBodyCollisions,
                champion.Worm.OtherWormCollisions,
                champion.Worm.DeathReason);

            var rankedPopulation = rankedCandidates
                .Select(candidate => candidate.Worm)
                .ToList();
            var newBrains = generationMutator.CreateNextGeneration(rankedPopulation);

            Field.Clear();
            var newWorms = new List<Worm>(newBrains.Count);
            foreach (var brain in newBrains)
            {
                var worm = CreateWormOnField(brain);
                newWorms.Add(worm);
            }

            Worms = newWorms;
            InitFood();
            CurrentTick = 0;
            CurrentGeneration++;
            SaveCheckpoint();
        }

        private void RestoreCheckpoint(SimulationCheckpoint checkpoint)
        {
            CurrentGeneration = checkpoint.Generation;
            foreach (var genome in checkpoint.Population)
            {
                if (genome is null)
                {
                    throw new InvalidDataException("Checkpoint population contains an empty genome.");
                }

                var brain = WormNeuroBrain.FromGenome(genome);
                Worms.Add(CreateWormOnField(brain));
            }
        }

        private void SaveCheckpoint()
        {
            if (checkpointStore is null)
            {
                return;
            }

            checkpointStore.Save(CheckpointStore.Create(CurrentGeneration, Worms));
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
