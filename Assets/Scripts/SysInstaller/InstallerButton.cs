using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InstallerButton : MonoBehaviour
{
    public TextMeshProUGUI TMP_L;
    public TextMeshProUGUI TMP_R;
    public Image image;
    public Color Color
    {
        get { return this.image.color; }
        set { this.image.color = value; }
    }

    void Start()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(this.ShowNextOp);
    }

    public string[] ops;
    public int ptr_ops = 0;
    public void SetOption(InstallerSettingData.Option option)
    {
        this.TMP_L.text = option.question;
        this.ops = option.options;
        TMP_R.text = ops.Length == 0 ? "" : ops[0];
    }
    public void ShowNextOp()
    {
        if (ops.Length == 0 || ops.Length == 1) return;
        ptr_ops += 1;
        if (ptr_ops >= ops.Length)
        {
            ptr_ops = 0;
        }
        TMP_R.text = ops[ptr_ops];
    }
}
