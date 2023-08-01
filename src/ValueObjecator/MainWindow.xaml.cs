using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ValueObjecator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _classNameField = "";
        public string ClassNameField
        {
            get => _classNameField;
            set
            {
                _classNameField = value.Trim().Replace(" ", "");

                OnPropertyChanged();
            }
        }

        private string _inPut = "";
        public string InputField
        {
            get => _inPut;
            set
            {
                if (!string.IsNullOrEmpty(value.Trim()))
                {
                    _inPut = ListConverter.Parser(_classNameField, value.Trim());
                }
                else
                {
                    _inPut = string.Empty;
                }
                    
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            ClassNameField = "MyClass";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
