using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeleeBeetle : BaseEnemy
{
    protected override void Start()
    {
        base.Start();
        Attack += PlayMeleeBeetleAttackAnimation;
        Run += PlayMeleeBeetleRunAnimation;
        Death += PlayMeleeBeetleDeathAnimation;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }


    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
    }

    protected override void SearchWalkPoint()
    {
        base.SearchWalkPoint();
    }

    protected override void Patroling()
    {
        base.Patroling();
    }

    protected override void ChasePlayer()
    {
        base.ChasePlayer();
    }

    protected override void AttackPlayer()
    {
        base.AttackPlayer();
    }

    protected override void ResetAttack()
    {
        base.ResetAttack();
    }

    public override IEnumerator C_OnDefeat()
    {
        return base.C_OnDefeat();
    }

    public override IEnumerator C_ResetFollowingPlayer()
    {
        return base.C_ResetFollowingPlayer();
    }

    private void PlayMeleeBeetleAttackAnimation()
    {
        _animator.Play("AttackBug");
    }

    private void PlayMeleeBeetleRunAnimation()
    {
        _animator.Play("RunBug");
    }

    private void PlayMeleeBeetleDeathAnimation()
    {
        _animator.Play("DeathBug");
    }
    
}
