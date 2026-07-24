using UnityEngine;
using TMPro;
using DG.Tweening;
using static PlayerEnums;

public class StatInfoUI : MonoBehaviour
{
    public PlayerStatType statType { get; private set; }
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI statValueText;

    [Header("Settings")]
    [SerializeField] private Color highlightColor = Color.yellow; 
    [SerializeField] private Color originColor;
    private Tween _activeTween;

    public void Init(string statName, string statValue, PlayerStatType statType)
    {
        statNameText.text = statName;
        statValueText.text = statValue;
        this.statType = statType;
    }

    public void RefreshUI(string statValue)
    {
        if (statValueText.text == statValue) return;

        statValueText.text = statValue;

        _activeTween?.Kill();
        statValueText.transform.localScale = Vector3.one;

        statValueText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f)
            .SetUpdate(true); // 일시정지 중에도 작동하게 설정

        statValueText.color = highlightColor;
        _activeTween = statValueText.DOColor(originColor, 0.5f)
            .SetEase(Ease.InQuad)
            .SetUpdate(true);
    }
}