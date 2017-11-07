using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlimeStateType
{
    Eatting,
    Splitting
}

public class Slime : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    PlayerCharacter myPlayer;
    SlimeStateType slimeState_m;

    public string slimeSkinName;
    public SlimeSplit slimeSplit;

    //---------------------------------------
    //      Properties
    //---------------------------------------
    public SlimeStateType slimeState
    {
        get
        {
            return slimeState_m;
        }
        set
        {
            slimeState_m = value;
            TryUpdateSlimeSplit();
        }
    }

    //=======================================
    //      Functions
    //=======================================
    // Use this for initialization
    void Start () {
		
	}

    public void Init(PlayerCharacter player)
    {
        myPlayer = player;

        GameObject.Find("SlimeButton").GetComponent<UISlimeButton>().currentSlime = this;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //---------------------------------------
    //      Slime Split/Eat Ability
    //---------------------------------------
    public void TryUpdateSlimeSplit()
    {
        if (slimeState == SlimeStateType.Splitting)
        {
            if (myPlayer.CurrentTile.slimeSplit != null)
                return;

            if ((myPlayer.Health - slimeSplit.splittingSlimeDamage) <= 0)
                return;

            GenerateSlimeSplit();
        }
        else if (slimeState == SlimeStateType.Eatting)
        {
            if (myPlayer.CurrentTile.slimeSplit == null)
                return;

            EatSlimeSplit();
        }
    }

    public void GenerateSlimeSplit()
    {
        myPlayer.RecieveDamage(slimeSplit.splittingSlimeDamage);

        myPlayer.CurrentTile.SpawnTileSlimeSplit(slimeSplit.gameObject);
    }

    public void EatSlimeSplit()
    {
        myPlayer.RestoreHealth(myPlayer.CurrentTile.slimeSplit.eattingSlimeRecover);

        myPlayer.CurrentTile.DestroyTileSlimeSplit();
    }
}
