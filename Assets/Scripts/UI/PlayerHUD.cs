using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 플레이어 HUD UI 요소(HP/EXP 바)를 업데이트하는 단순 표시 컴포넌트.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("HP UI")]
    [SerializeField] private Image hpBar;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("MP UI")]
    [SerializeField] private Image mpBar;

    [Header("EXP UI")]
    [SerializeField] private Material expBarMatirial;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Gold UI")]
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("Interaction UI")]
    [SerializeField] private UIButtonCoolDown interactButton;

    [SerializeField] private UIButtonCoolDown skillActiveButton;

    [SerializeField] private UIButtonCoolDown inventoryButton;

    public UIButtonCoolDown SkillActiveBtn => skillActiveButton;

    public void UpdateHP(int current, int max)
    {
        if (hpBar != null)
            hpBar.fillAmount = max > 0 ? (float)current / max : 0f;

        if (hpText != null)
            hpText.text = $"{current}/{max}";
    }

    public void UpdateMP(int current, int max)
    {
        if (mpBar != null)
            mpBar.fillAmount = max > 0 ? (float)current / max : 0f;
    }

    public void UpdateExp(int currentExp, int requiredExp, int level)
    {
        if (expBarMatirial != null)
            expBarMatirial.SetFloat("_FillAmount", requiredExp > 0 ? (float)currentExp / requiredExp : 0f); 

        if (levelText != null)
            levelText.text = $"Lv. {level}";
    }

    public void UpdateGold(int currentGold)
    {
        goldText.text = currentGold.ToString("N0") + "G";
    }

    public void SetInteractButtonVisible(bool visible)
    {
        if (interactButton != null)
            interactButton.gameObject.SetActive(visible);
    }

    public void SetInteractButtonHandler(UnityAction onClick)
    {
        if (interactButton == null) return;

        Button btn = interactButton.Button;
        btn.onClick.RemoveAllListeners();

        btn.onClick.AddListener(() => {
            interactButton.OnButtonClicked(onClick);
        });
    }

    public void SetSkillActiveButtonHandler(UnityAction onClick)
    {
        if (skillActiveButton == null) return;

        Button btn = skillActiveButton.Button;
        btn.onClick.RemoveAllListeners();

        btn.onClick.AddListener(() => {
            skillActiveButton.OnButtonClicked(onClick);
        });
    }

    public void SetInventoryOpenButtonHandler(UnityAction onClick)
    {
        if (inventoryButton == null) return;

        Button btn = inventoryButton.Button;
        btn.onClick.RemoveAllListeners();

        btn.onClick.AddListener(() => {
            inventoryButton.OnButtonClicked(onClick);
            SoundManager.Instance.PlayInventoryOpenSound();
        });
    }

   

}
