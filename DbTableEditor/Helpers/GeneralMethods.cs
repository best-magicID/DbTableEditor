using System.Windows;

namespace DbTableEditor.Helpers
{
    /// <summary>
    /// Общие методы 
    /// </summary>
    public abstract class GeneralMethods
    {
        /// <summary>
        /// Отображение уведомления в MessageBox
        /// </summary>
        /// <param name="text"></param>
        public static void ShowNotification(string text)
        {
            MessageBox.Show(text, 
                            "Внимание", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
        }
        
        /// <summary>
        /// Открыть окно выбора
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static MessageBoxResult ShowSelectionWindow(string text)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(text,
                                                                "Внимание",
                                                                MessageBoxButton.YesNo,
                                                                MessageBoxImage.Information, 
                                                                MessageBoxResult.No);
            return messageBoxResult;
        }
    }
}
