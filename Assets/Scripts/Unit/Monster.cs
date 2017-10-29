using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Unit {

	//---------------------------------------
	// Class-specific data types
	//---------------------------------------

	public enum statusType
	{
		Hyper2, // invariant: enemy is always in this status if top attention is "playerDetect or playerLastDetect (eventTile not investigated)"
		Hyper, // invariant: enemy is always in this status if top attention is "playerLastDetect (eventTile investigated, but player not found)"
	}

	public struct status
	{
		public statusType type;
		public int duration;

		public status (statusType type0, int duration0)	{
			type = type0; 
			duration = duration0;
		}
	}
		

	//=======================================
	//      Variables
	//=======================================

	bool debugMode = true;

	// Dynamic lists and variables 
	AttentionList attnList = new AttentionList();
	List<status> statusList = new List<status>(); // Invariant: If enemy is in Hyper or Hyper2, these should always be the first element
	MonsterAction currAction = null;
	SensedEvent currTopPriority = null;
	Tile prevTile = null;

	// Priority Look-Up Table (LUT)
	Dictionary <SensedEvent.EventType, int> PriorityLUT = new Dictionary<SensedEvent.EventType, int>();

	// Attributes
	protected int visionLevel = 2;
	protected float baseMoveSpeed = 35.0f;

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
		MoveSpeed = baseMoveSpeed;
	}


	// Update behavior state based on top-priorty attention
	public override void Update ()
	{
		if (debugMode) {
			if (attnList.Count () > 0) {
				Debug.Log ("******* This is the start of Enemy.Update *******");
				attnList.PrintAttnList ();
			}
		}

		// =============================================================================
		//      Sensory input
		// =============================================================================

		// Try to locate player in all 4 directions
		See ();

		// =============================================================================
		//      Internal state changes
		// =============================================================================

		// Determine top-priority event
		UpdateCurrentTopPriority ();

		// Prolong memLife of top-priority event (if eventType allows); so enemy finishes topPriority investigation without forgetting
		ResetMemLifeForTopPriority ();

		// Reduce the memLife of all events by 1; remove sensedEvent from attnList if memLife becomes negative
		// This mechanism removes non-top-priority events that enemy sensed
		UpdateAttnListMemory ();

		// =============================================================================
		//      Action 
		// =============================================================================
		if (currAction == null) { // If true, monster is available do next action
			DetermineAction (); // Updates currAction based on attnList
			InitiateAction (); // Starts to perform currAction by coroutine
		} 

	}
		
	// Output is to update the attention list if monster sees player
	protected void See ()
	{
		List<Tile> visibleTileList = MazeUTL.GetVisibleTiles (this.CurrentTile, visionLevel);
			
		Tile playerTile = LevelManager.instance.playerCharacter.CurrentTile;
		if (visibleTileList.Contains (playerTile)) {
			attnList.AddEvent (SensedEvent.EventType.PlayerDetected, playerTile);
			// Note: Player's last MoveToTile call's target tile is also recorded by attnList.AddEvent.
			AddPriorityBasedStatus(statusType.Hyper2);
		}
	}

	protected void UpdateCurrentTopPriority() {
		// If attnList is not empty, find top priority event
		// If attnList is empty, return null
		if (attnList.Count () != 0) {
			currTopPriority = attnList.TopPriorityEvent (this.PriorityLUT);
		} else {
			currTopPriority = null;
		}
	}

	protected void ResetMemLifeForTopPriority() {
		if (currTopPriority != null) {
			attnList.ResetEventMemLife (currTopPriority);
		}
	}

	protected void UpdateAttnListMemory(){
		attnList.UpdateMemory ();
	}

	// Priority-based-status (hyper, hyper2) related functions
	// Invariant: only use these functions to mutate statusList!

	protected void AddPriorityBasedStatus (statusType type0) {
		if (statusList.Count == 0) {
			statusList.Add (new status (type0, 99999999)); // top-priority-based status have infinite duration; they can only be removed when top-priority changes
			RefreshAttributesBasedOnStatus ();
			return;
		} else {
			if (statusList [0].type == statusType.Hyper | statusList [0].type == statusType.Hyper2) {
				if (statusList [0].type != type0) {
					statusList.RemoveAt (0);
					statusList.Insert (0, new status (type0, 99999999));
					RefreshAttributesBasedOnStatus ();
					return;
				}
			}
		}
	}

	protected void RemovePriorityBasedStatus () {
		if (statusList.Count == 0) {
			Debug.LogError ("RemovePriorityBasedStatus called when monster has no status.");
		} else {
			if (statusList [0].type == statusType.Hyper | statusList [0].type == statusType.Hyper2) {
				statusList.RemoveAt (0);
				RefreshAttributesBasedOnStatus ();
			} else {
				Debug.LogError ("RemovePriorityBasedStatus called when priority-based status is not first element in status list.");
			}
		}
	}
		
	protected void RefreshAttributesBasedOnStatus(){

		// dafault values
		float moveSpeedModifier = 1.0f;
		gameObject.GetComponent<Renderer> ().material.color = Color.white;

		for (int i = 0; i < statusList.Count; i++) {
			switch (statusList [i].type) {
			case statusType.Hyper2:
				moveSpeedModifier = moveSpeedModifier * 2.0f;
				gameObject.GetComponent<Renderer> ().material.color = Color.red;
				break;
			case statusType.Hyper:
				moveSpeedModifier = moveSpeedModifier * 1.5f;
				gameObject.GetComponent<Renderer> ().material.color = Color.yellow;
				break;
			}
		}

		MoveSpeed = baseMoveSpeed * moveSpeedModifier;
	}
		

	// DetermineAction has 2 major responsibilities:
	// (1) updates currAction based on topPriorty
	// (2) updates top-priority-based status (besides Hyper2, which is added by Monster.See() );
	protected void DetermineAction()
	{
		if (currAction != null)
			Debug.LogError ("DetermineAction is called when currAction is non-null.");
		// Default action when there is no entry in AttnList (thus no top priority)
		if (currTopPriority == null) {
			currAction = idleAction ();
			if (statusList.Count > 0) {
				if (statusList [0].type == statusType.Hyper) {
					RemovePriorityBasedStatus ();
				}
			}
		} else {
			switch (currTopPriority.Type) {
			case SensedEvent.EventType.PlayerDetected:
				if (!IsOnEventTile ())
					currAction = ProceedToEventTile ();
				else
					currAction = new MonsterAction (MonsterAction.actionType.Attack, this.CurrentTile);
				break;
			case SensedEvent.EventType.PlayerLastDetected:
				if (!IsOnEventTile ()) {
					if (currTopPriority.IsTileInvestigated) {
						currAction = RandomMovementForwardStraight ();
					} else {
						currAction = ProceedToEventTile ();
					}
				} else {
					// This is the case when EventTile has been investigated; player has been lost
					currTopPriority.IsTileInvestigated = true;
					currAction = RandomMovementForwardStraight ();
					RemovePriorityBasedStatus (); // removes Hyper2
					AddPriorityBasedStatus (statusType.Hyper); // adds Hyper
				}
				break;
				
			}
			
			if (currAction == null) {
				Debug.LogError ("currAction is not set at the end of DetermineAction call.");
			} else {
				if (debugMode) {
					Debug.Log ("currAction is set to " + currAction.type + "; TargetTile is X = " + currAction.targetTile.X + "; Z = " + currAction.targetTile.Z);
				}
			}
		}

	}

	protected virtual MonsterAction idleAction(){
		return RandomMovement ();
	}

	protected bool IsOnEventTile(){
		return this.CurrentTile == currTopPriority.EventTile;
	}

	protected virtual MonsterAction ProceedToEventTile(){
		List<Tile> path = MazeUTL.FindPathByCompleteSearch (this.CurrentTile, currTopPriority.EventTile, false, visionLevel + 2, "Shortest_Random");
		return new MonsterAction (MonsterAction.actionType.Movement, path [0]); // Sets action to walking towards first element in path
	}

	protected virtual MonsterAction RandomMovement(){
		List<Tile> possibleTiles = MazeUTL.GetNeighborTilesWithoutWall(this.CurrentTile);
	
		// Draw a random tile from possibleTiles and set as Movement EnemyAction
		System.Random rnd = new System.Random ();
		return new MonsterAction (MonsterAction.actionType.Movement, possibleTiles [rnd.Next (0, possibleTiles.Count)]);
	}


	protected virtual MonsterAction RandomMovementForwardStraight(){

		List<Tile> possibleTiles = MazeUTL.GetNeighborTilesWithoutWall(this.CurrentTile);

		// if forward tile is within possibleTiles, pick forward
		int dir = MazeUTL.GetNeighborTileDir(prevTile,CurrentTile);
		Tile forwardTile = MazeUTL.GetDirNeighborTile (CurrentTile, dir);
		if (possibleTiles.Contains (forwardTile)) {
			// Debug.Log ("Forward tile available");
			return new MonsterAction (MonsterAction.actionType.Movement, forwardTile);
		} else {
			// avoids previous tile unless in dead end
			if (prevTile != null) {
				if (possibleTiles.Count > 1) {
					possibleTiles.Remove (prevTile);
				} 
			}

			if (possibleTiles.Count == 0)
				Debug.LogError ("No possible tiles to walk towards.");

			// Draw a random tile from possibleTiles and set as Movement EnemyAction
			System.Random rnd = new System.Random ();
			return new MonsterAction (MonsterAction.actionType.Movement, possibleTiles [rnd.Next (0, possibleTiles.Count)]);
		}

	}

	protected void InitiateAction()
	{
		// Based on currAction, initiate currAction
		if (currAction == null)
			Debug.LogError ("InitiateAction is called while currAction is null.");

		switch (currAction.type) {
		case MonsterAction.actionType.Attack:
			Debug.Log ("Monster attack is initiated!");
			StartCoroutine (MeleeAttackCoroutine (this.CurrentTile));
			break;
		case MonsterAction.actionType.Movement:
			if (!MazeUTL.TilesAreNeighbors (this.CurrentTile, currAction.targetTile))
				Debug.LogError ("Initiating movement towards a non-neighboring tile.");
			TryMoveToTile (currAction.targetTile); // careful -- other types of errors (e.g. walking towards wall) is currently silent
			break;
		}
	}

	// Priority List Related Functions
	protected virtual void LoadPriorityList()
	{
		PriorityLUT.Add (SensedEvent.EventType.PlayerDetected, 100);
		PriorityLUT.Add (SensedEvent.EventType.PlayerLastDetected, 90);
	//	PriorityLUT.Add (SensedEvent.EventType.OpponentDetected, 150);
	//	PriorityLUT.Add (SensedEvent.EventType.OpponentLastDetected, 140);
	//	PriorityLUT.Add (SensedEvent.EventType.AlertingNoise, 30);
	//	PriorityLUT.Add (SensedEvent.EventType.AlertingSight, 50);
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
	protected override IEnumerator MoveToTileCoroutine (Tile targetTile)
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
			transform.Translate((target - transform.position).normalized * Time.deltaTime * MoveSpeed * movementMultiplier);

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

	//	 Invariant: need to check cancelability before calling this
	protected void QuitCurrentAction()
	{
		if (!currAction.isCancelable) 
		{
			Debug.LogError ("'quitCurrentAction' is called upon a non-cancelable monster action.");
		}
		// ...
	}

}
