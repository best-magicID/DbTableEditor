using DbTableEditor.Data;
using DbTableEditor.Models;
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

        #region КОНСТРУКТОР

        /// <summary>
        /// Пустой конструктор для дизайнера
        /// </summary>
        public MainViewModel()
        {

        }

        public MainViewModel(IGetDataFromDb dbService)
        {
            _dbService = dbService;

            if (_dbService.CheckConnect())
            {
                //LoadNamesTables();
                LoadStructureTables();
            }
        }

        #endregion

        #region МЕТОДЫ

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

        #endregion

    }
}
