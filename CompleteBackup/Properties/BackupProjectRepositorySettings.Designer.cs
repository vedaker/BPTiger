﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CompleteBackup.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.3.0.0")]
    internal sealed partial class BackupProjectRepositorySettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static BackupProjectRepositorySettings defaultInstance = ((BackupProjectRepositorySettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new BackupProjectRepositorySettings())));
        
        public static BackupProjectRepositorySettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.ObjectModel.ObservableCollection<CompleteBackup.Models.Backup.Project.BackupProjectData> BackupProjectList {
            get {
                return ((global::System.Collections.ObjectModel.ObservableCollection<CompleteBackup.Models.Backup.Project.BackupProjectData>)(this["BackupProjectList"]));
            }
            set {
                this["BackupProjectList"] = value;
            }
        }
    }
}