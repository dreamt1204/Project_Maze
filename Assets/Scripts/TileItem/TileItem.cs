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
        if (itemType != ItemType.BodyPart)
            bodyPart = null;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
