using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataSystem;
using Kuchinashi.DataSystem;
using Newtonsoft.Json;
using SimSys;
using UnityEngine;
public class FileSave : ISimFile, IHasPath
{
    #region Interafce
    string IHasPath.Path { get => _Path; set { _Path = value; } }
    public string _Path = "_";

    public string Name => _Path.Split("_").Last();
    public bool IsDirectory => _isDirectory;
    public bool _isDirectory = false;
    public string FsPath => _Path;
    public void Init(string path)
    {
        this._Path = CreateUniquePath(path);
    }

    public bool Load(out List<string> content)
    {
        throw new NotImplementedException();
    }
    #endregion

    public string PrefabName;
    public Vector3 Pos = new();

    public string Serialize()
    {
        // TODO: This Object will create a data representation of file object
        // We will need a PrefabName to load prefab from Resouce.
        // We will need a Pos to locate the position.
        string ret = $"{PrefabName};{Pos.ToString()}${PrefabName}";
        Debug.Log(ret);
        return ret;
    }
    public static FileSave DeSerialize(string raw)
    {
        // Deserialize raw string, packing to the object
        Debug.Log("尝试解析");
        var temp = new FileSave();
        var strs = raw.Split(";");
        temp.PrefabName = strs[0].Split("(Clone)")[0];
        temp.Pos = VectorFromString(strs[1]);
        return temp;
    }

    static Vector3 VectorFromString(string str) => 
    new(
        float.Parse(str.Split('(', ',', ')')[1]),
        float.Parse(str.Split('(', ',', ')')[2]),
        float.Parse(str.Split('(', ',', ')')[3])
    );

    public static string CreateUniquePath(string rawPath)
    {
        // Get a unique name for ExecutableFileItem::fileContents key, maybe append timestemp?
        return $"{rawPath}{System.Guid.NewGuid():N}";
    }
}