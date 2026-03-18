using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static InventoryManager;

/// <summary>
/// ItemSlot의 자식 오브젝트로 존재하며, 실제 아이템 정보를 표시하고 드래그 앤 드롭을 처리합니다.
/// ItemDragHandler가 이 컴포넌트와 같은 GameObject에 있어야 합니다.
/// </summary>
public class ItemUI : BaseItemUI, IPointerClickHandler
{
    private Coroutine _clickCoroutine;

    public override void Awake()
    {
        base.Awake();
        _parentSlot = GetComponentInParent<ItemSlot>();
    }


    public override void Refresh(InventoryManager.ItemInstance slotData)
    {
        //EnsureParentSlot();
        RestoreCanvasGroup();

        if (slotData == null || slotData.itemDataSO == null)
        {
            Clear();
            return;
        }

        SetItem(slotData);
    }


    public void SetItem(InventoryManager.ItemInstance slotData)
    {
        if (slotData == null || slotData.itemDataSO == null)
            return;

        //EnsureParentSlot();

        // 아이콘
        ItemIcon.sprite = slotData.itemDataSO.itemIcon;
        ItemIcon.enabled = true;

        // 레어리티
        RarityImage.color = GetRarityColor(slotData.itemDataSO.rarity);
        RarityImage.enabled = true;

        // 수량
        if (slotData.quantity > 1)
        {
            QuantityText.text = slotData.quantity.ToString();
            QuantityText.gameObject.SetActive(true);
        }
        else
        {
            QuantityText.gameObject.SetActive(false);
            QuantityText.text = string.Empty;
        }

        // 이름
        NameText.text = slotData.itemDataSO.itemNameKR;
        NameText.gameObject.SetActive(true);

        // 드래그 활성
        if (_dragHandler != null)
        {
            _dragHandler.enabled = true;
            _dragHandler.sourceSlot = _parentSlot;
        }
    }
    public override void Clear()
    {
        ItemIcon.sprite = null;
        ItemIcon.enabled = false;
        RarityImage.enabled = false;
        QuantityText.gameObject.SetActive(false);
        NameText.gameObject.SetActive(false);

        if (_dragHandler != null)
        {
            _dragHandler.enabled = false;
            _dragHandler.sourceSlot = null;
        }

    }

    private void RestoreCanvasGroup()
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }

    private void EnsureParentSlot()
    {
        if (_parentSlot == null)
            _parentSlot = GetComponentInParent<ItemSlot>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging) return;

        // 이미 대기 중인 클릭 코루틴이 있다면 (즉, 두 번째 클릭이 들어왔다면)
        if (_clickCoroutine != null)
        {
            StopCoroutine(_clickCoroutine);
            _clickCoroutine = null;
            HandleDoubleClick(); // 더블 클릭 실행
        }
        else
        {
            // 첫 번째 클릭 시 코루틴 시작
            _clickCoroutine = StartCoroutine(ClickTimerRoutine());
        }
    }

    private IEnumerator ClickTimerRoutine()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        _clickCoroutine = null;
        HandleSingleClick();
    }

    private void HandleSingleClick()
    {
        ItemInstance item = GetItemInstance();
        if (item != null && item.itemDataSO != null)
        {
            UIManager.Instance.ShowItemDetailModal(item.itemDataSO);
        }
    }

    public void HandleDoubleClick()
    {
        if (_parentSlot == null || _parentSlot.ownerInventory == null)
            return;

        InventoryManager targetInventory =
            UIManager.Instance.InventoryWindowController.GetOtherInventory(_parentSlot.ownerInventory);

        if (targetInventory == null)
            return;

        _parentSlot.ownerInventory.TransferToFirstEmpty(
            targetInventory,
            _parentSlot.slotIndex
        );
    }

    public ItemInstance GetItemInstance()
    {
        if (_parentSlot == null || _parentSlot.currentSlotData == null)
        {
            return null;
        }

        return _parentSlot.currentSlotData;
    }
}

