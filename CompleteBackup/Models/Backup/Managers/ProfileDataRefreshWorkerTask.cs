﻿using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Utilities;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.Models.Profile
{
    public class ProfileDataRefreshWorkerTask : BackgroundWorker
    {
        //private BackupProfileData m_Profile;

        private ProfileDataRefreshWorkerTask() { }

        private void UpdateAlerts(BackupProfileData profile)
        {
            var storage = profile.GetStorageInterface();

            profile.BackupAlertList.Clear();
            profile.RestoreAlertList.Clear();

            foreach (var item in profile.BackupFolderList)
            {
                if (item.IsFolder)
                {
                    if (storage.DirectoryExists(item.Path))
                    {
                        item.IsAvailable = true;
                    }
                    else
                    {
                        profile.BackupAlertList.Add(new BackupPerfectAlertData() { Name = $"Backup directory is not available: {item.Path}" });
                    }
                }
                else
                {
                    if (storage.FileExists(item.Path))
                    {
                        item.IsAvailable = true;
                    }
                    else
                    {
                        profile.BackupAlertList.Add(new BackupPerfectAlertData() { Name = $"Backup file is not available: {item.Path}" });
                    }
                }
            }

            if (profile.TargetBackupFolder != null && profile.TargetBackupFolder != string.Empty)
            {
                if (!storage.DirectoryExists(profile.TargetBackupFolder))
                { 
                    profile.BackupAlertList.Add(new BackupPerfectAlertData() { Name = $"The target backup directory is not available: {profile.TargetBackupFolder}",
                    Description = "The target backp directory is the destination directory where the backup files will be stored, it is recomended to select an empty directory"});
                }
            }
            else
            {
                profile.BackupAlertList.Add(new BackupPerfectAlertData() { Name = $"Target backup directory is not set" });
            }

            if (profile.TargetRestoreFolder != null && profile.TargetRestoreFolder != string.Empty)
            {
                if (!storage.DirectoryExists(profile.TargetRestoreFolder))
                {
                    profile.RestoreAlertList.Add(new BackupPerfectAlertData() { Name = $"Restore directory is not available {profile.TargetRestoreFolder}" });
                }
            }
            else
            {
                profile.RestoreAlertList.Add(new BackupPerfectAlertData() { Name = $"Restore directory is not set" });
            }

        }


        public ProfileDataRefreshWorkerTask(BackupProfileData profile)
        {
            //m_Profile = profile;

            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;

            DoWork += (sender, e) =>
            {
                var storage = profile.GetStorageInterface();
                try
                {
                    UpdateAlerts(profile);


                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        profile.BackupTargetDiskSize = "n/a";
                        profile.BackupTargetUsedSize = "n/a";
                        profile.BackupTargetFreeSize = "n/a";
                    }));

                    //Source folders sizes and number of files
                    profile.BackupSourceFilesNumber = 0;
                    profile.BackupSourceFoldersSize = 0;
                    foreach (var item in profile.BackupFolderList.Where(i => i.IsAvailable))
                    {
                        if (item.IsFolder)
                        {
                            item.NumberOfFiles = storage.GetNumberOfFiles(item.Path);
                            item.TotalSize = storage.GetSizeOfFiles(item.Path);
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                profile.BackupSourceFilesNumber += item.NumberOfFiles;
                                profile.BackupSourceFoldersSize += item.TotalSize;
                            }));
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                profile.BackupSourceFilesNumber++;
                            }));
                        }
                    }


                    //Restore folders sizes and number of files
                    profile.RestoreSourceFilesNumber = 0;
                    profile.RestoreSourceFoldersSize = 0;
                    foreach (var item in profile.RestoreFolderList)
                    {
                        if (item.IsFolder)
                        {
                            item.NumberOfFiles = storage.GetNumberOfFiles(item.Path);
                            item.TotalSize = storage.GetSizeOfFiles(item.Path);
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                profile.RestoreSourceFilesNumber += item.NumberOfFiles;
                                profile.RestoreSourceFoldersSize += item.TotalSize;
                            }));
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                profile.RestoreSourceFilesNumber++;
                            }));
                        }
                    }


                    if (profile.TargetBackupFolder != null)
                    {
                        //last backup time
                        DateTime? lastTime = null;
                        var lastSet = BackupBase.GetLastBackupSetName(profile);
                        if (lastSet != null)
                        {
                            var sessionHistory = BackupSessionHistory.LoadHistory(profile.TargetBackupFolder, lastSet);
                            lastTime = sessionHistory?.TimeStamp;
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                profile.LastBackupDateTime = lastTime;
                            }));
                        }
                    }

                    if (profile.TargetBackupFolder != null)
                    {
                        profile.BackupTargetDiskSizeNumber = 0;
                        //Target disk size
                        string rootDrive = System.IO.Path.GetPathRoot(profile.TargetBackupFolder);
                        foreach (System.IO.DriveInfo drive in System.IO.DriveInfo.GetDrives().Where(d => d.ToString().Contains(rootDrive)))
                        {
                            if (drive.IsReady)
                            {
                                profile.BackupTargetDiskSizeNumber = drive.TotalSize;
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    profile.BackupTargetDiskSize = FileFolderSizeHelper.GetNumberSizeString(profile.BackupTargetDiskSizeNumber);
                                }));

                                break;
                            }
                        }

                        profile.BackupTargetUsedSizeNumber = 0;
                        //Target used Space
                        profile.BackupTargetUsedSizeNumber = storage.GetSizeOfFiles(profile.TargetBackupFolder);
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            profile.BackupTargetUsedSize = FileFolderSizeHelper.GetNumberSizeString(profile.BackupTargetUsedSizeNumber);
                        }));

                        //Target free space
                        profile.BackupTargetFreeSizeNumber = 0;
                        //rootDrive = Path.GetPathRoot(profile.TargetBackupFolder);
                        //foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.ToString().Contains(rootDrive)))
                        //{
                        //    if (drive.IsReady)
                        //    {
                        //        profile.BackupTargetFreeSizeNumber = drive.AvailableFreeSpace;
                        //        //m_BackupTargetFreeSizeNumber = drive.TotalFreeSpace;
                        //        Application.Current.Dispatcher.Invoke(new Action(() =>
                        //        {
                        //            profile.BackupTargetFreeSize = FileFolderSizeHelper.GetNumberSizeString(profile.BackupTargetFreeSizeNumber);
                        //        }));

                        //        break;
                        //    }
                        //}
                    }

                    ////Source Foldes Size
                    //m_BackupSourceFoldersSizeNumber = 0;
                    //foreach (var item in FolderList)
                    //{
                    //    m_BackupSourceFoldersSizeNumber += new DirectoryInfo(item.Path).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
                    //    Application.Current.Dispatcher.Invoke(new Action(() =>
                    //    {
                    //        BackupSourceFoldersSize = FileFolderSizeHelper.GetNumberSizeString(m_BackupSourceFoldersSizeNumber);
                    //    }));
                    //}

                    if (m_ProfileDataUpdateEventCallback != null)
                    {
                        m_ProfileDataUpdateEventCallback(profile);
                    }

                    //update data to persistent storage
                    DataRepository.BackupProjectRepository.Instance.SaveProject();
                }
                catch (TaskCanceledException ex)
                {
                    Trace.WriteLine($"Profile Data Update exception: {ex.Message}");
                    e.Result = $"Profile Data Update exception: {ex.Message}";
                    throw (ex);
                }
                finally
                {
                }
            };
        }

        public delegate void ProfileDataUpdateEvent(BackupProfileData profile);
        private event ProfileDataUpdateEvent m_ProfileDataUpdateEventCallback;

        public void RegisterEvent(ProfileDataUpdateEvent callback)
        {
            m_ProfileDataUpdateEventCallback += callback;
        }

        public void UnRegisterEvent(ProfileDataUpdateEvent callback)
        {
            m_ProfileDataUpdateEventCallback -= callback;
        }
    }
}
