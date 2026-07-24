using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatUpgradeHUDController : MonoBehaviour
{
    [SerializeField] private GameObject statCardPrefab;
    [SerializeField] private Transform statCardParent;
    [SerializeField] private PlayerStatUpgradeConfigSO playerStatUpgradeConfigSO;
    [SerializeField] private CurrentPlayerStateLevelSO currentPlayerStateLevelSO;
    [SerializeField] private PlayerStatsSO playerStatsSO;
    [SerializeField] private Button applyButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI totalCostText;
    [SerializeField] private AudioClip applySoundClip;

    private List<StatUpgradeUI> _statUpgradeUIs = new List<StatUpgradeUI>();

    private int _totalCost = 0;
    private int _displayedGold;
    private Vector2 _originalCurrencyPos;
    private bool _isPosInitialized = false;
    private void Awake()
    {
        for (int i = 0; i< playerStatUpgradeConfigSO.configs.Count; i++)
        {
            GameObject sc = Instantiate(statCardPrefab, statCardParent);
            var ui = sc.GetComponent<StatUpgradeUI>();
            ui.Init(
                playerStatUpgradeConfigSO.configs[i],
                currentPlayerStateLevelSO.GetCurrentLevel(playerStatUpgradeConfigSO.configs[i].statType),
                playerStatsSO.GetStatValue(playerStatUpgradeConfigSO.configs[i].statType)
                );
            ui.OnValueChanged += HandleCardValueChanged;
            _statUpgradeUIs.Add(ui);
        }
        //applyButton.onClick.RemoveAllListeners();
        applyButton.onClick.AddListener(OnClickApply);        
    }

    private void OnEnable()
    {
        _displayedGold = PlayerWallet.Instance.Gold;
        UpdateCurrency(false);

        foreach (StatUpgradeUI v in _statUpgradeUIs)
        {
            v.ResetQuantity();
        }

        scrollRect.verticalNormalizedPosition = 1f;

        PlayerWallet.Instance.OnGoldChanged += HandleGoldChangedExternally;

        totalCostText.text = "0";
    }

    private void OnDisable()
    {
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnGoldChanged -= HandleGoldChangedExternally;

        // 트윈 정리
        DOTween.Kill(this.GetHashCode() + "Currency");
    }


    public void OnClickApply()
    {
        int totalCost = _statUpgradeUIs
            .Sum(card => card.Cost);

        if (totalCost <= 0) return;

        bool isSuccess = PlayerWallet.Instance.SpendGold(totalCost);

        if (isSuccess)
        {
            UpdateCurrency(true);

            SoundManager.Instance.PlaySFX(applySoundClip);

            currencyText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);

            currencyText.DOColor(Color.yellow, 0.1f).OnComplete(() => {
                currencyText.DOColor(Color.white, 0.3f);
            });

            for (int i = 0; i < _statUpgradeUIs.Count; i++)
            {
                StatUpgradeUI card = _statUpgradeUIs[i];
                if (card.Cost > 0)
                {
                    PlayerEnums.PlayerStatType type = card.StatType;
                    int level = card.LevelAfterUpgrade;
                    currentPlayerStateLevelSO.SetLevel(type, level);
                    card.Refresh(level);
                }
            }
            RecalculateTotalCost();
        }
        else
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

    private void HandleCardValueChanged(StatUpgradeUI _)
    {
        RecalculateTotalCost();
    }

    private void RecalculateTotalCost()
    {
        _totalCost = _statUpgradeUIs.Sum(c => c.Cost);
        UpdateTotalCostUI();
    }
    private void HandleGoldChangedExternally()
    {
        UpdateCurrency(false);
    }

    private void UpdateTotalCostUI()
    {
        totalCostText.text = _totalCost.ToString("N0");
    }

    private void UpdateCurrency(bool animate = true)
    {
        int targetGold = PlayerWallet.Instance.Gold;
        DOTween.Kill(this.GetHashCode() + "Currency");
        if (animate)
        {
            DOTween.To(() => _displayedGold, x => {
                _displayedGold = x;
                currencyText.text = _displayedGold.ToString("N0") + "G";
            }, targetGold, 1.0f)
            .SetEase(Ease.OutQuad) // 서서히 느려지는 효과
            .SetId(this.GetHashCode() + "Currency")
            .SetUpdate(true); // 타임스케일이 0일 때도 동작하게 설정 (필요시)
        }
        else
        {
            _displayedGold = targetGold;
            currencyText.text = _displayedGold.ToString("N0") + "G";
        }
    }
}
