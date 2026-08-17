using System.Text.Json;
using NeuroWorms.Core;
using NeuroWorms.Core.Neuro;

namespace NeuroWorms.Tests;

public class CheckpointTests
{
    [Fact]
    public void MissingCheckpointStartsAtZeroAndCreatesJsonFile()
    {
        var checkpointPath = CreateCheckpointPath();
        try
        {
            var engine = new SimulationEngine(checkpointPath);

            Assert.Equal(0, engine.CurrentGeneration);
            Assert.Equal(Constants.StartWormCount, engine.Worms.Count);
            Assert.Equal(Path.GetFullPath(checkpointPath), engine.SaveFilePath);
            Assert.True(File.Exists(checkpointPath));

            using var json = JsonDocument.Parse(File.ReadAllText(checkpointPath));
            Assert.Equal(2, json.RootElement.GetProperty("version").GetInt32());
            Assert.Equal(0, json.RootElement.GetProperty("generation").GetInt32());
            Assert.Equal(
                Constants.StartWormCount,
                json.RootElement.GetProperty("population").GetArrayLength());
            var firstGenome = json.RootElement.GetProperty("population")[0];
            Assert.Equal(19, firstGenome.GetProperty("biases").GetArrayLength());
            Assert.Equal(258, firstGenome.GetProperty("weights").GetArrayLength());
        }
        finally
        {
            DeleteCheckpoint(checkpointPath);
        }
    }

    [Fact]
    public async Task ExistingCheckpointRestoresGenerationAndPopulationGenomes()
    {
        var checkpointPath = CreateCheckpointPath();
        try
        {
            var originalEngine = new SimulationEngine(checkpointPath);
            SetCurrentTick(originalEngine, Constants.MaxGenerationTicks);
            await originalEngine.NextMove();

            var originalGenome = Assert.IsType<WormNeuroBrain>(originalEngine.Worms[0].Brain)
                .ExportGenome();

            var restoredEngine = new SimulationEngine(checkpointPath);
            var restoredGenome = Assert.IsType<WormNeuroBrain>(restoredEngine.Worms[0].Brain)
                .ExportGenome();

            Assert.Equal(1, restoredEngine.CurrentGeneration);
            Assert.Equal(0, restoredEngine.CurrentTick);
            Assert.Equal(Constants.StartWormCount, restoredEngine.Worms.Count);
            Assert.All(restoredEngine.Worms, worm => Assert.Equal(0, worm.Age));
            Assert.Equal(originalGenome.Biases, restoredGenome.Biases);
            Assert.Equal(originalGenome.Weights, restoredGenome.Weights);
        }
        finally
        {
            DeleteCheckpoint(checkpointPath);
        }
    }

    [Fact]
    public void InvalidCheckpointDoesNotSilentlyRestartAtZero()
    {
        var checkpointPath = CreateCheckpointPath();
        try
        {
            File.WriteAllText(checkpointPath, "{ this is not valid JSON }");

            var exception = Assert.Throws<InvalidDataException>(
                () => new SimulationEngine(checkpointPath));

            Assert.Contains(checkpointPath, exception.Message);
        }
        finally
        {
            DeleteCheckpoint(checkpointPath);
        }
    }

    [Fact]
    public void OldNetworkCheckpointFailsWithExplicitVersionError()
    {
        var checkpointPath = CreateCheckpointPath();
        try
        {
            _ = new SimulationEngine(checkpointPath);
            var json = File.ReadAllText(checkpointPath)
                .Replace("\"version\": 2", "\"version\": 1", StringComparison.Ordinal);
            File.WriteAllText(checkpointPath, json);

            var exception = Assert.Throws<InvalidDataException>(
                () => new SimulationEngine(checkpointPath));

            Assert.Contains("version 1", exception.Message);
            Assert.Contains("expected 2", exception.Message);
        }
        finally
        {
            DeleteCheckpoint(checkpointPath);
        }
    }

    [Fact]
    public async Task DisabledCheckpointLoadingStartsAtZeroAndReplacesSaveNormally()
    {
        var checkpointPath = CreateCheckpointPath();
        try
        {
            var previousRun = new SimulationEngine(checkpointPath);
            SetCurrentTick(previousRun, Constants.MaxGenerationTicks);
            await previousRun.NextMove();
            Assert.Equal(1, previousRun.CurrentGeneration);
            Assert.True(File.Exists(checkpointPath));

            var cleanRun = new SimulationEngine(checkpointPath, loadCheckpoint: false);

            Assert.Equal(0, cleanRun.CurrentGeneration);
            Assert.True(File.Exists(checkpointPath));

            var restoredCleanRun = new SimulationEngine(checkpointPath);
            Assert.Equal(0, restoredCleanRun.CurrentGeneration);
        }
        finally
        {
            DeleteCheckpoint(checkpointPath);
        }
    }

    private static string CreateCheckpointPath()
    {
        return Path.Combine(Path.GetTempPath(), $"neuroworms-checkpoint-{Guid.NewGuid():N}.json");
    }

    private static void DeleteCheckpoint(string checkpointPath)
    {
        File.Delete(checkpointPath);
        File.Delete(checkpointPath + ".tmp");
    }

    private static void SetCurrentTick(SimulationEngine engine, int value)
    {
        var setter = typeof(SimulationEngine)
            .GetProperty(nameof(SimulationEngine.CurrentTick))!
            .GetSetMethod(nonPublic: true)!;
        setter.Invoke(engine, [value]);
    }
}
