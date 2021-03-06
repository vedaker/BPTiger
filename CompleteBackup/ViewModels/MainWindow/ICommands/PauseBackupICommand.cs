﻿using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Profile;
using CompleteBackup.Views;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.ICommands
{
    internal class PauseBackupICommand<T> : ICommand
    {
        public PauseBackupICommand()
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
            bool bExecute = false;
            var profile = parameter as BackupProfileData;

            if (profile == null)
            {
                var paramList = parameter as IList<object>;
                if (paramList != null && paramList.Count() > 0)
                {
                    profile = paramList[0] as BackupProfileData;
                }
            }

            if (profile != null)
            {
                var aaa = BackupTaskManager.Instance.IsBackupWorkerBusy(profile);
                var bbb = BackupTaskManager.Instance.IsBackupWorkerPaused(profile);

                bExecute = (BackupTaskManager.Instance.IsBackupWorkerBusy(profile) == true) && (BackupTaskManager.Instance.IsBackupWorkerPaused(profile) == false);
            }

            return bExecute;
        }

        public void Execute(object parameter)
        {
            var profile = parameter as BackupProfileData;

            if (profile == null)
            {
                var paramList = parameter as IList<object>;
                if (paramList != null && paramList.Count() > 0)
                {
                    profile = paramList[0] as BackupProfileData;
                }
            }            

            BackupTaskManager.Instance.PauseBackup(profile);
            profile.IsBackupWorkerPaused = false; //value does not matter, this will trigger property change
        }
    }
}
