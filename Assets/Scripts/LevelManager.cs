﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	public static LevelManager instance = null;

	// Preset prefab
	public GameObject startPointPrefab;
	public GameObject levelObjectivePrefab;
	public GameObject playerCharacterPrefab;

	// Helper class that attached to the GM object
	private MazeGenerator mazeGenerator;

	// Global variables
	[HideInInspector]
	public Maze maze;
	[HideInInspector]
	public PlayerCharacter playerCharacter;

    // State variables
    private bool finsiedInit = false;


    //=======================================
    //      Functions
    //=======================================   
    // Use this for initialization
    void Awake()
    {
		// This enforces our singleton pattern, meaning there can only ever be one instance of a LevelManager.
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);    

		// Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad(gameObject);

		// Get component reference to the attached script
		mazeGenerator = GetComponent<MazeGenerator>();

		// Start init the game
		InitGame();
    }

    // Game init function
	void InitGame()
    {
        // Generate a maze
		GenerateMaze();

		// Spawn game mode object
		InitGameMode();

		// Spawn enemies
		// .....

		// Spawn player character
		SpawnStartingCharacter();

        // Set finsiedInit
        finsiedInit = true;
    }

	//---------------------------------------
	//      Maze
	//---------------------------------------
	public void GenerateMaze()
    {
		maze = mazeGenerator.BuildMaze();
    }

	//---------------------------------------
	//      Unit spawn functions
	//---------------------------------------
	public void SpawnStartingCharacter()
	{
		playerCharacter = SpawnPlayerCharacter(playerCharacterPrefab, maze.tile[0, 0]);
	}

	public PlayerCharacter SpawnPlayerCharacter(GameObject prefab, Tile targetTile)
	{
		PlayerCharacter character = Instantiate (prefab, targetTile.transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<PlayerCharacter>();
		character.Init(this, targetTile);

		return character;
	}

    //---------------------------------------
    //      Game Mode
    //---------------------------------------
    public void InitGameMode()
	{
		Tile startTile = maze.tile [0, 0];
		Tile objectiveTile = maze.tile [2, 0];

		startTile.SpawnTileItem (startPointPrefab);
		objectiveTile.SpawnTileItem (levelObjectivePrefab);
	}

	public void CheckWinningCondition()
	{
        if (!finsiedInit)
            return;

        if (playerCharacter.hasObjective)
        {
            Debug.Log("Level Finished!!");  // Debug: tmp message        
        }   
	}
}