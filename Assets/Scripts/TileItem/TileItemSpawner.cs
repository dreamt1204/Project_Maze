using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileItemSpawner : MonoBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    public ItemType itemType;
    public List<TileItem> itemList;

    //=======================================
    //      Functions
    //=======================================
    public TileItem GetTileItem()
    {
        TileItem tileItem;

        tileItem = GetRandomTileItem();

        return tileItem;
    }

    TileItem GetRandomTileItem()
    {
        return itemList[Random.Range(0, itemList.Count)];
    }
}
