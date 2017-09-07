using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight;

namespace Ayane.ViewModels
{
    public class CoreViewModel : ViewModelBase
    {
        protected bool IsInBackgroundMode { get; set; }

        public CoreViewModel()
        {
            MemoryManager.AppMemoryUsageLimitChanging += MemoryManagerOnAppMemoryUsageLimitChanging;
            MemoryManager.AppMemoryUsageIncreased += MemoryManagerOnAppMemoryUsageIncreased;
            Application.Current.EnteredBackground += App_EnteredBackground;
            Application.Current.LeavingBackground += App_LeavingBackground;
        }

        private void App_LeavingBackground(object sender, LeavingBackgroundEventArgs args)
        {
            IsInBackgroundMode = false;
            (Application.Current as App)?.RecreateFrame();
        }

        private void App_EnteredBackground(object sender, EnteredBackgroundEventArgs args)
        {
            IsInBackgroundMode = true;
            GC.Collect();
        }

        private void MemoryManagerOnAppMemoryUsageIncreased(object sender, object o)
        {
            if (MemoryManager.AppMemoryUsageLevel == AppMemoryUsageLevel.OverLimit || MemoryManager.AppMemoryUsageLevel == AppMemoryUsageLevel.High) ReduceMemoryUsage();
        }

        private void MemoryManagerOnAppMemoryUsageLimitChanging(object sender, AppMemoryUsageLimitChangingEventArgs args)
        {
            if (MemoryManager.AppMemoryUsage > args.NewLimit) ReduceMemoryUsage();
        }

        protected virtual void ReduceMemoryUsage()
        {
            if (IsInBackgroundMode) Window.Current.Content = null;
            GC.Collect();
        }

        private void PopToast()
        {
            // Generate the toast notification content and pop the toast
            var toast = Windows.UI.Notifications.ToastTemplateType.ToastImageAndText01;
            var template = ToastNotificationManager.GetTemplateContent(toast);

            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(template));
        }

    }
}
