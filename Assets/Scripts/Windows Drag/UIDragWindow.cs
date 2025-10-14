using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragWindow : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("���������϶���header����")]
    public RectTransform dragArea;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;
    private bool canDrag = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // ����Ƿ����������϶���������
        if (dragArea != null)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(dragArea, eventData.position, eventData.pressEventCamera))
            {
                canDrag = false;
                return;
            }
        }

        canDrag = true;

        //UI�ö�
        rectTransform.SetAsLastSibling();

        // ������������������������λ��ƫ��
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out offset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag || canvas == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            
            rectTransform.localPosition = localPoint - offset;
        }
    }
}