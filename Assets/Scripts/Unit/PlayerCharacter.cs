using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Unit {
	//=======================================
	//      Variables
	//=======================================
	private Camera playerCamera;
    [HideInInspector]
	public bool hasObjective = false;

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
            currentTile.CheckTileAction();
            currentTile.CheckPlayerTileAction();
            levelManager.maze.UpdateWalkableTiles(currentTile);
        }
	}

	//=======================================
	//      Functions
	//=======================================
	// Use this for initialization
	public override void Init (LevelManager gm, Tile spawnTile)
	{
		base.Init(gm, spawnTile);

		// Setup player camera
		playerCamera = ((GameObject)Instantiate (Resources.Load ("PlayerCamera"))).GetComponent<Camera>();
		playerCamera.transform.parent = this.transform;
	}
}
