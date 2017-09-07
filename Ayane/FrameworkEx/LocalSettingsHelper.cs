using Windows.Storage;

namespace Ayane.FrameworkEx
{
    class LocalSettingsHelper
    {
        public static T LoadValue<T>(string key, T defaultValue)
        {
            return ApplicationData.Current.LocalSettings.Values.ContainsKey(key) ? (T)ApplicationData.Current.LocalSettings.Values[key] : defaultValue;
        }

        public static void SaveValue<T>(string key, T value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }
    }
}
