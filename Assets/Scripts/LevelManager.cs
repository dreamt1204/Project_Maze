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
    public MazeGenerator mazeGenerator;

    // Global variables
    [HideInInspector] public bool finishedInit = false;
    [HideInInspector] public Maze maze;
    [HideInInspector] public Tile tileStart;
    [HideInInspector] public Tile tileObjective;
    [HideInInspector] public float timer;
    [HideInInspector] public PlayerCharacter playerCharacter;

    // Global variables in Inspector
    [Header("Maze")]
    [Range(1, 10)]
    public int mazeDifficulty = 1;
    public MazeSetting mazeSetting;

    [Header("Custom Maze (Optional)")]
    public bool customMazeSize;
    public int width = 10;
    public int length = 10;
    [Space(15)]
    public GameObject customMazeObject;

    [Header("Game Mode")]
    public float timerStart = 3600;

    [Header("Required Prefabs")]
    public GameObject playerCharacterPrefab;
    public GameObject startPointPrefab;
    public GameObject objectivePrefab;
    public GameObject HealthPackPrefab;
    public GameObject BodyPartChestPrefab;
    public GameObject CompassPrefab;

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

        Utilities.TryCatchError((mazeSetting == null), "Maze Setting cannot be null");
    }

    void Start()
    {
        StartCoroutine("InitLevel");
    }

    void Update()
    {
        if (!finishedInit)
            return;

        // Update timer
        UpdateTimer();
    }

    #region Initialization
    //---------------------------------------
    //      Initialization
    //---------------------------------------
    IEnumerator InitLevel()
    {
        InitMaze();
        InitMonster();
        InitPlayerCharacter();
        InitGameMode();

        finishedInit = true;
        yield return null;
    }

    void InitMaze()
    {
        maze = mazeGenerator.GenerateMaze();
    }

    void InitMonster()
    {
        UnitSpawner.SpawnMonsters();
    }

    void InitPlayerCharacter()
    {
        playerCharacter = UnitSpawner.SpawnPlayerCharacter(tileStart);
    }

    void InitGameMode()
    {
        timer = timerStart;
    }
    #endregion

    #region Game Mode
    //---------------------------------------
    //      Game Mode
    //---------------------------------------
    void UpdateTimer()
    {
        if (timer > 0)
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, timerStart);
    }

    public void CheckLevelFailedCondition()
	{
		if (!finishedInit)
			return;

		if (playerCharacter.Health > 0)
			return;

		FailLevel ();
	}

	public bool CheckLevelPassedCondition()
	{
		if (!finishedInit)
			return false;

		if (!playerCharacter.hasObjective)
			return false;

		PassLevel ();

        return true;
	}

	void PassLevel()
	{
		Debug.Log("Level Finished!!");  // Debug: tmp message 
        Debug.Log("Time used: " + UIManager.GetTimerText(timerStart - timer));
    }

	void FailLevel()
	{
		Debug.Log("Level Failed...");  // Debug: tmp message 
	}
    #endregion
}