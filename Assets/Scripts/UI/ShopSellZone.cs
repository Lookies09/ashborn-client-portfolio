using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public static class InventoryEvents
{
    public static System.Action<bool> OnItemDragStateChanged;
}


public class ShopSellZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private ShopController shopController;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform visualContent;

    [Header("설정")]
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float idleScale = 0.9f;


    private void Awake()
    {

        // 초기 상태 설정
        canvasGroup.alpha = 0f;
        if (visualContent != null) visualContent.localScale = Vector3.one * idleScale;
        SetRaycastActive(false);
    }

    private void OnEnable()
    {
        // 이벤트 구독 시작
        InventoryEvents.OnItemDragStateChanged += HandleDragState;
    }

    private void OnDisable()
    {
        // 오브젝트가 꺼질 때 구독 해제 (메모리 누수 방지)
        InventoryEvents.OnItemDragStateChanged -= HandleDragState;
    }


    public void OnDrop(PointerEventData eventData)
    {
        var itemUI = eventData.pointerDrag?.GetComponent<ItemUI>();
        if (itemUI != null)
        {
            var instance = itemUI.GetItemInstance();
            if (instance != null)
            {
                var dragHandler = eventData.pointerDrag.GetComponent<ItemDragHandler>();
                dragHandler?.ReturnToSource();

                shopController.OpenSellModal(instance);
            }
        }
    }

    public void ShowZone()
    {
        if (!gameObject.activeInHierarchy) return;

        // 드롭 이벤트를 받기 위해 이미지 레이캐스트 활성화
        SetRaycastActive(true);

        // 기존 트윈 취소
        canvasGroup.DOKill();
        visualContent.DOKill();

        // [연출] 페이드 인 + 스케일 업 (살짝 튕기는 느낌)
        canvasGroup.DOFade(1f, duration).SetUpdate(true);
        visualContent.DOScale(1f, duration).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void HideZone()
    {
        canvasGroup.DOKill();
        visualContent.DOKill();

        // [연출] 페이드 아웃 + 스케일 다운
        canvasGroup.DOFade(0f, duration).SetUpdate(true).OnComplete(() => {
            SetRaycastActive(false); // 연출이 완전히 끝난 후 레이캐스트 꺼서 뒷면 클릭 허용
        });
        visualContent.DOScale(idleScale, duration).SetUpdate(true);
    }


    private void SetRaycastActive(bool active)
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = active;
    }

    private void HandleDragState(bool isDragging)
    {
        if (isDragging) ShowZone(); 
        else HideZone();            
    }
}
