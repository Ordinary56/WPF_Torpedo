using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using WPF_Torpedo.Models;
using WPF_Torpedo.Pages;
using WPF_Torpedo.Services;

namespace WPF_Torpedo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        readonly IServiceProvider _provider;
        public App()
        {
            IServiceCollection collection = new ServiceCollection();
            collection.AddSingleton<StateManager>();
            collection.AddSingleton<Player>();
            collection.AddSingleton<MainWindow>();
            collection.AddSingleton<MainMenu>();
            collection.AddSingleton<IPageNavigator, PageNavigator>();
            collection.AddSingleton<Func<Type, Page>>(provider => page => (Page)provider.GetRequiredService(page));
            collection.AddSingleton<CreateTable>();
            collection.AddSingleton<Gameplay>();
            // TODO: add pages here
            _provider = collection.BuildServiceProvider();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            _provider.GetRequiredService<IPageNavigator>().CurrentPage = _provider.GetRequiredService<MainMenu>();
            MainWindow window = _provider.GetRequiredService<MainWindow>();
            window.Show();
        }
    }
}

