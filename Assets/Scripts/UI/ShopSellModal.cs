using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShopSellModal : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI TotalPriceText;
    [SerializeField] private GameObject QuantityButtonsGroup;
    [SerializeField] private Button qtyIncButton;
    [SerializeField] private Button qtyDescButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private RectTransform modalWindow;

    private ShopController _shopController;
    private InventoryManager.ItemInstance _itemInstance;
    private int _currentQty = 1;

    private void Awake()
    {

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClick);

        if (sellButton != null)
            sellButton.onClick.AddListener(OnSellButtonClick);

        if (qtyIncButton != null)
            qtyIncButton.onClick.AddListener(OnIncreaseQuantityClicked);

        if (qtyDescButton != null)
            qtyDescButton.onClick.AddListener(OnDecreaseQuantityClicked);

    }

    public void Open(ShopController controller, InventoryManager.ItemInstance item)
    {
        _shopController = controller;
        _itemInstance = item;
        _currentQty = 1; // 오픈 시 항상 1개부터 시작

        gameObject.SetActive(true);

        modalWindow.localScale = Vector3.one * 0.8f;
        modalWindow.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);

        Bind();
    }

    private void Bind()
    {
        if (_itemInstance == null) return;

        // 아이템 수량이 1개보다 많을 때만 수량 조절 버튼 표시
        QuantityButtonsGroup.SetActive(_itemInstance.quantity > 1);

        iconImage.sprite = _itemInstance.itemDataSO.itemIcon;
        nameText.text = _itemInstance.itemDataSO.itemNameKR;
        UpdateUI();
    }

    private void UpdateUI()
    {
        quantityText.text = _currentQty.ToString();
        priceText.text = _itemInstance.itemDataSO.sellPrice.ToString();
        TotalPriceText.text = (_currentQty * _itemInstance.itemDataSO.sellPrice).ToString();
    }

    private void UIClear()
    {
        iconImage.sprite = null;
        nameText.text = null;
        priceText.text = null;
        TotalPriceText.text = "0";
    }


    public void OnIncreaseQuantityClicked()
    {
        // 인벤토리에 보유한 수량까지만 증가 가능
        if (_currentQty < _itemInstance.quantity)
        {
            _currentQty++;
            UpdateUI();
        }
    }

    public void OnDecreaseQuantityClicked()
    {
        if (_currentQty > 1)
        {
            _currentQty--;
            UpdateUI();
        }
    }

    public void OnSellButtonClick()
    {
        if (_itemInstance == null) return;

        int finalPrice = _currentQty * _itemInstance.itemDataSO.sellPrice;
        _shopController.ExecuteSell(_itemInstance, _currentQty, finalPrice);

        OnCloseButtonClick();
    }


    private void OnCloseButtonClick()
    {
        modalWindow.DOScale(0.8f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
            _itemInstance = null;
            gameObject.SetActive(false);
        });
    }
}
