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
    DetectingState detectingState_m = DetectingState.Idle;
    int detectionRange = 3;

    // Monster Behavior
    public MonsterBehaviour currentBehaviour;
    public Dictionary<DetectingState, List<MonsterBehaviour>> behaviourList;

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
        }
    }

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    public override void Init(Tile spawnTile)
    {
        behaviourList.Add(DetectingState.Idle, new List<MonsterBehaviour>());
        behaviourList.Add(DetectingState.Warning, new List<MonsterBehaviour>());
        behaviourList.Add(DetectingState.Alerted, new List<MonsterBehaviour>());

        behaviourList[DetectingState.Idle].Add(new BasicMonsterPatrol());
        behaviourList[DetectingState.Alerted].Add(new BasicMonsterAttack());

        base.Init(spawnTile);
    }

    public override void Update()
    {
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
        if (!IsPlayerInDetectionRange())
            return;

        if (!IsPlayerInSameDetectionRegion())
            return;

        detectingState = DetectingState.Alerted;
    }

    bool IsPlayerInDetectionRange()
    {
        return MazeUTL.CheckTargetInRange(CurrentTile, level.playerCharacter.CurrentTile, detectionRange);
    }

    bool IsPlayerInSameDetectionRegion()
    {
        // (WOM)
        return true;
    }

    //---------------------------------------
    //      Monster Behaviour Logic
    //---------------------------------------
    void UpdateMonsterBehaviour()
    {
        if (currentBehaviour != null)
            return;

        currentBehaviour = PickMonsterBehaviour();
        currentBehaviour.ExecuteActiveBehaviour();
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

        foreach (MonsterBehaviour behaviour in behaviourList[detectingState])
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
