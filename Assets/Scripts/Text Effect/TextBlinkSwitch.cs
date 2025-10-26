using UnityEngine;
using TMPro;
using System.Collections;

public class TextBlinkSwitch : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public string textA = "HELLO";
    public string textB = "WORLD";
    public float fadeDuration = 1f;    // 淡入淡出时长
    public float stayDuration = 1f;    // 保持停留时长

    private bool showingA = true;
    private Coroutine loopRoutine;

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshProUGUI>();

        textMeshPro.text = textA;
        textMeshPro.alpha = 1f;

        loopRoutine = StartCoroutine(SwitchLoop());
    }

    IEnumerator SwitchLoop()
    {
        while (true)
        {
            //淡出当前文字
            yield return FadeTo(0f, fadeDuration);

            //切换文字内容
            showingA = !showingA;
            textMeshPro.text = showingA ? textA : textB;

            //淡入新文字
            yield return FadeTo(1f, fadeDuration);

            //停留
            yield return new WaitForSeconds(stayDuration);
        }
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = textMeshPro.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            textMeshPro.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        textMeshPro.alpha = targetAlpha;
    }

    void OnDisable()
    {
        if (loopRoutine != null)
            StopCoroutine(loopRoutine);
    }
}
