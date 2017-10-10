﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	public static LevelManager instance = null;

    // Helper classes
    private MazeGenerator mazeGenerator;
    private UnitSpawner unitSpawner;

    #region Inspector
    [Header("Player Character")]
    public GameObject playerCharacterPrefab;

    [Header("Maze")]
    public int mazeWidth = 10;
    public int mazeLength = 10;
    public MazeSetting mazeSetting;

    [Header("Game Mode")]
    public GameObject startPointPrefab;
    public GameObject levelObjectivePrefab;
    #endregion

    // Global variables
    [HideInInspector]
	public Maze maze;
    [HideInInspector]
    public Tile tileStart;
    [HideInInspector]
    public Tile tileObjective;
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

        // Get component reference to the attached script
        mazeGenerator = new MazeGenerator
                            (
                                mazeWidth, 
                                mazeLength, 
                                mazeSetting, 
                                startPointPrefab, 
                                levelObjectivePrefab
                             );
        unitSpawner = new UnitSpawner();

        // Start init the game
        InitGame();
    }

    // Game init function
	void InitGame()
    {
        // Generate a maze
        maze = mazeGenerator.GenerateMaze();

		// Spawn enemies
		// (WIP......)

		// Spawn player character
		playerCharacter = unitSpawner.SpawnPlayerCharacter(tileStart);

        // Set finsiedInit
        finsiedInit = true;
    }

    //---------------------------------------
    //      Game Mode
    //---------------------------------------
	public void CheckWinningCondition()
	{
        if (!finsiedInit)
            return;

        if (playerCharacter.hasObjective)
        {
            Debug.Log("Level Finished!!");  // Debug: tmp message        
        }   
	}

    //---------------------------------------
    //      Static Functions
    //---------------------------------------
    public static LevelManager GetLevelManager()
    {
        return GameObject.Find("LevelManager").GetComponent<LevelManager>();
    }
}
