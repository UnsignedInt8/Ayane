using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;

namespace Ayane.ViewModels
{
    class ViewModelLocator : ViewModelBase
    {
        public ViewModelLocator()
        {
            DispatcherHelper.Initialize();
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MediaLibraryViewModel>();
            SimpleIoc.Default.Register<PlayerViewModel>();
            SimpleIoc.Default.Register<SleepingModeViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<AudioEffectsViewModel>();
        }

        public MediaLibraryViewModel MediaLibraryViewModel => SimpleIoc.Default.GetInstance<MediaLibraryViewModel>();
        public PlayerViewModel PlayerViewModel => SimpleIoc.Default.GetInstance<PlayerViewModel>();
        public SecondaryPlaylistViewModel SecondaryPlaylistViewModel { get; set; }
        public SleepingModeViewModel SleepingModeViewModel => SimpleIoc.Default.GetInstance<SleepingModeViewModel>();
        public SettingsViewModel SettingsViewModel => SimpleIoc.Default.GetInstanceWithoutCaching<SettingsViewModel>();
        public AudioEffectsViewModel AudioEffectsViewModel => SimpleIoc.Default.GetInstanceWithoutCaching<AudioEffectsViewModel>();

        public static ViewModelLocator Instance => (ViewModelLocator)Application.Current.Resources["ViewModelLocator"];
    }
}
