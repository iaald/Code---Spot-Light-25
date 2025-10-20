using System.Collections;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMng : MonoSingleton<SceneMng>
{
    private Coroutine currentSwitching;

    [Header("Fade Configs")]
    [SerializeField] private float fadeInDuration = 0.25f;   // 0->1
    [SerializeField] private float fadeOutDuration = 0.25f;  // 1->0
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Load Configs")]
    [SerializeField] private float minCoveredDuration = 0.4f; // 遮罩完全盖上后至少覆盖多久（可选）

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SwitchSceneImmediately("Installer Scene");
    }

    public bool SwitchScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        if (SceneManager.GetActiveScene().name == sceneName) return false;
        if (currentSwitching != null) return false;

        currentSwitching = StartCoroutine(IESwitchScene(sceneName));
        return true;
    }

    public void SwitchSceneImmediately(string sceneName)
    {
        if (currentSwitching != null) { StopCoroutine(currentSwitching); currentSwitching = null; }
        if (string.IsNullOrEmpty(sceneName)) return;
        if (SceneManager.GetActiveScene().name == sceneName) return;

        SceneManager.LoadScene(sceneName);
        SetCoverAlpha(0f); // 立即切换不需要遮罩
    }

    IEnumerator IESwitchScene(string sceneName)
    {
        FakeLoadingCover.Instance.SetVisible(true);
        // 1) 进场：把遮罩从 0->1 盖上
        // （若当前已是 1，就会在 Fade01 内快速归一）
        yield return Fade01(GetCoverAlpha(), 1f, fadeInDuration);

        // 可选：保证遮罩盖上后至少显示一小段时间，避免“瞬开瞬关”的违和
        float coveredStart = Time.realtimeSinceStartup;

        // 2) 异步加载：在遮罩完全盖上的状态（a=1）下进行
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (op == null)
        {
            Debug.LogError($"[SceneMng] LoadSceneAsync null: {sceneName}");
            goto FadeOutAndFinish;
        }

        op.allowSceneActivation = false;

        // 等待引擎把场景资源加载到 0.9（待激活）
        while (op.progress < 0.9f)
        {
            // 遮罩保持 1，不露底
            SetCoverAlpha(1f);
            yield return null;
        }

        // 至少覆盖到 minCoveredDuration
        float elapsedCovered = Time.realtimeSinceStartup - coveredStart;
        if (elapsedCovered < minCoveredDuration)
            yield return new WaitForSeconds(minCoveredDuration - elapsedCovered);

        // 允许激活：切换到新场景
        op.allowSceneActivation = true;

        // 等到真正完成
        yield return new WaitUntil(() => op.isDone);

        // 保守：确保新场景 Active
        var newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid() && newScene.isLoaded)
            SceneManager.SetActiveScene(newScene);

        // 等到新场景真正渲染一帧（防止闪烁/黑场）
        yield return new WaitForEndOfFrame();
        yield return null; // 再等一帧更稳

    // 3) 出场：把遮罩从 1->0 拉开
    FadeOutAndFinish:
        yield return Fade01(GetCoverAlpha(), 0f, fadeOutDuration);

        currentSwitching = null;
        FakeLoadingCover.Instance.SetVisible(false);
    }

    /// <summary>
    /// 通用 0-1 插值的淡入淡出（用于控制遮罩透明度）
    /// </summary>
    private IEnumerator Fade01(float from, float to, float duration)
    {
        duration = Mathf.Max(0.0001f, duration);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float eased = Mathf.Lerp(from, to, fadeCurve != null ? fadeCurve.Evaluate(k) : k);
            SetCoverAlpha(eased);
            yield return null;
        }
        SetCoverAlpha(to);
    }

    /// <summary>
    /// 写入遮罩透明度：使用 FakeLoadingCover.Instance.Progress
    /// </summary>
    private void SetCoverAlpha(float value01)
    {
        value01 = Mathf.Clamp01(value01);
        if (FakeLoadingCover.Instance != null)
        {
            FakeLoadingCover.Instance.Progress = value01; // a = value01
        }
    }

    /// <summary>
    /// （可选）读回当前遮罩透明度；若没有实例则按 0 处理
    /// </summary>
    private float GetCoverAlpha()
    {
        return FakeLoadingCover.Instance != null ? Mathf.Clamp01(FakeLoadingCover.Instance.Progress) : 0f;
    }
}
