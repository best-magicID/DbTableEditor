using Microsoft.EntityFrameworkCore;

namespace DbTableEditor.Data
{
    /// <summary>
    /// Контекст БД
    /// </summary>
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {

        }

    }
}
