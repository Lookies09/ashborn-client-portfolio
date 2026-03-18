using static InventoryManager;
using static ItemEnums;
using static PlayerEnums;

public class ConsumeItemHandler : IItemUseHandler
{
    public bool CanUse(ItemInstance item, PlayerContext ctx)
    {
        return item.quantity > 0;
    }

    public void Use(ItemInstance item, PlayerContext ctx)
    {
        switch (item.itemDataSO.useType)
        {
            case ItemUseType.Instant:
                ApplyInstant(item, ctx);
                break;

            case ItemUseType.Buff:
                ApplyBuff(item, ctx);
                break;

            case ItemUseType.Cast:
                BeginCast(item, ctx);
                break;

            case ItemUseType.Script:
                ExecuteCustom(item, ctx);
                break;
        }
    }

    public void ApplyInstant(ItemInstance item, PlayerContext ctx)
    {
        var data = item.itemDataSO;


        SoundManager.Instance.PlaySFX(data.consumableUseSound);

        foreach (var modifier in data.consumableEffects)
        {
            switch (modifier.effectType)
            {
                case ConsumableEffectType.RecoverHp:
                    ctx.Health.Heal((int)modifier.value);
                    break;
                case ConsumableEffectType.RecoverMp:
                    ctx.SkillManager.AddMana((int)modifier.value);
                    break;
                case ConsumableEffectType.ExpGain:
                    ctx.PlayerProgress.AddExp((int)modifier.value);
                    break;
            }
        }
    }
    public void ApplyBuff(ItemInstance item, PlayerContext ctx)
    {

    }
    public void BeginCast(ItemInstance item, PlayerContext ctx)
    {

    }
    public void ExecuteCustom(ItemInstance item, PlayerContext ctx)
    {

    }
}
