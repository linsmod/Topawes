using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TopModel;

namespace LiteDB
{
    public static class LiteDbExtension
    {
        public static LiteCollection<T> Entity<T>(this LiteDatabase db)
            where T : new()
        {
            return db.GetCollection<T>(typeof(T).Name);
        }

        public static bool Any<T>(this LiteCollection<T> col)
            where T : new()
        {
            return col.Count() > 0;
        }

        public static bool Any<T>(this LiteCollection<T> col, Expression<Func<T, bool>> predicate)
            where T : new()
        {
            return col.Count(predicate) > 0;
        }

        public static T Single<T>(this LiteCollection<T> col)
            where T : new()
        {
            var count = col.Count();
            if (count == 1)
            {
                return col.Find(Query.All()).First();
            }
            else if (count > 1)
            {
                throw new LiteException("集合内不止一个元素！");
            }
            else
            {
                throw new LiteException("集合内没有任何元素！");
            }
        }
        public static LiteUpsertResult Upsert<T>(this LiteCollection<T> col, T newItem, BsonValue itemId)
            where T : new()
        {
            var x = col.FindById(itemId);
            if (x == null)
            {
                col.Insert(newItem);
                return LiteUpsertResult.Insert;
            }
            else
            {
                //var type = typeof(T);
                //var props = type.GetProperties();
                //foreach (var prop in props)
                //{
                //    var customAttrs = prop.GetCustomAttributes(typeof(LocalPropertyAttribute), false);
                //    if (customAttrs.Any())
                //    {
                //        var value = prop.GetValue(x);
                //        prop.SetValue(newItem, value);
                //    }
                //}
                col.Update(newItem);
                return LiteUpsertResult.Update;
            }
        }
    }
    public enum LiteUpsertResult
    {
        Update,
        Insert
    }
}
