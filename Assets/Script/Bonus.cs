using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour
{
    public enum BonusType
    {
        Health,
    }

    public BonusType bonusType;
    public void BonusProperty()
    {
        switch (bonusType)
        {
            case BonusType.Health:
            {
                Controller.playerHealth += 20;
                break;
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // применяет улучшение

        if(other.CompareTag("Player"))
        {
            BonusProperty();
            Destroy(gameObject);
        }
            
    }
}
