using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileItemSpawner : MonoBehaviour
{
    //=======================================
    //      Variables
    //=======================================

    //=======================================
    //      Functions
    //=======================================
    public virtual TileItem GetRandomTileItem()
    {
        // Override me
        return null;
    }
}
