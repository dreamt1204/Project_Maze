using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    public Maze maze;
    private MazeGenerator maze_generator;

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
    }

    // Update is called once per frame
    void Update()
    {
		// TEST branch
    }

    IEnumerator BuildMazeCoroutine()
    {
        maze = maze_generator.BuildMaze();

        yield return null;
    }
}
