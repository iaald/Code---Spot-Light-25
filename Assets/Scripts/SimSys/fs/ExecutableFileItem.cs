using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QFramework;
using SimSys;
using UnityEngine;

public class ExecutableFileItem : MonoBehaviour
{
    [HideInInspector] public string Name => FsPath.Split('_').Last();
    public bool IsDirectory;
    public string FsPath = "_";
    public ISimFile simFile;
    private Dictionary<string, string> fileContent;
    public bool Prepared { get; private set; } = false;

    private Queue<Action> actionQueue = new();
    private Coroutine worker;

    public void Start()
    {
        if (IsDirectory)
        {
            StartDir();
        }
        else
        {
            StartCoroutine(StartFile());
        }
    }
    void OnDisable()
    {
        if (!this.IsDirectory)
        {
            this.WriteFile();
        }
    }
    public void StartDir()
    {
        var a = SysRoot.Instance.SimFileSystem.GetDataObject<DirectorySave>(FsPath);
        simFile = a;
        a.Load(out var contents);
        fileContent = new();
        foreach (var c in contents.ToArray())
        {
            try
            {
                string[] temp = c.Split('$');
                fileContent.Add(temp[0], temp[1]);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{transform.name} File Save Format Error. Not compitable for: {c}.\n" + e.Message);
            }
        }

        // 启动Worker协程
        this.worker = StartCoroutine(Worker());

        this.Prepared = true;
    }
    public IEnumerator StartFile()
    {
        // 测试
        // WriteFile();
        if (!IsDirectory && transform.parent.TryGetComponent<ExecutableFileItem>(out var fileItem))
        {
            yield return new WaitUntil(() => fileItem.Prepared);
            FileSave fileSave;

            //If it exist, this FsPath is to be set by creator
            if (fileItem.fileContent.TryGetValue(FsPath, out string rawContent))
            {
                fileSave = FileSave.DeSerialize(rawContent);
                // Apply
                transform.position = fileSave.Pos;
            }
            else
            {
                //This is a new file on this directory, gennerate new FileSave
                fileSave = new FileSave();

                fileSave.Init(FsPath); // Create unique path
                this.FsPath = fileSave.FsPath;
            }
            simFile = fileSave;
        }
        this.Prepared = true;
        yield return null;
    }

    void OnDestroy()
    {
        if (worker != null)
        {
            StopCoroutine(worker);
            worker = null;
        }
        if (actionQueue.Count > 0)
        {
            foreach (var action in actionQueue)
            {
                action.Invoke();
            }
        }
        if (simFile != null)
        {
            if (simFile is DirectorySave ds)
            {
                List<string> temp = new();
                foreach (var kv in fileContent)
                {
                    temp.Add($"{kv.Key}${kv.Value}");
                }
                ds.contents = temp;
                ds.Serialize();
            }
        }
    }

    public void WriteFile()
    {
        // 子模拟文件信息写入父目录存档文件
        if (!IsDirectory && transform.parent.TryGetComponent<ExecutableFileItem>(out var fileItem))
        {
            if (this.simFile is FileSave fs)
            {
                // 写入在这里
                if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(gameObject))
                {
                    fs.PrefabName = UnityEditor.PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject).name;
                }
                else
                {
                    fs.PrefabName = name;
                }
                fs.Pos = transform.position;

                // Here FsPath should already be the unique one
                fileItem.EnqueueWrite($"{FsPath}${fs.Serialize()}");
            }
        }
    }

    private void EnqueueWrite(string arg)
    {
        Debug.Log("Enqueue write " + arg);
        this.actionQueue.Enqueue(() => { this.Write(arg); });// 这里不好 - 在引用时会持有这个函数的实例
    }
    private void Write(string content)
    {
        if (IsDirectory)
        {
            int idx = content.IndexOf("$");
            string key = content[..idx];
            Debug.Log(key);
            string value = content[(idx+1)..];
            Debug.Log(value);
            this.fileContent[key] = value;
        }
        else
        {
            // If is file
        }
    }
    IEnumerator Worker()
    {
        while (true)
        {
            if (this.actionQueue == null || this.actionQueue.Count == 0)
            {
                yield return null;
                continue;
            }
            this.actionQueue.Dequeue()?.Invoke();
            yield return null;
        }
    }
}
