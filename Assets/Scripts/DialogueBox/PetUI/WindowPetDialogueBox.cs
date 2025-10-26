using System.Collections;
using TMPro;
using UnityEngine;

public class WindowPetDialogueBox : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;
    public float textSpeed = 0.05f; // 每个字符出现的间隔时间

    private Coroutine typingCoroutine;

    // 调用此函数开始打印文字
    public void ShowText(string fullText)
    {
        // 如果上一次的打印还在进行，先停止
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(fullText));
    }

    private IEnumerator TypeText(string textToType)
    {
        textMeshProUGUI.text = "";  // 清空当前文本

        foreach (char c in textToType)
        {
            textMeshProUGUI.text += c;   // 逐字添加
            yield return new WaitForSeconds(textSpeed);
        }

        typingCoroutine = null; // 打印完成
    }

    // 可选：跳过动画，立即显示完整文字
    public void ShowFullTextInstant(string fullText)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        textMeshProUGUI.text = fullText;
    }
#if UNITY_EDITOR
    [SerializeField] [TextArea] private string stringTotest;
    [ContextMenu("TestPrint")]
    public void TestPrint()
    {
        ShowText(stringTotest);
    }
#endif
}
