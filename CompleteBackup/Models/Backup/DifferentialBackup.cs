﻿using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.Models.backup
{
    public class DifferentialBackup : IncrementalFullBackup
    {
        public string LastSetPath;

        public DifferentialBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }

        public override void ProcessBackup()
        {
            m_BackupSessionHistory.Reset(GetTimeStamp());

            var targetSet = GetTargetSetName();
            var lastSet = BackupBase.GetLastBackupSetName(m_Profile);
            if (lastSet == null)
            {
                //First backup
                ProcessNewBackupRootFolders(CreateNewBackupSetFolder(targetSet));
            }
            else
            {
                var lastFullTargetPath = m_IStorage.Combine(m_TargetBackupPath, lastSet);
                var newFullTargetPath = m_IStorage.Combine(m_TargetBackupPath, targetSet);

                CreateNewBackupSetFolderAndMoveDataToOldSet(newFullTargetPath, lastFullTargetPath);

                ProcessBackupRootFolders(newFullTargetPath, lastFullTargetPath);

                var sourceDirectoryEntriesList = m_SourceBackupPathList.Where(i => i.IsFolder).ToList();
                var sourceFileEntriesList = m_SourceBackupPathList.Where(i => !i.IsFolder).ToList();
                HandleDeletedItems(sourceDirectoryEntriesList, newFullTargetPath, lastFullTargetPath);
                HandleDeletedFiles(sourceFileEntriesList, newFullTargetPath, lastFullTargetPath);

            }

            BackupSessionHistory.SaveHistory(m_TargetBackupPath, targetSet, m_BackupSessionHistory);
        }

        protected override void ProcessBackupRootFolders(string newTargetPath, string lastTargetPath)
        {
            //process all items
            foreach (var item in m_SourceBackupPathList)
            {
                var targetdirectoryName = m_IStorage.GetFileName(item.Path);

                if (item.IsFolder)
                {
                    if (m_IStorage.DirectoryExists(item.Path))
                    {
                        item.IsAvailable = true;
                        var newTargetPathDir = m_IStorage.Combine(newTargetPath, targetdirectoryName);
                        var lastTargetPathDir = m_IStorage.Combine(lastTargetPath, targetdirectoryName);
                        ProcessDifferentialBackupFolderStep(item.Path, newTargetPathDir, lastTargetPathDir);
                    }
                    else
                    {
                        item.IsAvailable = false;
                        m_Logger.Writeln($"***Warning: Skipping unavailable backup folder: {item.Path}");
                    }
                }
                else
                {
                    ProcessDeferentialBackupFile(item.Path, newTargetPath, lastTargetPath, targetdirectoryName);
                }            
            }
        }



        protected void ProcessDeferentialBackupFile(string sourcePath, string currSetPath, string lastSetPath, string fileName)
        {
            if (CheckCancellationPending()) { return; }

            UpdateProgress("Running... ", ++ProcessFileCount, fileName);

            var lastSetFilePath = (lastSetPath == null) ? null : m_IStorage.Combine(lastSetPath, fileName);
            var currSetFilePath = m_IStorage.Combine(currSetPath, fileName);

            try
            {
                if (m_IStorage.FileExists(currSetFilePath))
                {
                    if (m_IStorage.IsFileSame(sourcePath, currSetFilePath))
                    {
                        //File is the same, do nothing
                        m_BackupSessionHistory.AddNoChangeFile(sourcePath, currSetFilePath);
                    }
                    else
                    {
                        //Keep current version in set
                        MoveFile(currSetFilePath, lastSetFilePath, true);

                        //Update new version to new set
                        if (!m_IStorage.DirectoryExists(currSetPath))
                        {
                            CreateDirectory(currSetPath);
                        }
                        CopyFile(sourcePath, currSetFilePath);

                        m_BackupSessionHistory.AddUpdatedFile(sourcePath, currSetFilePath);
                    }
                }
                else
                {
                    // new file, copy to current set
                    if (!m_IStorage.DirectoryExists(currSetPath))
                    {
                        CreateDirectory(currSetPath);
                    }
                    CopyFile(sourcePath, currSetFilePath);

                    m_BackupSessionHistory.AddNewFile(sourcePath, currSetFilePath);
                }
            }
            catch (Exception ex)
            {
                m_Logger.Writeln($"**Exception while procesing file set\nSource: {sourcePath}\nTarget: {currSetFilePath}\nLast: {lastSetFilePath}\n{ex.Message}");
            }
        }

        public void ProcessDifferentialBackupFolderStep(string sourcePath, string currSetPath, string lastSetPath)
        {
            var sourceFileList = m_IStorage.GetFiles(sourcePath);

            m_IStorage.CreateDirectory(currSetPath, true);

            foreach (var file in sourceFileList)
            {
                var fileName = m_IStorage.GetFileName(file);
                ProcessDeferentialBackupFile(file, currSetPath, lastSetPath, fileName);
            }

            HandleDeletedFiles(sourceFileList, currSetPath, lastSetPath);

            //Process directories
            var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);

            HandleDeletedItems(sourceSubdirectoryEntriesList, currSetPath, lastSetPath);

            foreach (string subdirectory in sourceSubdirectoryEntriesList)
            {
                string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
                string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);
                string newLastSetPath = m_IStorage.Combine(lastSetPath, subdirectory);

                try
                {
                    ProcessDifferentialBackupFolderStep(newSourceSetPath, newCurrSetPath, newLastSetPath);
                }
                catch (Exception ex)
                {
                    m_Logger.Writeln($"**Exception while procesing folder set\nSource: {newSourceSetPath}\nTarget: {newCurrSetPath}\nLast: {newLastSetPath}\n{ex.Message}");
                }
            }
        }
    }
}
