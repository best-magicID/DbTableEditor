namespace DbTableEditor.Models
{
    /// <summary>
    /// Информация о колонках таблицы
    /// </summary>
    public class ColumnInfoModel
    {
        /// <summary>
        /// Название колонки
        /// </summary>
        public string ColumnName { get; set; } = null!;

        /// <summary>
        /// Тип колонки
        /// </summary>
        public string DataType { get; set; } = null!;

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
        public bool IsPrimaryKey { get; set; } = false;

    }
}
