using DbTableEditor.Helpers;
using DbTableEditor.Models;
using Microsoft.EntityFrameworkCore;

namespace DbTableEditor.Data
{
    /// <summary>
    /// Операции с БД
    /// </summary>
    public class OperationsDb : IGetDataFromDb
    {
        private readonly MyDbContext _myDbContext;

        public OperationsDb(MyDbContext myDbContext)
        {
            _myDbContext = myDbContext;
        }

        /// <summary>
        /// Проверка соединения с БД
        /// </summary>
        /// <returns>true - есть подключение</returns>
        public bool CheckConnect()
        {
            try
            {
                return _myDbContext.Database.CanConnect();
            }
            catch (Exception ex)
            {
                GeneralMethods.ShowNotification("Нет доступа к БД.\r\n\r\nОшибка: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Получение списка таблиц
        /// </summary>
        public List<string> GetTables()
        {
            var sql = "SELECT TABLE_NAME " +
                      "FROM INFORMATION_SCHEMA.TABLES " +
                      "WHERE TABLE_TYPE = 'BASE TABLE'";

            return _myDbContext.Database.SqlQueryRaw<string>(sql).ToList();
        }

        /// <summary>
        /// Получение структур таблиц
        /// </summary>
        public List<TableInfoModel> GetTablesStructure()
        {
            var sql = @"SELECT 
                            c.TABLE_NAME,
                            c.COLUMN_NAME,
                            c.DATA_TYPE,
                            c.CHARACTER_MAXIMUM_LENGTH,
                            c.IS_NULLABLE,
                            c.COLUMN_DEFAULT,
                            CASE WHEN k.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey
                        FROM INFORMATION_SCHEMA.COLUMNS AS c
                        LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS k
                            ON c.TABLE_NAME = k.TABLE_NAME 
                            AND c.COLUMN_NAME = k.COLUMN_NAME
                            AND k.CONSTRAINT_NAME IN (
                                SELECT CONSTRAINT_NAME 
                                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                                WHERE CONSTRAINT_TYPE = 'PRIMARY KEY'
                            )
                        ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION;";

            using var command = _myDbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            _myDbContext.Database.OpenConnection();

            var tables = new Dictionary<string, TableInfoModel>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var tableName = reader.GetString(0);
                    if (!tables.ContainsKey(tableName))
                    {
                        tables[tableName] = new TableInfoModel { TableName = tableName };
                    }

                    var column = new ColumnInfoModel
                    {
                        ColumnName = reader.GetString(1),
                        DataType = reader.GetString(2),
                        MaxLength = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                        IsNullable = reader.GetString(4) == "YES",
                        DefaultValue = reader.IsDBNull(5) ? null : reader.GetString(5),
                        IsPrimaryKey = reader.GetInt32(6) == 1
                    };

                    tables[tableName].Columns.Add(column);
                }
            }
            _myDbContext.Database.CloseConnection();

            return tables.Values.ToList();
        }
    }

}
