using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLogick : MonoBehaviour
{
    public BaseWeapon baseWeapon;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(baseWeapon.weaponDamage);
            }
            Destroy(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }
}
