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

    public static int Calculate2x2SquareNum(int mazeDifficulty)
    {
        int width = CalculateMazeSideSize(mazeDifficulty);
        return Mathf.FloorToInt((width * width) / 50);
    }

    public static int Calculate3x3SquareNum(int mazeDifficulty)
    {
        int width = CalculateMazeSideSize(mazeDifficulty);
        return Mathf.FloorToInt((width * width) / 200);
    }

    public static int CalculateObjectiveLeastDistance(int mazeDifficulty)
    {
        return (int)Mathf.Floor(MazeUTL.GetMazeShortestSide() / 2);
    }

    public static int CalculateHealthPackNum(int mazeDifficulty)
    {
        int width = CalculateMazeSideSize(mazeDifficulty);
        return Mathf.FloorToInt((width * width) / 30);
    }

    public static int CalculateHealthPackLeastDistance(int mazeDifficulty)
    {
        return 3;
    }

    public static int CalculateBodyPartChestNum(int mazeDifficulty)
    {
        int numItems = Mathf.CeilToInt(CalculateMazeSideSize(mazeDifficulty) / 10.0f);
        return numItems;
    }

    public static int CalculateBodyPartChestLeastDistance(int mazeDifficulty)
    {
        return 3;
    }
}