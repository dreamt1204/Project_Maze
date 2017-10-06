using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	public static LevelManager instance = null;

    // Helper class that attached to the GM object
    private MazeGenerator mazeGenerator;
    private UnitSpawner unitSpawner;

    [Header("Game Mode")]
    // Preset prefab
    public GameObject startPointPrefab;
	public GameObject levelObjectivePrefab;

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

		// Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad(gameObject);

		// Get component reference to the attached script
		mazeGenerator = GetComponent<MazeGenerator>();
        unitSpawner = GetComponent<UnitSpawner>();

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
		playerCharacter = unitSpawner.SpawnStartingCharacter(tileStart); 

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
}
