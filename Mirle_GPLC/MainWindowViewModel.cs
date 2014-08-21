using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Mirle_GPLC
{
    public class AccentColorMenuData
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }

        private ICommand changeAccentCommand;

        public ICommand ChangeAccentCommand
        {
            get { return this.changeAccentCommand ?? (changeAccentCommand = new SimpleCommand { CanExecuteDelegate = x => true, ExecuteDelegate = x => this.DoChangeTheme(x) }); }
        }

        protected virtual void DoChangeTheme(object sender)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(this.Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }
    }
    
    public class SimpleCommand : ICommand
    {
        public Predicate<object> CanExecuteDelegate { get; set; }
        public Action<object> ExecuteDelegate { get; set; }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
                return CanExecuteDelegate(parameter);
            return true; // if there is no can execute default to true
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
                ExecuteDelegate(parameter);
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public MainWindowViewModel()
        {
            // create accent color menu items for the demo
            this.AccentColors = ThemeManager.Accents
                                            .Select(a => new AccentColorMenuData()
                                            { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                                            .ToList();

            BrushResources = FindBrushResources();
        }

        public string Title { get; set; }
        public int SelectedIndex { get; set; }
        public List<AccentColorMenuData> AccentColors { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        public IEnumerable<string> BrushResources { get; private set; }

        private IEnumerable<string> FindBrushResources()
        {
            var rd = new ResourceDictionary
            {
                Source = new Uri(@"/MahApps.Metro;component/Styles/Colors.xaml", UriKind.RelativeOrAbsolute)
            };

            var resources = rd.Keys.Cast<object>()
                    .Where(key => rd[key] is Brush)
                    .Select(key => key.ToString())
                    .OrderBy(s => s)
                    .ToList();

            return resources;
        }

        public string this[string columnName]
        {
            get
            {
                return null;
            }
        }

        public string Error { get { return string.Empty; } }

        private ListCollectionView _dataSource;
        public ListCollectionView DataSource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value;
                _dataSource.GroupDescriptions.Add(new PropertyGroupDescription("DeviceName"));
            }
        }
    }
}
