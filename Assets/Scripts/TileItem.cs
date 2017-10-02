using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
	Pickup,
	StartPortal,
	Objective
}

public class TileItem : MonoBehaviour {
	//=======================================
	//      Variables
	//=======================================
	public string itemName;
	public ItemType type;

	//=======================================
	//      Functions
	//=======================================
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
