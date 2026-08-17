using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NeuroWorms.Core.Neuro;

namespace NeuroWorms.Core;

internal sealed class CheckpointStore
{
    private const int CurrentVersion = 2;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static string DefaultFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NeuroWorms",
        "checkpoint.json");

    public string FilePath { get; }
    public bool Exists => File.Exists(FilePath);

    public CheckpointStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("A checkpoint file path is required.", nameof(filePath));
        }

        FilePath = Path.GetFullPath(filePath);
    }

    public SimulationCheckpoint Load()
    {
        try
        {
            var json = File.ReadAllText(FilePath);
            var checkpoint = JsonSerializer.Deserialize<SimulationCheckpoint>(json, JsonOptions)
                ?? throw new InvalidDataException("The checkpoint is empty.");

            Validate(checkpoint);
            return checkpoint;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Checkpoint '{FilePath}' is not valid JSON.", exception);
        }
    }

    public void Save(SimulationCheckpoint checkpoint)
    {
        Validate(checkpoint);

        var directoryPath = Path.GetDirectoryName(FilePath)
            ?? throw new InvalidOperationException($"Cannot determine the directory for '{FilePath}'.");
        Directory.CreateDirectory(directoryPath);

        var temporaryFilePath = FilePath + ".tmp";
        try
        {
            var json = JsonSerializer.Serialize(checkpoint, JsonOptions);
            File.WriteAllText(temporaryFilePath, json);
            File.Move(temporaryFilePath, FilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryFilePath))
            {
                File.Delete(temporaryFilePath);
            }
        }
    }

    public static SimulationCheckpoint Create(int generation, IReadOnlyList<Worm> worms)
    {
        var checkpoint = new SimulationCheckpoint
        {
            Version = CurrentVersion,
            Generation = generation,
            SavedAtUtc = DateTimeOffset.UtcNow,
        };

        foreach (var worm in worms)
        {
            if (worm.Brain is not WormNeuroBrain brain)
            {
                throw new InvalidOperationException(
                    $"Cannot save unsupported brain type '{worm.Brain.GetType().Name}'.");
            }

            checkpoint.Population.Add(brain.ExportGenome());
        }

        return checkpoint;
    }

    private static void Validate(SimulationCheckpoint checkpoint)
    {
        if (checkpoint.Version != CurrentVersion)
        {
            throw new InvalidDataException(
                $"Checkpoint version {checkpoint.Version} is not supported; expected {CurrentVersion}.");
        }

        if (checkpoint.Generation < 0)
        {
            throw new InvalidDataException("Checkpoint generation cannot be negative.");
        }

        if (checkpoint.Population is null || checkpoint.Population.Count != Constants.StartWormCount)
        {
            throw new InvalidDataException(
                $"Checkpoint population must contain exactly {Constants.StartWormCount} genomes.");
        }
    }
}
