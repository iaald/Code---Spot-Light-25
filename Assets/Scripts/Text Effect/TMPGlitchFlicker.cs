using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TMPGlitchFlicker : MonoBehaviour
{
    public TMP_Text tmpText;

    [Header("闪烁参数")]
    [Range(0f, 1f)] public float flickerIntensity = 0.5f; // 闪烁强度（透明度变化幅度）
    public float flickerSpeed = 8f;                       // 闪烁速度
    [Range(0f, 1f)] public float flickerProbability = 0.2f; // 每字符发生闪烁的概率

    [Header("色偏参数")]
    [Range(0f, 1f)] public float chromaShiftIntensity = 0.3f; // 色偏强度
    public float chromaShiftSpeed = 3f;                        // 色偏速度

    [Header("颜色")]
    public Color baseColor = Color.white;
    public Color flickerColor = new Color(1f, 0.9f, 0.9f); // 闪烁时稍带红偏

    private TMP_TextInfo textInfo;

    void Start()
    {
        if (tmpText == null) tmpText = GetComponentInChildren<TMP_Text>();
        tmpText.ForceMeshUpdate();
        textInfo = tmpText.textInfo;
    }

    void Update()
    {
        if (tmpText == null) return;

        tmpText.ForceMeshUpdate();
        textInfo = tmpText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int matIndex = charInfo.materialReferenceIndex;

            Color32[] colors = textInfo.meshInfo[matIndex].colors32;

            // 基础透明度
            float noise = Mathf.PerlinNoise(i * 2.17f, Time.time * flickerSpeed);
            float alphaMod = 1f - Mathf.Clamp01(noise * flickerIntensity);

            // 随机概率更明显闪烁
            if (Random.value < flickerProbability * Time.deltaTime * 10f)
                alphaMod = Random.Range(0.2f, 0.8f);

            // 色偏：轻微R/G/B波动
            float t = Mathf.Sin(Time.time * chromaShiftSpeed + i);
            float rShift = Mathf.Lerp(1f, 1f + chromaShiftIntensity, Mathf.Abs(t));
            float gShift = Mathf.Lerp(1f, 1f - chromaShiftIntensity * 0.5f, Mathf.Abs(t * 0.8f));
            float bShift = Mathf.Lerp(1f, 1f + chromaShiftIntensity * 0.7f, Mathf.Abs(t * 1.3f));

            Color flickColor = Color.Lerp(baseColor, flickerColor, noise);
            flickColor.r *= rShift;
            flickColor.g *= gShift;
            flickColor.b *= bShift;
            flickColor.a *= alphaMod;

            for (int v = 0; v < 4; v++)
                colors[vertexIndex + v] = flickColor;
        }

        // 应用颜色修改
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
            tmpText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}
