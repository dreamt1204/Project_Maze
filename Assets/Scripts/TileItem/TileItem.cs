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
	Pickup,
	StartPoint,
	Objective,
    BodyPart
}

public class TileItem : MonoBehaviour {
	//=======================================
	//      Variables
	//=======================================
	public string itemName;
	public ItemType itemType;
    public BodyPart bodyPart;

	//=======================================
	//      Functions
	//=======================================
	// Use this for initialization
	void Start ()
    {
        // Remove mis-input Body Part
        if (itemType != ItemType.BodyPart)
            bodyPart = null;
    }
}
