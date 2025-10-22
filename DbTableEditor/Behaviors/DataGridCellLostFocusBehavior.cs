using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DbTableEditor.Behaviors
{
    public class DataGridCellLostFocusBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(DataGridCellLostFocusBehavior));

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.CellEditEnding += OnCellEditEnding;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.CellEditEnding -= OnCellEditEnding;
        }

        private void OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (Command?.CanExecute(e) == true)
                Command.Execute(e);
        }
    }

}
