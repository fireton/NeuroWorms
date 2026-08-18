# NeuroWorms

NeuroWorms is an artificial-life experiment written in C# and MonoGame. Fifty snake-like worms live in one grid world and choose relative movements through small feed-forward neural networks. At the end of every generation, the best brains reproduce through cloning and mutation, and the resulting population is saved to JSON.

There is no gradient descent, backpropagation, crossover, scripted food-seeking, or topology evolution in the current implementation. Behaviour emerges from selection and mutation of neural-network weights and biases. Planned experiments, including parallel islands, evolving graph topologies, clustered food, and tournament selection, are documented in [TODO.md](TODO.md).

## Projects

The solution contains three projects:

- `NeuroWorms` — the MonoGame graphical application;
- `NeuroWorms.Trainer` — a headless console trainer that runs generations as quickly as possible;
- `NeuroWorms.Tests` — xUnit regression tests.

The graphical application and trainer use the same simulation engine, evolution strategy, and default checkpoint.

## Simulation rules

| Setting | Current value |
|---|---:|
| Field | `180 × 180` cells |
| Population | 50 worms |
| Initial worm size | 1 head + 3 body cells |
| Food on field | 50 cells |
| Hunger threshold | 300 hunger units |
| Generation limit | 5000 ticks |
| Fatal collision streak | 3 consecutive collisions |
| Radar field of view | 180° |
| Radar range | 70 cells |

All worms share one field. At the beginning of a tick, the engine takes the current list of living worms and processes them sequentially. Each brain chooses one of three relative actions: turn left, continue straight, or turn right.

The requested cell is then handled as follows:

- empty cell — the worm moves normally;
- food — the worm eats, grows by one cell, and moves;
- wall or any worm head/body — the worm remains in place and registers a collision.

A collision still advances age and hunger. The first two consecutive collisions are recoverable, giving the brain another decision on the next tick. The third consecutive collision kills the worm with `Wall`, `SelfBody`, or `OtherWorm` as the death reason. Any successful movement, including movement onto food, resets the consecutive-collision streak. The total number of collisions remains recorded for statistics and fitness.

Hunger increases on both movement and collision ticks. The metabolic cost of a tick is `1 + (Length - 1) / 50`, so longer worms must eat more frequently. Hunger is accumulated as a fractional value without length thresholds. Eating resets it before the eating movement is completed, and a worm dies when its hunger becomes greater than 300. A dead worm is removed from the field immediately; its body currently becomes empty cells, not food.

A generation ends when every worm is dead or after 5000 ticks. User-facing generation numbers start at 1.

### Food lifecycle

Food is currently distributed uniformly among empty cells:

1. Every new generation starts with exactly 50 food cells — one per worm.
2. When a food cell is eaten, one replacement food cell is created immediately at another random empty position.
3. No additional food is created over time, so the field always contains 50 food cells.
4. Food does not expire or move.
5. At the next generation boundary the field is cleared and reset to 50 food cells.

Consequently, eating does not reduce the amount of food on the field, but waiting no longer makes food increasingly abundant. Clustered spawning and recycling dead worms into food are ideas only; they are not implemented yet.

## Neural network

Every worm currently uses the same fixed dense topology:

```text
21 sensors → 12 hidden neurons → 6 hidden neurons → 1 motor neuron
```

This produces 19 trainable biases and 330 trainable weights. Hidden and motor neurons use `tanh`. New weights use Xavier uniform initialization for their respective layer sizes, and every bias starts at zero.

The motor output maps to relative movement:

- output below `-0.3333` — turn left;
- output from `-0.3333` through `0.3333` — continue straight;
- output above `0.3333` — turn right.

### Sensors

The 21 inputs are:

| Count | Inputs |
|---:|---|
| 3 | Food presence, relative angle, and distance |
| 3 | Other-worm presence, relative angle, and distance |
| 3 | Own-body presence, relative angle, and distance |
| 3 | Wall presence, relative angle, and distance |
| 3 | Contents of the immediately adjacent left, ahead, and right cells |
| 1 | Body length |
| 1 | Hunger |
| 1 | Consecutive-collision state |
| 3 | Own-body avoidance forward/right and surrounding body pressure |

Adjacent-cell sensors return `-1` for food, `0` for empty, and `+1` for a wall, worm body, or worm head.

The length sensor maps length 0–50 approximately onto `[-1; +1]`; it is not clamped above length 50. Hunger is clamped to `[-1; +1]`. The collision sensor returns `-1` with no current streak, `0` after one collision, and `+1` after two or more.

