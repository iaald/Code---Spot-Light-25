using System.Collections.Generic;
using System.Linq;
using Kuchinashi.DataSystem;
using UnityEngine;
using QFramework;

namespace DataSystem
{
    public partial class GameProgressData
    {
        public List<string> CurrentMissionIds = new List<string>();
        public List<string> FinishedMissionIds = new List<string>();

        public Dictionary<string, string> CurrentMissionStates = new Dictionary<string, string>();
    }

    public partial class GameProgressData
    {
        public static void MarkMissionAccomplished(string missionId)
        {
            Instance.FinishedMissionIds.Add(missionId);
        }
        public static bool IsMissionFinished(string missionId) => Instance.FinishedMissionIds.Contains(missionId);

        public static List<string> GetCurrentMissionIds() => Instance.CurrentMissionIds;
        public static void SetCurrentMissionIds(List<string> missionIds) => Instance.CurrentMissionIds = missionIds;

        public static string GetMissionStateById(string missionId) => Instance.CurrentMissionStates.TryGetValue(missionId, out var state) ? state : "";
        public static void SetMissionStateById(string missionId, string state) => Instance.CurrentMissionStates[missionId] = state;
        public static void SetMissionStates(Dictionary<string, string> missionStates) => Instance.CurrentMissionStates = missionStates;
    }
}