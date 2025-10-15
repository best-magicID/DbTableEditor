namespace DbTableEditor.Models
{
    /// <summary>
    /// Информация о таблице
    /// </summary>
    public class TableInfoModel
    {
        /// <summary>
        /// Название таблицы
        /// </summary>
        public string TableName { get; set; } = null!;

        /// <summary>
        /// Список столбцов
        /// </summary>
        public List<ColumnInfoModel> Columns { get; set; } = [];
    }
}
