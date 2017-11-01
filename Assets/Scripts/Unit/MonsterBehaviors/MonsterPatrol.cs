using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPatrol : MonsterBehaviour
{
    //=======================================
    //      Variables
    //=======================================
    const float patrolSpeedMultiplier = 0.4f;
    float idleTimer;
    const float idleTime_Min = 1.0f;
    const float idleTime_Max = 4.0f;

    //=======================================
    //      Functions
    //=======================================
    protected override void Start()
    {
        base.Start();

        AddActiveAction("PatrolToNeighborCoroutine");
    }

    //---------------------------------------
    //      Action
    //---------------------------------------
    IEnumerator PatrolToNeighborCoroutine()
    {
        // Idle for random seconds
        idleTimer = Random.Range(idleTime_Min, idleTime_Max);
        while (idleTimer > 0)
        {
            //Debug.Log(idleTimer);
            if (owner.detectingState != DetectingState.Idle)
                break;

            idleTimer -= Time.deltaTime;
            yield return null;
        }

        // Patrol to a random neighbor
        if (owner.detectingState == DetectingState.Idle)
        {
            List<Tile> neighbors = MazeUTL.GetNeighborTilesWithoutWall(owner.CurrentTile);
            Tile targetTile = MazeUTL.GetRandomTileFromList(neighbors);
            owner.TryMoveToTile(targetTile);
            do
            {
                yield return new WaitForSeconds(0.01f);
            } while (owner.CurrentAction == ActionType.Walking);
            owner.StopWalkingAnim();
        }

        SetActionFinished("PatrolToNeighborCoroutine", true);
        yield return null;
    }
}
