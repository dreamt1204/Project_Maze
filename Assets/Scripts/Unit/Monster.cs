﻿using System.Collections;
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
	public Unit alertedTarget;
	public int detectionRange = 3;

    DetectingState detectingState_m = DetectingState.Idle;
    const float detectionCDTime_Warning = 10;
    const float detectionCDTime_Alerted = 5;

    // Monster Behavior
    [HideInInspector] public MonsterBehaviour currentBehaviour;
	public Dictionary<DetectingState, List<MonsterBehaviour>> behaviourList_active = new Dictionary<DetectingState, List<MonsterBehaviour>>();
	public List<MonsterBehaviour> behaviourList_passive = new List<MonsterBehaviour>();

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
        }
    }

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    public override void Init(Tile spawnTile)
    {
		InitMonsterBehaviours();

        base.Init(spawnTile);
    }

	void InitMonsterBehaviours()
	{
		behaviourList_active.Add(DetectingState.Idle, new List<MonsterBehaviour>());
		behaviourList_active.Add(DetectingState.Warning, new List<MonsterBehaviour>());
		behaviourList_active.Add(DetectingState.Alerted, new List<MonsterBehaviour>());

		// Init Basic Behaviours
		behaviourList_active[DetectingState.Idle].Add(gameObject.AddComponent<MonsterPatrol>());
		behaviourList_active [DetectingState.Idle] [0].owner = this;
		behaviourList_active[DetectingState.Warning].Add(gameObject.AddComponent<MonsterSearch>());
		behaviourList_active [DetectingState.Warning] [0].owner = this;
		behaviourList_active[DetectingState.Alerted].Add(gameObject.AddComponent<MonsterAttack>());
		behaviourList_active [DetectingState.Alerted] [0].owner = this;

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
			behaviourList_active [data.part.monsterBehaviour.detectingStateType].Add (partBehaviour);

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

    //---------------------------------------
    //      Monster Behaviour Logic
    //---------------------------------------
    void UpdateMonsterBehaviour()
    {
        if (currentBehaviour != null)
            return;
        
        currentBehaviour = PickMonsterBehaviour();
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

        foreach (MonsterBehaviour behaviour in behaviourList_active[detectingState])
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
