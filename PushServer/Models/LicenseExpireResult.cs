using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushServer.Models
{
    public enum LicenseState
    {
        未授权,
        正常,
        授权过期
    }
    public static class LicenseExtension
    {
        public static LicenseState AsLicenseState(this DateTime licenseExpires)
        {
            return new DateTime?(licenseExpires).AsLicenseState();
        }
        public static LicenseState AsLicenseState(this DateTime? licenseExpires)
        {
            if (!licenseExpires.HasValue)
                return LicenseState.未授权;
            return licenseExpires.Value > DateTime.Now ? LicenseState.正常 : LicenseState.授权过期;
        }

        public static string AsRemainingTime(this DateTime licenseExpires)
        {
            return new DateTime?(licenseExpires).AsRemainingTime();
        }

        public static string AsRemainingTime(this DateTime? licenseExpires)
        {
            if (licenseExpires.HasValue)
            {
                if (licenseExpires < DateTime.Now)
                {
                    return "无剩余时长";
                }
                else
                {
                    var timespan = licenseExpires.Value - DateTime.Now;
                    return string.Format("{0}天{1}时{2}分", timespan.Days, timespan.Hours, timespan.Minutes);
                }
            }
            return string.Empty;
        }
        public static string AsReadable(this DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return string.Empty;
            }
            return dt.Value.AsReadable();
        }
        public static string AsReadable(this DateTime dt)
        {
            //估计DateTime中重载了运算符 "-"号 所以能够进行两个DateTime相减
            //其实DateTime.Now-dt与 DateTime.Now.Subtract(dt)是一样的,他们的返回值都是TimeSpan
            TimeSpan span = DateTime.Now - dt;
            if (span.TotalDays >= 10 * 365)
            {
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            }
            if (span.TotalDays >= 365)
            {
                return string.Format("{0}年前", Math.Floor(span.TotalDays / 365));
            }
            else
            if (span.TotalDays >= 30)
            {
                return string.Format("{0}个月前", Math.Floor(span.TotalDays / 30));
            }
            else
            if (span.TotalDays >= 14)
            {
                return
                "2周前";
            }
            else
            if (span.TotalDays >= 7)
            {
                return
                "1周前";
            }
            else
            if (span.TotalDays >= 1)
            {
                return
                string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
            }
            else
            if (span.TotalHours >= 1)
            {
                return
                string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
            }
            else
            if (span.TotalMinutes >= 1)
            {
                return
                string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
            }
            else
            if (span.TotalSeconds >= 1)
            {
                return
                string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
            }
            else
            {
                return "1秒前";
            }
        }
    }
}
