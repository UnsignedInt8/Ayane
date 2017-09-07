using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Ayane.Widgets
{
    public sealed partial class SongsCollectionItem : UserControl
    {
        private static readonly string AlbumsText = App.ResourceLoader.GetString("Text_Albums");
        private static readonly string AlbumText = App.ResourceLoader.GetString("Text_Album");
        private static readonly string SongsText = App.ResourceLoader.GetString("Text_Songs");
        private static readonly string SongText = App.ResourceLoader.GetString("Text_Song");

        public SongsCollectionItem()
        {
            InitializeComponent();
        }

        public string Title { get { return GetValue(TitleDependencyProperty) as string; } set { SetValue(TitleDependencyProperty, value); } }
        public static DependencyProperty TitleDependencyProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(SongsCollectionItem), new PropertyMetadata(null, (o, args) => ((SongsCollectionItem)o).TitleTextBlock.Text = args.NewValue.ToString()));

        public Uri CoverUri { get { return GetValue(CoverUriDependencyProperty) as Uri; } set { SetValue(CoverUriDependencyProperty, value); } }
        public static DependencyProperty CoverUriDependencyProperty = DependencyProperty.Register(nameof(CoverUri), typeof(Uri), typeof(SongsCollectionItem), new PropertyMetadata(null, (o, args) => ((SongsCollectionItem)o).Cover.UriSource = (args.NewValue as Uri)));

        public int SongsCount { get { return (int)GetValue(SongsCountDependencyProperty); } set { SetValue(SongsCountDependencyProperty, value); } }
        public static DependencyProperty SongsCountDependencyProperty = DependencyProperty.Register(nameof(SongsCount), typeof(int), typeof(SongsCollectionItem), new PropertyMetadata(0, RefreshSubtitle));

        private static void RefreshSubtitle(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var me = (SongsCollectionItem)obj;
            var albumsText = me.AlbumsCount > 1 ? AlbumsText : AlbumText;
            var songsText = me.SongsCount > 1 ? SongsText : SongText;

            me.SubtitleTextBlock.Text = me.AlbumsCount > 0 ? $"{me.AlbumsCount} {albumsText}, {me.SongsCount} {songsText}" : $"{me.SongsCount} {songsText}";
        }

        public int AlbumsCount { get { return (int)GetValue(AlbumsCountDependencyProperty); } set { SetValue(AlbumsCountDependencyProperty, value); } }
        public static DependencyProperty AlbumsCountDependencyProperty = DependencyProperty.Register(nameof(AlbumsCount), typeof(int), typeof(SongsCollectionItem), new PropertyMetadata(0, RefreshSubtitle));

        public Thickness ContentPadding { get { return RootGrid.Padding; } set { RootGrid.Padding = value; } }

        public UserControl XamlCoverImage => Cover;
        public TextBlock XamlTitleBlock => TitleTextBlock;
        public TextBlock XamlSubtitleTextBlock => SubtitleTextBlock;
    }

}
