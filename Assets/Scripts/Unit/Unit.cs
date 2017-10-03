using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    protected LevelManager levelManager;

    [SerializeField]
    protected float health = 100;
    [SerializeField]
    protected float moveSpeed = 100;
	private float movementMultiplier = 0.15f;
	protected Tile currentTile;
	protected bool isWalking = false;

	//---------------------------------------
	//      Properties
	//---------------------------------------
	public virtual float Health {get; set;}
	public virtual float MoveSpeed {get; set;}
    public virtual Tile CurrentTile
    {
        get
        {
            return currentTile;
        }
        set
        {
            currentTile = value;
            currentTile.CheckTileAction();
        }
    }

    //=======================================
    //      Functions
    //=======================================

    // Use this for initialization
    public virtual void Init (LevelManager lm, Tile spawnTile)
	{
		levelManager = lm;
		CurrentTile = spawnTile;
	}

	//---------------------------------------
	//      Movement
	//---------------------------------------
	public void TryMoveToTile(Tile targetTile)
	{
		if (isWalking)
			return;

		if (targetTile == CurrentTile)
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

		while(Vector3.Distance(transform.position, target) > 0.25f)
		{
			transform.Translate((target - transform.position).normalized * Time.deltaTime * moveSpeed * movementMultiplier);

            yield return null;
		}
        transform.position = target;
        CurrentTile = targetTile;
        isWalking = false;
	}
}
