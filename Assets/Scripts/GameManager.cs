using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    public Maze maze;
    private MazeGenerator maze_generator;

	public GameObject playerCharacterObject;

	[HideInInspector]
	public PlayerCharacter playerCharacter;

    //=======================================
    //      Functions
    //=======================================   
    // Use this for initialization
    void Awake()
    {
        maze_generator = this.gameObject.GetComponent<MazeGenerator>();
    }

    // Use this for initialization
    void Start()
    {
        // Generate a maze
        StartCoroutine("BuildMazeCoroutine");

		// Spawn player character
		SpawnPlayerCharacter();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator BuildMazeCoroutine()
    {
        maze = maze_generator.BuildMaze();

        yield return null;
    }

	void SpawnPlayerCharacter()
	{
		playerCharacter = Instantiate (playerCharacterObject, new Vector3 (0, 0, 0), Quaternion.Euler(0, 0, 0)).GetComponent<PlayerCharacter>();
		GameObject camera_obj = (GameObject)Instantiate (Resources.Load ("PlayerCamera"));
		camera_obj.transform.parent = playerCharacter.gameObject.transform;

		playerCharacter.GM = this;
		playerCharacter.current_tile = maze.tile [0, 0];
	}
}
