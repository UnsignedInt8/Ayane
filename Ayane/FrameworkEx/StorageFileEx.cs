using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Ayane.FrameworkEx
{
    static class StorageFileEx
    {
        public static async Task<StorageFile> GetFileFromPathAsync(string path)
        {
            try
            {
                return await StorageFile.GetFileFromPathAsync(path);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
