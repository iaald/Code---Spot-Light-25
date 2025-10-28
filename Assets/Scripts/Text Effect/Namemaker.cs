using DataSystem;
using TMPro;
using UnityEngine;
public class Namemaker : MonoBehaviour
{
    public TextMeshProUGUI hint;
    public LinkLinePuzzle llp;
    public string username;
    void Start()
    {
        llp = GetComponent<LinkLinePuzzle>(); 
    }

    public void CheckName()
    {
        var t = llp.GetAllResults();
        foreach (var t2 in t)
        {
            
            Debug.Log(t2.Length);
            if (t2.Length > 2)
            {
                string lastTwo = t2.Substring(t2.Length - 2);
                string message = $"真是奢侈的名字啊，要不还是叫{lastTwo}吧？";
                hint.text = message;
                username = lastTwo;
            }
            if(t2.Length==2)
            {
                username = t2;
                string message = $"你确定要叫“{t2}”吗？";
                hint.text = message;
            }
        }

    }
    public void Setname()
    {
        GameProgressData.SetUsername(username);
    }
    public void Resetmessage()
    {
        hint.text = "可我叫什么？";
    }

}
