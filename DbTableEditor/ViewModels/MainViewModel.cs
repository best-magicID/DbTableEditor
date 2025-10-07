using SalesAnalysis.Data;

namespace DbTableEditor.ViewModels
{
    public class MainViewModel
    {
        private readonly MyDbContext _context;

        public MainViewModel(MyDbContext context)
        {
            _context = context;
        }

        public int CountModels()
        {
            return _context.Database.CanConnect() ? 1 : 0;
        }
    }
}
