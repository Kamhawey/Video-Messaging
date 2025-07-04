using Blazored.Toast;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using Video.Messaging.App.Extensions;

namespace DesktopVersion;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        var builder = Host.CreateDefaultBuilder(e.Args);

        builder.ConfigureServices((context, services) =>
        {
            // Add Blazor WebView services
            services.AddWpfBlazorWebView();

            // Add your shared services from the class library

            // Add your shared services from the class library
            services.AddVideoMessagingServices();
            services.AddBlazoredToast();

            // Add logging if needed
            services.AddLogging();
            // Add logging if needed
            services.AddLogging();
        });

        _host = builder.Build();

        await _host.StartAsync();

        // Create and initialize the main window
        var mainWindow = new MainWindow();
        mainWindow.Initialize(_host.Services);
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
