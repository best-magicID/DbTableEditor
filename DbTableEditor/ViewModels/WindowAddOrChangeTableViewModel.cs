using DbTableEditor.Data;
using DbTableEditor.Helpers;
using DbTableEditor.Models;
using DbTableEditor.Services;
using SalesAnalysis.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DbTableEditor.ViewModels
{
    public class WindowAddOrChangeTableViewModel : BaseViewModel
    {
        #region ПОЛЯ И СВОЙСТВА

        private readonly IChangeDataFromDb _iChangeData;

        private readonly bool _isChangeTable = false;

        private string _nameWindow = "Окно добавления или редактирования таблицы";

        private TableInfoModel _newOrChangeTable = new();
        private ColumnInfoModel? _selectedRow = null;

        public event Action? RequestClose;

        /// <summary>
        /// Название окна
        /// </summary>
        public string NameWindow
        {
            get => _nameWindow;
            set => SetProperty(ref _nameWindow, value);
        }

        /// <summary>
        /// Выбранная таблица
        /// </summary>
        public TableInfoModel NewOrChangeTable 
        { 
            get => _newOrChangeTable; 
            set => SetProperty(ref _newOrChangeTable, value);
        }

        public string OldNameTable { get; set; } = string.Empty;

        /// <summary>
        /// Колонки таблицы
        /// </summary>
        public ObservableCollection<ColumnInfoModel> Columns { get; set; } = [];

        public ObservableCollection<ColumnInfoModel> OldColumns { get; set; } = [];

        /// <summary>
        /// Выбранная колонка
        /// </summary>
        public ColumnInfoModel? SelectedRow
        {
            get => _selectedRow;
            set => SetProperty(ref _selectedRow, value);
        }

        public IReadOnlyList<SqlDataType> DataTypes => DataTypeProvider.CommonTypes;

        public Visibility IsAddTable
        {
            get
            {
                if(_isChangeTable == false)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        #region Команды

        public RaiseCommand? AddColumnCommand { get; private set; }
        public RaiseCommand? DeleteColumnCommand { get; private set; }
        public RaiseCommand? SaveTableCommand { get; private set; }
        public RaiseCommand? TextBoxLostFocusCommand { get; private set; }
        public RaiseCommand? DataGridLostFocusCommand { get; private set; }


        #endregion

        #endregion

        #region КОНСТРУКТОРЫ

        /// <summary>
        /// Пустой конструктор для дизайнера
        /// </summary>
        public WindowAddOrChangeTableViewModel()
        {

        }

        public WindowAddOrChangeTableViewModel(IChangeDataFromDb iChangeData, string parameter, TableInfoModel? table = null)
        {
            _iChangeData = iChangeData;

            _isChangeTable = parameter == "change";

            LoadCommands();

            ChangeActionWindow(table);
        }

        #endregion

        #region МЕТОДЫ

        /// <summary>
        /// Загрузка команд
        /// </summary>
        public void LoadCommands()
        {
            AddColumnCommand = new RaiseCommand(AddColumnCommand_Execute);
            DeleteColumnCommand = new RaiseCommand(DeleteColumnCommand_Execute, DeleteColumnCommand_CanExecute);

            SaveTableCommand = new RaiseCommand(SaveTableCommand_Execute);

            TextBoxLostFocusCommand = new RaiseCommand(TextBoxLostFocusCommand_Execute, TextBoxLostFocusCommand_CanExecute);
            DataGridLostFocusCommand = new RaiseCommand(DataGridLostFocusCommand_Execute, DataGridLostFocusCommand_CanExecute);
        }

        /// <summary>
        /// Изменение назначения окна в зависимости от параметров
        /// </summary>
        /// <param name="parameter"></param>
        private void ChangeActionWindow(TableInfoModel? table = null)
        {
            if (!_isChangeTable)
            {
                NameWindow = "Окно добавления таблицы";
            }
            else
            {
                NameWindow = "Окно редактирования таблицы";
                if (table != null)
                {
                    NewOrChangeTable = table;

                    Columns = NewOrChangeTable.Columns;
                    OldColumns = new ObservableCollection<ColumnInfoModel>(NewOrChangeTable.Columns);

                    OldNameTable = NewOrChangeTable.TableName;
                }
            }
        }

        /// <summary>
        /// Выполнить команду "Удалить колонку"
        /// </summary>
        /// <param name="parameter"></param>
        private void DeleteColumnCommand_Execute(object parameter)
        {
            if (SelectedRow == null)
                return;

            if(_isChangeTable)
            {
                var result = GeneralMethods.ShowSelectionWindow($"Вы действительно хотите удалить колонку {SelectedRow.ColumnName}?");
                if (result == MessageBoxResult.No)
                    return;

                if (SelectedRow.IsPrimaryKey)
                {
                    GeneralMethods.ShowNotification("Нельзя удалить колонку, являющуюся первичным ключом.");
                    return;
                }
                else if(_iChangeData.CheckConnect())
                {
                    var resultDb = _iChangeData.DeleteColumn(NewOrChangeTable, SelectedRow);

                    if (resultDb)
                    {
                        NewOrChangeTable.Columns.Remove(SelectedRow);
                        GeneralMethods.ShowNotification("Колонка успешно удалена.");
                        return;
                    }
                    else
                    {
                        GeneralMethods.ShowNotification("Ошибка удаления колонки.");
                        return;
                    }
                }
            }
            else
            {
                Columns.Remove(SelectedRow);
            }
        }

        /// <summary>
        /// Выполнить проверку выполнения команды "Удалить колонку"
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool DeleteColumnCommand_CanExecute(object parameter)
        {
            return SelectedRow != null;
        }

        /// <summary>
        /// Выполнить команду "Добавить колонку"
        /// </summary>
        /// <param name="parameter"></param>
        private void AddColumnCommand_Execute(object parameter)
        {
            if (_isChangeTable && _iChangeData.CheckConnect())
            {
                var newNumber = NewOrChangeTable.Columns.Count + 1;
                var newColumn = new ColumnInfoModel { ColumnName = $"Новый столбец {newNumber}", DataType = SqlDataType.NVarChar, IsNullable = false, IsPrimaryKey = false };

                var result = _iChangeData.AddColumn(NewOrChangeTable, newColumn);

                if (result)
                {
                    NewOrChangeTable.Columns.Add(newColumn);
                    GeneralMethods.ShowNotification("Колонка успешно добавлена.");
                    return;
                }
                else
                {
                    GeneralMethods.ShowNotification("Ошибка добавления колонки.");
                    return;
                }
            }
            else
            {
                Columns.Add(new ColumnInfoModel { ColumnName = "Новый столбец", DataType = SqlDataType.NVarChar, IsNullable = true, IsPrimaryKey = false });
            }
        }

        /// <summary>
        /// Выполнить команду "Сохранить таблицу"
        /// </summary>
        /// <param name="parameter"></param>
        private void SaveTableCommand_Execute(object parameter)
        {
            if (!string.IsNullOrWhiteSpace(NewOrChangeTable.TableName))
            {
                NewOrChangeTable.Columns = Columns;

                int countPrimaryKey = 0;    

                foreach (var col in NewOrChangeTable.Columns)
                {
                    if (string.IsNullOrWhiteSpace(col.ColumnName))
                    {
                        GeneralMethods.ShowNotification("Имя колонки не может быть пустым.");
                        return;
                    }
                    if(col.IsPrimaryKey)
                    {
                        countPrimaryKey++;
                        if (countPrimaryKey > 1)
                        {
                            GeneralMethods.ShowNotification("В таблице больше одного первичного ключа. Оставьте только один!");
                            return;
                        }
                        //if (col.IsNullable)
                        //{
                        //    GeneralMethods.ShowNotification("Колонка не может быть одновременно первичным ключом и допускающей NULL.");
                        //    return;
                        //}
                    }
                }

                if (_iChangeData.CheckConnect())
                {
                    var result =_iChangeData.CreateTable(NewOrChangeTable);

                    if (result)
                    {
                        GeneralMethods.ShowNotification("Таблица успешно сохранена.");
                        OnClose();
                    }
                    else
                    {
                        GeneralMethods.ShowNotification("Ошибка сохранения таблицы.");
                    }
                }
            }
            else
            {
                GeneralMethods.ShowNotification("Ошибка сохранения таблицы. Нет названия таблицы");
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool TextBoxLostFocusCommand_CanExecute(object parameter)
        {
            return _isChangeTable && OldNameTable != NewOrChangeTable.TableName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        private void TextBoxLostFocusCommand_Execute(object parameter)
        {
            if (_isChangeTable)
            {
                if (string.IsNullOrWhiteSpace(NewOrChangeTable.TableName))
                {
                    NewOrChangeTable.TableName = OldNameTable;
                    GeneralMethods.ShowNotification("Нельзя оставлять поле пустым.");
                    return;
                }

                var result1 = GeneralMethods.ShowSelectionWindow($"Изменить имя таблицы с '{OldNameTable}' на '{NewOrChangeTable.TableName}'?");

                if (result1 == MessageBoxResult.Yes)
                {
                    var result2 = _iChangeData.ChangeNameTable(OldNameTable, NewOrChangeTable);
                    if (result2)
                    {
                        OldNameTable = NewOrChangeTable?.TableName ?? OldNameTable;
                        GeneralMethods.ShowNotification($"Имя таблицы изменено на '{NewOrChangeTable?.TableName}'.");
                    }
                }
                else
                {
                    NewOrChangeTable.TableName = OldNameTable;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool DataGridLostFocusCommand_CanExecute(object parameter)
        {
            return _isChangeTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        private void DataGridLostFocusCommand_Execute(object parameter)
        {
            if (_isChangeTable)
            {
                if (parameter is not DataGridCellEditEndingEventArgs e)
                    return;

                // Получаем изменяемую колонку и строку
                var editedItem = (ColumnInfoModel)e.Row.Item;
                var columnHeader = e.Column.Header?.ToString();

                if (e.EditingElement is TextBox textBox)
                {
                    var result = GeneralMethods.ShowSelectionWindow($"Изменить имя столбца с '{editedItem.ColumnName}' на '{textBox.Text}'?");
                    if(result == MessageBoxResult.Yes)
                    {
                        _iChangeData.ChangeNameColumn(NewOrChangeTable, editedItem.ColumnName, textBox.Text);
                        GeneralMethods.ShowNotification("Имя столбца изменено.");
                    }
                    else
                    {
                        editedItem.ColumnName = editedItem.ColumnName;
                    }
                }
                else if (e.EditingElement is ComboBox comboBox)
                {
                    var result = GeneralMethods.ShowSelectionWindow($"Изменить тип столбца с '{editedItem.DataType}' на '{comboBox.SelectedValue}'?");
                    if (result == MessageBoxResult.Yes)
                    {

                        GeneralMethods.ShowNotification("Тип столбца изменен.");
                    }
                    else
                    {

                    }
                }
                else if (e.EditingElement is CheckBox checkBox)
                {
                    var result = GeneralMethods.ShowSelectionWindow($"Изменить значение первичного ключа с '{editedItem.IsPrimaryKey}' на '{checkBox.IsChecked}'?");
                    if (result == MessageBoxResult.Yes)
                    {

                        GeneralMethods.ShowNotification("Значение первичного ключа изменено.");
                    }
                    else
                    {
                        editedItem.IsPrimaryKey = editedItem.IsPrimaryKey;
                    }
                }
            }
        }

        /// <summary>
        /// Закрытие окна
        /// </summary>
        private void OnClose()
        {
            RequestClose?.Invoke();
        }


        #endregion

    }
}
