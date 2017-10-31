using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterReachAction : MonsterBehaviour 
{
	//=======================================
	//      Variables
	//=======================================
	protected Unit actionTarget;
	protected int actionRange;

	//=======================================
	//      Functions
	//=======================================
	protected override void  Start()
	{
		actionTarget = LevelManager.instance.playerCharacter;

		AddActiveAction ("TryReachActionRangeCoroutine");
	}


	//---------------------------------------
	//      Action
	//---------------------------------------
	IEnumerator TryReachActionRangeCoroutine()
	{
		if (actionTarget != null)
		{
			List<Tile> path = MazeUTL.PathRemoveStartTile (MazeUTL.GetPathToActionRange (owner.CurrentTile, actionTarget.CurrentTile, owner.detectionRange, actionRange));
			for (int i = 0; i < path.Count; i++)
			{
				owner.TryMoveToTile (path [i]);

				do
				{
					yield return new WaitForSeconds(0.1f);
				} while (owner.CurrentAction == ActionType.Walking);
			}
		}

		owner.StopKeepWalkingAnim ();

		SetActionFinished ("TryReachActionRangeCoroutine");
	}
}
