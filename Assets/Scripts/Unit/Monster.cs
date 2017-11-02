using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DetectingState
{
    Idle,
    Warning,
    Alerted
}

public class Monster : Unit {
    //=======================================
    //      Variables
    //=======================================
    public List<MonsterBodyPartData> monsterBodyPartList;

    // Detection
	[HideInInspector] public Unit alertedTarget;
	public int detectionRange = 2;

    DetectingState detectingState_m = DetectingState.Idle;
    const float detectionCDTime_Warning = 10;
    const float detectionCDTime_Alerted = 5;

    AlertedIcon alertedIcon;

    // Stat
    public float moveSpeedIdle;
    public float moveSpeedWarning;
    public float moveSpeedAlerted;

    // Monster Behavior
    [HideInInspector] public MonsterBehaviour currentBehaviour;
    [HideInInspector] public Dictionary<DetectingState, List<MonsterBehaviour>> activeBehaviourList = new Dictionary<DetectingState, List<MonsterBehaviour>>();
	[HideInInspector] public List<MonsterBehaviour> passiveBehaviourList = new List<MonsterBehaviour>();
    public bool defaultIdleBehaviour = true;
    public bool defaultWarningBehaviour = true;
    public bool defaultAlertedBehaviour = true;

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
    public DetectingState detectingState
    {
        get
        {
            return detectingState_m;
        }
        set
        {
            detectingState_m = value;
            StartCoolDownDetectionLevel();
            UpdateAlertedTarget();
            alertedIcon.UpdateIconSprite(value);
            UpdateMoveSpeed();
        }
    }

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    public override void Init(Tile spawnTile)
    {
        InitMoveSpeed();
        InitAlertedIcon();
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
    }

    void InitAlertedIcon()
    {
        alertedIcon = ((GameObject)Instantiate(Resources.Load("Sprite_AlertedIcon"))).GetComponent<AlertedIcon>();
        alertedIcon.Init(this);
    }

	void InitMonsterBehaviours()
	{
		activeBehaviourList.Add(DetectingState.Idle, new List<MonsterBehaviour>());
		activeBehaviourList.Add(DetectingState.Warning, new List<MonsterBehaviour>());
		activeBehaviourList.Add(DetectingState.Alerted, new List<MonsterBehaviour>());

        // Init Default Behaviours
        if (defaultIdleBehaviour)
        {
            activeBehaviourList[DetectingState.Idle].Add(gameObject.AddComponent<MonsterPatrol>());
            activeBehaviourList[DetectingState.Idle][0].owner = this;
        }
        if (defaultWarningBehaviour)
        {
            activeBehaviourList[DetectingState.Warning].Add(gameObject.AddComponent<MonsterSearch>());
            activeBehaviourList[DetectingState.Warning][0].owner = this;
        }
        if (defaultAlertedBehaviour)
        {
            activeBehaviourList[DetectingState.Alerted].Add(gameObject.AddComponent<MonsterAttack>());
            activeBehaviourList[DetectingState.Alerted][0].owner = this;
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
		if (MazeUTL.CheckTargetInRangeAndDetectRegion (CurrentTile, level.playerCharacter.CurrentTile, detectionRange))
		{
			detectingState = DetectingState.Alerted;
		}   
    }

    void UpdateAlertedTarget()
    {
        if (detectingState == DetectingState.Idle)
        {
            alertedTarget = null;
        }
        else if (detectingState == DetectingState.Alerted)
        {
            alertedTarget = level.playerCharacter;
        }
    }

    void StartCoolDownDetectionLevel()
    {
        float time = 0;
        switch (detectingState)
        {
            case DetectingState.Alerted:
                time = detectionCDTime_Alerted;
                break;
            case DetectingState.Warning:
                time = detectionCDTime_Warning;
                break;
            default:
            case DetectingState.Idle:
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

        switch (detectingState)
        {
            case DetectingState.Alerted:
                detectingState = DetectingState.Warning;
                break;
            case DetectingState.Warning:
                detectingState = DetectingState.Idle;
                break;
            default:
            case DetectingState.Idle:
                break;
        }
    }

    void UpdateMoveSpeed()
    {
        switch (detectingState)
        {
            case DetectingState.Idle:
                MoveSpeed = moveSpeedIdle;
                break;
            case DetectingState.Warning:
                MoveSpeed = moveSpeedWarning;
                break;
            default:
            case DetectingState.Alerted:
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

        foreach (MonsterBehaviour behaviour in activeBehaviourList[detectingState])
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
