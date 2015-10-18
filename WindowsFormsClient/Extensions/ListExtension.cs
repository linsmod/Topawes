using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient.Extensions
{
    public static class ListExtension
    {
        public static DataTable AsDataTable<T>(this IEnumerable<T> list)
        {
            var dt = new DataTable();
            var type = typeof(T);
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                var attrs = prop.GetCustomAttributes(typeof(DisplayAttribute), false);
                var displayName = attrs.Any() ? ((DisplayAttribute)attrs.First()).Name : prop.Name;
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    //var t = prop.PropertyType.GetGenericArguments().First();
                    dt.Columns.Add(new DataColumn(displayName));
                }
                else
                    dt.Columns.Add(new DataColumn(displayName, prop.PropertyType));
            }

            foreach (var item in list)
            {
                var row = dt.NewRow();
                foreach (var prop in props)
                {
                    var attrs = prop.GetCustomAttributes(typeof(DisplayAttribute), false);
                    var displayName = attrs.Any() ? ((DisplayAttribute)attrs.First()).Name : prop.Name;
                    var value = prop.GetValue(item, null);
                    row[displayName] = value;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}
