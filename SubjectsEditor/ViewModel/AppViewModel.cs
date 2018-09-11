using SubjectsEditor.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SubjectsEditor.ViewModel
{
    /// <summary>
    /// Represents main ViewModel for app
    /// </summary>
    public class AppViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public string ExportPath { get; set; }

        public string SelectedOrder { get; set; }
        public List<String> SortOrders
        {
            get; private set;
        }

        public ICommand SaveToCsvCommand
        {
            get; set;
        }

        public NextSubjectViewModel NextSubjectViewModel { get; set; }

        public ObservableCollection<Subject> Subjects
        {
            get
            {
                return _model.Subjects;
            }
        }

        public AppViewModel()
        {
            NextSubjectViewModel = new NextSubjectViewModel
            {
                AddSubjectCommand = new Command(AddSubject, true)
            };
            SaveToCsvCommand = new Command(SaveSubjects, true);

            ExportPath = Directory.GetCurrentDirectory() + @"\export.csv";
            SortOrders = new List<string>() { "Имя, фамилия, отчество, д. р." };
            SelectedOrder = SortOrders[0];
        }

        private void AddSubject(object parameter)
        {
            _model.Subjects.Add(new Subject(NextSubjectViewModel.Name, NextSubjectViewModel.Surname,
                NextSubjectViewModel.Patronymic, NextSubjectViewModel.PassportNumber, NextSubjectViewModel.Birthday));
            NextSubjectViewModel.ClearInputs();

        }

        private void SaveSubjects(object parameter)
        {
            try
            {
                _model.SortAndSaveSubjects(ExportPath);
                MessageBox.Show("Successfull export");

            }
            catch (Exception exc)
            {
                MessageBox.Show("Error while export: " + exc.Message);
            }
        }




        private AppModel _model = new AppModel();
    }

    /// <summary>
    /// ViewModel for only new subject-form (stores and checks user-input values)
    /// </summary>
    public class NextSubjectViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("IsNameCorrect");
                OnPropertyChanged("IsSubjectCorrect");
            }
        }

        private string _surname;
        public string Surname
        {
            get
            {
                return _surname;
            }
            set
            {
                _surname = value;
                OnPropertyChanged("Surname");
                OnPropertyChanged("IsSurnameCorrect");
                OnPropertyChanged("IsSubjectCorrect");
            }
        }

        private string _patronymic;
        public string Patronymic
        {
            get
            {
                return _patronymic;
            }
            set
            {
                _patronymic = value;
                OnPropertyChanged("Patronymic");
                OnPropertyChanged("IsPatronymicCorrect");
                OnPropertyChanged("IsSubjectCorrect");
            }
        }

        private string _passportNumber;
        public string PassportNumber
        {
            get
            {
                return _passportNumber;
            }
            set
            {
                _passportNumber = value;
                OnPropertyChanged("PassportNumber");
                OnPropertyChanged("IsPassportNumberCorrect");
                OnPropertyChanged("IsSubjectCorrect");
            }
        }

        private DateTime _birthday = DateTime.Today;
        public DateTime Birthday
        {
            get
            {
                return _birthday;
            }
            set
            {
                _birthday = value;
                OnPropertyChanged("Birthday");
                OnPropertyChanged("IsBirthdayCorrect");
                OnPropertyChanged("IsSubjectCorrect");
            }
        }

        private bool _isNameCorrect;
        public bool IsNameCorrect
        {
            get
            {
                _isNameCorrect = Subject.CheckName(_name);
                return _isNameCorrect;
            }
        }
        private bool _isSurnameCorrect;
        public bool IsSurnameCorrect
        {
            get
            {
                _isSurnameCorrect = Subject.CheckSurname(_surname);
                return _isSurnameCorrect;
            }
        }
        private bool _isPatronymicCorrect;
        public bool IsPatronymicCorrect
        {
            get
            {
                _isPatronymicCorrect = Subject.CheckPatronymic(_patronymic);
                return _isPatronymicCorrect;
            }
        }
        private bool _isPassportNumberCorrect;
        public bool IsPassportNumberCorrect
        {
            get
            {
                _isPassportNumberCorrect = Subject.CheckPassportNumber(_passportNumber);
                return _isPassportNumberCorrect;
            }
        }
        private bool _isBirthdayCorrect;
        public bool IsBirthdayCorrect
        {
            get
            {
                _isBirthdayCorrect = Subject.CheckBirthday(_birthday);
                return _isBirthdayCorrect;
            }
        }

        public bool IsSubjectCorrect
        {
            get
            {
                return (_isNameCorrect && _isSurnameCorrect && _isPatronymicCorrect &&
                    _isPassportNumberCorrect && _isBirthdayCorrect);
            }
        }

        public ICommand AddSubjectCommand
        {
            get; set;
        }

        public void ClearInputs()
        {
            Name = "";
            Surname = "";
            Patronymic = "";
            PassportNumber = "";
            Birthday = DateTime.Today;
        }
    }

    /// <summary>
    /// Convert boolean values to background color for good/bad input
    /// </summary>
    public class BrushColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                {
                    return _correctBrush;
                }
                return _incorrectBrush;
            }
            catch
            {
                return _incorrectBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private SolidColorBrush _correctBrush = new SolidColorBrush
        {
            Color = Color.FromRgb(127, 255, 127)
        };
        private SolidColorBrush _incorrectBrush = new SolidColorBrush
        {
            Color = Color.FromRgb(255, 255, 255)
        };
    }

    /// <summary>
    /// Converter for extracting passport serial from full passport number
    /// </summary>
    public class PassportOnlySerialConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().Substring(0, 4);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
