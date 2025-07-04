using Microsoft.AspNetCore.Components.WebView.Wpf;
using System.Windows;

namespace DesktopVersion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
            // Set the services for the BlazorWebView
            blazorWebView.Services = serviceProvider;

            // Add the root component programmatically
            blazorWebView.RootComponents.Add(new RootComponent
            {
                Selector = "#app",
                ComponentType = typeof(SharedClassLibrary.App)
            });
        }
    }
}