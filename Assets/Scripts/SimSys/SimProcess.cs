using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QFramework;
using UnityEngine;

namespace SimSys
{
    public class ProcessManager
    {
        private Dictionary<uint, ProcessDescriptor> processes = new();
        private uint nextPid = 1;
        private Queue<uint> recycledPids = new(); // PID 回收池
        
        /// <summary>
        /// 创建 init 进程 (PID 0)
        /// </summary>
        public ProcessManager()
        {
            var init = new ProcessDescriptor(0, 0, "init");
            processes[0] = init;
        }
        
        /// <summary>
        /// Fork: 创建子进程
        /// </summary>
        public uint Fork(uint parentPid, string childName)
        {
            if (!processes.ContainsKey(parentPid) || !processes[parentPid].IsActive)
            {
                throw new InvalidOperationException($"Parent process {parentPid} not found or inactive");
            }
            
            // 优先使用回收的 PID
            uint childPid;
            if (recycledPids.Count > 0)
            {
                childPid = recycledPids.Dequeue();
            }
            else
            {
                childPid = nextPid++;
            }
            
            var child = new ProcessDescriptor(childPid, parentPid, childName);
            processes[childPid] = child;
            
            // 更新父进程的子进程列表
            processes[parentPid].Children.Add(childPid);
            
            return childPid;
        }
        
        /// <summary>
        /// 为进程绑定行为对象
        /// </summary>
        public void BindBehavior(uint pid, IProcessBehavior behavior)
        {
            if (!processes.ContainsKey(pid))
            {
                throw new InvalidOperationException($"Process {pid} not found");
            }
            
            var process = processes[pid];
            process.Behavior = behavior;
            
            // 触发启动回调
            behavior?.OnProcessStart(pid);
        }
        
        /// <summary>
        /// 解绑行为对象
        /// </summary>
        public void UnbindBehavior(uint pid)
        {
            if (processes.ContainsKey(pid))
            {
                var process = processes[pid];
                process.Behavior?.OnProcessTerminate();
                process.Behavior = null;
            }
        }
        
        /// <summary>
        /// 终止进程（标记为僵尸状态）
        /// </summary>
        public void Terminate(uint pid)
        {
            if (!processes.ContainsKey(pid))
                return;
                
            var process = processes[pid];
            if (!process.IsActive)
                return;
            
            // 通知行为对象
            process.Behavior?.OnProcessTerminate();
            
            // 标记为非活动状态（僵尸进程）
            process.IsActive = false;
            
            // 向父进程发送 SIGCHLD
            if (processes.ContainsKey(process.Ppid))
            {
                SendSignal(process.Ppid, new Signal(SIGS.SIGCHLD, pid));
            }
            
            // 递归终止所有子进程
            foreach (var childPid in process.Children.ToList())
            {
                Terminate(childPid);
            }
        }
        
        /// <summary>
        /// Wait: 父进程等待并清理子进程（类似 waitpid）
        /// </summary>
        public bool Wait(uint parentPid, uint childPid)
        {
            if (!processes.ContainsKey(parentPid) || !processes[parentPid].IsActive)
            {
                throw new InvalidOperationException($"Parent process {parentPid} not found or inactive");
            }
            
            if (!processes.ContainsKey(childPid))
            {
                return false; // 子进程不存在
            }
            
            var parent = processes[parentPid];
            var child = processes[childPid];
            
            // 检查是否真的是父子关系
            if (child.Ppid != parentPid)
            {
                throw new InvalidOperationException($"Process {childPid} is not a child of {parentPid}");
            }
            
            // 只能清理已终止的子进程
            if (child.IsActive)
            {
                return false; // 子进程还在运行
            }
            
            // 从父进程的子进程列表中移除
            parent.Children.Remove(childPid);
            
            // 从进程表中删除
            processes.Remove(childPid);
            
            // 回收 PID
            recycledPids.Enqueue(childPid);
            
            return true;
        }
        
        /// <summary>
        /// WaitAny: 等待并清理任意一个已终止的子进程
        /// </summary>
        public uint? WaitAny(uint parentPid)
        {
            if (!processes.ContainsKey(parentPid) || !processes[parentPid].IsActive)
            {
                throw new InvalidOperationException($"Parent process {parentPid} not found or inactive");
            }
            
            var parent = processes[parentPid];
            
            // 找到第一个已终止的子进程
            var terminatedChild = parent.Children
                .Where(childPid => processes.ContainsKey(childPid) && !processes[childPid].IsActive)
                .FirstOrDefault();
            
            if (terminatedChild == 0 && parent.Children.Count > 0)
            {
                return null; // 有子进程但都还在运行
            }
            
            if (terminatedChild == 0)
            {
                return null; // 没有子进程
            }
            
            // 清理这个子进程
            Wait(parentPid, terminatedChild);
            
            return terminatedChild;
        }
        
