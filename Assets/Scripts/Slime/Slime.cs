using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlimeType
{
    DefaultSlime,
    TestSlime
}

public class Slime : MonoBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    public SlimeType slimeType;
    public string slimeSprite;
    public string slimeName;
    public string slimeDescription;
    public string slimeSkinName;
    public SlimeSplit slimeSplit;
    public PlayerAbility Ability1;
    public PlayerAbility Ability2;

    public SlimeLevelData[] levelData = new SlimeLevelData[] { new SlimeLevelData(1, 3),
                                                                  new SlimeLevelData(2, 5),
                                                                  new SlimeLevelData(3, 10),
                                                                  new SlimeLevelData(4, 15),
                                                                  new SlimeLevelData(5, 20) };

    //=======================================
    //      Functions
    //=======================================
}

[System.Serializable]
public class SlimeLevelData
{
    [HideInInspector] public string level;
    public float requiredExp = 1;

    [Header("Rewards")]
    public bool unlockAbility1;
    public bool unlockAbility2;
    public float IncreaseHealth;
    public float IncreaseMoveSpeed;

    public SlimeLevelData(int lv, float exp)
    {
        level = "Level " + lv;
        requiredExp = exp;
        if (requiredExp <= 1)
            requiredExp = 1;
    }
}
