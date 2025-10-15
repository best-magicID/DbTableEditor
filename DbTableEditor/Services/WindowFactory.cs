using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DbTableEditor.Services
{
    public class WindowFactory : IWindowFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TWindow CreateWindow<TWindow, TViewModel>()
            where TWindow : Window
            where TViewModel : class
        {

            // если окно не зарегистрировано, создаём через new()
            var window = _serviceProvider.GetService<TWindow>() ?? Activator.CreateInstance<TWindow>();

            var viewModel = ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider);

            window.DataContext = viewModel;
            return window;
        }

        public TWindow CreateWindow<TWindow, TViewModel>(params object[] parameters)
            where TWindow : Window
            where TViewModel : class
        {
            var window = _serviceProvider.GetService<TWindow>() ?? Activator.CreateInstance<TWindow>();

            // создаём VM вручную, передаём параметры
            var viewModel = ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider, parameters);

            window.DataContext = viewModel;
            return window;
        }
    }
}
