using System.Text;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 文本转换组件，支持文本偏移转换和动态字符管理
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class UnreadableConvert : MonoBehaviour
{
    #region 公共属性

    public string text
    {
        set
        {
            _text = value;
            if (m_textMeshProUGUI != null)
            {
                string displayText = DoUnreadable ? GetCovered(value) : value;
                m_textMeshProUGUI.text = displayText;
                
                if (enableDynamicCharacters)
                {
                    AddMissingCharacters(displayText);
                }
            }
        }
        get { return _text; }
    }

    #endregion

    #region 序列化字段

    [Header("基础设置")]
    public bool DoUnreadable;
    [SerializeField] private int offset;
    public TextMeshProUGUI m_textMeshProUGUI;

    [Header("动态字符管理")]
    [Tooltip("启用动态字符添加")]
    public bool enableDynamicCharacters = true;
    
    [Tooltip("启用未使用字符清理")]
    public bool enableCharacterCleanup = true;
    
    [Tooltip("字符清理间隔（秒）")]
    [SerializeField] private float cleanupInterval = 30f;
    
    [Tooltip("字符未使用多久后清理（秒）")]
    [SerializeField] private float characterLifetime = 60f;

    [Header("字体Fallback设置")]
    [Tooltip("启用字体Fallback功能")]
    public bool enableFontFallback = true;
    
    [Tooltip("Fallback字体资源列表")]
    [SerializeField] private List<TMP_FontAsset> fallbackFonts = new List<TMP_FontAsset>();

    #endregion

    #region 私有字段

    private string _text = "";
    private TMP_FontAsset primaryFont;
    private List<TMP_FontAsset> activeFallbackFonts = new List<TMP_FontAsset>();
    
    // 字符追踪
    private Dictionary<uint, float> characterUsageTime = new Dictionary<uint, float>();
    private HashSet<uint> addedCharacters = new HashSet<uint>();
    private float lastCleanupTime;

    #endregion

    #region Unity生命周期

    void Start()
    {
        InitializeComponent();
    }

    void Update()
    {
        if (enableCharacterCleanup && Time.time - lastCleanupTime >= cleanupInterval)
        {
            CleanupUnusedCharacters();
            lastCleanupTime = Time.time;
        }
    }

    void OnDestroy()
    {
        CleanupAllAddedCharacters();
        ClearAllDataStructures();
    }

    #endregion

    #region 初始化

    private void InitializeComponent()
    {
        m_textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        primaryFont = m_textMeshProUGUI.font;
        
        if (enableFontFallback)
        {
            SetupFontFallback();
        }
        
        this.text = m_textMeshProUGUI.text;
        lastCleanupTime = Time.time;
    }

    private void ClearAllDataStructures()
    {
        characterUsageTime.Clear();
        addedCharacters.Clear();
        activeFallbackFonts.Clear();
    }

    #endregion

    #region 文本转换

    /// <summary>
    /// 对文本进行偏移转换
    /// </summary>
    public string GetCovered(string tex)
    {
        if (string.IsNullOrEmpty(tex) || tex.Length < 2)
            return "";

        byte[] utf16Bytes = Encoding.Unicode.GetBytes(tex);
        
        if (offset >= utf16Bytes.Length)
        {
            Debug.LogWarning($"偏移量 {offset} 超出字节数组长度 {utf16Bytes.Length}");
            return "";
        }

        byte[] coveredByte = new byte[utf16Bytes.Length];

        for (int i = 0; i < utf16Bytes.Length - offset; i++)
        {
            coveredByte[i] = utf16Bytes[i + offset];
        }

        try
        {
            char[] chars = Encoding.Unicode.GetChars(coveredByte);
            return new string(chars).Replace("\0", "");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"字符转换错误: {e.Message}");
            return "";
        }
    }

    #endregion

    #region 字体Fallback管理

    /// <summary>
    /// 设置字体Fallback链
    /// </summary>
    private void SetupFontFallback()
    {
        if (primaryFont == null)
        {
            Debug.LogWarning("主字体为空，无法设置Fallback");
            return;
        }

        activeFallbackFonts.Clear();
        
        foreach (var fallbackFont in fallbackFonts)
        {
            if (IsValidFallbackFont(fallbackFont))
            {
                activeFallbackFonts.Add(fallbackFont);
            }
        }

        ApplyFallbackFonts();
    }

    /// <summary>
    /// 动态添加Fallback字体
    /// </summary>
    public void AddFallbackFont(TMP_FontAsset fallbackFont)
    {
        if (!IsValidFallbackFont(fallbackFont))
        {
            Debug.LogWarning("无效的Fallback字体");
            return;
        }

        if (!activeFallbackFonts.Contains(fallbackFont))
        {
            activeFallbackFonts.Add(fallbackFont);
            ApplyFallbackFonts();
            Debug.Log($"添加Fallback字体: {fallbackFont.name}");
        }
    }

    /// <summary>
    /// 移除Fallback字体
    /// </summary>
    public void RemoveFallbackFont(TMP_FontAsset fallbackFont)
    {
        if (activeFallbackFonts.Remove(fallbackFont))
        {
            ApplyFallbackFonts();
            Debug.Log($"移除Fallback字体: {fallbackFont.name}");
        }
    }

    /// <summary>
    /// 获取当前的Fallback字体列表
    /// </summary>
    public List<TMP_FontAsset> GetActiveFallbackFonts()
    {
        return new List<TMP_FontAsset>(activeFallbackFonts);
    }

    /// <summary>
    /// 验证Fallback字体是否有效
    /// </summary>
    private bool IsValidFallbackFont(TMP_FontAsset fallbackFont)
    {
        return fallbackFont != null && fallbackFont != primaryFont;
    }

    /// <summary>
    /// 应用Fallback字体到主字体
    /// </summary>
    private void ApplyFallbackFonts()
    {
        if (activeFallbackFonts.Count > 0 && primaryFont != null)
        {
            primaryFont.fallbackFontAssetTable = activeFallbackFonts;
            Debug.Log($"已设置 {activeFallbackFonts.Count} 个Fallback字体");
        }
    }

    #endregion

    #region 动态字符管理 - 主接口

    /// <summary>
    /// 添加缺失的字符到字体资源
    /// </summary>
    private void AddMissingCharacters(string displayText)
    {
        if (m_textMeshProUGUI == null || primaryFont == null)
            return;

        // 添加到主字体
        ProcessFontCharacters(primaryFont, displayText, "主字体");

        // 添加到Fallback字体
        if (enableFontFallback && activeFallbackFonts.Count > 0)
        {
            foreach (var fallbackFont in activeFallbackFonts)
            {
                if (fallbackFont != null)
                {
                    ProcessFontCharacters(fallbackFont, displayText, $"Fallback字体 {fallbackFont.name}");
                }
            }
        }
    }

    #endregion

    #region 动态字符管理 - 核心逻辑

    /// <summary>
    /// 处理字体字符添加的核心方法
    /// </summary>
    private void ProcessFontCharacters(TMP_FontAsset fontAsset, string displayText, string fontDescription)
    {
        if (!ValidateFontForDynamicCharacters(fontAsset, fontDescription))
            return;

        var missingCharacters = CollectMissingCharacters(fontAsset, displayText);

        if (missingCharacters.Count > 0)
        {
            AddCharactersToTracking(missingCharacters);
            m_textMeshProUGUI.ForceMeshUpdate();
            Debug.Log($"在{fontDescription}中添加了 {missingCharacters.Count} 个缺失字符");
        }
    }

    /// <summary>
    /// 验证字体是否支持动态字符添加
    /// </summary>
    private bool ValidateFontForDynamicCharacters(TMP_FontAsset fontAsset, string fontDescription)
    {
        if (fontAsset == null)
            return false;

        if (fontAsset.atlasPopulationMode != AtlasPopulationMode.Dynamic)
        {
            Debug.LogWarning($"{fontDescription}不是动态模式，无法自动添加字符。请在字体设置中启用Dynamic模式。");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 收集字体中缺失的字符
    /// </summary>
    private HashSet<uint> CollectMissingCharacters(TMP_FontAsset fontAsset, string displayText)
    {
        HashSet<uint> missingCharacters = new HashSet<uint>();
        float currentTime = Time.time;

        foreach (char c in displayText)
        {
            uint unicode = c;
            
            // 更新字符使用时间
            characterUsageTime[unicode] = currentTime;

            // 检查字符是否缺失
            if (!fontAsset.HasCharacter(c))
            {
                missingCharacters.Add(unicode);
            }
        }

        return missingCharacters;
    }

    /// <summary>
    /// 将字符添加到追踪系统
    /// </summary>
    private void AddCharactersToTracking(HashSet<uint> characters)
    {
        foreach (uint unicode in characters)
        {
            addedCharacters.Add(unicode);
        }
    }

    #endregion

    #region 字符清理

    /// <summary>
    /// 清理未使用的字符
    /// </summary>
    private void CleanupUnusedCharacters()
    {
        if (primaryFont == null)
            return;

        var charactersToRemove = FindExpiredCharacters();

        if (charactersToRemove.Count > 0)
        {
            RemoveCharactersFromTracking(charactersToRemove);
            ClearFontData();
            Debug.Log($"清理了 {charactersToRemove.Count} 个未使用的字符");
        }
    }

    /// <summary>
    /// 查找过期的字符
    /// </summary>
    private List<uint> FindExpiredCharacters()
    {
        List<uint> charactersToRemove = new List<uint>();
        float currentTime = Time.time;

        foreach (var kvp in characterUsageTime)
        {
            if (currentTime - kvp.Value > characterLifetime)
            {
                charactersToRemove.Add(kvp.Key);
            }
        }

        return charactersToRemove;
    }

    /// <summary>
    /// 从追踪系统移除字符
    /// </summary>
    private void RemoveCharactersFromTracking(List<uint> characters)
    {
        foreach (uint unicode in characters)
        {
            characterUsageTime.Remove(unicode);
            addedCharacters.Remove(unicode);
        }
    }

    /// <summary>
    /// 清理所有添加的字符
    /// </summary>
    private void CleanupAllAddedCharacters()
    {
        if (primaryFont == null || addedCharacters.Count == 0)
            return;

        Debug.Log($"组件销毁时清理 {addedCharacters.Count} 个字符");
        ClearFontData();
    }

    /// <summary>
    /// 清理字体数据（主字体和Fallback字体）
    /// </summary>
    private void ClearFontData()
    {
        // 清理主字体
        ClearSingleFontData(primaryFont);

        // 清理Fallback字体
        if (enableFontFallback)
        {
            foreach (var fallbackFont in activeFallbackFonts)
            {
                ClearSingleFontData(fallbackFont);
            }
        }
    }

    /// <summary>
    /// 清理单个字体的数据
    /// </summary>
    private void ClearSingleFontData(TMP_FontAsset fontAsset)
    {
        if (fontAsset != null && fontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic)
        {
            fontAsset.ClearFontAssetData(true);
        }
    }

    #endregion

    #region 调试工具

    [ContextMenu("显示当前追踪的字符数")]
    private void ShowTrackedCharacterCount()
    {
        Debug.Log($"当前追踪字符数: {characterUsageTime.Count}");
        Debug.Log($"已添加字符数: {addedCharacters.Count}");
        Debug.Log($"活跃Fallback字体数: {activeFallbackFonts.Count}");
    }

    [ContextMenu("手动清理未使用字符")]
    private void ManualCleanup()
    {
        CleanupUnusedCharacters();
    }

    [ContextMenu("清理所有字符")]
    private void ManualCleanupAll()
    {
        CleanupAllAddedCharacters();
        ClearAllDataStructures();
    }

    [ContextMenu("显示Fallback字体列表")]
    private void ShowFallbackFonts()
    {
        string fontList = "活跃Fallback字体: ";
        foreach (var font in activeFallbackFonts)
        {
            fontList += font.name + ", ";
        }
        Debug.Log(fontList);
    }

    #endregion
}
