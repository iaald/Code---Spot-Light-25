using System.Collections;
using TextVFX;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnreadableMasker : UnreadableToLatin, IUnreadableMasker
{
    private static readonly WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    
    private string MaskedText = "";
    private string ContentText = "";
    
    // 使用TMP的SetText方法避免字符串分配
    private char[] _filteredCharsBuffer;
    private const char FULL_WIDTH_SPACE = '　';
    
    private TextMeshProUGUI Unreadable;
    private TextMeshProUGUI Content;
    public TextMeshProUGUI Filtered;
    public Canvas canvas;
    
    private int _lastVisibleIndex = -2; // 使用-2作为未初始化状态
    private bool _isDirty = false;

    private Material textMaterial;
    private RectTransform textRectTransform;
    void Start()
    {
        Unreadable = GetUnreadableTMP();
        Content = GetContentTMP();

        textMaterial = Unreadable.fontSharedMaterial;
        textRectTransform = Unreadable.GetComponent<RectTransform>();
        
        Refresh();
        UpdateFilteredText(-1); // 初始化为全空格
        SyncTMP();
    }
    
    void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            textRectTransform,
            Mouse.current.position.ReadValue(),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 mousePos
        );
        // 更新 Shader 参数
        textMaterial.SetVector("_MousePosition", new Vector4(mousePos.x, mousePos.y, 0, 0));

        if (GetPiontedIndex(IUnreadableConverter.TMP_WHERE.Content, out int srcIndex))
        {
            if (srcIndex != _lastVisibleIndex)
            {
                _lastVisibleIndex = srcIndex;
                UpdateFilteredText(srcIndex);
                _isDirty = true;
            }
        }
        else if (_lastVisibleIndex != -1)
        {
            _lastVisibleIndex = -1;
            UpdateFilteredText(-1);
            _isDirty = true;
        }
        
        if (_isDirty)
        {
            SyncTMP();
            _isDirty = false;
        }
    }

    IEnumerator Test()
    {
        int i = 0;
        while (true)
        {
            if (i != _lastVisibleIndex)
            {
                _lastVisibleIndex = i;
                UpdateFilteredText(i);
                _isDirty = true;
            }
            i = (i + 1) % ContentText.Length;
            yield return _waitForSeconds1;
        }
    }

    /// <summary>
    /// 零GC更新：直接修改字符数组，使用TMP的SetCharArray API
    /// </summary>
    void UpdateFilteredText(int visibleIndex)
    {
        // 填充全角空格
        for (int i = 0; i < _filteredCharsBuffer.Length; i++)
        {
            _filteredCharsBuffer[i] = FULL_WIDTH_SPACE;
        }

        // 显示指定索引的字符
        if (visibleIndex >= 0 && visibleIndex < ContentText.Length)
        {
            _filteredCharsBuffer[visibleIndex] = ContentText[visibleIndex];
        }
    }

    void SyncTMP()
    {
        if (Unreadable != null)
        {
            // 使用SetCharArray避免字符串分配（零GC）
            Filtered.SetCharArray(_filteredCharsBuffer);
            // Unreadable只在初始化时设置一次
            // 如果需要更新，也使用SetText(char[])
        }
    }

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        GenerateFullLengthMask();
        MaskedText = GetTextUnreadable();
        ContentText = GetTextContent();
        
        // 初始化字符缓冲区（只分配一次）
        _filteredCharsBuffer = new char[ContentText.Length];
        
        // 设置遮罩文本（初始化时允许GC）
        if (Unreadable != null)
        {
            Unreadable.text = MaskedText;
        }
        
        _lastVisibleIndex = -2;
    }
}
