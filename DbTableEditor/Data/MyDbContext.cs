using Microsoft.EntityFrameworkCore;

namespace SalesAnalysis.Data
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
