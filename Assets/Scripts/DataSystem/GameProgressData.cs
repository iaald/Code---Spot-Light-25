using System.Collections.Generic;
using System.Linq;
using Kuchinashi.DataSystem;
using UnityEngine;
using QFramework;
using Newtonsoft.Json;

namespace DataSystem
{
    public partial class GameProgressData : ReadableAndWriteableData, ISingleton
    {
        [JsonIgnore]
        public override string Path
        {
            get => System.IO.Path.Combine(Application.persistentDataPath, "save");
            set { }
        }
        public static GameProgressData Instance
        {
            get => _instance ??= new GameProgressData().DeSerialize<GameProgressData>();
            private set => _instance = value;
        }
        private static GameProgressData _instance;
        public void OnSingletonInit() { }

        public override void Init(string path)
        {
            throw new System.NotImplementedException();
        }

        private string username = "";
        protected static string Username => Instance.username;
        public static void SetUsername(string username)
        {
            Instance.username = username;
            Instance.Serialize();
        }
        public static string GetUsername() => Instance.username;
    }

    public partial class GameProgressData
    {
        public bool WriteToObject<T>(string fieldName, T value)
        {
            var field = this.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (field != null && field.FieldType.IsAssignableFrom(typeof(T)))
            {
                field.SetValue(this, value);
                return true;
            }
            return false;
        }

        [JsonIgnore] public bool IsNewGame => !System.IO.File.Exists(Path);
    }
}