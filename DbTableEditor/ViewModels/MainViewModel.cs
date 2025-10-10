using Microsoft.EntityFrameworkCore;
using SalesAnalysis.Data;

namespace DbTableEditor.ViewModels
{
    public class MainViewModel
    {
        private readonly MyDbContext _context;

        public MainViewModel()
        {

        }

        public MainViewModel(MyDbContext context)
        {
            _context = context;

            if(CountModels() == 1)
            {
                CountModels();

                LoadData();
            }
        }

        public int CountModels()
        {
            if (_context == null)
            {
                return 0;
            }

            return _context.Database.CanConnect() ? 1 : 0;
        }

        public void LoadData()
        {
            //var tables = _context.Set<Models.TableModel>().ToList();

                var tableNames = _context.Model
                    .GetEntityTypes()
                    .Select(t => t.GetTableName())
                    .Distinct()
                    .ToList();

                foreach (var name in tableNames)
                    Console.WriteLine(name);
            
        }
    }
}
