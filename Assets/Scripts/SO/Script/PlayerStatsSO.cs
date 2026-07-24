using System.IO;
using UnityEngine;
using static PlayerEnums;

[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Stats/PlayerStats")]
public class PlayerStatsSO : ScriptableObject
{
    public int maxHP = 100;
    public int attack = 10;
    public float moveSpeed = 5f;
    public float critRate = 0.1f;
    public float critDamage = 0.5f;
    public float defense = 5;
    public float luck = 0.05f;
    public float cooldownReduction = 0f;
    public float regenPerSecond = 0f;
    public int maxMp = 100;


    public float GetStatValue(PlayerStatType type)
    {
        return type switch
        {
            PlayerStatType.MaxHP => maxHP,
            PlayerStatType.Attack => attack,
            PlayerStatType.MoveSpeed => moveSpeed,
            PlayerStatType.CritRate => critRate,
            PlayerStatType.CritDamage => critDamage,
            PlayerStatType.Defense => defense,
            PlayerStatType.Luck => luck,
            PlayerStatType.CooldownReduction => cooldownReduction,
            PlayerStatType.Regen => regenPerSecond,
            PlayerStatType.MaxMp => maxMp,
            _ => 0f
        };
    }


}
