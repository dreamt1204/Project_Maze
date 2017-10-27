using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Unit {
    //=======================================
    //      Variables
    //=======================================
    public List<MonsterBodyPartData> monsterBodyPartList;

    [System.Serializable]
    public struct MonsterBodyPartData
    {
        public string partType;
        public List<BodyPart> partList;
    }

    //=======================================
    //      Functions
    //=======================================
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
