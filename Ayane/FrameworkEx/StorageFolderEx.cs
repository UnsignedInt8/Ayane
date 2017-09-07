using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace Ayane.FrameworkEx
{
    static class StorageFolderEx
    {
        public static async Task<IList<IStorageFile>> GetAllSubFoldersFileAsync(this StorageFolder folder, QueryOptions options = null)
        {
            return await folder.GetAllFilesAsync(options);
        }

        private static async Task<IList<IStorageFile>> GetAllFilesAsync(this StorageFolder folder, QueryOptions options = null, List<IStorageFile> files = null)
        {
            files = files ?? new List<IStorageFile>();

            var subFolders = await folder.GetFoldersAsync();
            foreach (var subfolder in subFolders)
            {
                await GetAllFilesAsync(subfolder, options, files);
            }

            var query = options == null ? folder.CreateFileQuery() : folder.CreateFileQueryWithOptions(options);
            var folderFiles = await query.GetFilesAsync();
            files.AddRange(folderFiles);
            return files;
        }
    }
}
