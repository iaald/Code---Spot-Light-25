using UnityEngine;
using TMPro;
using System.Collections;

public class TextBlinkSwitch : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public string textA = "HELLO";
    public string textB = "WORLD";
    public float fadeDuration = 1f;    // ���뵭��ʱ��
    public float stayDuration = 1f;    // ����ͣ��ʱ��

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
            //������ǰ����
            yield return FadeTo(0f, fadeDuration);

            //�л���������
            showingA = !showingA;
            textMeshPro.text = showingA ? textA : textB;

            //����������
            yield return FadeTo(1f, fadeDuration);

            //ͣ��
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
