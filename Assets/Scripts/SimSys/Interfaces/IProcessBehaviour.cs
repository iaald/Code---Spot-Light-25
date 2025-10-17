namespace SimSys
{
    /// <summary>
    /// 进程行为接口，MonoBehaviour 可以实现此接口来绑定到进程
    /// </summary>
    public interface IProcessBehavior
    {
        /// <summary>
        /// 进程启动时调用
        /// </summary>
        void OnProcessStart(uint pid);
        
        /// <summary>
        /// 收到信号时调用
        /// </summary>
        void OnProcessSignal(Signal signal);
        
        /// <summary>
        /// 进程终止时调用
        /// </summary>
        void OnProcessTerminate();
        
        /// <summary>
        /// 每帧更新（可选实现）
        /// </summary>
        void OnProcessUpdate();
    }
}
