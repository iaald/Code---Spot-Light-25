using UnityEngine;

namespace SimSys
{
    /// <summary>
    /// 窗口行为示例 - 每个窗口对应一个进程
    /// </summary>
    public class WindowBehavior : ProcessBehaviorBase
    {
        [SerializeField] private string windowTitle = "Untitled Window";
        
        private bool isMinimized = false;
        private float updateTimer = 0f;
        
        protected override void Start()
        {
            base.Start();
            
            // 从窗口管理器 fork 进程
            ForkAndBind(SysRoot.WINDOW_MANAGER_PID, $"window-{windowTitle}");
            
            // 设置信号处理器
            pm.SetSignalHandler(myPid, SIGS.SIGTERM, (signal) =>
            {
                Debug.Log($"[Window:{windowTitle}] Received SIGTERM, closing window...");
            });
            
            // 设置 SIGCHLD 处理器（如果这个窗口会创建子进程）
            pm.SetSignalHandler(myPid, SIGS.SIGCHLD, (signal) =>
            {
                Debug.Log($"[Window:{windowTitle}] Child process {signal.SenderPid} terminated");
                pm.WaitAll(myPid);
            });
            
            // 存储窗口特定数据
            pm.SetProcessData(myPid, "windowTitle", windowTitle);
            pm.SetProcessData(myPid, "isMinimized", false);
        }
        
        public override void OnProcessStart(uint pid)
        {
            base.OnProcessStart(pid);
            Debug.Log($"[Window:{windowTitle}] Window opened with PID {pid}");
        }
        
        public override void OnProcessUpdate()
        {
            // 每帧更新逻辑
            updateTimer += Time.deltaTime;
            if (updateTimer >= 5f)
            {
                updateTimer = 0f;
                Debug.Log($"[Window:{windowTitle}] Still running (PID: {myPid})");
            }
        }
        
        public override void OnProcessSignal(Signal signal)
        {
            base.OnProcessSignal(signal);
            
            // 可以根据信号类型做不同处理
            switch (signal.Type)
            {
                case SIGS.SIGTERM:
                    // SIGTERM 会自动触发 OnProcessTerminate
                    break;
                    
                case SIGS.SIGCHLD:
                    // 子进程相关处理
                    break;
            }
        }
        
        public override void OnProcessTerminate()
        {
            Debug.Log($"[Window:{windowTitle}] Window is closing...");
            
            // 播放关闭动画等
            // ...
            
            // 延迟销毁 GameObject
            Destroy(gameObject, 0.5f);
        }
        
        // 窗口特定方法
        public void MinimizeWindow()
        {
            isMinimized = true;
            pm.SetProcessData(myPid, "isMinimized", true);
            Debug.Log($"[Window:{windowTitle}] Minimized");
            
            // 隐藏窗口 UI
            // ...
        }
        
        public void MaximizeWindow()
        {
            isMinimized = false;
            pm.SetProcessData(myPid, "isMinimized", false);
            Debug.Log($"[Window:{windowTitle}] Maximized");
            
            // 显示窗口 UI
            // ...
        }
        
        public void CloseWindow()
        {
            // 发送 SIGTERM 给自己（会触发 OnProcessTerminate）
            pm.SendSignal(myPid, new Signal(SIGS.SIGTERM, myPid));
        }
        
        // Unity UI 按钮回调示例
        public void OnCloseButtonClick()
        {
            CloseWindow();
        }
        
        public void OnMinimizeButtonClick()
        {
            if (isMinimized)
                MaximizeWindow();
            else
                MinimizeWindow();
        }
    }
}
