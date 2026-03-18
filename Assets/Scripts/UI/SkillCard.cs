using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillCard : MonoBehaviour
{
    [SerializeField] private Image skillIconImage;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillCurrentLevelText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private TextMeshProUGUI skillLevelDescriptionText;

    public void Init(SkillDataSO skillData, int currentLevel, SkillManager skillManager, PlayerLevelManager levelManager)
    {
        skillIconImage.sprite = skillData.skillIcon;
        skillNameText.text = skillData.skillNameKR;
        skillCurrentLevelText.text = "Lv. " + (currentLevel + 1);
        skillDescriptionText.text = skillData.skillDescription;
        skillLevelDescriptionText.text = skillData.GetFormattedDescription(currentLevel+1);

        EventTrigger trigger = GetComponent<EventTrigger>();
        trigger.triggers.Clear();
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };

        entry.callback.AddListener((eventData) =>
        {
            if (skillManager == null || levelManager == null)
            {
                Debug.LogWarning("[SkillCard] SkillManager 또는 PlayerLevelManager가 설정되지 않았습니다.");
                return;
            }

            skillManager.AddSkill(skillData);
            levelManager.HideLevelUpSkillSelection();
        });

        trigger.triggers.Add(entry);

    }

}
