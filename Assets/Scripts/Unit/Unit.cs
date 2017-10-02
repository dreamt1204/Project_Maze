using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
	//=======================================
	//      Variables
	//=======================================
	public GameManager gameManager;

	protected float health = 100;
	protected float moveSpeed = 100;
	protected Tile currentTile;
	public bool isWalking = false;

	//---------------------------------------
	//      Properties
	//---------------------------------------
	public virtual float Health {get; set;}
	public virtual float MoveSpeed {get; set;}
	public virtual Tile CurrentTile {get; set;}

	//=======================================
	//      Functions
	//=======================================

	// Use this for initialization
	public virtual void Init (GameManager gm, Tile spawnTile)
	{
		gameManager = gm;
		CurrentTile = spawnTile;
	}

	//---------------------------------------
	//      Movement
	//---------------------------------------
	public void TryMoveToTile(Tile targetTile)
	{
		if (isWalking)
			return;

		MoveToTile (targetTile);
	}

	public void MoveToTile(Tile targetTile)
	{
		StartCoroutine ("MoveToTileCoroutine", targetTile);
	}

	IEnumerator MoveToTileCoroutine (Tile targetTile)
	{
		isWalking = true;
		Vector3 target = targetTile.gameObject.transform.position;

		while(Vector3.Distance(transform.position, target) > 0.05f)
		{
			transform.position = Vector3.Lerp(transform.position, target, (moveSpeed/10) * Time.deltaTime);

			yield return null;
		}

		CurrentTile = targetTile;
		isWalking = false;
	}
}
