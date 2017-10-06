using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
	Pickup,
	StartPortal,
	Objective,
    BodyPart
}

public class TileItem : MonoBehaviour {
	//=======================================
	//      Variables
	//=======================================
	public string itemName;
	public ItemType type;
    public BodyPart bodyPart;

	//=======================================
	//      Functions
	//=======================================
	// Use this for initialization
	void Start ()
    {
        if (type != ItemType.BodyPart)
            bodyPart = null;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
