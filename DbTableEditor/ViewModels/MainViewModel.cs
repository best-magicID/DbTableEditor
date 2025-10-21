using DbTableEditor.Data;
using DbTableEditor.Helpers;
using DbTableEditor.Models;
using DbTableEditor.Services;
using DbTableEditor.Views;
using SalesAnalysis.Commands;
using System.Collections.ObjectModel;

namespace DbTableEditor.ViewModels
{
    /// <summary>
    /// ViewModel для главного окна
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        #region ПОЛЯ И СВОЙСТВА

        private readonly IGetDataFromDb _dbService;
        private readonly IWindowFactory _windowFactory;

        private TableInfoModel? _selectedTable = null!;


        /// <summary>
        /// Список имен таблиц из БД
        /// </summary>
        public ObservableCollection<string> ListTables { get; set; } = [];

        /// <summary>
        /// Список со структурой таблиц
        /// </summary>
        public ObservableCollection<TableInfoModel> ListStructureTables { get; set; } = [];

        /// <summary>
        /// Выбранная таблица
        /// </summary>
        public TableInfoModel? SelectedTable
        {
            get => _selectedTable;
            set
            {
                _selectedTable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Columns));
            }
        }

        /// <summary>
        /// Колонки выбранной таблицы
        /// </summary>
        public IEnumerable<ColumnInfoModel> Columns => SelectedTable?.Columns ?? [];


        public RaiseCommand? DeleteTableCommand { get; private set; }
        public RaiseCommand? OpenWindowCreateTableCommand { get; private set; }
        public RaiseCommand? OpenWindowChangeTableCommand { get; private set; }
        public RaiseCommand? UpdateListTablesCommand { get; private set; }

        #endregion


        #region КОНСТРУКТОР

        /// <summary>
        /// Пустой конструктор для дизайнера
        /// </summary>
        public MainViewModel()
        {

        }

        public MainViewModel(IGetDataFromDb iDbService, IWindowFactory iWindowFactory)
        {
            _dbService = iDbService;
            _windowFactory = iWindowFactory;

            if (_dbService.CheckConnect())
            {
                LoadCommands();
                //LoadNamesTables();
                LoadStructureTables();
            }
            else
            {
                GeneralMethods.ShowNotification("Нет доступа к БД.");
            }
        }

        #endregion

        #region МЕТОДЫ

        /// <summary>
        /// Загрузка команд
        /// </summary>
        public void LoadCommands()
        {
            DeleteTableCommand = new RaiseCommand(DeleteTableCommand_Execute, DeleteTableCommand_CanExecute);
            OpenWindowCreateTableCommand = new RaiseCommand(OpenWindowCreateTableCommand_Execute);
            OpenWindowChangeTableCommand = new RaiseCommand(OpenWindowChangeTableCommand_Execute, DeleteTableCommand_CanExecute);
            UpdateListTablesCommand = new RaiseCommand(UpdateListTablesCommand_Execute);
        }

        /// <summary>
        /// Загрузка имен таблиц
        /// </summary>
        public void LoadNamesTables()
        {
            var tables = _dbService.GetTables();

            ListTables.Clear();
            foreach (var nameTable in tables)
            {
                ListTables.Add(nameTable);
            }
        }

        /// <summary>
        /// Загрузка структуры таблицы
        /// </summary>
        public void LoadStructureTables()
        {
            var structureTables = _dbService.GetTablesStructure();

            ListStructureTables.Clear();
            foreach (var table in structureTables)
            {
                ListStructureTables.Add(table);
            }
        }

        /// <summary>
        /// Выполнить команду "Удалить таблицу"
        /// </summary>
        /// <param name="parameter"></param>
        private bool DeleteTableCommand_CanExecute(object parameter)
        {
            return SelectedTable != null;
        }

        /// <summary>
        /// Выполнить команду "Удалить таблицу"
        /// </summary>
        /// <param name="parameter"></param>
        private void DeleteTableCommand_Execute(object parameter)
        {
            if (SelectedTable == null) 
                return;

            var result = GeneralMethods.ShowSelectionWindow($"Вы действительно хотите удалить таблицу {SelectedTable.TableName}?");

            if (result == System.Windows.MessageBoxResult.No)
                return;

            _dbService.DeleteTable(SelectedTable.TableName);

            ListStructureTables.Remove(SelectedTable);
            SelectedTable = null;

            GeneralMethods.ShowNotification("Таблица удалена");
        }

        /// <summary>
        /// Выполнить команду "Открыть окно создания таблицы"
        /// </summary>
        /// <param name="parameter"></param>
        private void OpenWindowCreateTableCommand_Execute(object parameter)
        {
            try
            {
                var typeAction = parameter.ToString()?.ToLower() ?? string.Empty;

                if (typeAction == "add")
                {
                    var window = _windowFactory.CreateWindow<WindowAddOrChangeTableView, WindowAddOrChangeTableViewModel>(typeAction);
                    window.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                GeneralMethods.ShowNotification("Ошибка окна.\r\n\r\n" + ex.Message);
            }
            
            // После закрытия окна обновить список таблиц
        }

        /// <summary>
        /// Выполнить команду "Открыть окно редактирования таблицы"
        /// </summary>
        /// <param name="parameter"></param>
        private void OpenWindowChangeTableCommand_Execute(object parameter)
        {
            try
            {
                var typeAction = parameter.ToString()?.ToLower() ?? string.Empty;

                if (typeAction == "change" && SelectedTable != null)
                {
                    var window = _windowFactory.CreateWindow<WindowAddOrChangeTableView, WindowAddOrChangeTableViewModel>(typeAction, SelectedTable);
                    window.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                GeneralMethods.ShowNotification("Ошибка окна.\r\n\r\n" + ex.Message);
            }
        }

        private void UpdateListTablesCommand_Execute(object parameter)
        {
            LoadStructureTables();
            SelectedTable = null;
            GeneralMethods.ShowNotification("Список таблиц обновлен.");
        }

        #endregion

    }
}
