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
        private TableInfoModel? _selectedTable = null!;

        /// <summary>
        /// Колонки выбранной таблицы
        /// </summary>
        public IEnumerable<ColumnInfoModel> Columns => SelectedTable?.Columns ?? [];

        #endregion

        public RaiseCommand DeleteTableCommand { get; private set; } 
        public RaiseCommand OpenCreateWindowCommand { get; private set; }

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
        }

        #endregion

        #region МЕТОДЫ

        public void LoadCommands()
        {
            DeleteTableCommand = new RaiseCommand(DeleteTableCommand_Execute);
            OpenCreateWindowCommand = new RaiseCommand(OpenCreateWindowCommand_Execute);
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

        private void OpenCreateWindowCommand_Execute(object parameter)
        {
            var window = _windowFactory.CreateWindow<WindowAddOrChangeTableView, MainViewModel>();
            window.ShowDialog();

            // После закрытия окна можно обновить список таблиц
            // например:
            // Tables = new ObservableCollection<TableInfo>(_dbService.GetTablesStructure());
        }

        #endregion

    }
}