Absolute X/Y direction sensors are intentionally absent. Vision and motor actions are expressed in the worm's local frame.

Each worm-owned field cell carries a compact integer `OwnerId`. The radar uses it to expose the nearest own-body cell and nearest other-worm cell through separate visual triples. Other heads and bodies are still treated as one visual category. The adjacent-cell sensors remain tactile obstacle sensors and intentionally do not distinguish walls, self, and other worms.

### Own-body sense

The three proprioceptive inputs are calculated together in one pass over the worm's body. The first three segments behind the head are ignored because their predictable position would otherwise dominate the result.

Every remaining segment contributes an imaginary repulsion weighted by `1 / (1 + distance²)`:

- `OwnBodyAvoidanceForward` indicates whether nearby body mass pushes the safe direction forward or backward;
- `OwnBodyAvoidanceRight` indicates whether the safe direction is toward the right or left;
- `OwnBodyPressure` measures nearby body density even when opposing directional contributions cancel out.

The calculation uses coordinates relative to the current heading, squared distance, and no trigonometry or square roots. Directional values are clamped to `[-1; +1]`, and pressure to `[0; +1]`. The three sensors share the cached result, so the body is traversed only once per neural-network decision.

### Radar-like vision

`EyeSight` behaves like a radar rather than occluded visual sight:

- it scans the complete integer-cell half-disk in front of the worm;
- it returns the nearest detected `Food`, `OwnBody`, `OtherWorm`, and `Wall` independently;
- relative angle is normalized from `-1` on the left to `+1` on the right, matching the motor convention;
- distance is normalized from approximately `-1` nearby to `+1` at maximum range;
- walls and bodies do not hide cells behind them.

The scan offsets are precomputed and cached. Every integer cell inside the 180° sector and 70-cell radius is included without sampling holes or duplicates. Offsets are ordered by squared Euclidean distance, so runtime scans proceed from near to far without calculating angles or square roots for every worm decision.

## Evolution

At a generation boundary, every worm receives the default weighted fitness:

```text
fitness = Age + FoodEaten × 100 - TotalCollisions × 50
```

Each food item also adds one body cell, so `FoodEaten` rewards growth directly. There is no fixed death penalty and no special reward for reaching the 5000-tick limit.

The population is ranked by:

1. fitness, descending;
2. food eaten, descending;
3. age, descending;
4. death reason, as a deterministic final tie-breaker.

### Default reproduction strategy

`MixedCloneAndMutate` selects the six best parents and creates exactly 50 brains:

| Offspring type | Per parent | Total | Parameters |
|---|---:|---:|---|
| Exact clones | 3 | 18 | No mutation |
| Fine-tuned clones | 3 | 18 | Strength `0.075`, 15% neuron coverage |
| Strongly mutated clones | 2 | 12 | Strength `0.15`, 25% neuron coverage |
| New random brains | — | 2 | Fresh blood |

For each selected neuron, bias mutation is attempted with 40% probability and synapse mutation with 60% probability. A synapse mutation changes approximately one third of that neuron's incoming weights using Gaussian jitter, clamped to `[-1; +1]`.

The 50 offspring are shuffled before placement so one lineage does not permanently benefit from its position in the sequential turn order.

Two older strategies remain available in code for experiments but are not selected by default:

- `FineTuningCloneAndMutate`;
- `LegacyCloneAndMutate`.

### Generation cycle

1. Simulate until all worms die or the tick limit is reached.
2. Capture age, food, fitness, collision, survivor, and death-reason statistics.
3. Rank the population and create the next 50 brains.
4. Clear the field and place the new worms.
5. Reset the field to 50 food cells and reset the generation tick counter.
6. Increment the completed-generation counter.
7. Atomically save the new population to the checkpoint.

## Build and run

Requirements:

- .NET 8 SDK or a compatible newer SDK;
- desktop graphics support for the MonoGame application.

Restore and build the complete solution:

```bash
dotnet restore NeuroWorms.sln
dotnet build NeuroWorms.sln -c Release
```

### Graphical application

```bash
dotnet run --project NeuroWorms/NeuroWorms.csproj -c Release
```

The application loads the default checkpoint when it exists. Otherwise it creates and saves the initial population for generation 1. `Escape` exits the application; `V` switches between visible ticking and calculating whole generations without rendering intermediate states.

### Headless trainer

