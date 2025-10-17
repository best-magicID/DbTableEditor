using DbTableEditor.Data;
using DbTableEditor.Helpers;
using DbTableEditor.Models;
using DbTableEditor.Services;
using SalesAnalysis.Commands;
using System.Collections.ObjectModel;

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
                Columns = new ObservableCollection<ColumnInfoModel>(NewOrChangeTable.Columns);
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

            Columns.Remove(SelectedRow);
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
            Columns.Add(new ColumnInfoModel { ColumnName = "", DataType = SqlDataType.NVarChar, IsNullable = false, IsPrimaryKey= false });
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
                    bool result = false;
                    if (!_isChangeTable)
                    {
                        result =_dbService.CreateTable(NewOrChangeTable);
                    }
                    else
                    {
                        /*result = */_dbService.ChangeTable(NewOrChangeTable);
                    }
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
