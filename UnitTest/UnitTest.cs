using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Search;
using Ayane.FrameworkEx;
using Ayane.Models;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestSongConstructor()
        {
            var file = KnownFolders.MusicLibrary.CreateFolderAsync("Day", CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult().GetFileAsync("7!! - バイバイ.mp3").GetAwaiter().GetResult();
            var song = new Song(file, string.Empty);
            Assert.IsTrue(song.Title == "バイバイ");
            Assert.IsTrue(song.ArtistName == "7!!");
            Assert.IsTrue(song.GenreTitle == "Anime");
            Assert.IsTrue(song.Year == 2011);
            Assert.IsTrue(song.AlbumTitle == "バイバイ");
        }

        [TestMethod]
        public async Task TestSongToMediaPlaybackItem()
        {
            var file = KnownFolders.MusicLibrary.CreateFolderAsync("Day", CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult().GetFileAsync("7!! - バイバイ.mp3").GetAwaiter().GetResult();
            var song = new Song(file, string.Empty);
            var player = new MediaPlayer { Source = song.ToMediaPlaybackItem(), AutoPlay = true };
            player.Play();
            await Task.Delay(TimeSpan.FromSeconds(1));
            Assert.IsTrue(player.PlaybackSession.NaturalDuration.TotalSeconds > 30);
        }

        [TestMethod]
        public void TestUnderMusicFolder()
        {
            var folder = KnownFolders.MusicLibrary;
            //Assert.IsFalse(folder.IsUnderMusicFolder());
            var token = StorageApplicationPermissions.FutureAccessList.Add(folder);
            var folderToken = StorageApplicationPermissions.FutureAccessList.GetItemAsync(token).GetAwaiter().GetResult() as StorageFolder;
            var sday = folderToken.GetFolderAsync("Day").GetAwaiter().GetResult();

            var day = KnownFolders.MusicLibrary.GetFolderAsync("Day").GetAwaiter().GetResult();
            Assert.IsTrue(day.Path == sday.Path);
        }

        [TestMethod]
        public void TestCueParser()
        {
            var cuefile = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/BCXA-1171.cue")).GetAwaiter().GetResult();
            var cueString = FileIO.ReadTextAsync(cuefile).GetAwaiter().GetResult();
            var cue = new CueSheet(cueString, null);
            Assert.IsTrue(cue.Tracks.Length == 2);
        }
    }
}
