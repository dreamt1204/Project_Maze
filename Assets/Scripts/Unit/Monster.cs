using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DetectState
{
    Idle,
    Warning,
    Alerted
}

public class Monster : Unit {
    //=======================================
    //      Variables
    //=======================================
	// Body Part List
    public List<MonsterBodyPartData> monsterBodyPartList;

    // Detection
	DetectState detectState_m = DetectState.Idle;
	[HideInInspector] public Unit alertedTarget;
	UIWorldSprite detectStateIcon;

	[Header("Detection")]
	public int detectRange = 2;

	[Header("Default Idle")]
	public float moveSpeedIdle;
	public bool defaultPatrol = true;

	[Header("Default Warning")]
	public float moveSpeedWarning;
	public float WarningCDTime = 10;
	public bool defaultSearch = true;

	[Header("Default Alerted")]
	public float moveSpeedAlerted;
	public float AlertedCDTime = 10;
	public bool defaultAttack = true;

    // Monster Behavior
    [HideInInspector] public MonsterBehaviour currentBehaviour;
    [HideInInspector] public Dictionary<DetectState, List<MonsterBehaviour>> activeBehaviourList = new Dictionary<DetectState, List<MonsterBehaviour>>();
	[HideInInspector] public List<MonsterBehaviour> passiveBehaviourList = new List<MonsterBehaviour>();

    //---------------------------------------
    //      Struct
    //---------------------------------------
    [System.Serializable]
    public struct MonsterBodyPartData
    {
        public string partType;
        public List<BodyPart> partList;
    }

    //---------------------------------------
    //      Properties
    //---------------------------------------
    public DetectState detectState
    {
        get
        {
            return detectState_m;
        }
        set
        {
            detectState_m = value;
            StartCoolDownDetectionLevel();
            UpdateAlertedTarget();
			UpdateDetectStateIcon(value);
			UpdateMoveSpeed(value);
        }
    }

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    public override void Init(Tile spawnTile)
    {
        InitMoveSpeed();
		InitDetectStateIcon();
        InitMonsterBehaviours();

        base.Init(spawnTile);
    }

    void InitMoveSpeed()
    {
        if (moveSpeedIdle <= 0)
            moveSpeedIdle = MoveSpeed * 0.4f;
        if (moveSpeedWarning <= 0)
            moveSpeedWarning = MoveSpeed * 0.7f;

        moveSpeedAlerted = MoveSpeed;

		UpdateMoveSpeed(detectState);
    }

	void InitDetectStateIcon()
    {
		detectStateIcon = transform.Find("DetectStateHUD").GetComponent<UIWorldSprite>();
		detectStateIcon.Init();
		UpdateDetectStateIcon(detectState);
    }

	void InitMonsterBehaviours()
	{
		activeBehaviourList.Add(DetectState.Idle, new List<MonsterBehaviour>());
		activeBehaviourList.Add(DetectState.Warning, new List<MonsterBehaviour>());
		activeBehaviourList.Add(DetectState.Alerted, new List<MonsterBehaviour>());

        // Init Default Behaviours
        if (defaultPatrol)
        {
            activeBehaviourList[DetectState.Idle].Add(gameObject.AddComponent<MonsterPatrol>());
            activeBehaviourList[DetectState.Idle][0].owner = this;
        }
        if (defaultSearch)
        {
            activeBehaviourList[DetectState.Warning].Add(gameObject.AddComponent<MonsterSearch>());
            activeBehaviourList[DetectState.Warning][0].owner = this;
        }
        if (defaultAttack)
        {
            activeBehaviourList[DetectState.Alerted].Add(gameObject.AddComponent<MonsterAttack>());
            activeBehaviourList[DetectState.Alerted][0].owner = this;
        }

		// Init Body Part Behaviours
		foreach (BodyPartData data in BodyParts)
		{
			if (data.part == null)
				continue;
			if (data.part.monsterBehaviour == null)
				continue;


			System.Type behaviourScriptType = System.Type.GetType (data.part.monsterBehaviour.name);
			MonsterBehaviour partBehaviour = (MonsterBehaviour)gameObject.AddComponent(behaviourScriptType);
			partBehaviour.owner = this;
			activeBehaviourList [data.part.monsterBehaviour.detectingStateType].Add (partBehaviour);

		}
    }

    public override void Update()
    {
        if (!finishedInit)
            return;

        base.Update();

        // If player gets caught by this Monster, change its detecting state to "Alerted".
        UpdateDetectingState();

        // If this Monster doesn't have any current behaviour, pick one and execute the behaviour.
        UpdateMonsterBehaviour();
    }

