using System.Windows;

namespace DbTableEditor.Services
{
    public interface IWindowFactory
    {
        TWindow CreateWindow<TWindow, TViewModel>()
            where TWindow : Window
            where TViewModel : class;

        TWindow CreateWindow<TWindow, TViewModel>(params object[] parameters)
            where TWindow : Window
            where TViewModel : class;

    }
}
