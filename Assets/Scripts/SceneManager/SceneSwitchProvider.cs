using UnityEngine;

public class SceneSwitchProvider : MonoBehaviour
{
    public string sceneName;
    public void SwitchScene()
    {
        SceneMng.Instance.SwitchScene(sceneName);
    }
    public void SwitchSceneImmediately()
    {
        SceneMng.Instance.SwitchSceneImmediately(sceneName);
    }
}
