using DbTableEditor.Data;
using System.Collections.ObjectModel;

namespace DbTableEditor.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IGetDataFromDb _dbService;

        public ObservableCollection<string> Tables { get; set; } = [];

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
                var tables = _dbService.GetTables();

                Tables.Clear();

                foreach (var nameTable in tables)
                {
                    Tables.Add(nameTable);
                }
            }
        }

    }
}
