using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickItemBtn : MonoBehaviour
{     
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemCount;

    private UIButtonCoolDown _cooldown;
    private int _index;
    private QuickSlotManager _quickSlotManager;
    private InventoryManager.ItemInstance _currentItem;

    public void Init(QuickSlotManager quickSlotManager, int index)
    {
        Refresh(null);
        this._quickSlotManager = quickSlotManager;
        this._index = index;
        _cooldown = GetComponent<UIButtonCoolDown>();

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(HandleClick);

        Refresh(null);
    }

    private void HandleClick()
    {
        if (_currentItem == null || _currentItem.quantity <= 0)
        {
            return;
        }

        // 2. 아이템이 있을 때만 쿨타임 컴포넌트 실행
        _cooldown.OnButtonClicked(() =>
        {
            _quickSlotManager.UseQuickSlot(_index);
        });
    }

    public void Refresh(InventoryManager.ItemInstance instance)
    {
        _currentItem = instance;

        if (instance == null || instance.quantity <= 0)
        {
            itemIcon.enabled = false;
            itemCount.gameObject.SetActive(false);
            return;
        }

        itemIcon.sprite = instance.itemDataSO.itemIcon;
        itemIcon.enabled = true;

        if (instance.quantity > 1)
        {
            itemCount.text = instance.quantity.ToString();
            itemCount.gameObject.SetActive(true);
        }
        else
        {
            itemCount.gameObject.SetActive(false);
        }
    }


}
