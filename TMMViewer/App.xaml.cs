using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using TMMViewer.Data.Render;
using TMMViewer.Data.Services;
using TMMViewer.ViewModels;
using TMMViewer.ViewModels.MonoGameControls;
using TMMViewer.Views;

namespace TMMViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs eventArgs)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<MainWindow>();
            serviceCollection.AddTransient<MainWindowViewModel>();

            serviceCollection.AddTransient<IDialogService, DialogService>();
            serviceCollection.AddSingleton<IModelIOService, IOModelService>();
            serviceCollection.AddSingleton<IMonoGameViewModel, ModelViewer>();
            serviceCollection.AddSingleton<Scene>();
        }
    }
}
