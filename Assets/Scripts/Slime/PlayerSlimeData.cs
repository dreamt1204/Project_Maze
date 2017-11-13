using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlimeData
{
    //=======================================
    //      Variables
    //=======================================
    public Slime slime;

    public bool unlocked;
    int slimeLevel_m;
    float slimeExp_m;

    public bool ability_1_unlocked;
    public bool ability_2_unlocked;

    public PlayerSlimeData(Slime newSlime)
    {
        slime = newSlime;
    }

    public PlayerSlimeData(Slime newSlime, int newLevel)
    {
        slime = newSlime;
        slimeLevel = newLevel;
        slimeExp_m = CalculateSlimeExperience(slimeLevel);
    }

    public PlayerSlimeData(Slime newSlime, float newExp)
    {
        slime = newSlime;
        slimeExp = newExp;
    }

    //---------------------------------------
    //      Properties
    //---------------------------------------
    public int slimeLevel
    {
        get
        {
            return slimeLevel_m;
        }
        set
        {
            slimeLevel_m = value;

            if ((!unlocked) && (value >= 1))
                unlocked = true;
        }
    }

    public float slimeExp
    {
        get
        {
            return slimeExp_m;
        }
        set
        {
            slimeExp_m = value;
            slimeLevel = CalculateSlimeLevel(value);
        }
    }

    //=======================================
    //      Functions
    //=======================================
    public int CalculateSlimeLevel(float exp)
    {
        int lv = 0;

        for (int i = 0; i < slime.levelData.Length; i++)
        {
            if (exp < slime.levelData[i].requiredExp)
            {
                lv = i;
                break;
            }

            if (i == (slime.levelData.Length - 1))
                lv = slime.levelData.Length;
            else
                exp -= slime.levelData[i].requiredExp;
        }

        return lv;
    }

    public float CalculateSlimeExperience(int lv)
    {
        float exp = 0;

        for (int i = 0; i < lv; i++)
        {
            exp += slime.levelData[i].requiredExp;
        }

        return exp;
    }

    public float GetExperienceLeft()
    {
        return (slimeExp - CalculateSlimeExperience(slimeLevel));
    }
}
