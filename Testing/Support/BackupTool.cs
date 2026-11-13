using System.Diagnostics;
using System.Text;

namespace VetControl.Testing;

public static class BackupTool
{
    public static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "VetControl.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository not found.");
    }

    public static async Task<string> ContainerAsync()
    {
        var configured = Environment.GetEnvironmentVariable("VET_TEST_PG_CONTAINER");
        if (!string.IsNullOrEmpty(configured)) return configured;
        var result = await Run("docker", ["ps", "-q", "--filter", "ancestor=postgres:18-alpine"], FindRoot());
        var found = result.Output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        return result.Exit == 0 && found.Length == 1 ? found[0] :
            throw new InvalidOperationException("Configure VET_TEST_PG_CONTAINER com o container do PostgreSQL de testes.");
    }

    public static async Task<(int Exit, string Output)> Run(string file, string[] arguments, string directory)
    {
        var start = new ProcessStartInfo(file) { WorkingDirectory = directory, RedirectStandardOutput = true,
            RedirectStandardError = true, StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false, CreateNoWindow = true };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch { process.Kill(entireProcessTree: true); throw; }
        return (process.ExitCode, await stdout + await stderr);
    }
}
