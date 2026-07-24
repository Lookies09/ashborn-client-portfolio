using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataList", menuName = "SO/Item/Item Data List")]
public class ItemDataListSO : ScriptableObject
{
    public List<ItemDataSO> items = new List<ItemDataSO>();

    public ItemDataSO GetItemById(int id)
    {
        return items.Find(item => item != null && item.itemId == id);
    }
}

