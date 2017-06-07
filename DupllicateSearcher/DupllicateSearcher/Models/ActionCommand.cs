using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DupllicateSearcher.Models
{
    public class ActionCommand : ICommand
    {
        private Action action;
        public event EventHandler CanExecuteChanged;

        public ActionCommand(Action action)
        {
            this.action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action();
        }
    }
}
