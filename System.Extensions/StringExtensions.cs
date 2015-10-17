using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class StringExtension
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
    }
}
