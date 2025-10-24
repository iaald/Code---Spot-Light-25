using UnityEngine;

public class ColorHelper
{
    public static Color ColorRotateGeneration(Color pack, float angle, float satuOfst = 0)
    {
        float h, s, v;
        Color.RGBToHSV(pack, out h, out s, out v);

        float newH = (h + angle / 360f) % 1f;
        if (newH < 0) newH += 1f;

        // 根据色相微调亮度以补偿感知差异
        float brightnessCompensation = GetBrightnessCompensation(newH);
        float newV = Mathf.Clamp01(v * brightnessCompensation);

        return Color.HSVToRGB(newH, Mathf.Clamp01(s + satuOfst), newV);
    }

    private static float GetBrightnessCompensation(float hue)
    {
        // 黄色区域降低亮度，蓝色区域提高亮度
        if (hue < 0.15f) return 1.0f;  // 红色
        else if (hue < 0.3f) return 0.9f; // 黄色 - 降低亮度
        else if (hue < 0.5f) return 1.0f; // 绿色
        else if (hue < 0.7f) return 1.1f; // 青色 - 提高亮度
        else return 1.2f; // 蓝色紫色 - 大幅提高亮度
    }
}
