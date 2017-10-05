using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Unit : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    protected LevelManager levelManager;

    [SerializeField]
    protected float health = 100;
    [SerializeField]
    protected float moveSpeed = 100;
	private const float movementMultiplier = 0.15f;
	protected Tile currentTile;

	protected bool isWalking = false;
    protected bool KeepWalking = false;
    protected bool ArrivedNextTile = false;

    protected bool facingRight = false;

    protected string currentAnim;
    protected SkeletonAnimation skeletonAnim;
    private const float walkAnimScaleMultiplier = 3f;

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
    void Start()
    {
        skeletonAnim = GetComponentInChildren<SkeletonAnimation>();
    }

    public virtual void Init (LevelManager lm, Tile spawnTile)
	{
		levelManager = lm;
		CurrentTile = spawnTile;
	}

    public virtual void Update()
    {
        if (isWalking)
            PlayLoopAnim("Walk");
        else
            PlayLoopAnim("Idle");
    }

    public void PlayLoopAnim(string animName)
    {
        if (currentAnim == animName) return;

        currentAnim = animName;
        skeletonAnim.state.SetAnimation(0, animName, true);
    }

	//---------------------------------------
	//      Movement
	//---------------------------------------
	public void TryMoveToTile(Tile targetTile)
	{
		if (isWalking && !KeepWalking)
			return;

		if (targetTile == CurrentTile)
			return;

		MoveToTile (targetTile);
	}

	public void MoveToTile(Tile targetTile)
	{
        TryTurn(targetTile);
        StartCoroutine ("MoveToTileCoroutine", targetTile);
	}

    public void TryTurn(Tile targetTile)
    {
        int dir = levelManager.maze.GetNeighborTileDir(currentTile, targetTile);
        facingRight = dir == 1 ? false : dir == 3 ? true : facingRight;

        skeletonAnim.skeleton.FlipX = facingRight;
    }

	IEnumerator MoveToTileCoroutine (Tile targetTile)
	{
        ArrivedNextTile = false;
        isWalking = true;
        Vector3 target = targetTile.gameObject.transform.position;

        skeletonAnim.timeScale = moveSpeed * 0.01f * walkAnimScaleMultiplier;

        while (Vector3.Distance(transform.position, target) > 0.25f)
		{
			transform.Translate((target - transform.position).normalized * Time.deltaTime * moveSpeed * movementMultiplier);

            yield return null;
		}
        transform.position = target;
        CurrentTile = targetTile;

        if (!KeepWalking)
            isWalking = false;

        ArrivedNextTile = true;
    }
}
