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
        owner.PlayAnim("Attack");

        while (!finishedAttack)
        {
            yield return null;
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
}
