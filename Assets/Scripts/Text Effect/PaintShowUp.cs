using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.UI;

public class PaintShowUp : MonoBehaviour
{
    public Image image;
    public float fadetime=1.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        image= GetComponent<Image>();
        image.DOFade(1f, fadetime);
    }
}
