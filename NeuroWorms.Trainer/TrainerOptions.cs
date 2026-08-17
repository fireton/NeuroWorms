namespace NeuroWorms.Trainer;

internal sealed record TrainerOptions(
    int? Generations,
    int? UntilGeneration,
    int ReportEvery,
    string? SaveFilePath,
    bool Clean,
    bool ShowHelp)
{
    public static TrainerOptions Parse(string[] args)
    {
        int? generations = null;
        int? untilGeneration = null;
        var reportEvery = 5;
        string? saveFilePath = null;
        var clean = false;
        var showHelp = false;

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--generations":
                case "-g":
                    generations = ReadNonNegativeInt(args, ref index, argument);
                    break;
                case "--until":
                case "-u":
                    untilGeneration = ReadNonNegativeInt(args, ref index, argument);
                    break;
                case "--report-every":
                case "-r":
                    reportEvery = ReadPositiveInt(args, ref index, argument);
                    break;
                case "--save-file":
                case "-s":
                    saveFilePath = ReadValue(args, ref index, argument);
                    break;
                case "--clean":
                    clean = true;
                    break;
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                default:
                    if (index == 0 && int.TryParse(argument, out var positionalGenerations) && positionalGenerations >= 0)
                    {
                        generations = positionalGenerations;
                        break;
                    }

                    throw new ArgumentException($"Unknown argument '{argument}'. Use --help for usage.");
            }
        }

        if (generations.HasValue && untilGeneration.HasValue)
        {
            throw new ArgumentException("Use either --generations or --until, not both.");
        }

        return new TrainerOptions(generations, untilGeneration, reportEvery, saveFilePath, clean, showHelp);
    }

    private static int ReadNonNegativeInt(string[] args, ref int index, string argument)
    {
        var value = ReadValue(args, ref index, argument);
        if (!int.TryParse(value, out var parsed) || parsed < 0)
        {
            throw new ArgumentException($"{argument} requires a non-negative integer.");
        }

        return parsed;
    }

    private static int ReadPositiveInt(string[] args, ref int index, string argument)
    {
        var value = ReadValue(args, ref index, argument);
        if (!int.TryParse(value, out var parsed) || parsed <= 0)
        {
            throw new ArgumentException($"{argument} requires a positive integer.");
        }

        return parsed;
    }

    private static string ReadValue(string[] args, ref int index, string argument)
    {
        index++;
        if (index >= args.Length)
        {
            throw new ArgumentException($"{argument} requires a value.");
        }

        return args[index];
    }
}
