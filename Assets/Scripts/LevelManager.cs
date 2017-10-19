//============================== Class Definition ==============================
// 
// This class runs all the core game logic.
// A level must has a empty gameobject with this attached.
// All the level information can be edited in its inspector.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelManager : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	public static LevelManager instance = null;

    // Helper classes instances
    private MazeGenerator mazeGenerator;

	// Global variables
	[HideInInspector] public bool finishedInitLevel = false;
	[HideInInspector] public Maze maze;
	[HideInInspector] public Tile tileStart;
	[HideInInspector] public Tile tileObjective;
	[HideInInspector] public PlayerCharacter playerCharacter;

	// Global variables in Inspector
    [Header("Maze")]
	[Range(1, 100)]
	public int mazeDifficulty = 1;
    public MazeSetting mazeSetting;

    [Header("Custom Maze (Optional)")]
    public bool customMazeSize;
    public int mazeWidth = 10;
    public int mazeLength = 10;
    [Space(15)]
    public GameObject customMazeObject;
    public bool customGameModePosition;
    public bool customBodyPartChestPosition;

    [Header("Required Prefabs")]
    public GameObject playerCharacterPrefab;
    public GameObject startPointPrefab;
    public GameObject objectivePrefab;
    public GameObject BodyPartChestPrefab;

    //=======================================
    //      Functions
    //=======================================   
	void Awake()
    {
		// This enforces our singleton pattern, meaning there can only ever be one instance of a LevelManager.
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

        // Get component reference to the attached script
		mazeGenerator = gameObject.AddComponent<MazeGenerator>();

		Utilities.TryCatchError ((mazeSetting == null), "Maze Setting cannot be null");
    }

	void Start()
	{
		// Generate a maze
		maze = mazeGenerator.GenerateMaze();

		// Spawn enemies
		// (WIP......)

		// Spawn player character
		playerCharacter = UnitSpawner.SpawnPlayerCharacter(tileStart);

		// Set init flag to true
		finishedInitLevel = true;
	}

    //---------------------------------------
    //      Game Mode
    //---------------------------------------
	public void CheckGameModeCondition()
	{
		if (!finishedInitLevel)
			return;

		if (playerCharacter.Health <= 0)
		{
			FailLevel ();
		}
		else if (playerCharacter.hasObjective)
        {
			PassLevel ();
        }
	}

	void PassLevel()
	{
		Debug.Log("Level Finished!!");  // Debug: tmp message 
	}

	void FailLevel()
	{
		Debug.Log("Level Failed...");  // Debug: tmp message 
	}
}