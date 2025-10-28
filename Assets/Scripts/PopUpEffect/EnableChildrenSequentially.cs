using UnityEngine;
using System.Collections;

public class EnableChildrenSequentially : MonoBehaviour
{
    public float delay = 0.5f; // ÿ��������֮��ļ��ʱ��

    public void Startpop()
    {
        StartCoroutine(EnableChildren());
    }

    IEnumerator EnableChildren()
    {
        // ��������������
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
            yield return new WaitForSeconds(delay);
        }
    }
}