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
        return (10 + (mazeDifficulty - 1) * 5);
    }

    public static int Calculate2x2SquareNum(int mazeWidth)
    {
        return Mathf.FloorToInt((mazeWidth * mazeWidth) / 50);
    }

    public static int Calculate3x3SquareNum(int mazeWidth)
    {
        return Mathf.FloorToInt((mazeWidth * mazeWidth) / 200);
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