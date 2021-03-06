﻿using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.Models.Utilities;
using CompleteBackup.ViewModels.ICommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace CompleteBackup.ViewModels
{
    public class LogConsoleViewModel
    {
        public ICommand ClearLogConsoleCommand { get; private set; } = new ClearLogConsoleICommand<object>();
        public BackupProjectData Project { get; } = BackupProjectRepository.Instance.SelectedBackupProject;

    }
}
