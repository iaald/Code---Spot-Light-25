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
        [JsonIgnore] public override string Path { get => System.IO.Path.Combine(Application.persistentDataPath, "save"); }
        public static GameProgressData Instance
        {
            get => _instance ??= new GameProgressData().DeSerialize<GameProgressData>();
            private set => _instance = value;
        }
        private static GameProgressData _instance;
        public void OnSingletonInit() { }
    }

    public partial class GameProgressData
    {
        [JsonIgnore] public bool IsNewGame => !System.IO.File.Exists(Path);
    }
}