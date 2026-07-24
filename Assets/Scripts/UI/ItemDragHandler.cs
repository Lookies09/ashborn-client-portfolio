using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public int sourceSlotIndex;
    [HideInInspector] public ItemSlot sourceSlot;
    // --- 내부 변수 ---

    private Transform originalParent; // 드래그 시작 시 원래 부모 (ItemSlot)
    private Canvas canvas; 
    private RectTransform rectTransform; 
    private CanvasGroup canvasGroup;
    private ScrollRect scrollRect;
    private bool dropSuccessful = false; // 드롭이 성공했는지 추적

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 최상위 캔버스 찾기
        canvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    private void OnDisable()
    {
        InventoryEvents.OnItemDragStateChanged?.Invoke(false);
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        InventoryEvents.OnItemDragStateChanged?.Invoke(true);

        dropSuccessful = false;

        originalParent = rectTransform.parent;

        // 부모 슬롯에서 ItemSlot 컴포넌트를 찾아, 인덱스를 가져와서 저장합니다.
        ItemSlot foundSlot = originalParent.GetComponentInParent<ItemSlot>();
        if (foundSlot != null) 
        {
            sourceSlot = foundSlot; 
            sourceSlotIndex = foundSlot.slotIndex;

            scrollRect = originalParent.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.enabled = false;
            }
        }
        else
        {
            sourceSlot = null; 
            sourceSlotIndex = -1;
        }

        rectTransform.SetParent(canvas.transform);
        rectTransform.SetAsLastSibling(); // 다른 UI 위에 표시

        // 드래그 중에는 레이캐스트를 차단
        canvasGroup.blocksRaycasts = false;

        // 드래그 중인 오브젝트를 반투명하게
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void ReturnToSource()
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InventoryEvents.OnItemDragStateChanged?.Invoke(false);

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;
        
        if (!dropSuccessful)
        {
            rectTransform.SetParent(originalParent);
            rectTransform.localPosition = Vector3.zero;
        }
        else
        {
            GameObject hitGO = eventData.pointerCurrentRaycast.gameObject;
            if (hitGO != null)
            {
                ItemSlot targetSlot = hitGO.GetComponentInParent<ItemSlot>();
                if (targetSlot is QuickSlot)
                {
                    // QuickSlot이면 UI는 원래 부모로 돌아가게
                    rectTransform.SetParent(originalParent);
                    rectTransform.localPosition = Vector3.zero;
                }
            }
        }

        if (scrollRect != null)
        {
            scrollRect.enabled = true;
        }
    }

    
    /// <summary>
    /// ItemSlot.OnDrop에서 호출하여 드롭이 성공했음을 알립니다.
    /// </summary>
    public void NotifyDropSuccessful()
    {
        dropSuccessful = true;
    }
}
