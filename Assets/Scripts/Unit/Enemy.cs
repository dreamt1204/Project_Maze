using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// Important invariants:
/// 
/// * For sensing
/// -- vision : check every, or every N, frames
/// -- hearing : use listener system to update to noise immediately (at the noise frame)
/// 
/// * No current action is marked by currAction = Null
/// - At completion of any action, or cancelation of any action, currAction need to be set to Null
/// 
/// 
/// </summary>


public class Enemy : Unit {

	//---------------------------------------
	// Class-specific data types
	//---------------------------------------

	public enum statusType
	{
		Hyper, // invariant: enemy is always in this status if top attention is "Alerting noise/sight"
		Hyper2, // invariant: enemy is always in this status if top attention is "player/opponent detected"
		Asleep,
		Injured,
		Blinded,
		Deafened
	}

	public struct status
	{
		statusType type;
		int duration;

		public status (statusType type0, int duration0)	{type = type0; duration = duration0;}
	}
		

	//=======================================
	//      Variables
	//=======================================

	// Dynamic lists and variables 
	AttentionList attnList = new AttentionList();
	List<status> statusList = new List<status>();
	EnemyAction currAction;
	SensedEvent currTopPriority;

	// Priority Look-Up Table (LUT)
	Dictionary <SensedEvent.EventType, int> PriorityLUT = new Dictionary<SensedEvent.EventType, int>();

	// Attributes
	protected int visionLevel = 3;
	protected int hearingLevel = 10;

	// System-wide attributes
	protected int hyperDurationAfterAttnListEmptied;
	protected int attnListMaxLength;
	protected int attnDurationIfNotAtTop;


	//---------------------------------------
	//      Properties
	//---------------------------------------



	//=======================================
	//      Functions
	//=======================================

	// Empty for now to disable bodyparts
	public override void Start()
	{
		attnList.SetLevelManager (levelManager);
		LoadPriorityList ();
		InvokeRepeating ("RunEveryPeriod", 0, 1);
	}

	public void RunEveryPeriod()
	{
		//attnList.PrintAttnList ();
	}

	// Update behavior state based on top-priorty attention
	public override void Update ()
	{
		if (attnList.Count() > 0) {
			Debug.Log ("******* This is the start of Enemy.Update *******");
			attnList.PrintAttnList ();
		}

		// =============================================================================
		//      Sensory (Updates AttnList)
		// =============================================================================

		//Debug.Log ("===== See called =====");
		See (); // Observe n tiles in all 4 directions, conn-4 only
		// ... Listen -- listening is handled by event-listener system

		//Debug.Log ("End of See");

		// =============================================================================
		//      Trigger AttnList-Based Downstream Effects (non-action long-term effects) 
		// =============================================================================

		//Debug.Log ("===== ActivateAttnList called =====");
		ActivateAttnList();

		if (attnList.tilePlayerHeadingTo != null) {
			Debug.Log ("TilePlayerLastHeadingTo: X = " + attnList.tilePlayerHeadingTo.X + "; Z = " + attnList.tilePlayerHeadingTo.Z);
		}
		// Debug.Log ("End of ActivateAttnList");

		// =============================================================================
		//      Action 
		// =============================================================================
		if (currAction == null) { // Can do next action
			DetermineAction (); // Updates currAction based on attnList;
			InitiateAction (); // Starts to perform currAction
		} else {
			ContinueAction (); // Continues currAction (and run completion check at end)
		}
	}

	// Updates attnList
	protected void See ()
	{
		List<Tile> visibleTileList = new List<Tile> ();

		if (visionLevel > 0) // if visionLevel <= 0, does not detect opponents even on the same tile (by vision system at least)
		{
			visibleTileList.Add (this.CurrentTile); // if visionLevel >= 1, guarantees one's own tile

			for (int dir = 0; dir < 4; dir++) {
				Tile tile0 = this.CurrentTile;
				for (int n = 1; n <= visionLevel; n++) { 
					Tile tile1 = Maze.GetDirNeighborTile (tile0, dir);
					// Check if tile out of bound
					if (tile1 == null) {
						break;
					}
					if (!Maze.WallInBetween (tile0, tile1)) {
						visibleTileList.Add (tile1);
						tile0 = tile1;
					} else {
						break; // Stops investigating this direction as soon as hits a wall
					}
				}
			}
		}

		// ExtensionMethods.PrintTileList ("Visible tiles by enemy", visibleTileList);

		Tile playerTile = levelManager.playerCharacter.CurrentTile;
		if (visibleTileList.Contains (playerTile)) {
			attnList.AddEvent (SensedEvent.EventType.PlayerDetected, playerTile);
			Debug.Log ("Enemy class Add Event of PlayerDetected by vision.");
			// Note: Player's last MoveToTile call's target tile is also recorded by attnList
		}
	}

	// (1) determine top-priority sEvent, (2) remove forgotten non-top sEvents, (3) if length > MaxLength, trim attnList (ignore for now)
	// (4) activate any status / attribute changes based on new attnList (status + change color)

	// Efficiency goal: recurse through entire attnList once, for getting top-priority
	protected void ActivateAttnList()
	{
		// ...
		// Determine top-priority event

		/*
		int[] priorityValues = new int[attnList.Count];
		for (int n = 0; n < attnList.Count; n++) {
			priorityValues [n] = sEventPriority [attnList [n].type];
		}

		int maxPriorityValue = priorityValues;
		attnList.Sort;

		RefactorAttnList();
		*/

		attnList.UpdateMemory ();

	}


	// Invariant: need to check cancelability before calling this
	protected void quitCurrentAction()
	{
		if (!currAction.isCancelable) 
		{
			Debug.LogError ("'quitCurrentAction' is called upon a non-cancelable enemy action.");
		}
		// ...
	}

	protected void DetermineAction(){}
	protected void InitiateAction(){}
	protected void ContinueAction(){}

	// Subscribe + Unsubscribe to Noise Events
	protected void OnEnable()
	{
		Debug.Log ("Enemy Enabled");
		EventManager.UponNoise += Hear;
	}


	protected void OnDisable()
	{
		Debug.Log ("Enemy Disabled");
		EventManager.UponNoise -= Hear;
	}


	protected void Hear(Tile source, int intensity)
	{
		// Even if stepNoiseLevel == 0, makes noise at player's own tile
		// To "not make noise", set player's stepNoiseLevel = -999;
		if (Maze.DistBtwTiles (this.currentTile, source) <= (intensity + hearingLevel) ) { 
			attnList.AddEvent (SensedEvent.EventType.AlertingNoise, source);
			Debug.Log ("Noise heard at X = " + source.X + ", Z = " + source.Z);
		}
	}

	// Priority List Related Functions
	protected virtual void LoadPriorityList()
	{
		PriorityLUT.Add (SensedEvent.EventType.PlayerDetected, 100);
		PriorityLUT.Add (SensedEvent.EventType.PlayerLastDetected, 90);
		PriorityLUT.Add (SensedEvent.EventType.OpponentDetected, 150);
		PriorityLUT.Add (SensedEvent.EventType.OpponentLastDetected, 140);
		PriorityLUT.Add (SensedEvent.EventType.AlertingNoise, 30);
		PriorityLUT.Add (SensedEvent.EventType.AlertingSight, 50);
	}

}
