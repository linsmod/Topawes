using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient.Extensions
{
    public static class StringExtensions
    {
        public static DateTime? AsNullableDateTime(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return DateTime.Parse(input);
            }
            return null;
        }

        public static DateTime AsDateTime(this string input)
        {
            return DateTime.Parse(input);
        }

        public static string AsZhConnectionState(this ConnectionState connstate)
        {
            switch (connstate)
            {
                case ConnectionState.Connecting:
                    return "正在连接...";
                case ConnectionState.Connected:
                    return "已连接";
                case ConnectionState.Reconnecting:
                    return "断线重连...";
                case ConnectionState.Disconnected:
                    return "未连接";
                default:
                    break;
            }
            return string.Empty;
        }
    }
}
