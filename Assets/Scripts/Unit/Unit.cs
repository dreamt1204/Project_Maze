using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
	//=======================================
	//      Variables
	//=======================================
	public GameManager GM;

	public float health = 100;
	public float move_speed = 100;
	public Tile current_tile;
	public bool isWalking = false;

	//=======================================
	//      Functions
	//=======================================

	// Use this for initialization
	void Start ()
	{
		
	}

	// Update is called once per frame
	void Update ()
	{

	}

	public void TryMoveToNeighborTile(Tile target_tile)
	{
		if (isWalking)
			return;

		if (!GM.maze.TilesAreNeighbors (current_tile, target_tile))
			return;

		if (GM.maze.WallInBetween (current_tile, target_tile))
			return;
		
		MoveToTile(target_tile);
	}

	public void MoveToTile(Tile target_tile)
	{
		StartCoroutine ("MoveToTileCoroutine", target_tile);
	}

	IEnumerator MoveToTileCoroutine (Tile target_tile)
	{
		isWalking = true;
		Vector3 target = target_tile.gameObject.transform.position;

		while(Vector3.Distance(transform.position, target) > 0.05f)
		{
			transform.position = Vector3.Lerp(transform.position, target, (move_speed/10) * Time.deltaTime);

			yield return null;
		}

		current_tile = target_tile;
		isWalking = false;
	}
}
