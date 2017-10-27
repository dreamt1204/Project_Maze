//============================== Class Definition ==============================
// 
// This container class store data for certain body part.
// Have to create prefab with this attached for future use.
//
//==============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyPartOwnerType
{
    PlayerCharacter,
    TestMonster
}

//---------------------------------------
//      Struct
//---------------------------------------
[System.Serializable]
public struct BodyPartData
{
    public string partType;
    public bool mustHave;
    public BodyPart part;
}

public class BodyPart : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    public BodyPartOwnerType ownerType;
    public string partName;
    public string partType;
    public string partSkinName;
    public PlayerAbility playerAbility;

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
