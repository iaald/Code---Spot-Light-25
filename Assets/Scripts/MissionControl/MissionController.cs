using UnityEngine;

namespace Mission
{
    public class MissionController : MonoBehaviour
    {
        public MissionBase data;

        public void StartMission(MissionContext ctx = null)
        {
            if (data == null) return;
            data.OnMissionStart(ctx);
        }

        public void EndMission()
        {
            if (data == null) return;
            data.OnMissionAccomplished();
        }

        public void AbortMission()
        {
            if (data == null) return;
            data.OnMissionAbort();
        }

        private void Update()
        {
            if (data == null) return;
            data.OnMissionUpdate(Time.deltaTime);
        }
    }
}



