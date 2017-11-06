using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehaviour_Template : MonsterBehaviour
{
    //=======================================
    //      Variables
    //=======================================


    //=======================================
    //      Functions
    //=======================================
    protected override void Start()
    {
        base.Start();

        // Add action coroutines here
        // ex: AddActiveAction("test_Coroutine");
        //     .
        //     .
        //     .
    }

    protected override void ResetBehaviour()
    {
        // Reset variable here if needed
    }

    //---------------------------------------
    //      Action
    //---------------------------------------
    // Create action coroutines here
    IEnumerator test_Coroutine()
    {
        // Do stuff

        // Add following with coroutine name
        SetActionFinished("test_Coroutine", true);
        yield return null;
    }
}
