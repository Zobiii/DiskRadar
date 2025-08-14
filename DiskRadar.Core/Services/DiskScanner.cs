using DiskRadar.Core.Models;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace DiskRadar.Core.Services;

public sealed class DiskScanner
{
    private readonly IFileSystem _fs;
    private readonly ILogger<DiskScanner>? _log;

    public DiskScanner(IFileSystem? fs = null, ILogger<DiskScanner>? log = null)
    {
        _fs = fs ?? new FileSystem();
        _log = log;
    }

    public async Task<Node> ScanAsync(ScanOptions options, CancellationToken ct = default)
    {
        var rootInfo = _fs.DirectoryInfo.New(options.RootPath);
        if (!rootInfo.Exists) throw new DirectoryNotFoundException(options.RootPath);

        var root = new Node { Name = rootInfo.Name, FullPath = rootInfo.FullName, IsDirectory = true };

        await Task.Run(() =>
        {
            ComputeSizeRecursive(root, 0);
        }, ct);

        return root;

        void ComputeSizeRecursive(Node node, int depth)
        {
            ct.ThrowIfCancellationRequested();

            if (!node.IsDirectory)
            {
                try
                {
                    node.SizeBytes = _fs.FileInfo.New(node.FullPath).Length;
                }
                catch (Exception ex) { _log?.LogDebug(ex, "File skipped: {p}", node.FullPath); }
                return;
            }

            if (depth > options.MaxDepth) { node.SizeBytes = 0; return; }

            long size = 0;
            try
            {
                foreach (var di in _fs.Directory.EnumerateDirectories(node.FullPath))
                {
                    if (!options.IncludeHidden && IsHidden(di)) continue;
                    if (!options.FollowSymLinks && IsSymlink(di)) continue;

                    var child = new Node
                    {
                        Name = _fs.Path.GetFileName(di),
                        FullPath = di,
                        IsDirectory = true,
                        Parent = node
                    };

                    node.Children.Add(child);
                    ComputeSizeRecursive(child, depth + 1);
                    size += child.SizeBytes;
                }

                foreach (var fi in _fs.Directory.EnumerateFiles(node.FullPath))
                {
                    var child = new Node
                    {
                        Name = _fs.Path.GetFileName(fi),
                        FullPath = fi,
                        IsDirectory = false,
                        Parent = node
                    };

                    node.Children.Add(child);
                    ComputeSizeRecursive(child, depth + 1);
                    size += child.SizeBytes;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _log?.LogDebug(ex, "No access: {p}", node.FullPath);
            }
            catch (Exception ex)
            {
                _log?.LogDebug(ex, "Error reading: {p}", node.FullPath);
            }

            node.SizeBytes = size;

            node.Children.Sort((a, b) => b.SizeBytes.CompareTo(a.SizeBytes));

            if (node.Children.Count > 256) node.Children = node.Children.Take(256).ToList();
        }

        bool IsHidden(string path)
        {
            try
            {
                var attrs = _fs.File.GetAttributes(path);
                return attrs.HasFlag(FileAttributes.Hidden);
            }
            catch
            {
                return false;
            }
        }

        bool IsSymlink(string path)
        {
            try
            {
                var attrs = _fs.File.GetAttributes(path);
                return attrs.HasFlag(FileAttributes.ReparsePoint);
            }
            catch
            {
                return false;
            }
        }
    }
}