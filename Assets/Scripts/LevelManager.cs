using System.Collections;
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
	private EnemyManager enemyManager;

    #region Inspector
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

<<<<<<< HEAD
    [Header("Items")]
    public bool useItemGenerateLogic;
    public int numberOfItems;

	[Header("Enemy Spawning")]
	public InitSpawnMethod spawnMethod;
	public int spawnQuantity;
	public int safeRadius;
=======
    [Header("Required Prefabs")]
    public GameObject playerCharacterPrefab;
    public GameObject startPointPrefab;
    public GameObject objectivePrefab;
    public GameObject BodyPartChestPrefab;
>>>>>>> Henry_Ability
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
	[HideInInspector]
	public List<Enemy> enemies;

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
        mazeGenerator = new MazeGenerator();
        unitSpawner = new UnitSpawner();
		enemyManager = new EnemyManager
			(
				spawnMethod,
				spawnQuantity,
				safeRadius
			);

        // Start init the game
        InitGame();
    }

    // Game init function
	void InitGame()
    {
        // Generate a maze
        maze = mazeGenerator.GenerateMaze();

		// Spawn enemies
		enemies = enemyManager.SpawnInitEnemies ();

		// Spawn player character
		playerCharacter = unitSpawner.SpawnPlayerCharacter(tileStart);

        // Set finsiedInit
        finsiedInit = true;
    }

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
