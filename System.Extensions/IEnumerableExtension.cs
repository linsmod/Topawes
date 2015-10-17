using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections
{
    public static class IEnumerableExtension
    {
        public static List<T> AsList<T>(this IEnumerable collection)
        {
            List<T> list = new List<T>();
            foreach (var item in collection)
            {
                list.Add((T)item);
            }
            return list;
        }
    }
}
