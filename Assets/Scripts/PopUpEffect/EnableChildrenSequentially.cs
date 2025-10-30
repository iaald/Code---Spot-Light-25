using UnityEngine;
using System.Collections;

public class EnableChildrenSequentially : MonoBehaviour
{
    public float delay = 0.5f; // ÿ��������֮��ļ��ʱ��
    static readonly System.Random random = new();
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
            AudioMng.Instance.PlaySound($"alert{random.Next(0, 3)}");
            yield return new WaitForSeconds(delay);
        }
    }
}