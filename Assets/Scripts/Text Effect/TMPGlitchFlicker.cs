using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TMPGlitchFlicker : MonoBehaviour
{
    public TMP_Text tmpText;

    [Header("��˸����")]
    [Range(0f, 1f)] public float flickerIntensity = 0.5f; // ��˸ǿ�ȣ�͸���ȱ仯���ȣ�
    public float flickerSpeed = 8f;                       // ��˸�ٶ�
    [Range(0f, 1f)] public float flickerProbability = 0.2f; // ÿ�ַ�������˸�ĸ���

    [Header("ɫƫ����")]
    [Range(0f, 1f)] public float chromaShiftIntensity = 0.3f; // ɫƫǿ��
    public float chromaShiftSpeed = 3f;                        // ɫƫ�ٶ�

    [Header("��ɫ")]
    public Color baseColor = Color.white;
    public Color flickerColor = new Color(1f, 0.9f, 0.9f); // ��˸ʱ�Դ���ƫ

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

            // ����͸����
            float noise = Mathf.PerlinNoise(i * 2.17f, Time.time * flickerSpeed);
            float alphaMod = 1f - Mathf.Clamp01(noise * flickerIntensity);

            // ������ʸ�������˸
            if (Random.value < flickerProbability * Time.deltaTime * 10f)
                alphaMod = Random.Range(0.2f, 0.8f);

            // ɫƫ����΢R/G/B����
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

        // Ӧ����ɫ�޸�
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
            tmpText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}
