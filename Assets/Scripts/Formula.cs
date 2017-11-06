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
		return 20;
    }

    public static int Calculate2x2SquareNum(int mazeDifficulty)
    {
		return 10;
    }

    public static int Calculate3x3SquareNum(int mazeDifficulty)
    {
		return 3;
    }

    public static int CalculateObjectiveLeastDistance(int mazeDifficulty)
    {
        return (int)Mathf.Floor(MazeUTL.GetMazeShortestSide() / 2);		// 10
    }

	public static int CalculateObjectiveLeastSteps(int mazeDifficulty)
	{
		return 10;
	}

    public static int CalculateHealthPackNum(int mazeDifficulty)
    {
		return 12;
    }

    public static int CalculateBodyPartChestNum(int mazeDifficulty)
    {
        return 1;
    }

	public static int CalculateCompassNum(int mazeDifficulty)
	{
		return 2;
	}

	public static int CalculateItemLeastDistance(int mazeDifficulty)
	{
		return 3;
	}

    public static int CalculateMonsterNum(int mazeDifficulty)
    {
		return 8;
    }

	public static int CalculateMonsterPlayerLeastDistance(int mazeDifficulty)
	{
		return 8;
	}

    public static int CalculateMonsterLeastDistance(int mazeDifficulty)
    {
        return 6;
    }
}