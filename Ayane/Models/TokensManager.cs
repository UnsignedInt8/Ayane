using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.WinRT;

namespace Ayane.Models
{
    class TokensManager : IDisposable
    {
        private static TokensManager _instance;
        public static TokensManager Instance => _instance ?? (_instance = new TokensManager());

        private SQLiteConnection _db;
        private readonly List<MediaItemToken> _tokens;

        public int Count => _tokens.Count;

        private TokensManager()
        {
            var tokenFolder = ApplicationData.Current.LocalFolder.CreateFolderAsync("Tokens", CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
            _db = new SQLiteConnection(new SQLitePlatformWinRT(), Path.Combine(tokenFolder.Path, "tokens.db"));
            _db.CreateTable<MediaItemToken>();
            _tokens = _db.Table<MediaItemToken>().ToList();
        }

        public void Dispose()
        {
            _db?.Dispose();
            _db = null;
        }

        /// <summary>
        /// Request Token from system. If it returns null, there is no more space for saving this item.
        /// </summary>
        /// <param name="accessibleItem"></param>
        /// <returns></returns>
        public string RequestToken(IStorageItem accessibleItem)
        {
            if (accessibleItem == null) return null;

            var item = _tokens.SingleOrDefault(i => i.Path.Equals(accessibleItem.Path, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.Count++;
                _db.Update(item);
                return item.Token;
            }

            if (_tokens.Count == 1000) return null;

            var file = accessibleItem as IStorageFile; // For saving tokens, just optimizing for IStorageFile
            if (file != null)
            {
                var fileFolder = Path.GetDirectoryName(file.Path);
                var reusableToken = _tokens.FirstOrDefault(t => t.Path.Equals(fileFolder, StringComparison.OrdinalIgnoreCase));
                if (reusableToken != null)
                {
                    reusableToken.Count++;
                    _db.Update(reusableToken);
                    return reusableToken.Token;
                }
            }

            var token = StorageApplicationPermissions.FutureAccessList.Add(accessibleItem);
            var newItem = new MediaItemToken { Path = accessibleItem.Path, Token = token, Count = 1 };
            _tokens.Add(newItem);
            _db.Insert(newItem);

            return token;
        }

        public void IncreaseReferenceCount(string token)
        {
            if (string.IsNullOrEmpty(token)) return;

            var item = _tokens.SingleOrDefault(i => i.Token.Equals(token));
            if (item == null) return;
            item.Count++;
            _db.Update(item);
        }

        public void RemoveToken(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            var item = _tokens.SingleOrDefault(i => i.Path.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            RemoveItem(item);
        }

        public void Remove(string token)
        {
            var item = _tokens.SingleOrDefault(i => i.Token.Equals(token, StringComparison.OrdinalIgnoreCase));
            RemoveItem(item);
        }

        private void RemoveItem(MediaItemToken item)
        {
            if (item == null) return;

            if (--item.Count > 0)
            {
                _db.Update(item);
                return;
            }

            _tokens.Remove(item);
            _db.Delete(item);
            StorageApplicationPermissions.FutureAccessList.Remove(item.Token);
        }

        public async Task<IStorageItem> GetStorageItemAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            try
            {
                return await StorageApplicationPermissions.FutureAccessList.GetItemAsync(token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }

    [Table(nameof(MediaItemToken))]
    class MediaItemToken
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column(nameof(Id))]
        public int Id { get; set; }

        [Column(nameof(Path))]
        public string Path { get; set; }

        [Column(nameof(Token))]
        public string Token { get; set; }

        [Column(nameof(Count))]
        public int Count { get; set; }
    }
}
