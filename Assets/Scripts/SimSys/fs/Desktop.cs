using UnityEngine;

[RequireComponent(typeof(ExecutableFileItem))]
public class Desktop : MonoBehaviour
{
    // 提供两个函数用于保存和加载当前桌面的状态，用[contexmenu]暴露
    public GameObject pref;
    public string test = "_IconNop0ef4a44f20bf44cdabd4684d49627de8";
    void Start()
    {
        // var go = Instantiate(pref, transform);
        // go.GetComponent<ExecutableFileItem>().FsPath = test;
    }
}
