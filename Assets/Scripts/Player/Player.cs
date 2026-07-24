using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerHealth health;
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private PlayerRuntimeStats runtimeStats;
    [SerializeField] private PlayerEquipment equipment;
    [SerializeField] private PlayerBuffController buffs;
    [SerializeField] private InventoryManager playerInventoryManager;
    [SerializeField] private PlayerProgress playerProgress;

    private PlayerContext _cachedContext = null;

    private void Awake()
    {
        _cachedContext = new PlayerContext
        {
            RuntimeStats = runtimeStats,
            Equipment = equipment,
            Health = health,
            AnimationController = animationController,
            Buffs = buffs,
            PlayerInventoryManager = playerInventoryManager,
            SkillManager = skillManager,
            PlayerProgress = playerProgress
        };
    }

    private void OnEnable()
    {
        health.OnRevived += animationController.ResetAnimation;
    }

    private void OnDisable()
    {
        health.OnRevived -= animationController.ResetAnimation;
    }

    public PlayerContext GetContext()
    {
        return _cachedContext ??= new PlayerContext
        {
            RuntimeStats = runtimeStats,
            Equipment = equipment,
            Health = health,
            AnimationController = animationController,
            Buffs = buffs,
            PlayerInventoryManager = playerInventoryManager,
            SkillManager = skillManager,
            PlayerProgress = playerProgress
        };
    }

}
