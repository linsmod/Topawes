using LiteDB;
using Moonlight.Cryptography;
using Moonlight.EntityStorage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Moonlight.SettingStorage
{
    public sealed class ApplicationGlobalSettingStorage : DbContext
    {
        private JsonSerializerSettings Jss = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include
        };
        public ApplicationGlobalSettingStorage(LiteDatabase db) : base(db)
        {

        }
        public ApplicationGlobalSettingStorage(string connectionString) : base(connectionString)
        {
        }

        public T Get<T>(string key, T @default = default(T))
        {
            var entity = this.Entity<ApplicationGlobalSetting>().FindById(key);
            if (entity == null)
            {
                return @default;
            }
            var valueString = entity.Protected ? Encryption.Decrypt(entity.Value, Encryption.DefaultKey) : entity.Value;
            return JsonConvert.DeserializeObject<T>(valueString, Jss);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="group">分组</param>
        /// <param name="protect">加密存储</param>
        public T Set<T>(string key, T value, string group = "default", bool protect = false)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                Entity<ApplicationGlobalSetting>().Delete(key);
                return value;
            }
            var valueString = JsonConvert.SerializeObject(value, Jss);
            if (protect)
                valueString = Encryption.Encrypt(valueString, Encryption.DefaultKey);

            var entity = new ApplicationGlobalSetting
            {
                Id = key,
                Value = valueString,
                TypeName = value.GetType().FullName,
                Group = group,
                Protected = protect
            };
            Entity<ApplicationGlobalSetting>().Upsert(entity, entity.Id);
            return value;
        }
    }
}
