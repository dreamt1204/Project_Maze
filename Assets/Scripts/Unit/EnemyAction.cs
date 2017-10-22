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
	public Tile targetTile = null;  // This is temporarily implementation; enemyAction might involve no tile, 1 tile, many tiles
	public bool isComplete = false; // This is debug / safety feature
	public bool isCancelable = false;

	public EnemyAction (actionType type0, Tile targetTile0) {
		type = type0;
		targetTile = targetTile0;
	}

}
