using DataSystem;
using TMPro;
using UnityEngine;

public class Namemaker : MonoBehaviour
{
    public TextMeshProUGUI hint;
    public LinkLinePuzzle llp;

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
                hint.text = "真是奢侈的名字啊，就不能取得再短一点吗？";
                llp.ResetAll();
            }
            if(t2.Length==2)
            {
                GameProgressData.SetUsername(t2);
            }
        }

    }
}
