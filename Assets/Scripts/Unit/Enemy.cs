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
	EnemyAction currAction = null;
	SensedEvent currTopPriority = null;
	Tile prevTile = null;

	// Priority Look-Up Table (LUT)
	Dictionary <SensedEvent.EventType, int> PriorityLUT = new Dictionary<SensedEvent.EventType, int>();

	// Attributes
	protected int visionLevel = 3;
	protected int hearingLevel = 10;

	protected const float movementMultiplier = 0.15f;

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
		attnList.SetLevelManager (level);
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
			//Debug.Log ("******* This is the start of Enemy.Update *******");
			//attnList.PrintAttnList ();
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

		//if (attnList.tilePlayerHeadingTo != null) {
		//	Debug.Log ("TilePlayerLastHeadingTo: X = " + attnList.tilePlayerHeadingTo.X + "; Z = " + attnList.tilePlayerHeadingTo.Z);
		//}
		// Debug.Log ("End of ActivateAttnList");

		// =============================================================================
		//      Action 
		// =============================================================================
		if (currAction == null) { // Can do next action
			DetermineAction (); // Updates currAction based on attnList;
			InitiateAction (); // Starts to perform currAction
		} else {
			// ContinueAction (); // Continues currAction (and run completion check at end)
			// By Unity Coroutine, there is currently no need for ContinueAction, at least for implementing simple walking + attack
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
					Tile tile1 = MazeUTL.GetDirNeighborTile (tile0, dir);
					// Check if tile out of bound
					if (tile1 == null) {
						break;
					}
					if (!MazeUTL.WallBetweenNeighborTiles (tile0, tile1)) {
						visibleTileList.Add (tile1);
						tile0 = tile1;
					} else {
						break; // Stops investigating this direction as soon as hits a wall
					}
				}
			}
		}

		// ExtensionMethods.PrintTileList ("Visible tiles by enemy", visibleTileList);

		Tile playerTile = LevelManager.instance.playerCharacter.CurrentTile;
		if (visibleTileList.Contains (playerTile)) {
			attnList.AddEvent (SensedEvent.EventType.PlayerDetected, playerTile);
			// Debug.Log ("Enemy class Add Event of PlayerDetected by vision.");
			// Note: Player's last MoveToTile call's target tile is also recorded by attnList
		}
	}

	// (1) determine top-priority sEvent, (2) remove forgotten non-top sEvents, (3) if length > MaxLength, trim attnList (ignore for now)
	// (4) activate any status / attribute changes based on new attnList (status + change color)

	// Efficiency goal: recurse through entire attnList once, for getting top-priority
	protected void ActivateAttnList()
	{
		// Determine top-priority event
		UpdateCurrentTopPriority ();

		// Prolong memLife of top-priority event (if eventType allows); so enemy finishes topPriority investigation without forgetting
		ResetMemLifeForTopPriority ();

		// Reduce the memLife of all events by 1; remove sensedEvent from attnList if memLife becomes negative
		// This mechanism removes non-top-priority events that enemy sensed (e.g. Noise event, when attnList is dominated by PlayerDetected).
		attnList.UpdateMemory ();
	}


	protected 	void UpdateCurrentTopPriority() {
		// If attnList is not empty, find top priority event
		// If attnList is empty, return null
		if (attnList.Count () != 0) {
			currTopPriority = attnList.TopPriorityEvent (this.PriorityLUT);
		} else {
			currTopPriority = null;
		}

		//PrintTopPriority ();
	}

	protected void ResetMemLifeForTopPriority() {
		if (currTopPriority != null) {
			attnList.ResetEventMemLife (currTopPriority);
		}
	}

	//	 Invariant: need to check cancelability before calling this
	protected void QuitCurrentAction()
	{
		if (!currAction.isCancelable) 
		{
			Debug.LogError ("'quitCurrentAction' is called upon a non-cancelable enemy action.");
		}
		// ...
	}


	// Current implementation (assuming enemy can only move and melee attack): 
	// (1) if not on eventTile, move to top-priority event tile
	// (2) if on eventTile of 	PlayerDetected: melee attack (this tile)
	//							PlayerLastDetected: keeps [move_random_but_avoid_turning_back] (turns only when hits dead end)
	//							AlertingNoise: keeps [move_random_but_avoid_turning_back] (turns only when hits dead end)
	protected void DetermineAction()
	{
		if (currAction != null)
			Debug.LogError ("DetermineAction is called when currAction is non-null."); 

		// Default action when there is no entry in AttnList (thus no top priority)
		if (currTopPriority == null) 
		{
			currAction = idleAction ();
		}
		else 
		{
			// Move to currTopPriority Tile if not already on the same tile
			if (this.CurrentTile != currTopPriority.EventTile) {
				List<Tile> path = MazeUTL.FindPathByAddress (this.CurrentTile, currTopPriority.EventTile);
				currAction = new EnemyAction (EnemyAction.actionType.Movement, path [0]); // Sets action to walking towards first element in path
			} else {
				// If on the same tile as currTopPriority
				switch (currTopPriority.Type) {
				case SensedEvent.EventType.PlayerDetected:
					currAction = new EnemyAction(EnemyAction.actionType.Attack, this.CurrentTile);
					break;
				case SensedEvent.EventType.PlayerLastDetected:
					currAction = RandomMovementForward ();
					break;
				case SensedEvent.EventType.AlertingNoise:
					currAction = RandomMovementForward ();
					break;
				}
			}
		}
			
		if (currAction == null) {
			Debug.LogError ("currAction is not set at the end of DetermineAction call.");
		} else {
			Debug.Log("currAction is set to " + currAction.type + "; TargetTile is X = " + currAction.targetTile.X + "; Z = " + currAction.targetTile.Z);
		}

	}

	protected virtual EnemyAction idleAction(){
		return RandomMovementForward ();
	}

	// Returns EnemyAction of movement; randomly choose direction to proceed, but avoids turning back (unless at dead end)
	protected virtual EnemyAction RandomMovementForward(){
		
		List<Tile> possibleTiles = MazeUTL.GetNeighborTiles (this.CurrentTile);
		int test = possibleTiles.RemoveAll (tile0 => MazeUTL.WallBetweenNeighborTiles (this.CurrentTile, tile0));

		if (prevTile != null) {
			if (possibleTiles.Count > 1) {
				possibleTiles.Remove (prevTile);
			} else {
				if (possibleTiles [0] != this.prevTile) {
					Debug.LogError ("The only available (non-walled) neighboring tile is not the enemy's previous tile");
				}
			}
		}

		if (possibleTiles.Count == 0)
			Debug.LogError ("No possible tiles to walk towards.");
		
		// Draw a random tile from possibleTiles and set as Movement EnemyAction
		System.Random rnd = new System.Random ();
		return new EnemyAction (EnemyAction.actionType.Movement, possibleTiles[rnd.Next(0,possibleTiles.Count)]);
	}

	protected void InitiateAction()
	{
		// Based on currAction, initiate currAction
		if (currAction == null)
			Debug.LogError ("InitiateAction is called while currAction is null.");

		switch (currAction.type) {
		case EnemyAction.actionType.Attack:
			Debug.Log ("Enemy attack is initiated!");
			StartCoroutine (MeleeAttackCoroutine (this.CurrentTile));
			break;
		case EnemyAction.actionType.Movement:
			if (!MazeUTL.TilesAreNeighbors (this.CurrentTile, currAction.targetTile))
				Debug.LogError ("Initiating movement towards a non-neighboring tile.");
			TryMoveToTile (currAction.targetTile); // careful -- other types of errors (e.g. walking towards wall) is currently silent
			break;
		}
	}

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
		if (MazeUTL.GetDistanceBetweenTiles (this.CurrentTile, source) <= (intensity + hearingLevel) ) { 
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

	protected void PrintTopPriority(){
		if (currTopPriority == null) {
			Debug.Log ("currTopPriority is null.");
		}
		else {
			Debug.Log("currTopPriority is " + currTopPriority.Type + "; Tile is X = " + currTopPriority.EventTile.X + "; Z = " + currTopPriority.EventTile.Z + "; memLife = " + currTopPriority.MemoryLife);
		}
	}

	// Overriding functions
	// ******** ANY CHANGE TO PARENT FUNCTION NEEDS COMMENTS RIGHT AFTER LINE **********
	protected virtual IEnumerator MoveToTileCoroutine (Tile targetTile)
	{
		CurrentAction = ActionType.Walking;
		keepWalkingAnim = true;
		playWalkingAnim = true;

		//  =========  temporarily commented out until enemy skeleton works ========= 
		/*

		// Update anim play speed
		float originalTimeScale = skeletonAnim.timeScale;
		skeletonAnim.timeScale = moveSpeed * 0.01f * walkAnimScaleMultiplier;

		*/

		Vector3 target = targetTile.gameObject.transform.position;
		while (Vector3.Distance(transform.position, target) > 0.25f)
		{
			transform.Translate((target - transform.position).normalized * Time.deltaTime * moveSpeed * movementMultiplier);

			yield return null;
		}
		transform.position = target;
		prevTile = CurrentTile; // <==== JY ADDED
		CurrentTile = targetTile;

		// Reset unit state

		//  =========  temporarily commented out until enemy skeleton works ========= 
		// skeletonAnim.timeScale = originalTimeScale;
		CurrentAction = ActionType.None;

		if (!keepWalkingAnim)
			playWalkingAnim = false;

		currAction.isComplete = true; // <==== JY ADDED
		currAction = null; // <==== JY ADDED
	}

	protected virtual IEnumerator MeleeAttackCoroutine (Tile targetTile)
	{
		//CurrentAction = ActionType.Walking;
		//keepWalkingAnim = true;
		//playWalkingAnim = true;

		int attackingTime = 50;
		while (attackingTime > 0)
		{
			//Debug.Log ("Running melee attack coroutine: Action Life = " + attackingTime);
			attackingTime--;
			yield return null;
		}

		// CurrentAction = ActionType.None;
		currAction.isComplete = true; // <==== JY ADDED
		currAction = null; // <==== JY ADDED
	}

}
