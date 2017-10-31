using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttack : MonsterReachAction
{
	//=======================================
	//      Functions
	//=======================================
	protected override void  Start()
	{
		actionTarget = LevelManager.instance.playerCharacter;

		base.Start ();
	}
}
