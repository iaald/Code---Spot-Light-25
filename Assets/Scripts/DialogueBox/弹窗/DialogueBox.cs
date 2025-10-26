using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBox : MonoBehaviour
{
    public GameObject Container;
    // private Vector4 margin; //L T R B

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        var childRoot = transform.Find("root");

        Action creatRoot = () =>
        {
            var t = Instantiate(Container, this.transform);
            t.name = "root";
            childRoot = t.transform;
        };

        if (childRoot == null)
        {
            creatRoot();
        }
        else
        {
            DestroyImmediate(childRoot.gameObject);
            creatRoot();
        }
    }
}

public class DialogueBoxData
{
    public string title;
    public string iconPath;
    public string content;
    public string[] buttons;
}