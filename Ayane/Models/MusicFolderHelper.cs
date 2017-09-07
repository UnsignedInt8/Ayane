using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Ayane.FrameworkEx;

namespace Ayane.Models
{
    static class MusicFolderHelper
    {
        public static string MusicLibraryPath { get; private set; }

        public static async Task InitalizeMusicFolderAsync()
        {
            if (!string.IsNullOrEmpty(MusicLibraryPath)) return;

            try
            {
                var folders = await KnownFolders.MusicLibrary.GetFoldersAsync();

                if (folders.Any())
                {
                    MusicLibraryPath = Path.GetDirectoryName(folders.First().Path);
                    return;
                }

                var files = await KnownFolders.MusicLibrary.GetFilesAsync();
                if (!files.Any())
                {
                    var file = await KnownFolders.MusicLibrary.CreateFileAsync("ayane_windows_10_app.db");
                    MusicLibraryPath = Path.GetDirectoryName(file.Path);
                    await file.DeleteAsync();
                    return;
                }

                MusicLibraryPath = Path.GetDirectoryName(files.First().Path);
            }
            catch (Exception)
            {

            }
        }

        public static bool IsMusicFolder(this StorageFolder folder)
        {
            return folder.Path.StartsWith(MusicLibraryPath, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsInMusicFolder(this StorageFile file)
        {
            return file.Path.StartsWith(MusicLibraryPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
