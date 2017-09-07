using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Ayane.Common;
using Ayane.Models;
using Ayane.Widgets;
using GalaSoft.MvvmLight;

namespace Ayane.ViewModels
{
    class NewPlaylistViewModel : ViewModelBase
    {
        private readonly StorageItemEqualityComparer _storageItemEqualityComparer = new StorageItemEqualityComparer();
        private List<IStorageItem> _tempItems = new List<IStorageItem>();

        private string _title = string.Empty;
        public string Title { get { return _title; } set { _title = value; RaisePropertyChanged(); } }

        private int _folderCount;
        public int FolderCount { get { return _folderCount; ; } set { _folderCount = value; RaisePropertyChanged(); } }

        private int _fileCount;
        public int FileCount { get { return _fileCount; } set { _fileCount = value; RaisePropertyChanged(); } }

        private bool _isFilesProcessing;
        public bool IsFilesProcessing { get { return _isFilesProcessing; } set { _isFilesProcessing = value; RaisePropertyChanged(); } }

        private bool _hasFiles;
        public bool HasFiles { get { return _hasFiles; } set { _hasFiles = value; RaisePropertyChanged(); } }

        private int _processedFilesCount;
        public int ProcessedFilesCount { get { return _processedFilesCount; } set { _processedFilesCount = value; RaisePropertyChanged(); } }

        private bool _isTitleAvailable;
        public bool IsTitleAvailable { get { return _isTitleAvailable; } set { _isTitleAvailable = value; RaisePropertyChanged(); } }

        public event EventHandler<CreatePlaylistEventArgs> Created;
        protected virtual void OnCreated(CreatePlaylistEventArgs e)
        {
            Created?.Invoke(this, e);
        }

        public void OnDragOver(object sender, DragEventArgs e)
        {
            if (MediaLibraryViewModel.CheckTokensThresold())
            {
                e.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        public async void OnDrop(object sender, DragEventArgs e)
        {
            await OnDropAsync(sender, e);
        }

        public async Task OnDropAsync(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count == 0) return;

            AddStoageItems(items);
            UpdateFolderFilesCount(_tempItems);

            if (!string.IsNullOrEmpty(Title)) return;
            var firstFolder = items.OfType<StorageFolder>().FirstOrDefault();
            if (firstFolder == null) return;
            Title = firstFolder.DisplayName;
        }

        public async void OnBrowseClick(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.MusicLibrary, ViewMode = PickerViewMode.List, };
            foreach (var container in Playlist.SupportedContainers)
            {
                picker.FileTypeFilter.Add(container);
            }

            var folder = await picker.PickSingleFolderAsync();
            _tempItems = new List<IStorageItem> { folder };
            UpdateFolderFilesCount(_tempItems);
            if (Title.Length == 0) Title = folder.DisplayName;
        }

        public async void OnCreatePlaylistClick(object sender, RoutedEventArgs e)
        {
            if (!await Playlist.IsTitleAvailable(Title))
            {
                Toast.ShowMessage(App.ResourceLoader.GetString("Message_PlaylistTitleExist"));
                return;
            }

            var box = CreateProcessingMessageBox();
            box.Show();
            await CreatePlaylistAsync();
            box.Close();
        }

        public void OnTitleTextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var title = ((TextBox)sender).Text;
            IsTitleAvailable = Playlist.IsTitleLegal(title);
        }

        private void UpdateFolderFilesCount(IReadOnlyList<IStorageItem> items)
        {
            FolderCount = items.Count(i => i is StorageFolder);
            FileCount = items.Count(i => Playlist.IsMediaFile(i as IStorageFile));

            HasFiles = FolderCount > 0 || FileCount > 0;
        }

        public MessageBox CreateProcessingMessageBox()
        {
            var processedText = App.ResourceLoader.GetString("MessageBox_Message_ProcessedFiles");
            var messageTip = new MessageBox
            {
                Title = App.ResourceLoader.GetString("MessageBox_Title_JustAMinute"),
                Message = $"{processedText} 0",
                ButtonsVisibility = Visibility.Collapsed,
            };

            PropertyChanged += (sender, args) => messageTip.Message = $"{processedText} {ProcessedFilesCount}";
            return messageTip;
        }

        public async Task CreatePlaylistAsync()
        {
            IsFilesProcessing = true;
            var playlist = new Playlist(Title);
            ProcessedFilesCount = 0;
            playlist.NewItemImported += (o, args) => ++ProcessedFilesCount;
            await playlist.ImportStorageItemsAsync(_tempItems);
            IsFilesProcessing = false;
            OnCreated(new CreatePlaylistEventArgs { Playlist = playlist });
        }

        public void AddStoageItems(IEnumerable<IStorageItem> items)
        {
            _tempItems.AddRange(items.Where(i =>
            {
                if (i is IStorageFolder) return true;
                var file = i as IStorageFile;
                return file != null && Playlist.SupportedContainers.Contains(file.FileType);
            }));

            _tempItems = _tempItems.Distinct(_storageItemEqualityComparer).ToList();
        }

        internal class CreatePlaylistEventArgs : EventArgs
        {
            public Playlist Playlist { get; set; }
        }

        private class StorageItemEqualityComparer : IEqualityComparer<IStorageItem>
        {
            public bool Equals(IStorageItem x, IStorageItem y)
            {
                return x.Path.Equals(y.Path, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(IStorageItem obj)
            {
                return obj.Path.GetHashCode();
            }
        }
    }
}
