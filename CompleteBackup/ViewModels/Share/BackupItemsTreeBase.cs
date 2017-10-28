﻿using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.FolderSelection;
using CompleteBackup.ViewModels.FolderSelection.ICommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace CompleteBackup.ViewModels
{
    abstract class BackupItemsTreeBase : ObservableObject
    {
        public ICommand CloseWindowCommand { get; private set; } = new CloseWindowICommand<object>();

        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;
        public ObservableCollection<FolderMenuItem> FolderMenuItemTree { get; set; } = new ObservableCollection<FolderMenuItem>();
        public ObservableCollection<FolderData> SelectedItemList { get; set; } = new ObservableCollection<FolderData>();


        private bool m_DirtyFlag = false;
        public bool DirtyFlag { get { return m_DirtyFlag; } set { m_DirtyFlag = value; OnPropertyChanged(); } }

        protected IStorageInterface m_IStorage;

        protected System.Windows.Media.ImageSource GetImageSource(string path)
        {
            System.Windows.Media.ImageSource imageSource = null;
            try
            {
                var icon = m_IStorage.ExtractIconFromPath(path);

                imageSource = Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to get Icon {path}\n{ex.Message}");
            }

            return imageSource;
        }


        public BackupItemsTreeBase()
        {
            m_IStorage = ProjectData.CurrentBackupProfile.GetStorageInterface();

            new Thread(new System.Threading.ThreadStart(() =>
            {
                AddRootItemsToTree();
            })) { IsBackground = true, Name = "Folder Tree Selection" }.Start();
        }

        protected virtual void ClearItemList()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                FolderMenuItemTree?.Clear();
            }));
        }

        protected virtual void ClearSelectedFolderList()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SelectedItemList?.Clear();
            }));
        }

        protected abstract void AddRootItemsToTree();
        protected abstract FolderMenuItem CreateMenuItem(bool isFolder, bool isSelected, string path, string relativePath, string name, FolderMenuItem parentItem, FileAttributes attr);

        protected List<string> GetDirectoriesNames(string path)
        {
            //Process directories
            string[] sourceSubdirectoryEntries = m_IStorage.GetDirectories(path);
            var sourceSubdirectoryEntriesList = new List<string>();
            if (sourceSubdirectoryEntries != null)
            {
                if (sourceSubdirectoryEntriesList != null)
                {
                    foreach (var entry in sourceSubdirectoryEntries)
                    {
                        sourceSubdirectoryEntriesList.Add(m_IStorage.GetFileName(entry));
                    }
                }
            }

            return sourceSubdirectoryEntriesList;
        }

        private bool IsHidden(FileAttributes attr)
        {
            if (((attr & FileAttributes.System) != FileAttributes.System) &&
                ((attr & FileAttributes.Hidden) != FileAttributes.Hidden))
            {
                return false;
            }

            return true;
        }

        protected abstract List<string> GetAllActiveSets(FolderMenuItem item);
        

        //Add and update all subitems
        protected void UpdateChildItemsInMenuItem(FolderMenuItem item)
        {
            if (item.IsFolder)
            {
                var pathList = GetAllActiveSets(item);

                int i = 0;
                foreach (var path in pathList)
                {
                    i++;
                    //Add all folders under item.Path
                    if (m_IStorage.DirectoryExists(path))
                    {
                        if (i > 1)
                        {
                            int tttt = 0;
                        }

                        var sourceSubdirectoryEntriesList = GetDirectoriesNames(path);
                        foreach (string subdirectory in sourceSubdirectoryEntriesList)
                        {
                            string newPath = m_IStorage.Combine(item.Path, subdirectory);
                            FileAttributes attr = m_IStorage.GetFileAttributes(newPath);
                            if (!IsHidden(attr))
                            {
                                bool bSelected = false;
                                if (item.Selected == true)
                                {
                                    bSelected = true;
                                }

                                var rp = m_IStorage.Combine(item.RelativePath, subdirectory);
                                item.SourceBackupItems.Add(CreateMenuItem(true, bSelected, newPath, rp, subdirectory, item, attr));
                            }
                        }

                        //Add all files under item.Path
                        var fileList = m_IStorage.GetFiles(path);
                        foreach (var file in fileList)
                        {
                            //var filePath = m_IStorage.Combine(item.Path, file);
                            var fileName = m_IStorage.GetFileName(file);
                            FileAttributes attr = File.GetAttributes(file);
                            if (!IsHidden(attr))
                            {
                                //find if item already added
                                var eItem = item.SourceBackupItems.FirstOrDefault(it => !it.IsFolder && it.Name == fileName);
                                if (eItem == null)
                                {                                    
                                    var rpeItemPath = m_IStorage.Combine(item.RelativePath, fileName);
                                    var repItem = CreateMenuItem(false, false, file, rpeItemPath, fileName, item, attr);
                                    item.SourceBackupItems.Add(repItem);

                                    eItem = repItem;
                                }

                                bool bSelected = false;
                                var rp = m_IStorage.Combine(item.RelativePath, fileName);
                                eItem.SourceBackupItems.Add(CreateMenuItem(false, bSelected, file, rp, "SIG", item, attr));

                                //bool bSelected = false;
                                //var rp = m_IStorage.Combine(item.RelativePath, fileName);
                                //item.SourceBackupItems.Add(CreateMenuItem(false, bSelected, file, rp, fileName, item, attr));
                            }
                        }
                    }
                }
            }
        }



        //----------

        protected void UpdateSelectedFolders(BackupProfileData profile, FolderMenuItem item)
        {
            SelectItemDown(item);
            SelectItemUp(item);

            UpdateSelectedFolderList(profile);
        }

        public void UpdateSelectedFolderList(BackupProfileData profile = null)
        {
            if (profile == null)
            {
                profile = ProjectData.CurrentBackupProfile;
            }

            //Update source folder selection list
            ClearSelectedFolderList();
            UpdateSelectedFolderListStep(profile, FolderMenuItemTree);

            //update folder properties/size in UI window
            //profile.UpdateProfileProperties();
        }
        void UpdateSelectedFolderListStep(BackupProfileData profile, ObservableCollection<FolderMenuItem> folderList)
        {
            foreach (var folder in folderList.Where(i => (i.IsFolder)))
            {
                if (folder.Selected == true)
                {
                    SelectedItemList.Add(new FolderData { Path = folder.Path });
                }
                else if (folder.Selected == null)
                {
                    UpdateSelectedFolderListStep(profile, folder.SourceBackupItems);
                }
            }
        }

        void SelectItemDown(FolderMenuItem item)
        {
            if (item != null)
            {
                foreach (var subItem in item.SourceBackupItems)
                {
                    subItem.Selected = item.Selected;

                    //FolderList.Remove(subItem.Path);

                    SelectItemDown(subItem);
                }
            }
        }
        void SelectItemUp(FolderMenuItem item)
        {
            if (item != null)
            {
                var parent = item.ParentItem;
                if (parent != null)
                {
                    bool? bValue = false;
                    foreach (var folder in parent.SourceBackupItems.Where(i => (i.IsFolder)))
                    {
                        if (folder.Selected != false)
                        {
                            bValue = null;
                            break;
                        }
                    }

                    parent.Selected = bValue;
                    SelectItemUp(parent);
                }
            }
        }


        public void ExpandFolder(ItemCollection itemList)
        {
            foreach (var item in itemList)
            {
                var folderItem = item as FolderMenuItem;
                if (folderItem.SourceBackupItems.Count() == 0)
                {
                    UpdateChildItemsInMenuItem(folderItem);
                }
            }
        }


        public void FolderTreeClick(FolderMenuItem item, bool bSelected)
        {
            if (item != null)
            {
                DirtyFlag = true;
                item.Selected = bSelected;

                UpdateSelectedFolders(ProjectData.CurrentBackupProfile, item);
            }
        }
    }
}
