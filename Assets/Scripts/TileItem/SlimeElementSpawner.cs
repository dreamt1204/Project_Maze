using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeElementSpawner : TileItemSpawner
{
    //=======================================
    //      Variables
    //=======================================
    public List<TileItem> SlimeElements;

    //=======================================
    //      Functions
    //=======================================
    public override TileItem GetRandomTileItem()
    {
        return SlimeElements[Random.Range(0, SlimeElements.Count)];
    }
}
