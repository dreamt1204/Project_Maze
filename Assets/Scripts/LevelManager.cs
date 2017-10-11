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

    #region Inspector
    [Header("Player Character")]
    public GameObject playerCharacterPrefab;

<<<<<<< HEAD
	private EnemyManager enemyManager;

	// Global variables
	[HideInInspector]
	public Maze maze;
	[HideInInspector]
	public PlayerCharacter playerCharacter;
	[HideInInspector]
	public Enemy[] enemy;
    [HideInInspector]
	public Tile startTile;  // JY added public accessibility; enemyManager need this info to avoid spawning around start tile
=======
    [Header("Maze")]
    public int mazeWidth = 10;
    public int mazeLength = 10;
    public MazeSetting mazeSetting;

    [Header("Game Mode")]
    public GameObject startPointPrefab;
    public GameObject levelObjectivePrefab;

    [Header("Items")]
    public bool useItemGenerateLogic;
    public int numberOfItems;
    #endregion

    // Global variables
    [HideInInspector]
	public Maze maze;
    [HideInInspector]
    public Tile tileStart;
>>>>>>> master
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
<<<<<<< HEAD
			Destroy(gameObject);    

		// Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad(gameObject);

		// Get component reference to the attached script
		mazeGenerator = GetComponent<MazeGenerator>();

		enemyManager = GetComponent<EnemyManager> ();

		// Start init the game
		InitGame();
=======
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
>>>>>>> master
    }

    // Game init function
	void InitGame()
    {
        // Generate a maze
        maze = mazeGenerator.GenerateMaze();

		// Spawn enemies
<<<<<<< HEAD
		SpawnInitEnemies();
=======
		// (WIP......)
>>>>>>> master

		// Spawn player character
		playerCharacter = unitSpawner.SpawnPlayerCharacter(tileStart);

        // Set finsiedInit
        finsiedInit = true;
    }

<<<<<<< HEAD
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
		playerCharacter = SpawnPlayerCharacter(playerCharacterPrefab, startTile);
	}

	public PlayerCharacter SpawnPlayerCharacter(GameObject prefab, Tile targetTile)
	{
		PlayerCharacter character = Instantiate (prefab, targetTile.transform.position, Quaternion.Euler(0, 0, 0)).GetComponent<PlayerCharacter>();
		character.Init(this, targetTile);

		return character;
	}

	public void SpawnInitEnemies()
	{
		enemyManager.SpawnInitEnemies ();
	}

=======
>>>>>>> master
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
