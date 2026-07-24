using System;
using System.Diagnostics;
using static ItemEnums;

public class ItemUseSystem
{
    private static readonly ConsumeItemHandler consumeHandler = new();

    public static void Use(InventoryManager.ItemInstance item, PlayerContext ctx)
    {
        if (item == null) return;

        if (item.itemDataSO.itemType == ItemType.Consumable)
        {
            if (!consumeHandler.CanUse(item, ctx)) return;

            consumeHandler.Use(item, ctx);
            ctx.PlayerInventoryManager.Consume(item);
        }
    }
}
