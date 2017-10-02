using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Unit {
	//=======================================
	//      Variables
	//=======================================
	public Camera playerCamera;

	//---------------------------------------
	//      Properties
	//---------------------------------------
	public override Tile CurrentTile
	{
		get
		{
			return currentTile;
		}
		set
		{
			currentTile = value;
			gameManager.maze.UpdateWalkableTiles (currentTile);
		}
	}

	//=======================================
	//      Functions
	//=======================================
	// Use this for initialization
	public override void Init (GameManager gm, Tile spawnTile)
	{
		base.Init(gm, spawnTile);

		// Setup player camera
		playerCamera = ((GameObject)Instantiate (Resources.Load ("PlayerCamera"))).GetComponent<Camera>();
		playerCamera.transform.parent = this.transform;
	}
}
