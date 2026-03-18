using UnityEngine;

[CreateAssetMenu(fileName = "SkillDataList", menuName = "Skills/SkillDataList")]
public class SkillDataListSO : ScriptableObject
{
    public SkillDataSO[] skillDataLists;
}
