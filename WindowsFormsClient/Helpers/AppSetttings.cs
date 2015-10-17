using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient
{

    public abstract class DataStorage
    {
        protected Dictionary<string, object> memory = new Dictionary<string, object>();

        public abstract void Load();
        public abstract void Save();

        public void LoadForType(Type type)
        {
            Load();
            var fields = GetValidFieldInfoList(type);
            foreach (var item in fields)
            {
                if (this.memory.ContainsKey(item.Name) && (item.FieldType.IsPrimitive || item.FieldType.Equals(typeof(string))))
                {
                    //有匹配的数据就赋值
                    item.SetValue(type, this.GetValueFromMemory(item.Name, null));
                }
                else
                {
                    //没有匹配的数据就赋默认值
                    var x = item.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault(); if (x != null)
                    {
                        var defaultValueAttr = x as DefaultValueAttribute;
                        if (!item.FieldType.IsAssignableFrom(defaultValueAttr.Value.GetType()))
                        {
                            throw new Exception(string.Format("参数{0}的默认值类型与参数类型不兼容！", item.Name));
                        }
                        item.SetValue(type, this.GetValueFromMemory(item.Name, defaultValueAttr.Value));
                    }
                }
            }
        }
        private IEnumerable<FieldInfo> GetValidFieldInfoList(Type type)
        {
            var fields = type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            foreach (var item in fields)
            {
                if (item.FieldType.IsGenericType)
                {
                    var args = item.FieldType.GetGenericArguments();
                    if (args.Length == 1 && args[0].IsPrimitive || args[0] == typeof(string) || args[0] == typeof(DateTime))
                    {
                        yield return item;
                    }
                }
                else if (item.FieldType.IsPrimitive || item.FieldType == typeof(string) || item.FieldType == typeof(DateTime))
                {
                    yield return item;
                }
            }
        }

        public void SaveForType(Type type)
        {
            var fields = GetValidFieldInfoList(type);
            foreach (var item in fields)
            {
                var value = item.GetValue(type);
                if (value != null)
                {
                    this.SetValueMemory(item.Name, value);
                }
            }
            this.Save();
        }


        protected object ParseObject(string value)
        {
            switch (value)
            {
                case "True":
                    return true;
                case "False":
                    return false;
            }
            return value;
        }

        public object GetValueFromMemory(string key, object defaultValue)
        {
            return this.memory.ContainsKey(key) ? memory[key] : defaultValue;
        }

        public void SetValueMemory(string key, object value)
        {
            this.memory[key] = value;
        }

        public string GetString(string key, string defaultValue = "")
        {
            object value;
            if (this.memory.TryGetValue(key, out value))
            {
                if (value != null)
                    return value.ToString();
            }
            return defaultValue;
        }

        public int GetInt32(string key, int defaultValue)
        {
            object value;
            if (this.memory.TryGetValue(key, out value))
            {
                int valueInt;
                if (value != null && int.TryParse(value.ToString(), out valueInt))
                {
                    return valueInt;
                }
            }
            return defaultValue;
        }

        public bool GetBoolen(string key, bool defaultValue)
        {
            object value;
            if (this.memory.TryGetValue(key, out value))
            {
                bool valuebool;
                if (value != null && bool.TryParse(value.ToString(), out valuebool))
                {
                    return valuebool;
                }
            }
            return defaultValue;
        }
    }

   

    public class AppTextDataStorage : DataStorage
    {
        public string fileName { get; set; }
        public AppTextDataStorage(string fileName)
        {
            this.fileName = fileName;
        }

        public override void Load()
        {
            memory.Clear();
            if (!System.IO.File.Exists(fileName))
                System.IO.File.WriteAllText(fileName, "");
            var lines = System.IO.File.ReadAllLines(fileName);
            foreach (var item in lines)
            {
                var x = item.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (x.Length == 2)
                {
                    memory.Add(x[0], ParseObject(x[1]));
                }
            }
        }

        public override void Save()
        {
            var lines = memory.ToList().Where(x => x.Value != null).Select(x => x.Key + "=" + x.Value);
            System.IO.File.WriteAllLines(fileName, lines);
        }
    }

    public class AppEncryptedDataStorage : DataStorage
    {
        public string fileName { get; set; }
        public AppEncryptedDataStorage(string fileName)
        {
            this.fileName = fileName;
        }

        public override void Load()
        {
            memory.Clear();
            if (!System.IO.File.Exists(fileName))
                System.IO.File.WriteAllText(fileName, "");
            var lines = System.IO.File.ReadAllLines(fileName);
            foreach (var item in lines)
            {
                var plain = Encryption.Decrypt(item, Encryption.DefaultKey);
                var x = plain.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (x.Length == 2)
                {
                    memory.Add(x[0], ParseObject(x[1]));
                }
            }
        }

        public override void Save()
        {
            var lines = memory.ToList().Where(x => x.Value != null).Select(x => Encryption.Encrypt(x.Key + "=" + x.Value, Encryption.DefaultKey));
            System.IO.File.WriteAllLines(fileName, lines);
        }
    }
}
