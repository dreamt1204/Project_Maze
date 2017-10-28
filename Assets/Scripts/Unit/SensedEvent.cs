using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SensedEvent {

	//---------------------------------------
	// Class-specific data types
	//---------------------------------------

	public enum EventType 
	{
		testTest,
		PlayerDetected, // event that player is detected by this enemy's vision / hearing / etc
		OpponentDetected, // opponent is another enemy that this enemy is aggresive to
		PlayerLastDetected, // event that player is last sighted / heard / detected at certain tile
		OpponentLastDetected,
		AlertingNoise, // e.g. player's step sound, explosion, destruction, yelling
		AlertingSight // e.g. player's illumination light source, corpse, objectile
	};
		
	//=======================================
	//      Variables
	//=======================================

	private EventType type;
	private Tile eventTile;
	private int memoryLife; 
	private bool isTileInvestigated = false;


	//---------------------------------------
	//      Properties
	//---------------------------------------
	public EventType Type { get; set;}
	public Tile EventTile { get; set;}
	public int MemoryLife { get; set; }
	public bool IsTileInvestigated { get; set; }

	//=======================================
	//      Functions
	//======================================= 

	// PROBLEM : CLASS INITIATOR INPUT TYPE IS MESSED UP
	public SensedEvent (EventType type0, Tile tile0, int m) {
		Type = type0; 
		EventTile = tile0; 
		MemoryLife = m;
	}


}