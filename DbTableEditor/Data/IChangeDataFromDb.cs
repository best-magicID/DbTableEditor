using DbTableEditor.Models;

namespace DbTableEditor.Data
{
    public interface IChangeDataFromDb
    {
        /// <summary>
        /// Проверка соединения с БД
        /// </summary>
        /// <returns>true - если есть подключение</returns>
        bool CheckConnect();

        /// <summary>
        /// Удаление выбранной таблицы в БД
        /// </summary>
        /// <param name="tableName"></param>
        void DeleteTable(string tableName);

        /// <summary>
        /// Создание новой таблицы в БД
        /// </summary>
        /// <param name="newOrChangeTable"></param>
        bool CreateTable(TableInfoModel newOrChangeTable);

        /// <summary>
        /// Добавление столбца в БД
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        bool AddColumn(TableInfoModel table, ColumnInfoModel column);

        /// <summary>
        /// Удаление столбца из БД
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        bool DeleteColumn(TableInfoModel table, ColumnInfoModel column);

        /// <summary>
        /// Изменение имени таблицы в БД
        /// </summary>
        /// <param name="oldNameTable"></param>
        /// <param name="newOrChangeTable"></param>
        /// <returns></returns>
        bool ChangeNameTable(string oldNameTable, TableInfoModel newOrChangeTable);

        /// <summary>
        /// Изменение имени столбца в БД
        /// </summary>
        /// <param name="table"></param>
        /// <param name="oldColumnName"></param>
        /// <param name="newNameColumn"></param>
        /// <returns></returns>
        bool ChangeNameColumn(string tableName, string oldColumnName, string newNameColumn);

        /// <summary>
        /// Изменение типа данных столбца в БД
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="newTypeColumn"></param>
        /// <returns></returns>
        bool ChangeTypeColumn(string tableName, string columnName, SqlDataType newTypeColumn);

        /// <summary>
        /// Изменение признака "Первичный ключ" столбца в БД
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="makePrimaryKey"></param>
        /// <returns></returns>
        bool ChangePrimaryKeyColumn(string tableName, string columnName, bool makePrimaryKey);
    }
}
