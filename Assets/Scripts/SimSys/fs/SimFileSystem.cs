using UnityEngine;
using Kuchinashi.DataSystem;
using MoonSharp.VsCodeDebugger.SDK;
using System.Collections.Generic;
namespace SimSys
{
    public class SimFileSystem
    {
        public T GetDataObject<T>(string path) where T : IReadableData, IHasPath, new()
        {
            var temp = new T();
            temp.Init(path);
            return temp.DeSerialize<T>();
        }
    }
    public interface ISimFile
    {
        public string Name { get; }
        public string FsPath { get; }
        public bool IsDirectory { get; }
        public bool Load(out List<string> content);
    }
}