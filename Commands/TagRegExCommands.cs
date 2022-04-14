using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AI_Note_Review
{
    class SaveTagRegEx : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            SqlTagRegExVM t = parameter as SqlTagRegExVM;
            t.SaveToDB();
        }
        #endregion
    }
    class DeleteTagRegEx : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            SqlTagRegExVM t = parameter as SqlTagRegExVM;
            t.ParentTag.RemoveTagRegEx(t);
        }
        #endregion
    }

    class EditTagRegEx : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            SqlTagRegExVM t = parameter as SqlTagRegExVM;
            WinEnterText wet = new WinEnterText("Edit Regular Expression value", t.RegExText);
            wet.ShowDialog();
            if (wet.ReturnValue != null)
            {
                t.RegExText = wet.ReturnValue;
            }
        }
        #endregion
    }
}
