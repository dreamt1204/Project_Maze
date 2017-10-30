using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehaviour : MonoBehaviour {
    //=======================================
    //      Variables
    //=======================================
    public bool isPassiveBehavior = false;
    public DetectingState detectingStateType = DetectingState.Alerted;
    public int behaviourPriority;
    [HideInInspector] public bool isCoolingDown;

    //=======================================
    //      Functions
    //=======================================
    public void ExecuteActiveBehaviour()
    {

    }

    public void ExecutePassiveBehaviour()
    {

    }
}
