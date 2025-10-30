using System.Collections;
using System.Collections.Generic;
using TMPro;
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Narration;
using Kuchinashi.Utils.Progressable;

[RequireComponent(typeof(RectTransform))]
public class WindowPetDialogueBox : MonoBehaviour
{
    public CanvasGroupAlphaProgressable CanvasGroup;
    public TextMeshProUGUI textMeshProUGUI;
    public float textSpeed = 0.05f; // 每个字符出现的间隔时间
    public Transform OptionsParent;
    [SerializeField] private string currentContent;
    private RectTransform rectTransform;
    private Coroutine typingCoroutine;

    public bool IsTypingComplete => typingCoroutine == null;

    private void Awake()
    {
        CanvasGroup = GetComponent<CanvasGroupAlphaProgressable>();

        TypeEventSystem.Global.Register<OnStoryEndEvent>(e =>
        {
            HideNarration();
            ClearOptions();
        }).UnRegisterWhenGameObjectDestroyed(gameObject);

        HideNarration(0f);
        ClearOptions();
    }

    // 调用此函数开始打印文字
    public void ShowText(string fullText)
    {
        currentContent = fullText;
        // 如果上一次的打印还在进行，先停止
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(fullText));
    }

    // 打印协程
    private IEnumerator TypeText(string textToType)
    {
        textMeshProUGUI.text = "";  // 清空当前文本

        foreach (char c in textToType)
        {
            textMeshProUGUI.text += c;   // 逐字添加
            textMeshProUGUI.rectTransform.sizeDelta = new Vector2(textMeshProUGUI.rectTransform.sizeDelta.x, textMeshProUGUI.preferredHeight);
            
            yield return new WaitForSeconds(textSpeed);
        }

        typingCoroutine = null; // 打印完成
    }

    // 直接显示完整文字
    public void ShowFullTextInstant(string fullText)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        textMeshProUGUI.text = fullText;
    }

    // 跳过打印过程
    public void SkipTyping()
    {
        if (currentContent.IsNullOrEmpty()) return;
        if (typingCoroutine != null)
        {
            ShowFullTextInstant(currentContent);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            return;
        }
    }

    public void ShowNarration(float time = 0.5f)
    {
        CanvasGroup.LinearTransition(time);
    }

    public void HideNarration(float time = 0.5f)
    {
        CanvasGroup.InverseLinearTransition(time);
    }

    public class Option
    {
        public int index;
        public string caption;
        public GameObject buttonObj;
        public Option(int index, string caption = "")
        {
            this.index = index;
            this.caption = caption;
        }
        override public string ToString()
        {
            return "" + index;
        }
    }

    public GameObject OptionButtonTemplate;
    List<Option> optsButtons = new();
    
    // pass options=null to clear all option buttons
    public void SetOptions(List<Option> options)
    {
        ClearOptions();

        if (options == null) return;
        foreach (var option in options)
        {
            var temp = Instantiate(OptionButtonTemplate, OptionsParent);
            TextMeshProUGUI TMP = temp.GetComponentInChildren<TextMeshProUGUI>();
            Button button = temp.GetComponentInChildren<Button>();
            TMP.text = option.caption;
            button.onClick.AddListener(() =>
            {
                // 在这里注册一个事件发送
                TypeEventSystem.Global.Send(new SelectOptionEvent() { index = option.index });
                ClearOptions();
            });
            option.buttonObj = temp;
            optsButtons.Add(option);
        }
    }

    public void ClearOptions()
    {
        if (optsButtons.Count != 0)
        {
            foreach (var item in optsButtons)
            {
                if (item.buttonObj != null)
                {
                    Destroy(item.buttonObj);
                }
            }
            optsButtons.Clear();
        }
    }

    // Example:
    // ShowText("Some test text"); // show text (printer)
    // yield return new WaitForSeconds(2);

    // Set options (switch to a new option group)
    // List<Option> options = new() { new(1, "A"), new(2, "B"), new(3, "C") };
    // SetOptions(options);

    // Invoke(nameof(tsan), 5f);
}
