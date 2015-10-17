using Moonlight.SettingStorage;

namespace Moonlight
{
    public sealed class AppSetting
    {
        public static bool UserSettingInitialized { get; private set; }
        public static bool GlobalSettingInitialized { get; private set; }
        public static ApplicationUserSettingStorage UserSetting { get; private set; }
        public static ApplicationGlobalSettingStorage GlobalSetting { get; private set; }
        public static void InitialzeGlobalSetting(string connectionString = "app.bin")
        {
            GlobalSetting = new ApplicationGlobalSettingStorage(connectionString);
            GlobalSettingInitialized = true;
        }

        public static void InitializeUserSetting(string userName, string connectionString = "user.bin")
        {
            if (UserSetting != null)
            {
                UserSetting.Dispose();
                UserSetting = null;
            }
            UserSetting = new ApplicationUserSettingStorage(userName, connectionString);
            UserSettingInitialized = true;
        }

        public static void Uninitialize()
        {
            if (UserSetting != null)
                UserSetting.Dispose();
            if (GlobalSetting != null)
                GlobalSetting.Dispose();
        }
    }
}
