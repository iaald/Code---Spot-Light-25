using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InstallerNextButton : MonoBehaviour
{
    [Header("按钮行为")]
    public InstallerController installerController;
    [SerializeField] private Button m_button;
    public InstallerSettingData[] settingDatas;
    public UnityEvent[] OnClick;
    private int currentIdx = 0;
    [Header("快照到存储")]
    public List<ContentMapping> contentMappings;
    void Start()
    {
        if (OnClick.Length == 0)
        {
            return;
        }
        m_button.onClick.AddListener(() =>
        {
            SaveSnapshotToObject();
            OnClick[currentIdx++].Invoke();
            if (currentIdx > OnClick.Length - 1)
            {
                m_button.onClick.RemoveAllListeners();
            }
        });
    }
    void NOP() { }
    public void LoadNewMenu(int i)
    {
        if (i > settingDatas.Length - 1)
        {
            Debug.LogWarning("Index out of bounds.");
            return;
        }
        installerController.LoadNewMenu(settingDatas[i]);
    }
    public void SwitchToScene(string sceneName)
    {
        SceneMng.Instance.SwitchScene(sceneName);
    }
    public void SaveSnapshotToObject()
    {
        var t = from p in contentMappings
                where p.idx == currentIdx
                select p;
        foreach (var cm in t)
        {
            Debug.Log(installerController.GetRightValue(cm.blckIdx) +
            ";\nField: " + cm.fieldName);
            if (
            GameProgressData.Instance.WriteToObject(
                cm.fieldName,
                installerController.GetRightValue(cm.blckIdx)
            )
            )
            {
                Debug.Log("Snapshot saved");
            }
            else
            {
                Debug.Log("Failed: Saving Snapshot");
            }
        }
        GameProgressData.Instance.Serialize();
    }

    [Serializable]
    public class ContentMapping
    {
        public int idx;
        public int blckIdx;
        public string fieldName;
    }
}
