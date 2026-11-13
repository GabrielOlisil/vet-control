namespace VetControl.Testing;

public sealed class TemporaryKeyDirectory : IDisposable
{
    private readonly string root = System.IO.Path.GetFullPath(System.IO.Path.GetTempPath());
    private readonly string name = "vet_test_keys_" + Guid.NewGuid().ToString("N");
    public string Path { get; }

    public TemporaryKeyDirectory()
    {
        Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(root, name));
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        var resolved = System.IO.Path.GetFullPath(Path);
        if (!string.Equals(System.IO.Path.GetDirectoryName(resolved)?.TrimEnd(System.IO.Path.DirectorySeparatorChar),
                root.TrimEnd(System.IO.Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
            System.IO.Path.GetFileName(resolved) != name)
            throw new InvalidOperationException("Refusing to remove an unowned key directory.");

        if (Directory.Exists(resolved)) Directory.Delete(resolved, recursive: true);
    }
}
