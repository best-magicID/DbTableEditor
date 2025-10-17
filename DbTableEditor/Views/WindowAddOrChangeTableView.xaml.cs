using DbTableEditor.ViewModels;
using System.Windows;

namespace DbTableEditor.Views
{
    /// <summary>
    /// Логика взаимодействия для WindowAddOrChangeTableView.xaml
    /// </summary>
    public partial class WindowAddOrChangeTableView : Window
    {
        public WindowAddOrChangeTableView()
        {
            InitializeComponent();

            Loaded += Window_Loaded;
            Unloaded += Window_Unloaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is WindowAddOrChangeTableViewModel vm)
            {
                vm.RequestClose += Vm_RequestClose;
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is WindowAddOrChangeTableViewModel vm)
            {
                vm.RequestClose -= Vm_RequestClose;
            }
        }

        private void Vm_RequestClose()
        {
            this.Close();
        }
    }
}
