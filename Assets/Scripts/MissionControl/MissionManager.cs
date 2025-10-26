using System.Collections.Generic;
using UnityEngine;
using DataSystem;
using System.Linq;
using QFramework;

namespace Mission
{
	public class MissionManager : MonoBehaviour
	{
		[Header("初始任务")]
		public List<MissionBase> initialMissions = new List<MissionBase>();

		[Header("当前进行中的任务")]
		public List<MissionBase> currentMissions = new List<MissionBase>();

		[Header("启动时自动恢复存档并尝试启动初始候选")]
		public bool autoLoadOnStart = true;

		// 遍历快照缓冲，避免在 Update 中因列表被修改而抛出异常
		private readonly List<MissionBase> _iterBuffer = new List<MissionBase>(16);

		private void Awake()
		{
			TypeEventSystem.Global.Register<OnMissionAccomplishedEvent>(e =>
            {
                HandleMissionCompleted(e.mission);
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

			DontDestroyOnLoad(gameObject);
		}

		private void Start()
		{
			if (autoLoadOnStart)
			{
				RestoreAndStart();
			}
		}

		private void Update()
		{
			if (currentMissions == null || currentMissions.Count == 0) return;
			_iterBuffer.Clear();
			_iterBuffer.AddRange(currentMissions);
			for (int i = 0; i < _iterBuffer.Count; i++)
			{
				var mission = _iterBuffer[i];
				if (mission == null) continue;
				mission.OnMissionUpdate(Time.deltaTime);
			}
		}

		private void OnApplicationQuit()
        {
			foreach (var mission in currentMissions)
			{
				mission.OnMissionAbort();
			}
            UpdateToGameProgressData();
        }

		private void UpdateToGameProgressData()
		{
			GameProgressData.SetCurrentMissionIds(currentMissions.Select(m => m.Id).ToList());
			GameProgressData.SetMissionStates(currentMissions.ToDictionary(m => m.Id, m => m.GetMissionStateJson()));

			GameProgressData.Instance.Serialize();
		}

		public void RestoreAndStart()
		{
			currentMissions.Clear();
			foreach (var id in GameProgressData.GetCurrentMissionIds())
			{
				if (GameDesignData.GetMission(id, out var mission))
				{
					mission.RestoreStateFromJson(GameProgressData.GetMissionStateById(id));
					mission.OnMissionRestore();

					currentMissions.Add(mission);
				}
			}

			if (currentMissions.Count == 0)
            {
                foreach (var mission in initialMissions)
                {
                    if (CheckPrerequisites(mission)) AddAndStartMission(mission);
                }
            }

			UpdateToGameProgressData();
		}

		private void HandleMissionCompleted(MissionBase mission)
		{
			if (mission == null) return;

			mission.OnMissionAccomplished();
			currentMissions.Remove(mission);
			GameProgressData.MarkMissionAccomplished(mission.Id);

			foreach (var postMissionId in mission.postAccomplishedMissionIds)
			{
				if (GameDesignData.GetMission(postMissionId, out var postMission))
				{
					if (CheckPrerequisites(postMission)) AddAndStartMission(postMission);
				}
			}

			UpdateToGameProgressData();
		}

		private bool IsMissionRunning(string missionId) => currentMissions.Any(m => m != null && m.Id == missionId);
		private bool IsMissionRunning(MissionBase mission) => currentMissions.Contains(mission);

		private void AddAndStartMission(MissionBase mission)
		{
			if (mission == null) return;
			if (IsMissionRunning(mission) || GameProgressData.IsMissionFinished(mission.Id)) return;

			currentMissions.Add(mission);
			mission.OnMissionStart(new MissionContext(mission.Id));

			UpdateToGameProgressData();
		}

		private bool CheckPrerequisites(MissionBase mission)
		{
			// 前置任务
			if (mission.prerequisiteMissionIds != null)
			{
				return mission.prerequisiteMissionIds.All(id => GameProgressData.IsMissionFinished(id));
			}

			// 前置状态（此处默认通过，留待接入具体状态系统）
			return true;
		}
	}
}





