using System;
using System.IO;
using Newtonsoft.Json;

namespace Kuchinashi.DataSystem
{
    public abstract partial class WriteableData : IWriteableData
    {
        public abstract string Path { get; }

        public void Serialize()
        {
            if (File.Exists(Path)) File.Delete(Path);
            File.Create(Path).Dispose();

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
            File.WriteAllText(Path, JsonConvert.SerializeObject(this, Formatting.Indented, settings));
        }
    }

    public partial class WriteableData
    {
        public static void Serialize(string _path, object _object)
        {
            if (File.Exists(_path)) File.Delete(_path);
            File.Create(_path).Dispose();

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
            File.WriteAllText(_path, JsonConvert.SerializeObject(_object, Formatting.Indented, settings));
        }
    }
}