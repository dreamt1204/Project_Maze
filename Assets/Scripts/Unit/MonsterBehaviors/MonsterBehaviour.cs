using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehaviour : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
	[HideInInspector] public Monster owner;

    public bool isPassiveBehavior = false;
    public DetectingState detectingStateType = DetectingState.Alerted;
    public int behaviourPriority;

	// Active behaviour variables
	[HideInInspector] public List<ActiveAction> ActiveActionList = new List<ActiveAction>();
    [HideInInspector] public bool isCoolingDown;
    public float actionCDTime = 2;
    public float actionCDTimer;

    //---------------------------------------
    //      Struct
    //---------------------------------------
    public struct ActiveAction
	{
		public string coroutine;
		public bool finished;
	}

    //=======================================
    //      Functions
    //=======================================
	protected virtual void  Start()
	{
		
	}

    protected virtual void Update()
    {
        
    }

    //---------------------------------------
    //      Active Behaviour
    //---------------------------------------
    public void StartActiveBehaviour()
    {
        ResetBehaviour();
        StartCoroutine ("ActiveBehaviourCoroutine");
    }

    protected virtual void ResetBehaviour()
    {
        // override me for each child behaviour
    }

    IEnumerator ActiveBehaviourCoroutine()
	{
		// Execute all the action coroutines in ActiveActionList
		for (int i = 0; i < ActiveActionList.Count; i++)
		{
            SetActionFinished(ActiveActionList[i].coroutine, false);
            StopCoroutine(ActiveActionList[i].coroutine);

            StartCoroutine(ActiveActionList[i].coroutine);

			while (!ActiveActionList[i].finished)
			{
				yield return null;
			}
		}

		// ClearBehaviour
		owner.currentBehaviour = null;
	}

	protected void AddActiveAction(string coroutineName)
	{
		ActiveActionList.Add (new ActiveAction () {coroutine = coroutineName});
	}

	protected void SetActionFinished(string coroutineName, bool finished)
	{
		int actionID = -1;

		for (int i = 0; i < ActiveActionList.Count; i++)
		{
			if (ActiveActionList [i].coroutine == coroutineName)
			{
				actionID = i;
				break;
			}	
		}

		Utilities.TryCatchError ((actionID == -1), "Cannot find correct action ID.");

		ActiveAction newAction = new ActiveAction ();
		newAction.coroutine = ActiveActionList [actionID].coroutine;
		newAction.finished = finished;
		ActiveActionList [actionID] = newAction;
	}

    //---------------------------------------
    //      Cool Down
    //---------------------------------------
    public void StartActionCD()
    {
        actionCDTimer = actionCDTime;
        StopCoroutine("UpdateActionCDCoroutine");
        StartCoroutine("UpdateActionCDCoroutine");
    }

    IEnumerator UpdateActionCDCoroutine()
    {
        while (actionCDTimer > 0)
        {
            actionCDTimer -= Time.deltaTime;
            yield return null;
        }

        actionCDTimer = 0;
        yield return null;
    }
}
