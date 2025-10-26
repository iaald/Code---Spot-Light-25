using System.Collections;
using System.Collections.Generic;
using Kuchinashi.DataSystem;
using QFramework;
using UnityEngine;
using Mission;

namespace DataSystem
{
    public partial class GameDesignData : ISingleton
    {
        public void OnSingletonInit() {}
        
        protected static Dictionary<string, MissionBase> MissionConfigs => _missionConfigs ??= GenerateConfig<MissionBase>("Missions");
        private static Dictionary<string, MissionBase> _missionConfigs;

        private static Dictionary<string, T> GenerateConfig<T>(string pathName) where T : ScriptableObject, IHaveId
        {
            var configs = new Dictionary<string, T>();
            var data = Resources.LoadAll<T>($"ScriptableObjects/{pathName}");
            foreach (var item in data)
            {
                configs.Add(item.Id, item);
            }

            return configs;
        }
    }

    public partial class GameDesignData
    {
        public static bool GetData<T>(string id, out T data) where T : ScriptableObject , IHaveId
        {
            data = Resources.Load<T>($"ScriptableObjects/{typeof(T).Name}/{id}");
            return data != null;
        }

        public static bool GetMission(string id, out MissionBase data) => MissionConfigs.TryGetValue(id, out data);
    }
}