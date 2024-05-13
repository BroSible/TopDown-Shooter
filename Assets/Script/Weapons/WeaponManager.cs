using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{ 
    public GameObject[] _weapons;
    public int currentWeaponIndex = 0;
    private bool isReloading;
    private bool isShooting;
    

    private void Start()
    {
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

    private void CheckStatus()
    {
        BaseWeapon currentWeapon = _weapons[currentWeaponIndex].GetComponent<BaseWeapon>();
        if (currentWeapon != null)
        {
            isReloading = currentWeapon.IsReloading;
            isShooting = currentWeapon.IsShooting;
        }
    }
}
