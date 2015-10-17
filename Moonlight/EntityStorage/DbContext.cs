using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlight.EntityStorage
{
    public class DbContext : IDisposable
    {
        public LiteDatabase Database { get; private set; }
        public DbContext(LiteDatabase db)
        {
            this.Database = db;
        }
        public DbContext(string connectionString)
        {
            Database = new LiteDatabase(connectionString);
        }
        public Dictionary<string, object> _entities = new Dictionary<string, object>();
        public LiteCollection<T> Entity<T>(string name)
            where T : new()
        {
            if (!_entities.ContainsKey(name))
                _entities[name] = Database.GetCollection<T>(name);
            return _entities[name] as LiteCollection<T>;
        }
        public LiteCollection<T> Entity<T>()
             where T : new()
        {
            return Entity<T>(typeof(T).Name);
        }

        public void Dispose()
        {
            this.Database.Dispose();
        }
    }
}
