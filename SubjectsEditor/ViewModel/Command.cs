using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SubjectsEditor.ViewModel
{
    public class Command : ICommand
    {
        public bool CanExecute
        {
            get
            {
                return _canExecute;
            }
            set
            {
                if (_canExecute != value)
                {
                    _canExecute = value;
                    if (CanExecuteChanged != null)
                    {
                        CanExecuteChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public event EventHandler CanExecuteChanged;

        public Command(Action<object> execute, bool canExecute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        bool ICommand.CanExecute(object parameter)
        {
            return _canExecute;
        }

        void ICommand.Execute(object parameter)
        {
            if (_execute != null)
            {
                _execute.Invoke(parameter);
            }
        }

        private bool _canExecute;
        private readonly Action<object> _execute;
    }
}
