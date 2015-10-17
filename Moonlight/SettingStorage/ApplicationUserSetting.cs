using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moonlight.SettingStorage
{
    public class ApplicationUserSetting
    {
        public string Id { get; set; }
        [BsonIndex]
        public string Key { get; set; }
        [BsonIndex]
        public string UserName { get; set; }
        public string Value { get; set; }
        public string TypeName { get; set; }
        [BsonIndex]
        public string Group { get; set; }
        public bool Protected { get; set; }
    }
}
