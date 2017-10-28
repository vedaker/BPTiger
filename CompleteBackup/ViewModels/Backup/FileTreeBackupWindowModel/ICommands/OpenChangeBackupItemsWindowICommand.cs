﻿using CompleteBackup.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.FolderSelection.ICommands
{
    internal class OpenChangeBackupItemsWindowICommand<T> : ICommand
    {
        public OpenChangeBackupItemsWindowICommand()
        {
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

        public bool CanExecute(object parameter)
        {
            bool bExecute = true;
            return bExecute;
        }

        public void Execute(object parameter)
        {
            new ChangeBackupItemsWindow().ShowDialog();
        }
    }
}
