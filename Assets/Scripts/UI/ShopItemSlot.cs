using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 상점 UI에서 하나의 상품 카드를 표현하는 슬롯.
/// </summary>
public class ShopItemSlot : MonoBehaviour, IItemInfoProvider
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI stockText;

    [Header("Formatting")]
    [SerializeField] private string currencySuffix = "G";
    [SerializeField] private string infiniteStockText = "∞";

    private ShopController _shopController;
    private ShopController.ShopItemEntry _entry;

    private void Awake()
    {
        GetComponent<Button>()?.onClick.AddListener(OnClickedMe);
    }


    /// <summary>
    /// 상점에서 슬롯을 재사용할 때 호출.
    /// </summary>
    public void Bind(ShopController controller, ShopController.ShopItemEntry entry, int index)
    {
        _shopController = controller;
        _entry = entry;
        UpdateView();
    }

    public void Refresh()
    {
        UpdateView();
    }

    public ItemDataSO GetItemData()
    {
        return _entry?.itemData;
    }

    private void UpdateView()
    {
        if (_entry == null)
        {
            SetTexts("-", "-", "-");
            if (iconImage != null) iconImage.enabled = false;
            return;
        }

        ItemDataSO data = _entry.itemData;
        if (iconImage != null)
        {
            if (data != null && data.itemIcon != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = data.itemIcon;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        if (nameText != null)
        {
            nameText.text = data != null ? data.itemNameKR : _entry.displayName;
        }

        if (priceText != null)
        {
            if (data != null)
            {
                priceText.text = $"{data.purchasePrice}{currencySuffix}";
            }
            else
            {
                Debug.Log("데이터가 존재하지 않음");
            }
            
        }

        if (stockText != null)
        {
            if (_entry.HasInfiniteStock)
            {
                stockText.text = infiniteStockText;
            }
            else
            {
                stockText.text = _entry.CurrentStock.ToString();
            }
        }

        bool canBuy = _entry.HasEnoughStock(1) && _shopController != null;
    }

    private void SetTexts(string name, string price, string stock)
    {
        if (nameText != null) nameText.text = name;
        if (priceText != null) priceText.text = price;
        if (stockText != null) stockText.text = stock;
    }


    public void OnClickedMe()
    {
        if (_shopController != null && _entry != null)
        {
            _shopController.OnShopItemSlotClicked(_entry);
        }
    }

}

