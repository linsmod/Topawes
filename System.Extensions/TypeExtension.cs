using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System
{
    public static class TypeExtension
    {
        public static Type[] SimpleTypes = new Type[] { typeof(string), typeof(DateTime), typeof(Guid) };

        public static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive || SimpleTypes.Any(x => x == type);
        }

        public static bool IsNullableSimpleType(this Type type)
        {
            if (type.IsGenericType)
            {
                var args = type.GetGenericArguments();
                return args.Length == 1 && args[0].IsSimpleType();
            }
            return false;
        }

        public static IEnumerable<PropertyInfo> GetSimplePropertyInfos(Type type)
        {
            var fields = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (var item in fields)
            {
                if (item.PropertyType.IsNullableSimpleType() || item.PropertyType.IsSimpleType())
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<FieldInfo> GetSimpleFieldInfos(Type type)
        {
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var item in fields)
            {
                if (item.FieldType.IsNullableSimpleType() || item.FieldType.IsSimpleType())
                {
                    yield return item;
                }
            }
        }
    }
}
