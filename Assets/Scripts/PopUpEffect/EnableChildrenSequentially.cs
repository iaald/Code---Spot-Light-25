using UnityEngine;
using System.Collections;

public class EnableChildrenSequentially : MonoBehaviour
{
    public float delay = 0.5f; // 每个子物体之间的间隔时间

    public void Startpop()
    {
        StartCoroutine(EnableChildren());
    }

    IEnumerator EnableChildren()
    {
        // 遍历所有子物体
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
            yield return new WaitForSeconds(delay);
        }
    }
}