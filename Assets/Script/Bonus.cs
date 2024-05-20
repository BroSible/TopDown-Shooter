using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour
{
    public static bool isMachineGun = false;
    public enum BonusType
    {
        Health,
        Ammo,
        MachineGun,
    }

    public BonusType bonusType;

    public void BonusProperty(GameObject player)
    {
        switch (bonusType)
        {
            case BonusType.Health:
            {
                Controller.playerHealth += 20;
                break;
            }

            case BonusType.Ammo:
            {
                BaseWeapon baseWeapon = player.GetComponentInChildren<BaseWeapon>();
                if (baseWeapon != null)
                {
                    baseWeapon.RefillAmmo();
                }
                break;
            }

            case BonusType.MachineGun:
            {
                isMachineGun = true;
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BonusProperty(other.gameObject);
            Destroy(gameObject);
        }
    }

    private IEnumerator C_MachineGunTimer()
    {
        yield return new WaitForSeconds(20f);
        isMachineGun = false;
    }
}
