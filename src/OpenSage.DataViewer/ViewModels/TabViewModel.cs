﻿using System.Collections.Generic;
using Caliburn.Micro;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class TabViewModel : PropertyChangedBase
    {
        public string DisplayName { get; }
        public string[] FileExtensions { get; }

        public List<FileSystemEntry> Files { get; } = new List<FileSystemEntry>();

        private FileSystemEntry _selectedFile;
        public FileSystemEntry SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                _selectedFile = value;
                NotifyOfPropertyChange();

                SelectedFileContent = FileContentViewModel.Create(value);
            }
        }

        private FileContentViewModel _selectedFileContent;
        public FileContentViewModel SelectedFileContent
        {
            get { return _selectedFileContent; }
            private set
            {
                _selectedFileContent = value;
                NotifyOfPropertyChange();
            }
        }

        public TabViewModel(string displayName, string[] fileExtensions)
        {
            DisplayName = displayName;
            FileExtensions = fileExtensions;
        }
    }
}
