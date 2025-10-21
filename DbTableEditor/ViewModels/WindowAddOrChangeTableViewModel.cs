using DbTableEditor.Data;
using DbTableEditor.Helpers;
using DbTableEditor.Models;
using DbTableEditor.Services;
using SalesAnalysis.Commands;
using System.Collections.ObjectModel;
using System.Windows;

namespace DbTableEditor.ViewModels
{
    public class WindowAddOrChangeTableViewModel : BaseViewModel
    {
        #region ПОЛЯ И СВОЙСТВА

        private readonly IGetDataFromDb? _dbService;

        private string _nameWindow = "Окно добавления или редактирования таблицы";

        private bool _isChangeTable = false;

        private TableInfoModel? _newOrChangeTable = null!;
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
        public TableInfoModel? NewOrChangeTable 
        { 
            get => _newOrChangeTable; 
            set => SetProperty(ref _newOrChangeTable, value);
        }

        /// <summary>
        /// Колонки таблицы
        /// </summary>
        public ObservableCollection<ColumnInfoModel> Columns { get; set; } = [];

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
        public RaiseCommand? SaveTableCommand { get; set; }

        #endregion

        #endregion

        #region КОНСТРУКТОРЫ

        /// <summary>
        /// Пустой конструктор для дизайнера
        /// </summary>
        public WindowAddOrChangeTableViewModel()
        {

        }

        public WindowAddOrChangeTableViewModel(IGetDataFromDb dbService, string parameter, TableInfoModel? table = null)
        {
            _dbService = dbService;

            _isChangeTable = parameter == "change";

            LoadCommands();

            ChangeActionWindow(parameter, table);
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
        }

        /// <summary>
        /// Изменение назначения окна в зависимости от параметров
        /// </summary>
        /// <param name="parameter"></param>
        private void ChangeActionWindow(string parameter, TableInfoModel? table = null)
        {
            if (!_isChangeTable)
            {
                NameWindow = "Окно добавления таблицы";
                NewOrChangeTable = new TableInfoModel();
            }
            else if (_isChangeTable && table != null)
            {
                NameWindow = "Окно редактирования таблицы";
                NewOrChangeTable = table;
                Columns = NewOrChangeTable.Columns;
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
                else if(_dbService != null && _dbService.CheckConnect() && NewOrChangeTable != null)
                {
                    var resultDb = _dbService.DeleteColumn(NewOrChangeTable, SelectedRow);

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
            if (_isChangeTable && _dbService != null && _dbService.CheckConnect() && NewOrChangeTable != null)
            {
                var newNumber = NewOrChangeTable.Columns.Count + 1;
                var newColumn = new ColumnInfoModel { ColumnName = $"Новый столбец {newNumber}", DataType = SqlDataType.NVarChar, IsNullable = false, IsPrimaryKey = false };

                var result = _dbService.AddColumn(NewOrChangeTable, newColumn);

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
            if (NewOrChangeTable != null && !string.IsNullOrWhiteSpace(NewOrChangeTable.TableName))
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

                if (_dbService != null && _dbService.CheckConnect())
                {
                    var result =_dbService.CreateTable(NewOrChangeTable);

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
        /// Закрытие окна
        /// </summary>
        private void OnClose()
        {
            RequestClose?.Invoke();
        }


        #endregion

    }
}
