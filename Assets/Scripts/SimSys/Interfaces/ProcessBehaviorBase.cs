using UnityEngine;

namespace SimSys
{
    /// <summary>
    /// 抽象基类，供其他 MonoBehaviour 继承以便绑定到进程
    /// </summary>
    public abstract class ProcessBehaviorBase : MonoBehaviour, IProcessBehavior
    {
        protected uint myPid;
        protected ProcessManager pm;
        
        public uint MyPid => myPid;
        public bool IsProcessActive => pm?.GetProcess(myPid)?.IsActive ?? false;
        
        protected virtual void Start()
        {
            pm = SysRoot.Instance.ProcessManager;
        }
        
        /// <summary>
        /// 从指定父进程 fork 并绑定自己
        /// </summary>
        protected uint ForkAndBind(uint parentPid, string processName)
        {
            myPid = pm.Fork(parentPid, processName);
            pm.BindBehavior(myPid, this);
            
            Debug.Log($"[{GetType().Name}] Forked process '{processName}' with PID {myPid}");
            return myPid;
        }
        
        // 实现接口方法 - 子类可重写
        public virtual void OnProcessStart(uint pid)
        {
            Debug.Log($"[{GetType().Name}] Process {pid} started");
        }
        
        public virtual void OnProcessSignal(Signal signal)
        {
            Debug.Log($"[{GetType().Name}] Received signal {signal.Type} from PID {signal.SenderPid}");
        }
        
        public virtual void OnProcessTerminate()
        {
            Debug.Log($"[{GetType().Name}] Process {myPid} terminating");
        }
        
        public virtual void OnProcessUpdate()
        {
            // 每帧调用，子类可重写
        }
        
        protected virtual void OnDestroy()
        {
            // 如果进程还活着，终止它
            if (pm != null && pm.GetProcess(myPid)?.IsActive == true)
            {
                pm.Terminate(myPid);
            }
        }
    }
}
