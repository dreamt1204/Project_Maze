using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit {
	//=======================================
	//      Variables
	//=======================================

	// Declaration of Type "State"
	// Specifies the level of awareness towards player
	// Changes sensation levels (e.g. improved sensitivity to noise in alerted states)
	// Dictates the set of actions performed

	protected enum State {Relaxed, Alerted, PlayerLocated};

	// public enum Direction {North, South, East, West};  <=== use default in namespace instead (integer direction)

	protected State state = State.Relaxed;

	protected int visionLevel; // number of tiles that enemy can see in 1 direction
	protected int hearingLevel; // number of *extra* tiles that enemy can hear noise from
	protected int proximityLevel; // number of tiles within which enemy can directly sense player (even without vision or step noise)

	protected Tile tileToCheck; // For Alerted state checking
	protected Tile tilePlayerLastLocated; 
	protected int dirPlayerLastLocated; // direction towards which player exits "tilePlayerLastLocated"

	protected int alertDuration = 0;
	protected int alertDurationMax = 10; // sec; after which switch back to State.Relaxed

	//---------------------------------------
	//      Properties
	//---------------------------------------



	//=======================================
	//      Functions
	//=======================================
	// Use this for initialization
	public override void Init (LevelManager lm, Tile spawnTile)
	{
		base.Init(lm, spawnTile);

	}

	// Update function
	void Update()
	{
		Sense ();
		Act ();
	}

	// Function to detect player with sensory inputs
	void Sense(){
		CheckVision ();
		CheckNoise ();
		CheckProximity ();
	}

	// Function to perform behavior based on state
	void Act(){
		switch (state) {
		case State.Relaxed:
			RelaxedBehavior ();
			break;
		case State.Alerted:
			AlertedBehavior ();
			break;
		case State.PlayerLocated:
			PlayerLocatedBehavior ();
			break;
		}
	}

	// Function to switch between states (update detection sensitivity)
	void SwitchToState(State nextState)
	{
		state = nextState;

		switch (state) {
		case State.Relaxed:
			UpdateSenseLevel (2, 0, 1);
			MoveSpeed = 90;
			break;
		case State.Alerted:
			UpdateSenseLevel (2, 2, 2);
			MoveSpeed = 100;

			alertDuration = 0;
			break;
		case State.PlayerLocated:
			UpdateSenseLevel (2, 2, 2);
			MoveSpeed = 120;
			break;
		}
	}

	// Group of actions corresponding to each state

	protected virtual void RelaxedBehavior(){
	}

	protected virtual void AlertedBehavior(){

		// After certain frames of being in alerted state, switch back to relaxed state
		alertDuration++;
		if (alertDuration >= alertDurationMax) {
			SwitchToState (State.Relaxed);
		}
	}

	protected virtual void PlayerLocatedBehavior(){
	}	

	// Functions corresponding to each sensory input
	void CheckVision(){
	}

	void CheckNoise(){
	}

	void CheckProximity(){
	}

	// Function to update sensitivity level
	void UpdateSenseLevel(int vLevel, int hLevel, int pLevel){
		visionLevel = vLevel;
		hearingLevel = hLevel;
		proximityLevel = pLevel;
	}
		

	// Function to move to certain tile of the entire map
	// Assuming monsters have perfect knowledge of the map, and pick shortest distance path
	// If two paths are of same distance, randomly pick one.
	void MoveToTile2(){
	}

	// When player vanish from sensed regions, check the tile where Player was heading towards.
	void PredictPlayerTile(){
		tileToCheck = Maze.GetDirNeighborTile (tilePlayerLastLocated, dirPlayerLastLocated);
	}



}
