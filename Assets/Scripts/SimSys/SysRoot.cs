using UnityEngine;
using UnityEngine.InputSystem;

namespace SimSys
{
    public class SysRoot : MonoBehaviour
    {
        public static SysRoot Instance { get; private set; }
        public ProcessManager ProcessManager { get; private set; }

        // 窗口管理器的 PID
        public const uint WINDOW_MANAGER_PID = 1;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化进程管理器
            ProcessManager = new ProcessManager();

            // 创建窗口管理器进程
            InitializeWindowManager();

            Debug.Log("SimSys initialized");
        }

        void Start()
        {
            SimFileSystem simFileSystem = new();
            var a = simFileSystem.GetDataObject<DirectorySave>("_哪儿来的");
            a.Load();
        }

        private void InitializeWindowManager()
        {
            // 从 init (PID 0) fork 窗口管理器
            var wmPid = ProcessManager.Fork(0, "window-manager");

            if (wmPid != WINDOW_MANAGER_PID)
            {
                Debug.LogError($"Window manager PID mismatch! Expected {WINDOW_MANAGER_PID}, got {wmPid}");
            }

            ProcessManager.SetSignalHandler(0, SIGS.SIGCHLD, (signal) =>
            {
                Debug.Log($"[init] Child {signal.SenderPid} terminated, cleaning up...");
                ProcessManager.WaitAll(0);
            });

            // 设置窗口管理器的 SIGCHLD 处理器
            ProcessManager.SetSignalHandler(wmPid, SIGS.SIGCHLD, (signal) =>
            {
                Debug.Log($"[WindowManager] Child {signal.SenderPid} terminated, cleaning up...");
                ProcessManager.WaitAll(wmPid);
            });


            Debug.Log($"Window Manager initialized at PID {wmPid}");
        }

        [ContextMenu("pstree()")]
        public void PrintProcessTree()
        {
            Debug.Log("=== Process Tree ===\n" + ProcessManager.GetProcessTree());
        }

        void OnApplicationQuit()
        {
            Debug.Log("Shutting down SimSys...");
        }

        public uint ToKillPid;

        [ContextMenu("Kill")]
        public void Kill()
        {
            ProcessManager.Terminate(ToKillPid);
        }
    }
}