Run indefinitely until `Ctrl+C`:

```bash
dotnet run --project NeuroWorms.Trainer/NeuroWorms.Trainer.csproj -c Release
```

Advance another 1500 generations from the loaded checkpoint:

```bash
dotnet run --project NeuroWorms.Trainer/NeuroWorms.Trainer.csproj -c Release -- --generations 1500
```

Ignore the existing checkpoint and begin again from generation 1:

```bash
dotnet run --project NeuroWorms.Trainer/NeuroWorms.Trainer.csproj -c Release -- --clean --generations 1500
```

`--clean` does not delete the selected file first. It ignores its contents and immediately replaces it with a new initial population. Use a different `--save-file` to preserve an existing experiment.

Trainer options:

```text
-g, --generations N   Advance N more generations, then stop.
-u, --until N         Run until absolute generation N.
-r, --report-every N  Print progress every N generations (default: 5).
-s, --save-file PATH  Use a custom checkpoint instead of the shared default.
    --clean            Ignore the selected checkpoint and start from generation 1.
-h, --help             Show command help.
```

A non-negative positional argument is also accepted as the number of generations. `--generations` and `--until` cannot be used together. `Ctrl+C` is observed at a generation boundary, after the most recently completed generation has been saved.

### Trainer output

```text
------------------------------------------------------------------------------
Gen    170 | ticks 2069 | alive  0 | 0.61 gen/s
Champion | fit 5169 | age 2069 | food 38 | len 42 | hits 14 | death Hunger
Average  | fit 1472.9 | age 685.9 | food 10.8 | hits 5.8
Hits     | W  65 | S  42 | O 182
Deaths   | H 14 (28%) | W  3 ( 6%) | S  8 (16%) | O 25 (50%)
```

- `Gen` — evaluated generation, numbered from 1;
- `ticks` — duration of the generation;
- `alive` — worms still alive when the generation reached its tick limit;
- `gen/s` — throughput since the previous report.
- `Champion` — all metrics of the single highest-ranked worm selected for reproduction; `death Alive` means it reached the generation limit;
- `Average` — population-average fitness, age, food eaten, and collisions;
- `Hits W/S/O` — total collision events with walls, the worm's own body, and other worms; recoverable collisions are included;
- `Deaths H/W/S/O` — deaths from hunger, walls, the worm's own body, and other worms, with percentages of the full population.

Best age, food, and fitness are independent maxima and may belong to different worms.

## Checkpoints

The default checkpoint path is based on the operating system's local application-data directory:

```text
NeuroWorms/checkpoint.json
```

On macOS this normally resolves to:

```text
~/Library/Application Support/NeuroWorms/checkpoint.json
```

Checkpoint schema version 4 stores:

- the number of completed generations;
- the UTC save timestamp;
- all 50 brain genomes;
- 19 biases and 330 weights per genome.

It does not store the in-progress field, worm positions, owner IDs, ages, hunger, food placement, statistics, or random-number-generator state. Loading always starts a fresh field using the saved population, and new runtime owner IDs are assigned to the worms.

Writes use a temporary file followed by an atomic replacement. Invalid JSON, an incorrect population size, or an unsupported schema version fails explicitly instead of silently restarting evolution. Checkpoint versions 1–3 from previous networks are intentionally incompatible; use `--clean` or a different `--save-file`.

## Current limitations

- Worm decisions and field updates are sequential, not simultaneous.
- The network topology is fixed and dense; only weights and biases evolve.
- Random-number-generator state is neither seeded from the command line nor persisted.
- Radar vision passes through walls and worm bodies.
- Other-worm vision distinguishes self from non-self but does not yet distinguish individual worms, heads, and bodies.
- Food is uniformly distributed and grows in quantity during a generation.
- There is no crossover, speciation, island training, or parallel simulation yet.

See [TODO.md](TODO.md) for the proposed parallel execution model, topology DNA, isolated species sandboxes, and tournament experiments.

## Tests

Run the complete test suite:

```bash
dotnet test NeuroWorms.sln -c Release
```

The suite covers neural-network topology and Xavier initialization, cloning and genome persistence, sensor refresh and direction conventions, own-body avoidance geometry, all radar orientations, complete radar-cell coverage, nearest-object ordering, owner-aware self/other vision and collision classification, recoverable and fatal collisions, collision statistics and fitness penalties, weighted parent selection, mutation strategies, generation boundaries, and checkpoint recovery/versioning.

## License

NeuroWorms is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt).
