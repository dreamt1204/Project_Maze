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
    public SlimeAbility Ability1;
    public SlimeAbility Ability2;

    public float[] experienceData = new float[] { 3, 5, 10, 15, 20 };

    //=======================================
    //      Functions
    //=======================================
}
