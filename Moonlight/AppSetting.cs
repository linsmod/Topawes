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
            InitializeUserSetting(userName, new LiteDB.LiteDatabase(connectionString));
        }
        public static void InitializeUserSetting(string userName, LiteDB.LiteDatabase db)
        {
            if (UserSetting != null)
            {
                if(UserSetting.UserName != userName)
                UserSetting.Dispose();
                UserSetting = null;
            }
            UserSetting = new ApplicationUserSettingStorage(userName, db);
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
