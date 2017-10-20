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
	[HideInInspector] public UIManager uiManager;

    //private UnitSpawner unitSpawner;
	private EnemyManager enemyManager;
	public EventManager eventManager; // FIX public to private

	// Global variables
	[HideInInspector] public bool finsiedInit = false;
	[HideInInspector] public Maze maze;
	[HideInInspector] public Tile tileStart;
	[HideInInspector] public Tile tileObjective;
	[HideInInspector] public PlayerCharacter playerCharacter;
	[HideInInspector] public List<Enemy> enemyList;

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

    [Header("Items")]
    public bool useItemGenerateLogic;
    public int numberOfItems;

    [Header("Required Prefabs")]
    public GameObject playerCharacterPrefab;
    public GameObject startPointPrefab;
    public GameObject objectivePrefab;
    public GameObject BodyPartChestPrefab;

	[Header("Enemy Spawning")]
	public InitSpawnMethod spawnMethod = InitSpawnMethod.Random;
	public int spawnQuantity = 1;
	public int safeRadius = 2;

	[Header("Enemy Prefabs")]
	public GameObject prefabA;
	public GameObject prefabB;
	public GameObject prefabC;

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
        uiManager = gameObject.AddComponent<UIManager>();

		enemyManager = gameObject.AddComponent<EnemyManager> ();
		eventManager = gameObject.AddComponent<EventManager> ();

		/*
		enemyManager = new EnemyManager
			(
				spawnMethod,
				spawnQuantity,
				safeRadius
			);
		eventManager = new EventManager();
		*/

		Utilities.TryCatchError ((mazeSetting == null), "Maze Setting cannot be null");
    }

	void Start()
	{
		// Generate a maze
		maze = mazeGenerator.GenerateMaze();

		if (tileStart == null){
			Debug.Log ("tileStart is null before enemy generation is called!!!!!");
		}

		// Spawn enemies
		enemyList = UnitSpawner.SpawnInitEnemies(enemyManager.GenerateInitSpawnList() );

		// Spawn player character
		playerCharacter = UnitSpawner.SpawnPlayerCharacter(tileStart);

		// Set finsiedInit
		finsiedInit = true;
	}

	/* 
	//---------------------------------------
	//      Unit spawn functions
	//---------------------------------------

	public PlayerCharacter SpawnPlayerCharacter(GameObject prefab, Tile targetTile)
	{
		PlayerCharacter character = Instantiate (prefab, targetTile.transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<PlayerCharacter>();
		character.Init(this, targetTile);

		return character;
	}

	public void SpawnInitEnemies()
	{
		
	}
	*/


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