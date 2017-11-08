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
	[Range(1, 5)] public int slimeLevel;
	public SlimeData slimeData;

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
	public void UpdateSlimeData(PlayerCharacter player, SlimeData data)
    {
		myPlayer = player;
		slimeData = data;
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

			if ((myPlayer.Health - slimeData.slimeSplit.splittingSlimeDamage) <= 0)
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
		myPlayer.RecieveDamage(slimeData.slimeSplit.splittingSlimeDamage);

		myPlayer.CurrentTile.SpawnTileSlimeSplit(slimeData.slimeSplit.gameObject);
    }

    public void EatSlimeSplit()
    {
        myPlayer.RestoreHealth(myPlayer.CurrentTile.slimeSplit.eattingSlimeRecover);

        myPlayer.CurrentTile.DestroyTileSlimeSplit();
    }
}
