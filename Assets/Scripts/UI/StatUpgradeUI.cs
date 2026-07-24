using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using static StatValueFormatter;

public class StatUpgradeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI changeValueText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Button decreseBtn;
    [SerializeField] private Button increaseBtn;
    [SerializeField] private GameObject[] points;

    public event Action<StatUpgradeUI> OnValueChanged;


    private int _currentLevel;
    private int _pendingLevel;
    private int _quantity = 0;
    private StatUpgradeConfig _config;
    private float _baseStat;
    private float _currentStat;

    public int Cost { private set; get; }
    public float ValueIncrease { private set; get; }
    public int LevelAfterUpgrade => _currentLevel + _quantity;
    public PlayerEnums.PlayerStatType StatType => _config.statType;
    private void NotifyChanged()
    {
        OnValueChanged?.Invoke(this);
    }


    public void Init(StatUpgradeConfig config, int currentLevel, float baseStat)
    {
        _currentLevel = currentLevel;
        _config = config;
        _baseStat = baseStat;
        _currentStat = baseStat + config.GetTotalValueIncreaseByLevels(0, currentLevel);
        typeText.text = config.StatName;

        ResetQuantity();

        foreach (GameObject v in points)
        {
            if (!v.activeSelf) continue;

            v.SetActive(false);
        }

        for (int i = 0; i < currentLevel; i++)
        {
            points[i].SetActive(true);
        }
    }

    public void Refresh(int currentLevel)
    {
        _currentLevel = currentLevel;
        _currentStat = _baseStat + _config.GetTotalValueIncreaseByLevels(0, currentLevel);
        typeText.text = _config.StatName;

        ResetQuantity();

        foreach (var v in points)
        {
            v.SetActive(false);
        }

        for (int i = 0; i < currentLevel; i++)
        {
            points[i].SetActive(true);
        }
    }

    public void ResetQuantity()
    {
        _quantity = 0;
        UpdateText(quantityText, "0");
        ChangeCost();
        ChangeStatValue();
    }

    public void OnClickIncrease()
    {
        ChangeQuantity(1);
        ChangeCost();
        ChangeStatValue();
        NotifyChanged();
    }

    public void OnClickDecrease()
    {
        ChangeQuantity(-1);
        ChangeCost();
        ChangeStatValue();
        NotifyChanged();
    }

    public void ChangeCost()
    {
        Cost = _config.GetTotalCostByLevels(_currentLevel, _quantity);
        UpdateText(costText, Cost.ToString("N0") );
    }

    public void ChangeStatValue()
    {
        ValueIncrease = _config.GetTotalValueIncreaseByLevels(_currentLevel, _quantity);
        if (_quantity == 0)
        {
            UpdateText(changeValueText, FormatStatValue(_config.statType ,_currentStat));
        }
        else
        {
            float finalValue = _currentStat + ValueIncrease;
            UpdateText(
                changeValueText,
                $"{FormatStatValue(_config.statType, _currentStat)} -> {FormatStatValue(_config.statType, finalValue)}"
            );
        }            
    }


    public void ChangeQuantity(int delta)
    {
        int maxAddable = _config.maxLevel - _currentLevel;
        _quantity = Mathf.Clamp(_quantity + delta, 0, maxAddable);
        UpdateText(quantityText, _quantity.ToString());
    }

    private void UpdateText(TextMeshProUGUI text, string value )
    {
        if (text.text == null) return;

        text.text = value;
    }


}