        /// <summary>
        /// WaitAll: 清理所有已终止的子进程
        /// </summary>
        public List<uint> WaitAll(uint parentPid)
        {
            if (!processes.ContainsKey(parentPid) || !processes[parentPid].IsActive)
            {
                throw new InvalidOperationException($"Parent process {parentPid} not found or inactive");
            }
            
            var parent = processes[parentPid];
            var cleaned = new List<uint>();
            
            // 找到所有已终止的子进程
            var terminatedChildren = parent.Children
                .Where(childPid => processes.ContainsKey(childPid) && !processes[childPid].IsActive)
                .ToList();
            
            // 清理它们
            foreach (var childPid in terminatedChildren)
            {
                Wait(parentPid, childPid);
                cleaned.Add(childPid);
            }
            
            return cleaned;
        }
        
        /// <summary>
        /// 获取子进程状态
        /// </summary>
        public List<(uint pid, string name, bool isActive)> GetChildren(uint parentPid)
        {
            if (!processes.ContainsKey(parentPid))
            {
                return new List<(uint, string, bool)>();
            }
            
            var parent = processes[parentPid];
            return parent.Children
                .Where(childPid => processes.ContainsKey(childPid))
                .Select(childPid => 
                {
                    var child = processes[childPid];
                    return (child.Pid, child.Name, child.IsActive);
                })
                .ToList();
        }
        
        /// <summary>
        /// 设置信号处理器
        /// </summary>
        public void SetSignalHandler(uint pid, SIGS signal, Action<Signal> handler)
        {
            if (processes.ContainsKey(pid))
            {
                processes[pid].SignalHandlers[signal] = handler;
            }
        }
        
        /// <summary>
        /// 发送信号
        /// </summary>
        public void SendSignal(uint pid, Signal signal)
        {
            if (!processes.ContainsKey(pid))
                return;
                
            var process = processes[pid];
            
            // 通知行为对象
            process.Behavior?.OnProcessSignal(signal);
            
            if (!process.IsActive)
                return;
            
            // 特殊处理 SIGTERM
            if (signal.Type == SIGS.SIGTERM)
            {
                Terminate(pid);
                return;
            }
            
            // 调用注册的处理器
            if (process.SignalHandlers.ContainsKey(signal.Type))
            {
                process.SignalHandlers[signal.Type]?.Invoke(signal);
            }
        }
        
        /// <summary>
        /// 获取进程信息
        /// </summary>
        public ProcessDescriptor GetProcess(uint pid)
        {
            return processes.ContainsKey(pid) ? processes[pid] : null;
        }
        
        /// <summary>
        /// 获取所有活动进程
        /// </summary>
        public IEnumerable<ProcessDescriptor> GetActiveProcesses()
        {
            return processes.Values.Where(p => p.IsActive);
        }
        
        /// <summary>
        /// 获取所有进程（包括僵尸进程）
        /// </summary>
        public IEnumerable<ProcessDescriptor> GetAllProcesses()
        {
            return processes.Values;
        }
        
        /// <summary>
        /// 更新所有绑定了行为的进程
        /// </summary>
        public void UpdateProcesses()
        {
            foreach (var process in processes.Values)
            {
                if (process.IsActive && process.Behavior != null)
                {
                    process.Behavior.OnProcessUpdate();
                }
            }
        }
        
        /// <summary>
        /// 获取进程的自定义数据
        /// </summary>
        public T GetProcessData<T>(uint pid, string key, T defaultValue = default)
        {
            if (processes.ContainsKey(pid) && processes[pid].Data.ContainsKey(key))
            {
                return (T)processes[pid].Data[key];
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 设置进程的自定义数据
        /// </summary>
        public void SetProcessData(uint pid, string key, object value)
        {
            if (processes.ContainsKey(pid))
            {
                processes[pid].Data[key] = value;
            }
        }
        
        /// <summary>
        /// 获取 PID 池状态（调试用）
        /// </summary>
        public (uint nextPid, int recycledCount) GetPidPoolStatus()
        {
            return (nextPid, recycledPids.Count);
        }
        
        /// <summary>
        /// 生成进程树字符串
        /// </summary>
        public string GetProcessTree()
        {
            var sb = new StringBuilder();
            BuildProcessTree(0, "", sb, true);
            
            // 添加 PID 池信息
            sb.AppendLine($"\nPID Pool Status: Next={nextPid}, Recycled={recycledPids.Count}");
            if (recycledPids.Count > 0)
            {
                sb.AppendLine($"Recycled PIDs: [{string.Join(", ", recycledPids)}]");
            }
            
            return sb.ToString();
        }
        
        private void BuildProcessTree(uint pid, string prefix, StringBuilder sb, bool isLast)
        {
            if (!processes.ContainsKey(pid))
                return;
                
            var process = processes[pid];
            
            // 绘制当前进程
            sb.Append(prefix);
            if (pid != 0) // 不是 root
            {
                sb.Append(isLast ? "└── " : "├── ");
            }
            
            string status = process.IsActive ? "" : " <zombie>";
            sb.AppendLine($"{process.Name} (PID: {pid}){status}");
            
            // 递归绘制子进程
            var children = process.Children.Where(childPid => processes.ContainsKey(childPid)).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                bool isLastChild = (i == children.Count - 1);
                string newPrefix = pid == 0 ? "" : prefix + (isLast ? "    " : "│   ");
                BuildProcessTree(children[i], newPrefix, sb, isLastChild);
            }
        }
    }
}
