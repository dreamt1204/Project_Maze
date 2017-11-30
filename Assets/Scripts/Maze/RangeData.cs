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
	public bool crossOnly = false;
    public bool outOfRange = false;
	public bool blockedByWall = false;
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

        // Check if target meets range requirement
        if (!MazeUTL.CheckTargetInRange(org, target, rangeData.range))
            return false;

		// Check if target on cross direction
		if (rangeData.crossOnly)
		{
			if (!MazeUTL.CheckTragetInCrossDir(org, target))
				return false;
		}

        // If target is org, check excludeOrigin
        if (org == target)
        {
            return !rangeData.excludeOrigin;
        }

        // Check if target is right on the range when excludeTilesBetweenRange is set
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

		// Check if target is blocked by wall
		if (rangeData.blockedByWall)
		{
			if (MazeUTL.CheckTargetBlockedByWall(org, target))
				return false;
		}

        return true;
    }
}

