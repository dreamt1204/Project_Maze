//============================== Class Definition ==============================
// 
// This a container class store all the info of tile item.
// Have to create prefab with this attached for future use.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
	StartPoint,
	Objective,
    BodyPart,
    SlimeElement,
    HealthPack,
    Compass
}

public class TileItem : MonoBehaviour
{
	//=======================================
	//      Variables
	//=======================================
	public ItemType itemType;

    [Header("Body Part")]
    public BodyPart bodyPart;

    [Header("Slime Element")]
    public Slime slime;

    [Header("Health Pack")]
    public float healAmount;

    [Header("Compass")]
    public float compassDuration;
}
