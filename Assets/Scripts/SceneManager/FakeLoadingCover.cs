using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FakeLoadingCover : MonoSingleton<FakeLoadingCover>
{
    public float Progress
    {
        set { _progress = value; }
        get
        {
            return animationCurve.Evaluate(_progress);
        }
    }
    private float _progress;
    public TextMeshProUGUI textMeshProUGUI;
    public Image image;
    public AnimationCurve animationCurve;
    void Start()
    {
        gameObject.SetActive(false);
    }
    void Update()
    {
        textMeshProUGUI.text = Progress.ToString();
        Color c = image.color;
        c.a = Mathf.Clamp01(Progress);
        image.color = c;
    }
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