    //---------------------------------------
    //      Detection Logic
    //---------------------------------------
    void UpdateDetectingState()
    {
		if (MazeUTL.CheckTargetInRangeAndDetectRegion (CurrentTile, level.playerCharacter.CurrentTile, detectRange))
		{
			detectState = DetectState.Alerted;
		}   
    }

	public void UpdateDetectStateIcon(DetectState state)
	{
		if (state == DetectState.Warning)
		{
			detectStateIcon.UpdateSprite("Icon-Warning");
			detectStateIcon.UpdateSpriteAlpha(1);
		}
		else if (state == DetectState.Alerted)
		{
			detectStateIcon.UpdateSprite("Icon-Alerted");
			detectStateIcon.UpdateSpriteAlpha(1);
		}
		else
		{
			detectStateIcon.UpdateSpriteAlpha(0);
		}
	}

    void UpdateAlertedTarget()
    {
        if (detectState == DetectState.Idle)
        {
            alertedTarget = null;
        }
        else if (detectState == DetectState.Alerted)
        {
            alertedTarget = level.playerCharacter;
        }
    }

    void StartCoolDownDetectionLevel()
    {
        float time = 0;
        switch (detectState)
        {
            case DetectState.Alerted:
                time = AlertedCDTime;
                break;
            case DetectState.Warning:
                time = WarningCDTime;
                break;
            default:
            case DetectState.Idle:
                return;
        }

        StopCoroutine("CoolDownDetectionLevel");
        StartCoroutine("CoolDownDetectionLevel", time);
    }

    IEnumerator CoolDownDetectionLevel(float time)
    {
        float timer = time;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        switch (detectState)
        {
            case DetectState.Alerted:
                detectState = DetectState.Warning;
                break;
            case DetectState.Warning:
                detectState = DetectState.Idle;
                break;
            default:
            case DetectState.Idle:
                break;
        }
    }

	void UpdateMoveSpeed(DetectState state)
    {
		switch (state)
        {
            case DetectState.Idle:
                MoveSpeed = moveSpeedIdle;
                break;
            case DetectState.Warning:
                MoveSpeed = moveSpeedWarning;
                break;
            default:
            case DetectState.Alerted:
                MoveSpeed = moveSpeedAlerted;
                break;
        }
    }

    //---------------------------------------
    //      Monster Behaviour Logic
    //---------------------------------------
    void UpdateMonsterBehaviour()
    {
        if (currentBehaviour != null)
            return;

        currentBehaviour = PickMonsterBehaviour();

        if (currentBehaviour != null)
            currentBehaviour.StartActiveBehaviour();
    }

    MonsterBehaviour PickMonsterBehaviour()
    {
        List<MonsterBehaviour> tmpList = GetHighPriBehaviours();

        return tmpList[Random.Range(0, tmpList.Count)];
    }

    List<MonsterBehaviour> GetHighPriBehaviours()
    {
        List<MonsterBehaviour> tmpList = new List<MonsterBehaviour>();
        int hightestPri = 5;

        foreach (MonsterBehaviour behaviour in activeBehaviourList[detectState])
        {
			if (behaviour.isCoolingDown)
                continue;

            if (behaviour.behaviourPriority < hightestPri)
            {
                hightestPri = behaviour.behaviourPriority;
                tmpList.Clear();
            }

            if (behaviour.behaviourPriority == hightestPri)
                tmpList.Add(behaviour);
        }

        return tmpList;
    }

    //---------------------------------------
    //      Body Part
    //---------------------------------------
    public override void AssignMustHaveBodyParts()
    {
        List<string> mustHaveTypes = new List<string>();

        foreach (BodyPartData data in BodyParts)
        {
            if (data.mustHave)
                mustHaveTypes.Add(data.partType);
        }

        foreach (string type in mustHaveTypes)
        {
            AssignBodyPart(GetRandomMonsterBodyPart(type));
        }
    }

    BodyPart GetRandomMonsterBodyPart(string partType)
    {
        List<BodyPart> list = new List<BodyPart>();

        foreach (MonsterBodyPartData data in monsterBodyPartList)
        {
            if (data.partType == partType)
            {
                list = data.partList;
                break;
            }
        }

        Utilities.TryCatchError((list.Count == 0), "Can't find Body Part Type '" + partType + "' in Monster Body Part List.");

        return list[Random.Range(0, list.Count)];
    }
}
