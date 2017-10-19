﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Major purpose for this class is to not let external classes (esp. Enemy and its children) to 
// alter list content directly, to preserve invariants (for efficiency)

// Handles all creation of sEvents, and adding / removing sEvents from AttnList

// Major invariant:
// isPlayerDetected must reflect whether list contains PlayerDetected, and if so, first entry must be PlayerDetected
// PlayerDetected OR PlayerLastDetected is placed first in the list ([0])

// PlayerDetected has 1 frame lifeTime -- as soon as player is gone from senses, sEvent is removed

public class AttentionList {

	//=======================================
	//      Variables
	//=======================================

	LevelManager levelManager;

	List<SensedEvent> attnList = new List<SensedEvent>();
	bool isPlayerDetected = false; // efficiency feature; keep this updated always
	int veryLargeNumber = 99999999;
	int noiseEventMemoryLife = 200;

//	Tile tilePlayerDetected = null; // always set to null unless player is detected; set back to null at D -> LD logic
	public Tile tilePlayerHeadingTo = null; // always set to null unless player is detected; set back to null at D -> LD logic
	// Even if player is detected, can still be null if Player is static when seen -- so check if null before use

	//=======================================
	//      Functions
	//=======================================

	public void SetLevelManager (LevelManager lm) {
		levelManager = lm;
	}

	public void AddEvent(SensedEvent.EventType type, Tile tile){
		Debug.Log ("AddEvent called.");

		switch (type) {

		case SensedEvent.EventType.PlayerDetected: 
			// Note: PlayerDetected is constantly false due to lifeTime of 1 (needs new entry every frame)
			// replace PlayerLastDetected
			if (!isPlayerDetected) { // avoid addition with multiple senses giving detection
				isPlayerDetected = true;
				if (attnList.Count > 0) { // need this test so the next conditional does not have NullException when list is empty.
					if (attnList [0].Type == SensedEvent.EventType.PlayerLastDetected) {
						RemoveEventByIndex (0);
					}
				}
				attnList.Insert (0, new SensedEvent (SensedEvent.EventType.PlayerDetected, tile, 1));
				tilePlayerHeadingTo = levelManager.playerCharacter.TileWalkingTowards;
				// Debug.Log ("(2) tilePlayerHeadingTo X = " + tilePlayerHeadingTo.X + "; Z = " + tilePlayerHeadingTo.Z);
				// Debug.Log ("PlayerDetected Event added.");
			}
			break;

		case SensedEvent.EventType.PlayerLastDetected:
			Debug.LogError ("Error: PlayerLastDetected should never be an externally added event; only method is by removing PlayerDetected");
			break;

		case SensedEvent.EventType.AlertingNoise:
			attnList.Add (new SensedEvent (SensedEvent.EventType.AlertingNoise, tile, noiseEventMemoryLife));
			Debug.Log ("Noise Event added.");  ///// PROBLEM IS HERE (BEFORE THIS LINE OK, AFTER THIS LINE BECOMES WIERD
			break;
			
		}
	}

	public void UpdateMemory(){

		List<int> indexEventsOutdated = new List<int> ();

		for (int n = 0; n < attnList.Count; n++) {
			//Debug.Log ("Updating Event of Type " + attnList [n].Type + ", with memLife = " + attnList [n].MemoryLife);
			attnList [n].MemoryLife--;
			//Debug.Log ("Complete update of Event of Type " + attnList [n].Type + ", with memLife = " + attnList [n].MemoryLife);
			if (attnList [n].MemoryLife <= 0) {
				indexEventsOutdated.Add (n);
			}
		}

		if (indexEventsOutdated.Count > 0) {
			// Debug.Log ("Removing outdated memory(s)");

			indexEventsOutdated.Reverse (); // remove from the end, to avoid index shifting
			for (int n = 0; n < indexEventsOutdated.Count; n++) {
				RemoveEventByIndex (indexEventsOutdated [n]);
			}
		}
	}

	// Set to Private -- external (Enemy Class) should only be able to add to List
	private void RemoveEventByIndex(int index){

		// Debug.Log ("RemoveEventByIndex called");
		
		switch (attnList[index].Type) {

		case SensedEvent.EventType.PlayerDetected:
			// add PlayerLastDetected
			if (index == 0) {
				
				isPlayerDetected = false;
				Tile tileToInvestigate = attnList[0].EventTile; // Default (if no HeadingTo info, then look at player's simple last tile
				if (tilePlayerHeadingTo != null) {
					tileToInvestigate = tilePlayerHeadingTo;
					tilePlayerHeadingTo = null; // reset memory
				}
				attnList.RemoveAt (0);
				attnList.Insert (0, new SensedEvent (SensedEvent.EventType.PlayerLastDetected, tileToInvestigate, veryLargeNumber));

				// Debug.Log ("PlayerDetected Event removed.");

			} else {
				Debug.LogError ("Error: removing PlayerDetected sEvent but PlayerDetected is not at first entry");
			}
			break;

		case SensedEvent.EventType.PlayerLastDetected:
			if (index == 0) {
				attnList.RemoveAt (0);

				// Debug.Log ("PlayerLastDetected Event removed.");
			} else {
				Debug.LogError ("Error: removing PlayerLastDetected sEvent but PlayerLastDetected is not at first entry");
			}
			break;

		case SensedEvent.EventType.AlertingNoise:
			attnList.RemoveAt (index);

			// Debug.Log ("Noise Event removed.");
			break;
		}
	}

	public void PrintAttnList(){

		if (attnList.Count != 0) {
			Debug.Log ("====== PrintAttnList called ======");
			for (int n = 0; n < attnList.Count; n++) {
				Debug.Log ("Entry# " + (n+1) + "; Type = " + attnList [n].Type + "; memLife = " + attnList [n].MemoryLife);
			}
		}
	}

	public int Count(){
		return attnList.Count;
	}


}