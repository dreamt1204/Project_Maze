using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	public static GameManager instance = null;

	// Preset prefab
	public GameObject playerCharacterPrefab;

	// Helper class that attached to the GM object
	private MazeGenerator mazeGenerator;

	// Global variables
	[HideInInspector]
	public Maze maze;
	[HideInInspector]
	public PlayerCharacter playerCharacter;

    //=======================================
    //      Functions
    //=======================================   
    // Use this for initialization
    void Awake()
    {
		// This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
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

		// Spawn enemies
		// .....

		// Spawn player character
		SpawnStartingCharacter();

		// Spawn game mode object
		// .....
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
}
