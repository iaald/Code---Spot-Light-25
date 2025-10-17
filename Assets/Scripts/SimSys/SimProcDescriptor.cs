using System;
using System.Collections.Generic;

namespace SimSys
{
    public class ProcessDescriptor
    {
        public uint Pid { get; set; }
        public uint Ppid { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public List<uint> Children { get; set; } = new();
        public Dictionary<SIGS, Action<Signal>> SignalHandlers { get; set; } = new();
        
        // 绑定的行为对象
        public IProcessBehavior Behavior { get; set; }
        
        // 自定义数据存储
        public Dictionary<string, object> Data { get; set; } = new();
        
        public ProcessDescriptor(uint pid, uint ppid, string name)
        {
            Pid = pid;
            Ppid = ppid;
            Name = name;
            IsActive = true;
        }
    }
}
