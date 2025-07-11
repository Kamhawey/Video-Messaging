using Blazored.Toast;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedClassLibrary.Config;
using SharedClassLibrary.Services;
using SharedClassLibrary.Services.Auth;
using SharedClassLibrary.Services.WS;
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
            services.AddSingleton<IConfiguration>(provider =>
            {
                return new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["ApiUrl"] = "https://localhost:8887"
                    }).Build();
            });

            // Add HttpClient
            services.AddHttpClient<AuthService>();
            services.AddHttpClient<VideoApiService>();

            #region Configuration
            IConfiguration configuration = context.Configuration;

            services.Configure<WebSocketSettings>(
              configuration.GetSection("WebSocketSettings"));

            services.Configure<VideoApiSettings>(
                configuration.GetSection("VideoApiSettings"));

            #endregion
            // Add services
            services.AddSingleton<ITokenStorageService, TokenStorageService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IVideoApiService, VideoApiService>();
            services.AddScoped<IWebSocketService, WebSocketService>();



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
