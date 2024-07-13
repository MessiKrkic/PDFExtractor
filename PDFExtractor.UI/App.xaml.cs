using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PDFExtractor.Core.Interfaces;
using PDFExtractor.Core.Services;
using PDFExtractor.UI.ViewModels;
using System.Windows;

namespace PDFExtractor.UI
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
            services.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
            services.AddSingleton<IDocumentParser, PDFParser>();
            services.AddSingleton<IPDFGenerator, PDFGenerator>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainWindow>();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (_serviceProvider is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                _serviceProvider.Dispose();
            }
        }
    }
}