using DbTableEditor.Helpers;
using DbTableEditor.Models;
using DbTableEditor.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;

namespace DbTableEditor.Data
{
    /// <summary>
    /// Операции с БД
    /// </summary>
    public class OperationsDb : IGetDataFromDb
    {
        private readonly MyDbContext _myDbContext;

        //Удалить потом
        private enum _typeChangeTable
        {
            ChangeNameTable,
            ChangeNameColumn,
            ChangeTypeColumn,
            AddColumn,
            DeleteColumn,
            ChangePrimaryKey
        }

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
                        DataType = DataTypeProvider.GetSqlDataType(reader.GetString(2)),
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

        /// <summary>
        /// Удаление выбранной таблицы
        /// </summary>
        /// <param name="tableName"></param>
        /// <exception cref="ArgumentException"></exception>
        public void DeleteTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Имя таблицы не может быть пустым.", nameof(tableName));
            }

            var sql = $"DROP TABLE [{tableName}]";

            _myDbContext.Database.ExecuteSqlRaw(sql);
        }

        /// <summary>
        /// Создание новой таблицы
        /// </summary>
        /// <param name="newOrChangeTable"></param>
        public bool CreateTable(TableInfoModel newOrChangeTable)
        {
            if (TableExists(newOrChangeTable.TableName))
            {
                GeneralMethods.ShowNotification($"Таблица с именем '{newOrChangeTable.TableName}' уже существует.");
                return false;
            }

            var sql = BuildCreateTableSql(newOrChangeTable);
            try
            {
                _myDbContext.Database.ExecuteSqlRaw(sql);
                return true;
            }
            catch (SqlException ex)
            {
                GeneralMethods.ShowNotification("Ошибка при создании таблицы.\r\n\r\n" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Проверяет, существует ли таблица в базе данных.
        /// </summary>
        private bool TableExists(string tableName)
        {
            var sql = $@"SELECT COUNT(*) 
                         FROM INFORMATION_SCHEMA.TABLES 
                         WHERE TABLE_TYPE = 'BASE TABLE' 
                         AND TABLE_NAME = @p0";

            var count = _myDbContext.Database.SqlQueryRaw<int>(sql, tableName)
                                             .AsEnumerable()
                                             .FirstOrDefault();

            return count > 0;
        }

        /// <summary>
        /// Построение SQL для создания таблицы
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private static string BuildCreateTableSql(TableInfoModel table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE [{table.TableName}] (");

            for (int i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                sb.Append($" [{column.ColumnName}] {DataTypeProvider.ToSqlTypeString(column.DataType)} ");

                //if (!column.IsNullable)
                //    sb.Append(" NOT NULL");
                if(column.IsPrimaryKey)
                {
                    sb.AppendLine($" CONSTRAINT [PK_{table.TableName}] PRIMARY KEY ({$"[{column.ColumnName}]"})");
                }

                if (i < table.Columns.Count - 1)
                    sb.AppendLine(",");
                else
                    sb.AppendLine();
            }

            sb.AppendLine(");");
            return sb.ToString();
        }


        public bool AddColumn(TableInfoModel table, ColumnInfoModel column)
        {
            try
            {
                if (ColumnExists(table.TableName, column.ColumnName))
                {
                    GeneralMethods.ShowNotification($"Колонка с именем '{column.ColumnName}' уже существует.");
                    return false;
                }

                var sql = $"ALTER TABLE [{table.TableName}] ADD [{column.ColumnName}] {DataTypeProvider.ToSqlTypeString(column.DataType)}";

                _myDbContext.Database.ExecuteSqlRaw(sql);
                return true;
            }
            catch (SqlException ex)
            {
                GeneralMethods.ShowNotification("Ошибка при добавлении столбца.\r\n\r\n" + ex.Message);
                return false;
            }
        }

        public bool DeleteColumn(TableInfoModel table, ColumnInfoModel column)
        {
            try
            {
                //if (!ColumnExists(table.TableName, column.ColumnName))
                //{
                //    GeneralMethods.ShowNotification($"Колонка с именем '{column.ColumnName}' отсутствует.");
                //    return false;
                //}

                var sql = $"ALTER TABLE [{table.TableName}] DROP COLUMN [{column.ColumnName}]";

                _myDbContext.Database.ExecuteSqlRaw(sql);
                return true;
            }
            catch (SqlException ex)
            {
                GeneralMethods.ShowNotification("Ошибка при удалении столбца.\r\n\r\n" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Проверяет, существует ли колонка в таблице.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private bool ColumnExists(string tableName, string columnName)
        {
            var sql = $@"SELECT COUNT(*) 
                         FROM INFORMATION_SCHEMA.COLUMNS 
                         WHERE TABLE_NAME = @p0
                         AND COLUMN_NAME = @p1";

            var count = _myDbContext.Database.SqlQueryRaw<int>(sql, tableName, columnName)
                                             .AsEnumerable()
                                             .FirstOrDefault();
            return count > 0;
        }

        /// <summary>
        /// Строит SQL для добавления колонки
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private static string BuildSqlQuery(_typeChangeTable typeChangeTable, string tableName, ColumnInfoModel column)
        {
            var sql = string.Empty;

            if (typeChangeTable == _typeChangeTable.AddColumn)
            {
                sql = $"ALTER TABLE [{tableName}] ADD [{column.ColumnName}] {DataTypeProvider.ToSqlTypeString(column.DataType)}";
                return sql;
            }
            else if(typeChangeTable == _typeChangeTable.DeleteColumn)
            {
                sql = $"ALTER TABLE [{tableName}] DROP COLUMN [{column.ColumnName}]";
                return sql;
            }
            else if(typeChangeTable == _typeChangeTable.ChangeNameColumn)
            {
                sql = $"EXEC sp_rename '{tableName}.{column.ColumnName}', '{column.ColumnName}_New', 'COLUMN'";
                return sql;
            }
            else if(typeChangeTable == _typeChangeTable.ChangeTypeColumn)
            {
                sql = $"ALTER TABLE [{tableName}] ALTER COLUMN [{column.ColumnName}] {DataTypeProvider.ToSqlTypeString(column.DataType)}";
                return sql;
            }
            else if(typeChangeTable == _typeChangeTable.ChangeNameTable)
            {
                sql = $"EXEC sp_rename '{tableName}', '{tableName}_New'";
                return sql;
            }
            else if(typeChangeTable == _typeChangeTable.ChangePrimaryKey)
            {
                // Пример SQL для изменения первичного ключа можно добавить здесь
                throw new NotImplementedException("Изменение первичного ключа не реализовано.");
            }

            
            throw new NotImplementedException("Тип изменения таблицы не реализован.");


        }







        //private void ChangeTableStructure(string oldTableName, TableInfoModel newTable)
        //{
        //    // Переименование таблицы, если имя изменилось
        //    if (oldTableName != newTable.TableName)
        //    {
        //        RenameTable(oldTableName, newTable.TableName);
        //    }
        //    // Получение текущих колонок таблицы
        //    var currentTable = GetTablesStructure().FirstOrDefault(t => t.TableName == newTable.TableName);
        //    if (currentTable == null)
        //    {
        //        throw new InvalidOperationException($"Таблица '{newTable.TableName}' не найдена в базе данных.");
        //    }
        //    // Обновление колонок таблицы
        //    UpdateTableColumns(newTable.TableName, currentTable.Columns, newTable.Columns);
        //}

        //private void RenameTable(string oldTableName, string newTableName)
        //{
        //    var sql = $"EXEC sp_rename '{oldTableName}', '{newTableName}'";
        //    _myDbContext.Database.ExecuteSqlRaw(sql);
        //}

        //private void UpdateTableColumns(string tableName, List<ColumnInfoModel> oldColumns, List<ColumnInfoModel> newColumns)
        //{
        //    // Удаление колонок
        //    foreach (var oldColumn in oldColumns)
        //    {
        //        if (!newColumns.Any(c => c.ColumnName == oldColumn.ColumnName))
        //        {
        //            DeleteColumn(tableName, oldColumn.ColumnName);
        //        }
        //    }
        //    // Добавление и изменение колонок
        //    foreach (var newColumn in newColumns)
        //    {
        //        var oldColumn = oldColumns.FirstOrDefault(c => c.ColumnName == newColumn.ColumnName);
        //        if (oldColumn == null)
        //        {
        //            AddColumn(tableName, newColumn);
        //        }
        //        else
        //        {
        //            ModifyColumn(tableName, oldColumn, newColumn);
        //        }
        //    }
        //}

        //private void ModifyColumn(string tableName, ColumnInfoModel oldColumn, ColumnInfoModel newColumn)
        //{
        //    if (oldColumn.ColumnName != newColumn.ColumnName)
        //    {
        //        RenameColumn(tableName, oldColumn.ColumnName, newColumn.ColumnName);
        //    }
        //    if (oldColumn.DataType != newColumn.DataType)
        //    {
        //        ChangeColumnType(tableName, newColumn.ColumnName, newColumn.DataType);
        //    }
        //    // Дополнительные изменения (например, изменение NULL/NOT NULL) можно добавить здесь
        //}

        //private bool DeleteColumn(string tableName, string columnName)
        //{
        //    var sql = $"ALTER TABLE [{tableName}] DROP COLUMN [{columnName}]";
        //     return _myDbContext.Database.ExecuteSqlRaw(sql);
        //}

        //private void AddColumn(string tableName, ColumnInfoModel column)
        //{
        //    var sql = BuildSqlQuery(tableName, column);
        //    _myDbContext.Database.ExecuteSqlRaw(sql);
        //}

        //private void ChangeColumnType(string tableName, string columnName, SqlDataType newDataType)
        //{
        //    var sql = $"ALTER TABLE [{tableName}] ALTER COLUMN [{columnName}] {DataTypeProvider.ToSqlTypeString(newDataType)}";
        //    _myDbContext.Database.ExecuteSqlRaw(sql);
        //}

        //private void RenameColumn(string tableName, string oldColumnName, string newColumnName)
        //{
        //    var sql = $"EXEC sp_rename '{tableName}.{oldColumnName}', '{newColumnName}', 'COLUMN'";
        //    _myDbContext.Database.ExecuteSqlRaw(sql);
        //}



        

    }

}
