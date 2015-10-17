using LiteDB;
using Moonlight.EntityStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Moonlight.EntityStorage
{
    public static class LiteDbExtension
    {
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
        public static UpsertType Upsert<T>(this LiteCollection<T> col, T item, BsonValue itemId)
           where T : new()
        {
            var x = col.FindById(itemId);
            if (x == null)
            {
                col.Insert(item);
                return UpsertType.Insert;
            }
            else
            {
                col.Update(item);
                return UpsertType.Update;
            }
        }
    }
}
