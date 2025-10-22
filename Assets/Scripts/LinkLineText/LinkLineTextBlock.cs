using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LinkLineTextBlock : MonoBehaviour
{
    public string Text;
    public int Seq;
    public TextMeshProUGUI textMeshProUGUI;
    public Image image;
    void Start()
    {
        textMeshProUGUI.text = Text;
    }
}
