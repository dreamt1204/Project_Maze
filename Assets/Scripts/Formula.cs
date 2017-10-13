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
}