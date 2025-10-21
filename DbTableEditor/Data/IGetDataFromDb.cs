using DbTableEditor.Models;

namespace DbTableEditor.Data
{
    public interface IGetDataFromDb
    {
        /// <summary>
        /// Проверка соединения с БД
        /// </summary>
        /// <returns>true - если есть подключение</returns>
        bool CheckConnect();

        /// <summary>
        /// Получает список таблиц.
        /// </summary>
        List<string> GetTables();

        /// <summary>
        /// Получение структур таблиц
        /// </summary>
        List<TableInfoModel> GetTablesStructure();

        /// <summary>
        /// Удаление выбранной таблицы
        /// </summary>
        /// <param name="tableName"></param>
        void DeleteTable(string tableName);

        /// <summary>
        /// Создание новой таблицы
        /// </summary>
        /// <param name="newOrChangeTable"></param>
        bool CreateTable(TableInfoModel newOrChangeTable);


        bool AddColumn(TableInfoModel table, ColumnInfoModel column);

        bool DeleteColumn(TableInfoModel table, ColumnInfoModel column);

    }
}
