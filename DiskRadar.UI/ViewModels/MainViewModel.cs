using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiskRadar.Core.Models;
using DiskRadar.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace DiskRadar.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DiskScanner _scanner;
    private readonly ILogger<MainViewModel>? _log;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private string _path = "/";
    [ObservableProperty] private Node? _root;
    [ObservableProperty] private Node? _hoverNode;
    [ObservableProperty] private bool _isScanning;

    public ObservableCollection<string> LogLines { get; } = new();

    public MainViewModel(DiskScanner scanner, ILogger<MainViewModel>? log = null)
    {
        _scanner = scanner;
        _log = log;
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        if (_isScanning) return;
        _cts = new();
        IsScanning = true;
        Log("Scan start: " + Path);

        try
        {
            Root = await _scanner.ScanAsync(new ScanOptions { RootPath = Path }, _cts.Token);
            Log("Scan done.");
        }
        catch (OperationCanceledException)
        {
            Log("Abgebrochen");
        }
        finally { IsScanning = false; }
    }

    [RelayCommand] private void Cancel() => _cts?.Cancel();

    private void Log(string msg)
    {
        if (LogLines.Count > 1000) LogLines.RemoveAt(0);
        LogLines.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
        _log?.LogInformation(msg);
    }
}