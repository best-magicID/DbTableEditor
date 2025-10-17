namespace DbTableEditor.Models
{
    /// <summary>
    /// Информация о колонках таблицы
    /// </summary>
    public class ColumnInfoModel : BaseModel
    {
        private string _columnName = string.Empty;
        private SqlDataType _dataType = SqlDataType.VarChar;
        private bool _isPrimaryKey = false;

        /// <summary>
        /// Название колонки
        /// </summary>
        public string ColumnName
        {
            get => _columnName;
            set
            {
                _columnName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Тип колонки
        /// </summary>
        public SqlDataType DataType
        {
            get => _dataType;
            set
            {
                _dataType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Максимальная длина строки
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Может быть Null или нет
        /// </summary>
        public bool IsNullable { get; set; } = false;

        /// <summary>
        /// Стандартное значение
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Первичный ключ
        /// </summary>
        public bool IsPrimaryKey
        {             
            get => _isPrimaryKey;
            set
            {
                _isPrimaryKey = value;
                OnPropertyChanged();
            }
        }
    }
}
