using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Moonlight.Helpers
{
    //http://stackoverflow.com/questions/18333459/c-sharp-webbrowser-ajax-call
    //updated http://stackoverflow.com/questions/28526826/web-browser-control-emulation-issue-feature-browser-emulation/28626667#28626667
    public class IEFeatureControl
    {
        private static void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            var featureControlRegKey = @"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\";
            Registry.SetValue(featureControlRegKey + feature, appName, value, RegistryValueKind.DWord);
            SetBrowserFeatureControlKey64(feature, appName, value);
        }

        private static void SetBrowserFeatureControlKey64(string feature, string appName, uint value)
        {
            var featureControlRegKey = @"HKEY_CURRENT_USER\Software\Wow6432Node\Microsoft\Internet Explorer\Main\FeatureControl\";
            Registry.SetValue(featureControlRegKey + feature, appName, value, RegistryValueKind.DWord);
        }

        public static void SetWebBrowserFeatures()
        {
            // don't change the registry if running in-proc inside Visual Studio
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", appName, GetBrowserEmulationMode());

            // enable the features which are "On" for the full Internet Explorer browser

            SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", appName, 1);
            SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", appName, 1);
            SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING", appName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", appName, 1);
            SetBrowserFeatureControlKey("FEATURE_NINPUT_LEGACYMODE", appName, 0);
        }
        static UInt32 GetBrowserEmulationMode()
        {
            int browserVersion = 0;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree,
                System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }

            if (browserVersion < 7)
            {
                throw new ApplicationException("Unsupported version of Microsoft Internet Explorer!");
            }

            UInt32 mode = 11000; // Internet Explorer 11. Webpages containing standards-based !DOCTYPE directives are displayed in IE11 Standards mode. 

            switch (browserVersion)
            {
                case 7:
                    mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. 
                    break;
                case 8:
                    mode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. 
                    break;
                case 9:
                    mode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode.                    
                    break;
                case 10:
                    mode = 10000; // Internet Explorer 10.
                    break;
            }

            return mode;
        }
    }
}
