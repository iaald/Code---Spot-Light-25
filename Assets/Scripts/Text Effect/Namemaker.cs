using DataSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Namemaker : MonoBehaviour
{
    public TextMeshProUGUI hint;
    public LinkLinePuzzle llp;
    public string name;
    void Start()
    {
        llp = GetComponent<LinkLinePuzzle>(); 

    }

    // Update is called once per frame
    public void CheckName()
    {
        //Debug.Log("fuuuuuuu");
        var t = llp.GetAllResults();
        foreach (var t2 in t)
        {
            
            Debug.Log(t2.Length);
            if (t2.Length > 2)
            {
                string lastTwo = t2.Substring(t2.Length - 2);
                string message = $"�����ݳ޵����ְ���Ҫ�����ǽ�{lastTwo}�ɣ�";
                hint.text = message;
                name = lastTwo;
            }
            if(t2.Length==2)
            {
                name = t2;
                string message = $"��ȷ��Ҫ�С�{t2}����";
                hint.text = message;
            }
        }

    }
    public void Setname()
    {
        GameProgressData.SetUsername(name);
    }
    public void Resetmessage()
    {
        hint.text = "���ҽ�ʲô��";
    }

}
