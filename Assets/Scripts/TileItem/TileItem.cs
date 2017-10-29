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
    HealthPack,
    BodyPart,
	Compass
}

public class TileItem : MonoBehaviour {
	//=======================================
	//      Variables
	//=======================================
	public string itemName;
	public ItemType itemType;
    public float healAmount;
    public BodyPart bodyPart;
	public float compassDuration;

	//=======================================
	//      Functions
	//=======================================
}
