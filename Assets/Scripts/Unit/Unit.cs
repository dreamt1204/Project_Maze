//============================== Class Definition ==============================
// 
// This is the base class for Units (PlayerCharacter and Enemy).
// To spawn a unit, use UnitSpawner static functions.
//
//==============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public enum ActionType
{
    None,
    Walking,
}

public class Unit : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	protected LevelManager level;

	// Flags
	protected bool finishedInit;

    // Stat
	[SerializeField] protected float healthMax = 100;
	[SerializeField] protected float health = 100;
	[SerializeField] protected float moveSpeed = 100;

	// Tile
	protected Tile currentTile;

    // Action
    protected ActionType currentAction;

    // Body Part
    public BodyPartOwnerType ownerType;
    public List<BodyPartData> BodyParts;

    // Anim
    protected bool facingRight = false;
    protected string currentAnim;
    protected SkeletonAnimation skeletonAnim;
    private const float walkAnimScaleMultiplier = 3f;

    protected bool keepWalkingAnim = false;
    protected bool playWalkingAnim = false;

    // Const
    private const float movementMultiplier = 0.15f;

    //---------------------------------------
    //      Struct
    //---------------------------------------
    [Serializable]
    public struct BodyPartData
    {
        public string partType;
        public BodyPart part;
    }

    //---------------------------------------
    //      Properties
    //---------------------------------------
    public virtual float Health
	{
		get
		{
			return health;
		}
		set
		{
			health = Mathf.Clamp (value, 0, healthMax);
		}
	}
	public virtual float MoveSpeed
	{
		get
		{
			return moveSpeed;
		}
		set
		{
			moveSpeed = value;
		}
	}
    public virtual Tile CurrentTile
    {
        get
        {
            return currentTile;
        }
        set
        {
            currentTile = value;
			currentTile.TileAction ();
			UnitTileAction (currentTile);
        }
    }
    public virtual ActionType CurrentAction
    {
        get
        {
			return currentAction;
        }
        set
        {
            currentAction = value;
        }
    }

    //=======================================
    //      Functions
    //=======================================
    void Start()
    {
        skeletonAnim = GetComponentInChildren<SkeletonAnimation>();
        InitBodyParts();
        UpdateBody();
    }

    public virtual void Init (Tile spawnTile)
	{
        level = LevelManager.instance;
        CurrentTile = spawnTile;

		finishedInit = true;
    }

    public virtual void Update()
    {
		if (!finishedInit)
			return;

		if (playWalkingAnim)
            PlayLoopAnim("Walk");
        else
            PlayLoopAnim("Idle");
    }

    //---------------------------------------
    //      Action State
    //---------------------------------------
    public bool isAvailable()
    {
        return (CurrentAction == ActionType.None);
    }

    //---------------------------------------
    //      Body Part
    //---------------------------------------
    public BodyPart GetBodyPartWithType(string partType)
    {
        BodyPart part = null;

        foreach (BodyPartData data in BodyParts)
        {
            if (data.partType == partType)
            {
                part = data.part;
            }
        }

        return part;
    }

    public int GetBodyPartIDWithType(string partType)
    {
        int id = -1;

        for (int i = 0; i < BodyParts.Count; i++)
        {
            if (BodyParts[i].partType == partType)
            {
                id = i;
                break;
            }
        }

        return id;
    }

    public virtual void InitBodyParts()
    {
        // Child unit class can force init BodyParts or specific stuff here
    }

    public void InitBodyPartData(string partType)
    {
        foreach (BodyPartData data in BodyParts)
        {
            if (data.partType == partType)
                return;
        }

        BodyPartData newData = new BodyPartData();
        newData.partType = partType;
        newData.part = null;

        BodyParts.Add(newData);
    }

    public void UpdateBodyPart(BodyPart newPart)
    {
        if (newPart.ownerType != this.ownerType) return;

        int id = GetBodyPartIDWithType(newPart.partType);
        if (id == -1) return;

        BodyPartData newData = BodyParts[id];
        newData.part = newPart;
        BodyParts[id] = newData;

        UpdateBody();
    }

    public void RemoveBodyPart(string partType)
    {
        int id = GetBodyPartIDWithType(partType);
        if (id == -1) return;

        BodyPartData newData = BodyParts[id];
        newData.part = null;
        BodyParts[id] = newData;

        UpdateBody();
    }

    public void UpdateBody()
    {
        Skin newBody = new Skin("");
        SkeletonData skeletonData = skeletonAnim.skeletonDataAsset.GetSkeletonData(false);

        foreach (BodyPartData data in BodyParts)
        {
            // Assign body part via name
            if ((data.part != null) && (data.part.partType == data.partType))
            {
                AddSkinEntries(skeletonData.FindSkin(data.part.partSkinName), newBody);
            }
            // Try assign the default body part
            else
            {
                Skin defaultPart = skeletonData.FindSkin(data.partType + "_default");
                if (defaultPart != null)
                    AddSkinEntries(defaultPart, newBody);
            }

            BodyPartUpdatedEvent(data.partType);
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

    public virtual void BodyPartUpdatedEvent(string partType)
    {
        // Child unit class can update specific BodyPart stuff here
    }

	//---------------------------------------
	//      Anim
	//---------------------------------------
	public void PlayLoopAnim(string animName)
	{
		if (currentAnim == animName) return;

		currentAnim = animName;
		skeletonAnim.state.SetAnimation(0, animName, true);
	}

	public void StopKeepWalkingAnim()
	{
		if (keepWalkingAnim)
			keepWalkingAnim = false;
	}

	//---------------------------------------
	//      Tile Action
	//---------------------------------------
	public virtual void UnitTileAction(Tile tile)
	{
		// Apply effect when unit steps on this tile
	}

    //---------------------------------------
    //      Movement
    //---------------------------------------
    public void TryMoveToTile(Tile targetTile)
	{
        if (!isAvailable())
            return;

		if (targetTile == CurrentTile)
			return;

		if ((targetTile == null) || (MazeUTL.WallBetweenNeighborTiles(currentTile, targetTile)))
        {
            playWalkingAnim = false;
            return;
        }

        MoveToTile (targetTile);
	}

    public void TryMoveToDirTile(int dir)
    {
        TryMoveToTile(MazeUTL.GetDirNeighborTile(currentTile, dir));
    }

    public void MoveToTile(Tile targetTile)
	{
        TryTurn(targetTile);
        StartCoroutine ("MoveToTileCoroutine", targetTile);
	}

    public void TryTurn(Tile targetTile)
    {
        int dir = MazeUTL.GetNeighborTileDir(currentTile, targetTile);
        facingRight = dir == 1 ? false : dir == 3 ? true : facingRight;

        skeletonAnim.skeleton.FlipX = facingRight;
    }

	IEnumerator MoveToTileCoroutine (Tile targetTile)
	{
        CurrentAction = ActionType.Walking;
        keepWalkingAnim = true;
        playWalkingAnim = true;

        // Update anim play speed
        float originalTimeScale = skeletonAnim.timeScale;
        skeletonAnim.timeScale = moveSpeed * 0.01f * walkAnimScaleMultiplier;
        
        Vector3 target = targetTile.gameObject.transform.position;
        while (Vector3.Distance(transform.position, target) > 0.25f)
        {
			transform.Translate((target - transform.position).normalized * Time.deltaTime * moveSpeed * movementMultiplier);

            yield return null;
		}
        transform.position = target;
        CurrentTile = targetTile;

        // Reset unit state
        skeletonAnim.timeScale = originalTimeScale;
        CurrentAction = ActionType.None;

        if (!keepWalkingAnim)
            playWalkingAnim = false;
    }

	//---------------------------------------
	//      Action
	//---------------------------------------
	public virtual void RecieveDamage(float amount)
	{
		Health = (Health - amount);
	}

	public virtual void RestoreHealth(float amount)
	{
		Health = (Health + amount);
	}

	public virtual void ApplyDamageToTarget(Unit target, float amount)
	{
		target.RecieveDamage (amount);
	}
}
