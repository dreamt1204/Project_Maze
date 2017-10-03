﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Unit {
	//=======================================
	//      Variables
	//=======================================
	private Camera playerCamera;
	[HideInInspector]
	private bool holdingWalkingButton = false;
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
        Vector3 cameraPos = playerCamera.transform.position;
        Vector3 playerPos = this.transform.position;
        playerCamera.transform.position = new Vector3(cameraPos.x + playerPos.x, cameraPos.y + playerPos.y, cameraPos.z + playerPos.z);
        playerCamera.transform.parent = this.transform;
	}

	// Update function
	void Update()
	{
		HoldingMove ();
	}

	// This feature lets player move to the next time while player is holding the mouse during movement.
	// The direction is calculate based on the mouse and character origin.
	public void HoldingMove()
	{
		// Check if player is holding the mouse during movement. If true, set holdingWalkingButton flag.
		if (!holdingWalkingButton)
		{
			if (isWalking && Input.GetMouseButton(0))
				holdingWalkingButton = true;
		}
		// If holdingWalkingButton is set...
		else
		{
			// Unset holdingWalkingButton while player release the mouse.
			if (Input.GetMouseButtonUp (0))
			{
				holdingWalkingButton = false;
			}
			// If player finished his movement and is ready for the next move, move the character to the walkable tile with holding direction.
			else if (!isWalking)
			{
				Tile nextTile = levelManager.maze.GetDirNeighborTile (currentTile, GetHoldingMoveDir());
				if ((nextTile != null) && (nextTile.State == TileState.Walkable))
					TryMoveToTile (nextTile);
			}
		}
	}

	// Get the direction calculated based on the mouse and character origin.
	public int GetHoldingMoveDir()
	{
		int dir = 0;

		Vector3 charScreenPos = playerCamera.WorldToScreenPoint (transform.position);
		charScreenPos = new Vector3 (charScreenPos.x, charScreenPos.y, 0);

		float length_x = (Input.mousePosition.x - charScreenPos.x);
		float length_y = (Input.mousePosition.y - charScreenPos.y);

		if (Mathf.Abs (length_x) >= Mathf.Abs (length_y))
		{
			if (length_x >= 0)
				dir = 1;
			else
				dir = 3;
		}
		else
		{
			if (length_y >= 0)
				dir = 0;
			else
				dir = 2;
		}

		return dir;
	}
}
