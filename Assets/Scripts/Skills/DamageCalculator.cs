using static PlayerEnums;
using UnityEngine;

public static class DamageCalculator
{
    public static float Calculate(
        float baseDamage,
        PlayerRuntimeStats stats
    )
    {
        float dmg = baseDamage * stats.GetStat(PlayerStatType.Attack);

        float critRate = stats.GetStat(PlayerStatType.CritRate);
        float critDamage = stats.GetStat(PlayerStatType.CritDamage);

        if (Random.value < critRate)
            dmg *= critDamage;

        return dmg;
    }
}
