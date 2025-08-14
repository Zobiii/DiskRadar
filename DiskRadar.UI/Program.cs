using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using DiskRadar.Core.Services;
using DiskRadar.UI;
using DiskRadar.UI.ViewModels;
using DiskRadar.UI.Views;
using Microsoft.Extensions.Logging;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var app = BuildAvaloniaApp();

        // Optional: "--console" öffnet Log in StdOut (wenn aus GUI gestartet)
        bool withConsole = args.Contains("--console");
        using var loggerFactory = LoggerFactory.Create(b =>
        {
            if (withConsole) b.AddSimpleConsole();
        });

        var scanner = new DiskScanner(log: loggerFactory.CreateLogger<DiskScanner>());
        var vm = new MainViewModel(scanner, loggerFactory.CreateLogger<MainViewModel>());

        app.StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>().UsePlatformDetect().UseReactiveUI().LogToTrace();
}