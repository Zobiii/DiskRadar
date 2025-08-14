namespace DiskRadar.Core.Models;

public sealed class Node
{
    public string Name { get; init; } = "";
    public string FullPath { get; init; } = "";
    public bool IsDirectory { get; init; }
    public long SizeBytes { get; set; }
    public List<Node> Children { get; set; } = new();
    public Node? Parent { get; init; }

    public IEnumerable<Node> Ancestors()
    {
        for (var p = Parent; p != null; p = p.Parent) yield return p;
    }
}