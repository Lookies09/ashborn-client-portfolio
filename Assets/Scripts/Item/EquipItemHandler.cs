using System.Diagnostics;
using static InventoryManager;
using static ItemEnums;

public class EquipItemHandler
{
    public void OnEquip(ItemInstance item, PlayerContext ctx)
    {
        switch (item.itemDataSO.equipmentApplyType)
        {
            case EquipmentApplyType.Stat:
                ApplyStats(item, ctx);
                break;

            case EquipmentApplyType.Buff:
                ApplyBuff(item, ctx);
                break;

            case EquipmentApplyType.StatAndBuff:
                ApplyStats(item, ctx);
                ApplyBuff(item, ctx);
                break;

            case EquipmentApplyType.Script:
                ExecuteCustom(item, ctx);
                break;
        }
    }

    public void OnUnequip(ItemInstance item, PlayerContext ctx)
    {
        switch (item.itemDataSO.equipmentApplyType)
        {
            case EquipmentApplyType.Stat:
                RemoveStats(item, ctx);
                break;

            case EquipmentApplyType.Buff:
                RemoveBuffs(item, ctx);
                break;

            case EquipmentApplyType.StatAndBuff:
                RemoveStats(item, ctx);
                RemoveBuffs(item, ctx);
                break;

            case EquipmentApplyType.Script:
                ExecuteCustomUnequip(item, ctx);
                break;
        }

    }

    public void ApplyStats(ItemInstance item, PlayerContext ctx)
    {
        
        foreach (var mod in item.itemDataSO.baseModifiers)
        {
            ctx.RuntimeStats.AddModifier(mod, item.instanceId);            
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
    public void RemoveStats(ItemInstance item, PlayerContext ctx)
    {
        ctx.RuntimeStats.RemoveModifiersBySource(item.instanceId);
    }
    public void RemoveBuffs(ItemInstance item, PlayerContext ctx)
    {

    }
    public void ExecuteCustomUnequip(ItemInstance item, PlayerContext ctx)
    {

    }
}
