using UnityEngine;
using TMPro;
using System;

[Serializable]
public class TextShaker : MonoBehaviour
{
    public float amplitude = 5f;   // �ζ�����
    public float frequency = 2f;   // �ζ��ٶ�
    public bool randomize = true;  // �Ƿ������λ������ͬ���ζ���

    private Vector3 originalPos;
    private float randomOffset;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponentInChildren<TextMeshProUGUI>().rectTransform;
        originalPos = rectTransform.anchoredPosition;
        randomOffset = randomize ? UnityEngine.Random.Range(0f, 100f) : 0f;
    }

    void Update()
    {
        float x = Mathf.Sin((Time.time + randomOffset) * frequency) * amplitude;
        float y = Mathf.Cos((Time.time + randomOffset * 0.5f) * frequency) * amplitude * 0.5f; // ��΢��ͬ��
        rectTransform.anchoredPosition = originalPos + new Vector3(x, y,0);
    }
}