using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;


public class LobbyUIManager : MonoBehaviour
{
    [Header("UI Element")]
    [SerializeField] private Button dungeonEnterButton;
    [SerializeField] private Button statUpgradeButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button closeUpgradeButton;

    [SerializeField] private TextMeshProUGUI currencyText;
    private int _displayedGold;
    private PlayerWallet _playerWallet;

    private Vector2 _originalCurrencyPos;
    private bool _isPosInitialized = false;

    private SettingManager _settingManager;
    private SoundManager _soundManager;

    private void Awake()
    {
        settingButton.onClick.RemoveAllListeners();
        statUpgradeButton.onClick.RemoveAllListeners();
        dungeonEnterButton.onClick.RemoveAllListeners();        
        settingButton.onClick.RemoveAllListeners();
        
    }

    private void Start()
    {    

        if (SettingManager.Instance != null)
        {
            _settingManager = SettingManager.Instance;
        }

        if (SoundManager.Instance != null)
        {
            _soundManager = SoundManager.Instance;
        }

        if (_playerWallet == null)
        {
            _playerWallet = PlayerWallet.Instance;
        }

        settingButton.onClick.AddListener(() => { _settingManager.SetSettinPanel(true); });
        settingButton.onClick.AddListener(_soundManager.PlaySettingOpenSound);

        dungeonEnterButton.onClick.AddListener(() => { GameManager.Instance.GoToGame(); });
        dungeonEnterButton.onClick.AddListener(_soundManager.EnterDungeonSound);

        statUpgradeButton.onClick.AddListener(_soundManager.PlayUpgradeUIOpenSound);
        closeUpgradeButton.onClick.AddListener(_soundManager.PlayUpgradeUICloseSound);

    }

    private void OnEnable()
    {
        if (_playerWallet == null)
        {
            _playerWallet = PlayerWallet.Instance != null ? PlayerWallet.Instance : FindFirstObjectByType<PlayerWallet>();
        }

        if (_playerWallet != null)
        {
            _playerWallet.OnGoldChanged -= RefreshInventoryGoldUI;
            _playerWallet.OnGoldChanged += RefreshInventoryGoldUI;
            _displayedGold = _playerWallet.Gold;
        }

        UpdateCurrency(false);
    }

    private void OnDisable()
    {
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.OnGoldChanged -= RefreshInventoryGoldUI;
        }
    }


    private void RefreshInventoryGoldUI()
    {
        UpdateCurrency(true);
    }

    private void UpdateCurrency(bool animate = true)
    {
        int targetGold = _playerWallet.Gold;
        DOTween.Kill(this.GetHashCode() + "Currency");
        if (animate)
        {
            DOTween.To(() => _displayedGold, x => {
                _displayedGold = x;
                currencyText.text = _displayedGold.ToString("N0") + "G";
            }, targetGold, 1.0f)
            .SetEase(Ease.OutQuad) 
            .SetId(this.GetHashCode() + "Currency")
            .SetUpdate(true); 
        }
        else
        {
            _displayedGold = targetGold;
            currencyText.text = _displayedGold.ToString("N0") + "G";
        }
    }

    public void NotifyGoldShortage()
    {
        if (!_isPosInitialized)
        {
            _originalCurrencyPos = currencyText.rectTransform.anchoredPosition;
            _isPosInitialized = true;
        }

        DOTween.Kill(this.GetHashCode() + "Shortage");
        currencyText.rectTransform.anchoredPosition = _originalCurrencyPos;
        currencyText.color = Color.white; // 색상도 초기화

        currencyText.rectTransform.DOShakeAnchorPos(0.5f, 10f, 20)
            .SetId(this.GetHashCode() + "Shortage");

        Sequence colorSeq = DOTween.Sequence();
        colorSeq.Append(currencyText.DOColor(Color.red, 0.1f))
                .Append(currencyText.DOColor(Color.white, 0.1f))
                .Append(currencyText.DOColor(Color.red, 0.1f))
                .Append(currencyText.DOColor(Color.white, 0.2f))
                .SetId(this.GetHashCode() + "Shortage");
    }
}
