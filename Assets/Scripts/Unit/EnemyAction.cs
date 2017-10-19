using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAction {

	//---------------------------------------
	// Class-specific data types
	//---------------------------------------

	public enum actionType 
	{
		Attack,
		Movement
	}

	//=======================================
	//      Variables
	//=======================================
	public actionType type;
	public bool isCancelable;

}
