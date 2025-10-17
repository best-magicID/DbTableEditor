using System.Collections.ObjectModel;

namespace DbTableEditor.Models
{
    /// <summary>
    /// Информация о таблице
    /// </summary>
    public class TableInfoModel : BaseModel
    {
        private string _tableName = string.Empty;

        /// <summary>
        /// Название таблицы
        /// </summary>
        public string TableName 
        { 
            get => _tableName; 
            set 
            { 
                _tableName = value; 
                OnPropertyChanged(); 
            }
        }

        /// <summary>
        /// Список столбцов
        /// </summary>
        public ObservableCollection<ColumnInfoModel> Columns { get; set; } = [];
    }
}
