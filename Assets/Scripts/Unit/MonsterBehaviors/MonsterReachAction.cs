using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterReachAction : MonsterBehaviour 
{
    //=======================================
    //      Variables
    //=======================================
    public Unit actionTarget;
	public int actionRange;
    public bool reached;
    public bool finishedPostReachAction;

    //=======================================
    //      Functions
    //=======================================
    protected override void  Start()
	{
        base.Start();

		AddActiveAction("TryReachActionRangeCoroutine");
        AddActiveAction("PostReachActionCoroutine");
    }

    protected override void ResetBehaviour()
    {
        reached = false;
        finishedPostReachAction = false;
        actionTarget = owner.alertedTarget;
    }

    void CheckReached()
    {
        if (reached)
            return;
        if (actionTarget == null)
            return;
        if (!MazeUTL.CheckTargetInRangeAndDetectRegion(owner.CurrentTile, actionTarget.CurrentTile, actionRange))
            return;

        reached = true;
    }

    //---------------------------------------
    //      Action
    //---------------------------------------
    IEnumerator TryReachActionRangeCoroutine()
	{
        CheckReached();

        if (!reached)
        {
            if (!MazeUTL.CheckTargetInRangeAndDetectRegion(owner.CurrentTile, actionTarget.CurrentTile, actionRange))
            {
                List<Tile> path = MazeUTL.GetShortestPath(owner.CurrentTile, actionTarget.CurrentTile, owner.detectRange);
                for (int i = 0; i < path.Count; i++)
                {
                    owner.TryMoveToTile(path[i]);
                    do
                    {
                        CheckReached();
                        yield return new WaitForSeconds(0.01f);
                    } while (owner.CurrentAction == ActionType.Walking);

                    if (reached)
                        break;
                }
                owner.StopWalkingAnim();
            }
        }

        SetActionFinished ("TryReachActionRangeCoroutine", true);
	}

    IEnumerator PostReachActionCoroutine()
    {
        // If the target is in action range, execute PostReachAction
		if (reached)
		{
			StartCoroutine("PostReachAction");
			while (!finishedPostReachAction)
			{
				yield return null;
			}
		}
        // If lost target, turns to Warning state
        else if (!MazeUTL.CheckTargetInRangeAndDetectRegion(owner.CurrentTile, actionTarget.CurrentTile, owner.detectRange))
        {
			yield return new WaitForSeconds(0.5f);
			if (owner.detectState == DetectState.Alerted)
				owner.detectState = DetectState.Warning;
        }

        SetActionFinished("PostReachActionCoroutine", true);
        yield return null;
    }

    public virtual IEnumerator PostReachAction()
    {
        // override me, remember to copy the following lines.
        finishedPostReachAction = true;
        yield return null;
    }
}
