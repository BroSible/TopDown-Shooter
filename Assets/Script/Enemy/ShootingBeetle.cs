using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingBeetle : BaseEnemy
{
    protected override void Start()
    {
        base.Start();
        Attack += PlayShootingBeetleAttackAnimation;
        Run += PlayShootingBeetleRunAnimation;
        Death += PlayShootingBeetleDeathAnimation;
    }

    public Transform spellPosition;
    public GameObject spell;
    protected override void AttackPlayer()
    {
        _agent.SetDestination(transform.position);
        transform.LookAt(new Vector3(_target.position.x, transform.position.y, _target.position.z));

        if(!isAlreadyAttacked)
        {
            OnAttack();
            StartCoroutine(SpellAttacking());
            isAlreadyAttacked = true;
            Invoke(nameof(ResetAttack),cooldowns[0]); 
        }

    }

    private IEnumerator SpellAttacking()
    {
        _agent.SetDestination(transform.position);
        yield return new WaitForSeconds(0.5f);
        for(int i = 0; i < 3; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-1f, 1f),                        Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * 2f; 
            Vector3 direction = (_target.position + offset - spellPosition.position).normalized;  
           GameObject _spell = Instantiate(spell, spellPosition.position, Quaternion.identity);
            _spell.SetActive(true);
            Rigidbody bulletRb = _spell.GetComponent<Rigidbody>();
            bulletRb.velocity = direction * 70f; // Умножаем направление на скорость
            GameObject.Destroy(_spell, 10f);
        }

        
        yield return new WaitForSeconds(0.3f);
    }

    private void PlayShootingBeetleAttackAnimation()
    {
        _animator.Play("AttackBug");
    }

    private void PlayShootingBeetleRunAnimation()
    {
        _animator.Play("RunBug");
    }

    private void PlayShootingBeetleDeathAnimation()
    {
        _animator.Play("DeathBug");
    }

}
