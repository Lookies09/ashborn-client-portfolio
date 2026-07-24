using static InventoryManager;

public interface IItemUseHandler
{
    bool CanUse(ItemInstance item, PlayerContext ctx);
    void Use(ItemInstance item, PlayerContext ctx);
}
