using UnityEngine;
using TMPro;
using System;

[Serializable]
public class TextShaker : MonoBehaviour
{
    public float amplitude = 5f;   // 晃动幅度
    public float frequency = 2f;   // 晃动速度
    public bool randomize = true;  // 是否随机相位（避免同步晃动）

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
        float y = Mathf.Cos((Time.time + randomOffset * 0.5f) * frequency) * amplitude * 0.5f; // 稍微不同步
        rectTransform.anchoredPosition = originalPos + new Vector3(x, y,0);
    }
}