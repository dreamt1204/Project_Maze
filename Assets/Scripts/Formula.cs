//============================== Class Definition ==============================
// 
// This class contains static functions to calculate game info based on Maze Diificulty.
// Putting all the formula functions into this class so we can modify the formula easily in the future.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Formula
{
    public static int CalculateMazeSideSize(int mazeDifficulty)
    {
        int mazeWidth = (Mathf.CeilToInt(mazeDifficulty / 5.0f) * 5) + 5;
        return mazeWidth;
    }

    public static int CalculateBodyPartChestNum(int mazeDifficulty)
    {
        int numItems = Mathf.CeilToInt(CalculateMazeSideSize(mazeDifficulty) / 10.0f);
        return numItems;
    }

    public static int CalculateObjectiveLeastDistance()
    {
        return (int)Mathf.Floor(MazeUTL.GetMazeShortestSide() / 2);
    }

    public static int CalculateBodyPartChestLeastDistance()
    {
        return (int)Mathf.Floor(MazeUTL.GetMazeShortestSide() / 2);
    }
}