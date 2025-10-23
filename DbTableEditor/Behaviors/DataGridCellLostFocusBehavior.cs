using DbTableEditor.Models;
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

        private string? _oldValue1;
        private SqlDataType? _oldValue2;
        private bool? _oldValue3;


        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.BeginningEdit += OnBeginningEdit;
            AssociatedObject.CellEditEnding += OnCellEditEnding;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.BeginningEdit -= OnBeginningEdit;
            AssociatedObject.CellEditEnding -= OnCellEditEnding;
        }

        private void OnBeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Item != null && e.Row.Item is ColumnInfoModel column)
            {
                _oldValue1 = new string(column.ColumnName.ToString());
                _oldValue2 = Enum.TryParse(column.DataType.ToString(), out SqlDataType dataTypeParsed) ? dataTypeParsed : SqlDataType.NVarChar;
                _oldValue3 = bool.TryParse(column.IsPrimaryKey.ToString(), out bool isPrimaryKeyParsed) && isPrimaryKeyParsed;
            }
        }

        private void OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item is null)
                return;

            object? newValue = null;

            if (e.EditingElement is TextBox textBox)
                newValue = textBox.Text;

            else if (e.EditingElement is ComboBox comboBox)
                newValue = comboBox.SelectedValue ?? comboBox.Text;

            else if (e.EditingElement is CheckBox checkBox)
                newValue = checkBox.IsChecked;

            var parameter = new DataGridEditInfo
            {
                RowItem = e.Row.Item,
                ColumnHeader = e.Column.Header?.ToString(),
                OldValue1 = _oldValue1,
                OldValue2 = _oldValue2,
                OldValue3 = _oldValue3,
                NewValue = newValue
            };

            if (Command?.CanExecute(parameter) == true)
                Command.Execute(parameter);
        }
    }

    public class DataGridEditInfo
    {
        public object? RowItem { get; set; }
        public string? ColumnHeader { get; set; }
        public string? OldValue1 { get; set; }
        public SqlDataType? OldValue2 { get; set; }
        public bool? OldValue3 { get; set; }
        public object? NewValue { get; set; }
    }

}
