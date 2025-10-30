using QFramework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mission
{
    [CreateAssetMenu(fileName = "TestSampleMission", menuName = "Scriptable Objects/Missions/Test Sample Mission")]
    public class TestSampleMission : MissionBase
    {
        public TestSampleMissionState state = new TestSampleMissionState();
        public override void OnMissionStart(MissionContext ctx = null)
        {
            Debug.Log("TestSampleMission OnMissionStart");
        }

        public override void OnMissionRestore(MissionContext ctx = null)
        {
            Debug.Log("TestSampleMission OnMissionRestore");
        }

        public override void OnMissionUpdate(float deltaTime)
        {
            state.totalTime += deltaTime;
            // Debug.Log("TestSampleMission OnMissionUpdate: " + state.totalTime);

            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                TypeEventSystem.Global.Send(new OnMissionAccomplishedEvent(this));
            }
        }

        public override void OnMissionAccomplished()
        {
            Debug.Log("TestSampleMission OnMissionAccomplished");
        }
        
        public override void OnMissionAbort()
        {
            Debug.Log("TestSampleMission OnMissionAbort");
        }

        public override string GetMissionStateJson()
        {
            Debug.Log("TestSampleMission GetMissionStateJson: \"" + state.totalTime + "\"");
            return JsonUtility.ToJson(state);
        }

        public override void RestoreStateFromJson(string json)
        {
            state = JsonUtility.FromJson<TestSampleMissionState>(json);
            Debug.Log("TestSampleMission RestoreStateFromJson: \"" + state.totalTime + "\"");
        }
    }

    [System.Serializable]
    public class TestSampleMissionState
    {
        public float totalTime = 0f;
    }
}