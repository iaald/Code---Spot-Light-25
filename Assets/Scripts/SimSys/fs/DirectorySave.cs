using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataSystem;
using Kuchinashi.DataSystem;
using Newtonsoft.Json;
using SimSys;
using UnityEngine;
public class DirectorySave : ReadableAndWriteableData, ISimFile
{
    public string _Path = "_";
    [JsonIgnore]
    public override string Path
    {
        get { return System.IO.Path.Combine(Application.persistentDataPath, _Path); }
        set
        {
            _Path = value;
        }
    }
    public override void Init(string path)
    {
        this.Path = path;
        if (string.IsNullOrEmpty(Path) || !File.Exists(Path))
        {
            this.Serialize();
        }
    }
    [JsonIgnore] public string Name => _Path.Split("_").Last();
    [JsonIgnore] public bool IsDirectory => _isDirectory;
    public bool _isDirectory = true;
    [JsonIgnore] public string FsPath => _Path;

    public List<string> contents = new();

    public bool Load(out List<string> content)
    {
        Debug.Log("User Name: " + GameProgressData.GetUsername());
        Debug.Log("This is: " + Name);
        Debug.Log((IsDirectory ? "Dir" : "File") + ": " + FsPath);
        // this.Serialize();
        content = this.contents;
        return true;
    }
}