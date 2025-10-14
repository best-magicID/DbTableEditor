using DbTableEditor.Helpers;
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

    }

}
