namespace DiskRadar.Core.Services;

public sealed class ScanOptions
{
    public string RootPath { get; init; } = "/";
    public int MaxDepth { get; init; } = 8;
    public bool FollowSymLinks { get; init; } = false;
    public bool IncludeHidden { get; init; } = true;
}