﻿using CompleteBackup.DataRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.ViewModels.ExtendedControls
{
    class ProcessingSpinnerViewModel
    {
        public BackupProjectRepository Repository { get; set; } = BackupProjectRepository.Instance;
    }
}