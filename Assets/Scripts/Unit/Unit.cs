using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
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

    [Serializable]
    public struct BodyPartStruct
    {
        public string type;
        public BodyPart part;
    }

    public BodyPartStruct[] BodyPart;

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

        UpdateBody();
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
    //      Body Part
    //---------------------------------------
    void UpdateBody()
    {
        Skin newBody = new Skin("");
        SkeletonData skeletonData = skeletonAnim.skeletonDataAsset.GetSkeletonData(false);

        foreach (BodyPartStruct data in BodyPart)
        {
            // Assign body part via name
            if ((data.part != null) && (data.part.partType == data.type))
                AddSkinEntries(skeletonData.FindSkin(data.part.partName), newBody);
            // Try assign the default body part
            else
            {
                Skin defaultPart = skeletonData.FindSkin(data.type + "_default");
                if (defaultPart != null)
                    AddSkinEntries(defaultPart, newBody);
            }
        }

        // Now that your custom generated skin is complete, assign it to the skeleton...
        var skeleton = skeletonAnim.skeleton;
        skeleton.SetSkin(newBody);

        // ...and make sure the changed attachments are visually applied.
        skeleton.SetSlotsToSetupPose();
        skeletonAnim.state.Apply(skeleton);
    }

    static void AddSkinEntries(Skin source, Skin destination)
    {
        var sourceAttachments = source.Attachments;
        var destinationAttachments = destination.Attachments;
        foreach (var m in sourceAttachments)
            destinationAttachments[m.Key] = m.Value;
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
