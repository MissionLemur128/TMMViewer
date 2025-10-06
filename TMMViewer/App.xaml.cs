using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TMMViewer.ViewModels;
using TMMViewer.Views;

namespace TMMViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs eventArgs)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>();
            mainWindow.Show();
        }

        private void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<MainWindow>();
            serviceCollection.AddSingleton<MainWindowViewModel>();
            serviceCollection.AddTransient<IDialogService, DialogService>();
        }
    }
}
