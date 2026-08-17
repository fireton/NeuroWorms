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
        Console.WriteLine($"Ignoring existing checkpoint and starting from generation 0: {checkpointPath}");
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
    Console.WriteLine($"Start generation: {startGeneration}");
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
            + result.WormBodyDeaths
            + result.Survivors;
        var hungerDeathPercent = AsPercent(result.HungerDeaths, populationSize);
        var wallDeathPercent = AsPercent(result.WallDeaths, populationSize);
        var wormBodyDeathPercent = AsPercent(result.WormBodyDeaths, populationSize);

        Console.WriteLine(
            $"Gen {result.Generation,6} | ticks {result.Ticks,4} | " +
            $"age {result.BestAge,4}/{result.AverageAge,7:F1} best/avg | " +
            $"food {result.BestFoodEaten,3}/{result.AverageFoodEaten,6:F1} best/avg | " +
            $"deaths H/W/B {result.HungerDeaths,2}/{result.WallDeaths,2}/{result.WormBodyDeaths,2} " +
            $"({hungerDeathPercent,3:F0}%/{wallDeathPercent,3:F0}%/{wormBodyDeathPercent,3:F0}%) | " +
            $"survivors {result.Survivors,2} | {generationsPerSecond:F2} gen/s");

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
              --clean           Ignore the selected checkpoint and start from generation 0.
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
