using QFramework;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InstallerButton : MonoBehaviour
{
    public TextMeshProUGUI TMP_L;
    public TextMeshProUGUI TMP_R;
    public TMP_InputField inputField;
    private InstallerController ctrl;
    public Image image;
    public Color Color
    {
        get { return this.image.color; }
        set { this.image.color = value; }
    }
    private void Awake()
    {
        ctrl = transform.parent.GetComponent<InstallerController>();
    }
    void Start()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(this.ShowNextOp);
    }

    [HideInInspector] public string ques = "";
    [HideInInspector] public string[] ops;
    [HideInInspector] public int ptr_ops = 0;
    [HideInInspector] public bool isInput = false;
    public void SetOption(InstallerSettingData.Option option)
    {
        this.ques = option.question;
        this.ops = option.options;
        this.isInput = option.UseInput;
        this.TMP_L.text = ctrl.ResolveOptionString(ques);
        TMP_R.text = ops.Length == 0 || isInput ? "" : ctrl.ResolveOptionString(ops[ptr_ops]);
        this.inputField.gameObject.SetActive(isInput);
    }

    public void Refresh()
    {
        this.TMP_L.text = ctrl.ResolveOptionString(ques);
        TMP_R.text = ops.Length == 0 || isInput ? "" : ctrl.ResolveOptionString(ops[ptr_ops]);
    }
    public void ShowNextOp()
    {
        if (ops.Length == 0 || ops.Length == 1) return;
        ptr_ops += 1;
        if (ptr_ops >= ops.Length)
        {
            ptr_ops = 0;
        }
        ctrl.RefreshAllOptionText();
    }
    public void CallRefreshAll()
    {
        ctrl.RefreshAllOptionText();
    }
    public string GetR()
    {
        return this.isInput ? inputField.text : TMP_R.text;
    }
}
