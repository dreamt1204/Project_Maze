using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2 : Unit {

	//---------------------------------------
	// Class-specific data type 
	//---------------------------------------

	public enum sEventType 
	{
		PlayerDetected, // event that player is detected by this enemy's vision / hearing / etc
		OpponentDetected, // opponent is another enemy that this enemy is aggresive to
		PlayerLastDetected, // event that player is last sighted / heard / detected at certain tile
		OpponentLastDetected,
		AlertingNoise, // e.g. player's step sound, explosion, destruction, yelling
		AlertingSight // e.g. player's illumination light source, corpse, objectile
	}




	// sEvent (sensed event) is a struct that holds (1) type of sensed event, and (2) location of sensed event
	public struct sEvent
	{
		sEventType type;
		Tile tile;

		public sEvent(sEventType type0, Tile tile0){
			type = type0;
			tile = tile0;
		}
	}

	//=======================================
	//      Variables
	//=======================================



	//---------------------------------------
	//      Properties
	//---------------------------------------



	//=======================================
	//      Functions
	//=======================================

	// Update behavior state based on top-priorty attention


}
