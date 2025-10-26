using UnityEngine;
using Kuchinashi.DataSystem;
using Kuchinashi;
using System.Collections.Generic;

namespace Mission
{
    public abstract class MissionBase : ScriptableObject, IHaveId
    {
        public string Id => _id;
        [SerializeField] private string _id;

        public string Name = "";
        public string Description = "";

        public List<string> prerequisiteMissionIds = new List<string>();
        public SerializableDictionary<string, string> prerequisiteStates = new SerializableDictionary<string, string>();

        public List<string> postAccomplishedMissionIds = new List<string>();

        public virtual void OnMissionStart(MissionContext ctx = null) { }
        public virtual void OnMissionRestore(MissionContext ctx = null) { }
        public virtual void OnMissionUpdate(float deltaTime) { }
        public virtual void OnMissionAccomplished() { }
        public virtual void OnMissionAbort() { }

        public virtual string GetMissionStateJson() { return ""; }
        public virtual void RestoreStateFromJson(string json) { }
    }

    public class MissionContext
    {
        public string missionId;
        public MissionContext(string missionId)
        {
            this.missionId = missionId;
        }
    }
}



