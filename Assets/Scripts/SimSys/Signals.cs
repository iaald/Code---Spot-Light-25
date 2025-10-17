namespace SimSys
{
    public enum SIGS
    {
        SIGCHLD,  // 子进程状态改变
        SIGTERM,  // 终止信号
    }
    
    public class Signal
    {
        public SIGS Type { get; set; }
        public uint SenderPid { get; set; }
        public object Data { get; set; }
        
        public Signal(SIGS type, uint senderPid, object data = null)
        {
            Type = type;
            SenderPid = senderPid;
            Data = data;
        }
    }
}
