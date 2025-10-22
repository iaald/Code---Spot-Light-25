using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragWindow : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("挂上允许被拖动的header部分")]
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
        // 检查是否点击在允许拖动的区域中
        if (dragArea != null)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(dragArea, eventData.position, eventData.pressEventCamera))
            {
                canDrag = false;
                return;
            }
        }

        canDrag = true;

        //UI置顶
        rectTransform.SetAsLastSibling();

        // 计算鼠标点击点相对于整个面板的位置偏移
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