# NeuroWorms

NeuroWorms is an artificial-life experiment written in C# and MonoGame. A population of snake-like worms is controlled by small feed-forward neural networks. Each generation runs inside a shared grid world, the best worms reproduce with mutation, and their genomes are saved so training can continue in either the graphical application or a fast headless trainer.

The project is deliberately simple: there is no gradient descent, backpropagation, crossover, or scripted food-seeking behavior. Useful behavior must emerge through selection and mutation.

## Current simulation

- Field: `180 × 180` cells.
- Population: 50 worms.
- Initial food: 100 cells, with additional food generated during a generation.
- Hunger limit: 300 successful moves without food.
- Generation limit: 5000 ticks, or earlier when every worm has died.
- Food resets hunger, increases `FoodEaten`, and grows the body by one segment.
- A worm dies from hunger, a wall collision, or a collision with its own or another worm's body.

All worms currently share one field and take their turns sequentially. Ideas for parallel and simultaneous simulation are tracked in [TODO.md](TODO.md).

## Brain and sensors

The current dense network has this shape:

```text
15 sensors → 25 hidden neurons → 10 hidden neurons → 1 motor neuron
```

Hidden and motor neurons use `tanh`. The motor output maps to one of three relative actions:

- less than `-0.3333`: turn left;
- from `-0.3333` to `0.3333`: continue straight;
- greater than `0.3333`: turn right.

The 15 inputs are:

- food presence, relative angle, and distance;
- worm presence, relative angle, and distance;
- wall presence, relative angle, and distance;
- the contents of the immediately adjacent left and right cells;
- body length;
- current X and Y direction components;
- hunger level.

### Radar-like field of view

`EyeSight` currently behaves more like a radar or directional sense than occluded visual sight:

- field of view: 180 degrees in front of the worm;
- range: 70 cells;
- one nearest result is returned for each of `Food`, `Worm`, and `Wall`;
- angle is normalized from `-1` on the right to `+1` on the left;
- distance is normalized from approximately `-1` nearby to `+1` at maximum range;
- bodies and walls do not hide objects behind them.

The scan pattern is precomputed once and shared between brains. It contains every integer cell in the front half-disk, with no sampling gaps or duplicates, and is ordered by squared Euclidean distance. Runtime scans therefore proceed from the nearest discrete distance shells to the farthest without calculating angles or square roots for every worm movement.

## Evolution cycle

Each generation follows this cycle:

1. Run ticks until all worms die or the 5000-tick limit is reached.
2. Record age, food, survivors, and death-reason statistics.
3. Rank worms by body length, then by age when lengths are equal, then by death reason as a stable final tie-breaker.
4. Build the next population using the configured `GenerationMutator`.
5. Place the new worms and food on a cleared field.
6. Increment the generation and atomically save a JSON checkpoint.

The current selection is deliberately lexicographic: one extra body segment always wins over any age difference. A weighted fitness function is being considered but is not implemented yet.

### Default mutation strategy

`MixedCloneAndMutate` is the default strategy. It selects the six highest-ranked parents and creates exactly 50 brains:

| Offspring type | Per parent | Total | Mutation |
|---|---:|---:|---|
| Exact clones | 3 | 18 | none |
| Fine-tuned clones | 3 | 18 | strength `0.075`, 15% neuron coverage |
| Strongly mutated clones | 2 | 12 | strength `0.15`, 25% neuron coverage |
| New random brains | — | 2 | fresh blood |

The resulting population is shuffled so a lineage does not receive a permanent advantage from its position in the sequential turn order.

Two older named strategies remain in the code for experimentation:

- `FineTuningCloneAndMutate`;
- `LegacyCloneAndMutate`.

## Applications

The solution contains three projects:

- `NeuroWorms`: the MonoGame graphical application;
- `NeuroWorms.Trainer`: a headless console trainer;
- `NeuroWorms.Tests`: xUnit regression tests.

Both applications use the same simulation engine and the same default checkpoint.

## Requirements

- .NET 8 SDK or a compatible newer SDK;
- desktop graphics support for the MonoGame application.

Restore and build the complete solution:

```bash
dotnet restore NeuroWorms.sln
dotnet build NeuroWorms.sln -c Release
```

## Graphical application

Run the visual simulation:

```bash
dotnet run --project NeuroWorms/NeuroWorms.csproj -c Release
```

If the default checkpoint exists, the application loads its population and generation number. Otherwise it creates and saves generation 0.

## Headless trainer

Run indefinitely until `Ctrl+C`:

```bash
dotnet run --project NeuroWorms.Trainer/NeuroWorms.Trainer.csproj -c Release
```

Advance 1500 generations from the loaded checkpoint:

```bash
dotnet run --project NeuroWorms.Trainer/NeuroWorms.Trainer.csproj -c Release -- --generations 1500
```

Ignore the existing checkpoint and start a new 1500-generation experiment:

```bash
dotnet run --project NeuroWorms.Trainer/NeuroWorms.Trainer.csproj -c Release -- --clean --generations 1500
```

`--clean` does not explicitly delete the selected file, but generation 0 is saved normally and replaces its contents. Use `--save-file` if the previous experiment must be preserved.

Available options:

```text
-g, --generations N   Advance N more generations, then stop.
-u, --until N         Run until absolute generation N.
-r, --report-every N  Print progress every N generations (default: 5).
-s, --save-file PATH  Use a custom checkpoint instead of the shared default.
    --clean            Ignore the selected checkpoint and start at generation 0.
-h, --help             Show command help.
```

`Ctrl+C` is handled at a generation boundary so the most recently completed generation remains saved.

### Trainer statistics

Example:

```text
Gen   1605 | ticks 5000 | age 5000/1089.5 best/avg | food 70/16.3 best/avg | deaths H/W/B 15/20/14 (30%/40%/28%) | survivors 1 | 0.68 gen/s
```

- `Gen`: evaluated generation. The checkpoint advances to the following generation after reproduction.
- `ticks`: generation duration.
- `age`: best and population-average age.
- `food`: best and population-average amount of food eaten.
- `H`: deaths from hunger.
- `W`: deaths from walls.
- `B`: deaths from a worm body or head, including both self-collisions and other worms.
- `survivors`: worms still alive when the generation tick limit was reached.
- `gen/s`: generation throughput since the previous report.

Best age and best food are independent population maxima and may belong to different worms.

## Checkpoints

Checkpoints are formatted JSON and contain:

- schema version;
- generation number;
- UTC save timestamp;
- all 50 brain genomes;
- 36 biases and 635 weights per current genome.

They intentionally do not preserve the in-progress field, worm positions, ages, food placement, or random-number-generator state. Loading a checkpoint starts a fresh environment using the saved population.

The default file is stored under the operating system's local application-data directory:

```text
NeuroWorms/checkpoint.json
```

On macOS this normally resolves to:

```text
~/Library/Application Support/NeuroWorms/checkpoint.json
```

Writes use a temporary file followed by an atomic replacement. Invalid or incompatible checkpoints fail explicitly instead of silently restarting evolution.

## Tests

Run the complete test suite:

```bash
dotnet test NeuroWorms.sln -c Release
```

The tests cover neural-network cloning and genome persistence, sensor refresh behavior, all four radar orientations, complete radar cell coverage, nearest-object ordering, generation boundaries, selection, mutation strategies, and checkpoint recovery.

## License

NeuroWorms is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt).
