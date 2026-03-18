using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatLevelData
{
    public PlayerEnums.PlayerStatType statType;
    public int level;
}

[CreateAssetMenu(menuName = "Stats/Player Stat Upgrade State")]
public class CurrentPlayerStateLevelSO : ScriptableObject
{
    public List<StatLevelData> levels;
    public int GetCurrentLevel(PlayerEnums.PlayerStatType type)
    {
        return levels.Find(l => l.statType == type)?.level ?? 0;
    }

    public void SetLevel(PlayerEnums.PlayerStatType type, int level)
    {
        var data = levels.Find(l => l.statType == type);
        if (data != null) data.level = level;
    }    
}


