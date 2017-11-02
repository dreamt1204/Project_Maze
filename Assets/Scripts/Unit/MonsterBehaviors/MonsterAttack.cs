using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttack : MonsterReachAction
{
    //=======================================
    //      Variables
    //=======================================
    public float attackDamage = 10;
    bool finishedAttack;
    public float attackCDTime = 2;
    public float attackCDTimer;

    //=======================================
    //      Functions
    //=======================================
    protected override void Start()
    {
        base.Start();

        owner.skeletonAnim.state.Event += AttackEvent;
        owner.skeletonAnim.state.Complete += FinishAttackEvent;
    }

    protected override void ResetBehaviour()
    {
        base.ResetBehaviour();

        finishedAttack = false;
    }

    //---------------------------------------
    //      Action
    //---------------------------------------
    public override IEnumerator PostReachAction()
    {
        if (attackCDTimer <= 0)
        {
            StartAttackCD();

            owner.PlayAnim("Attack");

            while (!finishedAttack)
            {
                yield return null;
            }
        } 

        finishedPostReachAction = true;
        yield return null;
    }

    //---------------------------------------
    //      Event
    //---------------------------------------
    void AttackEvent(Spine.TrackEntry entry, Spine.Event e)
    {
        if (e.Data.Name == "Attack")
            actionTarget.RecieveDamage(attackDamage);
    }

    void FinishAttackEvent(Spine.TrackEntry entry)
    {
        if (entry.animation.Name == "Attack")
        {
            finishedAttack = true;
            owner.skeletonAnim.state.SetEmptyAnimation(entry.TrackIndex, 0);
        }
    }

    //---------------------------------------
    //      Cool Down
    //---------------------------------------
    public void StartAttackCD()
    {
        attackCDTimer = attackCDTime;
        StopCoroutine("UpdateAttackCDCoroutine");
        StartCoroutine("UpdateAttackCDCoroutine");
    }

    IEnumerator UpdateAttackCDCoroutine()
    {
        while (attackCDTimer > 0)
        {
            attackCDTimer -= Time.deltaTime;
            yield return null;
        }

        attackCDTimer = 0;
        yield return null;
    }
}
