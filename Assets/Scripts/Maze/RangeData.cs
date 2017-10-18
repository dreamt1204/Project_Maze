//============================== Class Definition ==============================
// 
// Range Data is used for getting range tiles from org with data restriction.
//
// How to use?
//  1) Create new RangeData and set its properties.
//      ex: RangeData rangeData = new RangeData();
//          rangeData.range = 3;
//
//  2) Use CheckRangeDataCondition() with the RangeData you created to get desired tiles.
//      ex: foreach (Tile t in tileList)
//          {
//              if (RangeData.CheckRangeDataCondition(org, t, rangeData))
//                    newList.Add(t);
//          }
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RangeData
{
    //=======================================
    //      Variables
    //=======================================
    public int range = 0;
    public bool outOfRange = false;
    public bool excludeOrigin = false;
    public bool excludeTilesBetweenRange = false;
    public List<Tile> targetTiles;

    //=======================================
    //      Functions
    //=======================================
    public static bool CheckRangeDataCondition(Tile org, Tile target, RangeData rangeData)
    {
        if (target == null)
            return false;

        // Check target if target meets range requirement
        if (!MazeUTL.CheckTargetInRange(org, target, rangeData.range))
            return false;

        // If target is org, check excludeOrigin
        if (org == target)
        {
            return !rangeData.excludeOrigin;
        }

        // Check uf target is right on the range when excludeTilesBetweenRange is set
        if (rangeData.excludeTilesBetweenRange)
        {
            if (!MazeUTL.CheckTargetIsRightOnRange(org, target, rangeData.range))
                return false;
        }

        // Check target is in targetTiles
        if ((rangeData.targetTiles != null) && (rangeData.targetTiles.Count > 0))
        {
            if (!rangeData.targetTiles.Contains(target))
                return false;
        }

        return true;
    }
}

