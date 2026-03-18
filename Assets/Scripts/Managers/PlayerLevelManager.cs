using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerLevelManager : MonoBehaviour
{
    public static PlayerLevelManager Instance;
    private SkillManager _skillManager;
    [SerializeField] private SkillDataListSO skillDataList;
    [SerializeField] private GameObject skillSelectionUI;
    [SerializeField] private SkillCard[] skillCards;

    private PlayerProgress _progress;

    [SerializeField]
    private int[] requiredExpTable = new int[]
    {
        0, 40, 80, 130, 200, 300, 420, 560, 720, 900,
        1150, 1450, 1800, 2200, 2650, 3150, 3700, 4300, 4950, 5650,
        6450, 7350, 8350, 9450, 10650, 12000, 13450, 15000, 16650, 18400,
        20300, 22350, 24550, 26900, 29400, 32050, 34850, 37800, 40900, 44150
    };

    private void Awake()
    {
        Instance = this;
    }

    public bool TryLevelUp(PlayerProgress progress)
    {
        if(_progress == null)
        {
            _progress = progress;
        }

        int level = progress.CurrentLevel;

        if (level >= requiredExpTable.Length)
            return false;

        int needExp = requiredExpTable[level];

        if (progress.CurrentExp >= needExp)
        {
            progress.LevelUp(progress.CurrentExp - needExp);
            return true;
        }

        return false;
    }

    public void ShowLevelUpSKillSelection()
    {
        UIManager.Instance.SetPause(true);

        List<SkillDataSO> availableSkills = new List<SkillDataSO>();

        foreach (var skill in skillDataList.skillDataLists)
        {
            int currentLevel = _skillManager.GetSkillLevel(skill);

            if (currentLevel >= skill.levelData.Count)
                continue;

            availableSkills.Add(skill);
        }

        availableSkills = availableSkills.OrderBy(x => Random.value).ToList();

        int displayCount = Mathf.Min(skillCards.Length, availableSkills.Count);

        for (int i = 0; i < displayCount; i++)
        {
            SkillDataSO skillData = availableSkills[i];

            skillCards[i].Init(
                skillData,
                _skillManager.GetSkillLevel(skillData),
                _skillManager,
                this
            );

            skillCards[i].gameObject.SetActive(true);
        }

        for (int i = displayCount; i < skillCards.Length; i++)
            skillCards[i].gameObject.SetActive(false);

        skillSelectionUI.SetActive(true);


    }

    public void HideLevelUpSkillSelection()
    {
        skillSelectionUI.SetActive(false);
        UIManager.Instance.SetPause(false);

        CheckExtraLevelUp();
    }

    private void CheckExtraLevelUp()
    {
        if (_progress == null) return;

        if (TryLevelUp(_progress))
        {
            // 경험치가 남아서 레벨업이 또 됐다면 UI를 다시 띄움
            ShowLevelUpSKillSelection();
        }
    }
    public void SetSkillManager(SkillManager skillManager)
    {
        _skillManager = skillManager;

    }

    [ContextMenu("LevelUPTest")]
    public void LevelUPTest()
    {
        ShowLevelUpSKillSelection();
    }

    public int GetRequiredExpToNextLevel(int currentLevel)
    {
        if (requiredExpTable == null || requiredExpTable.Length == 0)
            return 0;

        if (currentLevel >= requiredExpTable.Length)
            return requiredExpTable[requiredExpTable.Length-1];

        return requiredExpTable[currentLevel];
    }
}
