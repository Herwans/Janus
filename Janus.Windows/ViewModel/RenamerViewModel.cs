using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Janus.Lib.Helper;
using Janus.Lib.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Janus.Windows.ViewModel
{
    public partial class RenamerViewModel : ObservableObject
    {
        private readonly ICollectionView filteredFiles;

        [ObservableProperty]
        private bool useRegex;
        partial void OnUseRegexChanged(bool oldValue, bool newValue)
            => ApplyReplacePattern();

        [ObservableProperty]
        private bool caseSensitive;
        partial void OnCaseSensitiveChanged(bool oldValue, bool newValue)
            => ApplyReplacePattern();

        [ObservableProperty]
        private bool removeSearch;
        partial void OnRemoveSearchChanged(bool oldValue, bool newValue)
            => ApplyReplacePattern();

        [ObservableProperty]
        private string searchPattern = "";
        partial void OnSearchPatternChanged(string? oldValue, string newValue)
            => ApplyReplacePattern();

        [ObservableProperty]
        private string replacePattern = "";
        partial void OnReplacePatternChanged(string? oldValue, string newValue)
            => ApplyReplacePattern();

        [ObservableProperty]
        private string folderPath = "";

        [ObservableProperty]
        private ObservableCollection<FileItem> files;

        public ICollectionView FilteredFiles
        {
            get => filteredFiles;
        }

        public ICommand LoadFilesCommand { get; private set; }
        public ICommand ReplaceCommand { get; private set; }

        public RenamerViewModel()
        {
            Files = [];
            filteredFiles = CollectionViewSource.GetDefaultView(Files);
            filteredFiles.Filter = FilterFiles;

            LoadFilesCommand = new RelayCommand(OpenFolder);
            ReplaceCommand = new RelayCommand(Replace);
        }

        private bool FilterFiles(object item)
        {
            if (item is FileItem fileItem)
            {
                if (string.IsNullOrEmpty(SearchPattern))
                {
                    return true;
                }

                if (UseRegex)
                {
                    try
                    {
                        return Regex.IsMatch(fileItem.CurrentName, SearchPattern, CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        return false;
                    }
                }
                else
                {
                    return fileItem.CurrentName.Contains(SearchPattern, CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }

        private void OpenFolder()
        {
            OpenFolderDialog opd = new();
            if (opd.ShowDialog() == true)
            {
                FolderPath = opd.FolderName;
                LoadFiles();
            }
        }

        private void LoadFiles()
        {
            Files.Clear();
            if (Directory.Exists(FolderPath))
            {
                DirectoryInfo directoryInfo = new(FolderPath);
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    Files.Add(new()
                    {
                        CurrentName = file.Name,
                        NewName = file.Name,
                    });
                }
            }
        }

        private void ApplyReplacePattern()
        {
            if (!RegexHelper.IsValidRegex(SearchPattern)) return;

            foreach (FileItem file in FilteredFiles)
            {
                file.NewName = RegexHelper.PatternsReplacer(file,
                    new()
                    {
                        CaseSensitive = CaseSensitive,
                        SearchPattern = SearchPattern,
                        ReplacePattern = ReplacePattern,
                        KeepSearch = !RemoveSearch,
                        IsRegex = UseRegex
                    }).NewName;
            }
            filteredFiles.Refresh();
        }

        private void Replace()
        {
            List<FileItem> alreadyExistsNewName = FilteredFiles.Cast<FileItem>()
                .Where(item => File.Exists(Path.Combine(FolderPath + item.NewName)))
                .ToList();

            List<string> duplicateNewNames = FilteredFiles.Cast<FileItem>()
               .GroupBy(item => item.NewName)
               .Where(group => group.Count() > 1)
               .Select(group => group.Key)
               .ToList();

            try
            {
                if (alreadyExistsNewName.Any())
                {
                    throw new Exception($"Can't apply the changes, one or more item with the same name already exist.");
                }

                if (duplicateNewNames.Any())
                {
                    throw new Exception($"Can't apply the changes, one or more item have the same new name.");
                }

                foreach (FileItem file in FilteredFiles)
                {
                    if (file.CurrentName != file.NewName)
                    {
                        File.Move(
                            Path.Combine(FolderPath, file.CurrentName),
                            Path.Combine(FolderPath, file.NewName)
                            );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error while replacing names", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadFiles();
            filteredFiles.Refresh();
        }
    }
}