using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{ 
    public GameObject[] _weapons;
    public GameObject _bonusGun;
    public int currentWeaponIndex = 0;
    private bool isReloading;
    private bool isShooting;
    BaseWeapon currentWeapon;
    
    

    private void Start()
    {
        currentWeapon = _weapons[currentWeaponIndex].GetComponentInParent<BaseWeapon>();
        for (int i = 1; i < _weapons.Length; i++)
        {
            _weapons[i].SetActive(false);
        }
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Alpha1) && !isReloading && !isShooting)
        {
            SwitchWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && _weapons.Length >= 2 && !isReloading && !isShooting)
        {
            SwitchWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && _weapons.Length >= 3 && !isReloading && !isShooting)
        {
            SwitchWeapon(2);
        }

        CheckStatus();
        CheckBonusGun();
    }

    private void SwitchWeapon(int newIndex)
    {
        if (newIndex < 0 || newIndex >= _weapons.Length)
        {
            Debug.LogError("Invalid weapon index!");
            return;
        }
        
        _weapons[currentWeaponIndex].SetActive(false);

        _weapons[newIndex].SetActive(true);

        currentWeaponIndex = newIndex;
    }

    public void CheckBonusGun()
    {
        if(Bonus.isPickedMachineGun)
        {
            _weapons[currentWeaponIndex].SetActive(false);
            currentWeapon.IsReloading = false;
            _bonusGun.SetActive(true);
            StartCoroutine(C_MachineGunTimer());
        }

        else
        {
            _weapons[currentWeaponIndex].SetActive(true);
            _bonusGun.SetActive(false);
        }
    }

    private IEnumerator C_MachineGunTimer()
    {
        Debug.Log("Корутина стартанула");
        yield return new WaitForSeconds(20f);
        Bonus.isPickedMachineGun = false;
    }

    private void CheckStatus()
    {
        
        if (currentWeapon != null)
        {
            isReloading = currentWeapon.IsReloading;
            isShooting = currentWeapon.IsShooting;
        }
    }
}
