using System.ComponentModel;
using System.Windows.Controls;

namespace WPF_Torpedo.Services
{
    public interface IPageNavigator
    {
        public Page CurrentPage { get; set; }
        public void MoveToPage<T>() where T : Page;
    }
    public class PageNavigator(Func<Type, Page> factory) : IPageNavigator, INotifyPropertyChanged
    {
        Page _currentPage;
        Func<Type, Page> _factory = factory;

        public Page CurrentPage { get => _currentPage;
            set
            {
                _currentPage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPage)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void MoveToPage<T>() where T : Page
        {
            Page page = _factory.Invoke(typeof(T));
            CurrentPage = page;
        }
    }
}
