using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellLogic : MonoBehaviour
{
    public float damage;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Controller.TakeDamage(damage);
            Destroy(gameObject);
        }
        Destroy(gameObject);
    }
   
}
