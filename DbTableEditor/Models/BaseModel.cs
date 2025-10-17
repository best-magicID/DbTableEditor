using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DbTableEditor.Models
{
    public class BaseModel : INotifyPropertyChanged
    {

        #region Обновление UI

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
