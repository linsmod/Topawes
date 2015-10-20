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
    public sealed class ApplicationUserSettingStorage : DbContext
    {
        public string UserName { get; private set; }
        private JsonSerializerSettings Jss = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include
        };
        public ApplicationUserSettingStorage(string userName, LiteDatabase db) : base(db)
        {
            this.UserName = userName;
        }
        public ApplicationUserSettingStorage(string userName, string connectionString) : base(connectionString)
        {
            this.UserName = userName;
        }

        public T Get<T>(string key, T @default = default(T))
        {
            var entity = this.Entity<ApplicationUserSetting>().FindOne(x => x.Key == key && x.UserName == UserName);
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
        public T Set<T>(string key, T value, string group, bool protect)
        {
            var entity = this.Entity<ApplicationUserSetting>().FindOne(x => x.Key == key && x.UserName == UserName);
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                if (entity != null)
                    Entity<ApplicationUserSetting>().Delete(entity.Id);
            }
            else
            {
                var valueString = JsonConvert.SerializeObject(value, Jss);
                if (protect)
                    valueString = Encryption.Encrypt(valueString, Encryption.DefaultKey);
                if (entity == null)
                {
                    entity = new ApplicationUserSetting();
                    entity.Id = ObjectId.NewObjectId().ToString();
                    entity.Key = key;
                    entity.UserName = UserName;
                    entity.Value = valueString;
                    entity.Protected = protect;
                    entity.TypeName = value.GetType().FullName;
                    entity.Group = group;
                    Entity<ApplicationUserSetting>().Insert(entity);
                }
                else
                {
                    entity.Value = valueString;
                    entity.Protected = protect;
                    entity.TypeName = value.GetType().FullName;
                    entity.Group = group;
                    Entity<ApplicationUserSetting>().Update(entity);
                }
            }
            return value;
        }

        public T Set<T>(string key, T value, bool protect = false)
        {
            return Set<T>(key, value, group: "default", protect: protect);
        }

        public void SetNull(string key)
        {
            var entity = this.Entity<ApplicationUserSetting>().FindOne(x => x.Key == key && x.UserName == UserName);
            if (entity != null)
            {
                Entity<ApplicationGlobalSetting>().Delete(entity.Id);
            }
        }
    }
}
