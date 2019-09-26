using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace THColumnInfo
{
    public class DelegateCommand : ICommand
    {
        public Func<object, bool> canExecute;
        public Action<object> executeAction;
        public bool canExecuteCache;
        public DelegateCommand()
        {

        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }
    }
}
