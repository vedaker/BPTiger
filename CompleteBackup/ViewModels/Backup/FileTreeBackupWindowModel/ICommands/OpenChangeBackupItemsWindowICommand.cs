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
            //CHANGE to poolomorphsimmm
            var vmRestore = parameter as RestoreSourceDestinationItemsViewModel;
            var vmBackup = parameter as SourceDestinationItemsViewModel;

            if (vmRestore != null)
            {
                vmRestore.OpenSelectionWindow();
            }
            else if (vmBackup != null)
            {
                vmBackup.OpenSelectionWindow();
            }

            //            new ChangeBackupItemsWindow().ShowDialog();
        }
    }
}
