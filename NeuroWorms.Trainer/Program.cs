using System.Diagnostics;
using NeuroWorms.Core;
using NeuroWorms.Trainer;

try
{
    var options = TrainerOptions.Parse(args);
    if (options.ShowHelp)
    {
        PrintHelp();
        return 0;
    }

    var checkpointPath = Path.GetFullPath(
        options.SaveFilePath ?? SimulationEngine.DefaultSaveFilePath);
    if (options.Clean)
    {
        Console.WriteLine($"Ignoring existing checkpoint and starting from generation 1: {checkpointPath}");
    }

    var engine = new SimulationEngine(checkpointPath, loadCheckpoint: !options.Clean);

    var startGeneration = engine.CurrentGeneration;
    int? targetGeneration = options.UntilGeneration;
    if (options.Generations.HasValue)
    {
        targetGeneration = checked(startGeneration + options.Generations.Value);
    }

    Console.WriteLine("NeuroWorms headless trainer");
    Console.WriteLine($"Checkpoint: {engine.SaveFilePath}");
    Console.WriteLine($"Next generation: {startGeneration + 1}");
    Console.WriteLine(
        targetGeneration.HasValue
            ? $"Target generation: {targetGeneration.Value}"
            : "Target generation: unlimited (press Ctrl+C to stop)");

    if (targetGeneration.HasValue && targetGeneration.Value <= startGeneration)
    {
        Console.WriteLine("Target generation has already been reached.");
        return 0;
    }

    using var cancellation = new CancellationTokenSource();
    Console.CancelKeyPress += (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        cancellation.Cancel();
    };

    var stopwatch = Stopwatch.StartNew();
    var lastReportGeneration = startGeneration;
    var lastReportTime = TimeSpan.Zero;

    while ((!targetGeneration.HasValue || engine.CurrentGeneration < targetGeneration.Value) &&
           !cancellation.IsCancellationRequested)
    {
        engine.RunTillNextGeneration();

        var generationsSinceReport = engine.CurrentGeneration - lastReportGeneration;
        if (generationsSinceReport < options.ReportEvery &&
            (!targetGeneration.HasValue || engine.CurrentGeneration < targetGeneration.Value))
        {
            continue;
        }

        var elapsedSinceReport = stopwatch.Elapsed - lastReportTime;
        var generationsPerSecond = elapsedSinceReport.TotalSeconds > 0
            ? generationsSinceReport / elapsedSinceReport.TotalSeconds
            : 0.0;
        var result = engine.LastGenerationResult;
        var populationSize = result.HungerDeaths
            + result.WallDeaths
            + result.SelfBodyDeaths
            + result.OtherWormDeaths
            + result.Survivors;
        var hungerDeathPercent = AsPercent(result.HungerDeaths, populationSize);
        var wallDeathPercent = AsPercent(result.WallDeaths, populationSize);
        var selfBodyDeathPercent = AsPercent(result.SelfBodyDeaths, populationSize);
        var otherWormDeathPercent = AsPercent(result.OtherWormDeaths, populationSize);

        Console.WriteLine(new string('-', 78));
        Console.WriteLine(
            $"Gen {result.Generation,6} | ticks {result.Ticks,4} | " +
            $"alive {result.Survivors,2} | {generationsPerSecond:F2} gen/s");
        Console.WriteLine(
            $"Champion | fit {result.ChampionFitness:F0} | age {result.ChampionAge} | " +
            $"food {result.ChampionFoodEaten} | len {result.ChampionLength} | " +
            $"hits {result.ChampionTotalCollisions} | death {FormatDeathReason(result.ChampionDeathReason)}");
        Console.WriteLine(
            $"Average  | fit {result.AverageFitness:F1} | age {result.AverageAge:F1} | " +
            $"food {result.AverageFoodEaten:F1} | hits {result.AverageCollisions:F1}");
        Console.WriteLine(
            $"Hits     | W {result.WallCollisions,3} | S {result.SelfBodyCollisions,3} | " +
            $"O {result.OtherWormCollisions,3}");
        Console.WriteLine(
            $"Deaths   | H {result.HungerDeaths,2} ({hungerDeathPercent,2:F0}%) | " +
            $"W {result.WallDeaths,2} ({wallDeathPercent,2:F0}%) | " +
            $"S {result.SelfBodyDeaths,2} ({selfBodyDeathPercent,2:F0}%) | " +
            $"O {result.OtherWormDeaths,2} ({otherWormDeathPercent,2:F0}%)");

        lastReportGeneration = engine.CurrentGeneration;
        lastReportTime = stopwatch.Elapsed;
    }

    stopwatch.Stop();
    var advancedGenerations = engine.CurrentGeneration - startGeneration;
    Console.WriteLine();
    Console.WriteLine(
        $"Advanced {advancedGenerations} generations in {stopwatch.Elapsed}. " +
        $"Checkpoint is at generation {engine.CurrentGeneration}.");

    if (cancellation.IsCancellationRequested)
    {
        Console.WriteLine("Stopped after the last completed and saved generation.");
    }

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Trainer failed: {exception.Message}");
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine(
        """
        NeuroWorms headless trainer

        Usage:
          dotnet run --project NeuroWorms.Trainer -c Release -- [generations]
          dotnet run --project NeuroWorms.Trainer -c Release -- [options]

        Options:
          -g, --generations N   Advance N more generations, then stop.
          -u, --until N         Run until absolute generation N.
          -r, --report-every N  Print progress every N generations (default: 5).
          -s, --save-file PATH  Use a custom checkpoint instead of the shared default.
              --clean           Ignore the selected checkpoint and start from generation 1.
          -h, --help            Show this help.

        With no generation limit, the trainer runs until Ctrl+C is pressed.

        Examples:
          dotnet run --project NeuroWorms.Trainer -c Release -- 500
          dotnet run --project NeuroWorms.Trainer -c Release -- --until 1000
          dotnet run --project NeuroWorms.Trainer -c Release -- --clean
        """);
}

static double AsPercent(int count, int total)
{
    return total == 0 ? 0.0 : (double)count / total * 100.0;
}

static string FormatDeathReason(DeathReason deathReason)
{
    return deathReason == DeathReason.None ? "Alive" : deathReason.ToString();
}
