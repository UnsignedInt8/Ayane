using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Ayane.Common;
using Ayane.Controls;
using Ayane.FrameworkEx;
using Ayane.Models;
using Ayane.Themes;
using Ayane.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Toolkit.Uwp.Helpers;
using UmengSDK;

namespace Ayane
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        public bool IsFileActivated { get; private set; }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);
            IsFileActivated = true;

            await InitializeApp();

            var files = await Task.Run(() => args.Files.OfType<StorageFile>().Where(i => Playlist.SupportedContainers.Any(t => t.Equals(i.FileType, StringComparison.OrdinalIgnoreCase))).Select(f =>
            {
                try
                {
                    return new TempSong(f);
                }
                catch (Exception)
                {
                    return null;
                }
            }).OfType<Song>().ToList());

            RecreateFrame();

            if (files.Count == 0) return;
            var playerVm = ViewModelLocator.Instance.PlayerViewModel;
            playerVm.AutoPlay = true;
            playerVm.Play(files, 0, true);
        }

        private async Task InitializeApp()
        {
            var themeManager = Resources["ThemeManager"] as ThemeManager;
            MusicFolderHelper.InitalizeMusicFolderAsync();

            var viewModelLocator = ViewModelLocator.Instance;
            await viewModelLocator.PlayerViewModel.InitializeAsync();
            await viewModelLocator.MediaLibraryViewModel.InitializeAsync();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            CreateRootFrame();

            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                //TODO: Load state from previously suspended application
            }

            await InitializeApp();

            if (e.PrelaunchActivated == false)
            {
                NavigateToAppPage();
            }
            else
            {
                return;
            }

            if (!ViewModelLocator.Instance.PlayerViewModel.IsAudioEffectsSupport)
            {
                const string message = "Your device doesn't support EQ.";
                Toast.ShowMessage(string.Format(message));
            }

            UnhandledException += App_UnhandledException;

            MediaLibraryViewModel.CheckTokensUsage();

            await UmengAnalytics.StartTrackAsync("58548eb6aed17965d00005ec");
        }

        public void RecreateFrame()
        {
            if (Window.Current.Content is Frame) return;
            CreateRootFrame();
            NavigateToAppPage();
        }

        private void CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame != null) return;

            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new FrameX();
            rootFrame.NavigationFailed += OnNavigationFailed;

            // Place the frame in the current Window
            Window.Current.Content = rootFrame;

            CustomizeWindow();
        }

        private void NavigateToAppPage()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (ViewModelLocator.Instance.MediaLibraryViewModel.HasAnyPlaylist)
                {
                    //rootFrame.Navigate(typeof(Pages.MiniPlayerPage), null);
                    rootFrame.Navigate(typeof(Pages.MainPage));
                }
                else
                {
                    rootFrame.Navigate(typeof(Pages.FirstLaunchPage));
                }
                //rootFrame.Navigate(typeof(Pages.UIDebuggingPage));
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Toast.ShowMessage(e.Message);
            e.Handled = true;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private void CustomizeWindow()
        {
            if (SystemInformation.DeviceFamily.Contains("Mobile"))
            {
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            }

            var preferredSize = new Size(360, 560);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.PreferredLaunchViewSize = preferredSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(preferredSize);
            ApplicationView.GetForCurrentView().TryResizeWindowAnimation(preferredSize.Width, preferredSize.Height);

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            ResetTitleBarToAccentColor();
        }

        public static void SetTitleBarInDarkBackground()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (titleBar == null) return;

            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonHoverForegroundColor = Colors.DarkGray;
            titleBar.ButtonPressedForegroundColor = Colors.Black;
        }

        public static void SetTitleTransparent()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (titleBar == null) return;

            titleBar.ButtonForegroundColor = Colors.Transparent;
            titleBar.ButtonHoverForegroundColor = Colors.Transparent;
            titleBar.ButtonPressedForegroundColor = Colors.Transparent;
        }

        public static void SetTitleBarInLightBackground()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (titleBar == null) return;

            titleBar.ButtonForegroundColor = Colors.LightGray;
            titleBar.ButtonHoverForegroundColor = Colors.DimGray;
            titleBar.ButtonPressedForegroundColor = Colors.Black;
        }

        public static void ResetTitleBarToAccentColor()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (titleBar == null) return;

            titleBar.ButtonForegroundColor = App.Current.Resources["AccentColor"] as Color?;

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonPressedBackgroundColor = Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = Colors.Transparent;
            titleBar.ButtonPressedForegroundColor = Colors.Black;
            titleBar.ButtonHoverForegroundColor = Colors.Gray;
            titleBar.ButtonInactiveBackgroundColor = titleBar.InactiveBackgroundColor = titleBar.InactiveForegroundColor = Colors.Transparent;

            titleBar.BackgroundColor = Colors.Transparent;
        }

        public static ResourceLoader ResourceLoader { get; } = new ResourceLoader();
    }
}
