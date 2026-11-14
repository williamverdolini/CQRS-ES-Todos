namespace CqrsEsTodo.Infrastructure;

public sealed class LegacyImportRun
{
    public string BundleSha256 { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int CommitCount { get; set; }
    public int SourceEventCount { get; set; }
    public int ImportedEventCount { get; set; }
    public DateTimeOffset ImportedAtUtc { get; set; }
}