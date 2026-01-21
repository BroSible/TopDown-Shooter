using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class WeaponManager : MonoBehaviour
{ 
    [Header("Weapons")]
    public GameObject[] _weapons;
    public GameObject _bonusGun;
    public static int currentWeaponIndex = 0;
    
    [Header("UI")]
    public Texture[] weaponIcons;
    public RawImage weaponIcon;
    
    [Header("Cinemachine Cameras")]
    public CinemachineVirtualCamera normalCamera; 
    public CinemachineVirtualCamera aimCamera;    
    
    [Header("Bonus Weapon Settings")]
    public float bonusGunDuration = 20f;

    [Header("Player Animator")]
    public Animator playerAnimator; 

    [Header("Artillery Reference")]
    public ArtilleryStrike artilleryStrike;
    
    private bool isReloading;
    private bool isShooting;
    private BaseWeapon currentWeapon;

    // Обновляем метод Start()
    private void Start()
    {
        for (int i = 1; i < _weapons.Length; i++)
        {
            _weapons[i].SetActive(false);
        }
        
        AssignCamerasToWeapons();
        AssignAnimatorToWeapons(); 
        
        UpdateWeaponIcon();
    }

    private void Update()
    {
        currentWeapon = _weapons[currentWeaponIndex].GetComponentInChildren<BaseWeapon>();
        
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

        if (Bonus.isPickedMachineGun)
        {
            if (currentWeapon != null)
            {
                currentWeapon.IsReloading = false;
            }
        }

        if (artilleryStrike != null && !artilleryStrike.CanUseWeapons())
    {
        return; // Блокируем стрельбу во время артудара
    }

        CheckStatus();
        CheckBonusGun();
    }
    
    private void AssignAnimatorToWeapons()
    {
        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInParent<Animator>();
            
            if (playerAnimator == null)
            {
                Debug.LogWarning("Player Animator не назначен и не найден автоматически!");
                return;
            }
        }

        foreach (GameObject weapon in _weapons)
        {
            if (weapon != null)
            {
                BaseWeapon baseWeapon = weapon.GetComponentInChildren<BaseWeapon>();
                if (baseWeapon != null)
                {
                    baseWeapon.playerAnimator = playerAnimator;
                }
            }
        }

        if (_bonusGun != null)
        {
            BaseWeapon bonusWeapon = _bonusGun.GetComponentInChildren<BaseWeapon>();
            if (bonusWeapon != null)
            {
                bonusWeapon.playerAnimator = playerAnimator;
            }
        }

        Debug.Log("Аниматор назначен всем оружиям!");
    }

    private void AssignCamerasToWeapons()
    {
        if (normalCamera == null || aimCamera == null)
        {
            Debug.LogError("Cinemachine камеры не назначены в WeaponManager!");
            
            if (normalCamera == null)
            {
                normalCamera = FindObjectOfType<CinemachineVirtualCamera>();
                Debug.LogWarning("NormalCamera найдена автоматически");
            }
            
            return;
        }

        foreach (GameObject weapon in _weapons)
        {
            if (weapon != null)
            {
                BaseWeapon baseWeapon = weapon.GetComponentInChildren<BaseWeapon>();
                if (baseWeapon != null)
                {
                    baseWeapon.normalCamera = normalCamera;
                    baseWeapon.aimCamera = aimCamera;
                }
            }
        }

        if (_bonusGun != null)
        {
            BaseWeapon bonusWeapon = _bonusGun.GetComponentInChildren<BaseWeapon>();
            if (bonusWeapon != null)
            {
                bonusWeapon.normalCamera = normalCamera;
                bonusWeapon.aimCamera = aimCamera;
            }
        }

        Debug.Log("Cinemachine камеры назначены всем оружиям!");
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
        
        UpdateWeaponIcon();
        
        Debug.Log($"Переключено на оружие {newIndex + 1}");
    }

    public void CheckBonusGun()
    {
        if (Bonus.isPickedMachineGun)
        {
            _bonusGun.SetActive(true);
            
            _weapons[currentWeaponIndex].SetActive(false);
            
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
        Debug.Log($"Бонусное оружие активно на {bonusGunDuration} секунд!");
        yield return new WaitForSeconds(bonusGunDuration);
        Bonus.isPickedMachineGun = false;
        Debug.Log("Бонусное оружие закончилось!");
    }

    private void CheckStatus()
    {
        if (currentWeapon != null)
        {
            isReloading = currentWeapon.IsReloading;
            isShooting = currentWeapon.IsShooting;
        }
    }

    private void UpdateWeaponIcon()
    {
        if (weaponIcon == null)
        {
            Debug.LogWarning("Weapon Icon UI не назначен!");
            return;
        }

        if (weaponIcons.Length > currentWeaponIndex)
        {
            weaponIcon.texture = weaponIcons[currentWeaponIndex];
        }
        else
        {
            Debug.LogError("Weapon icon not found for index " + currentWeaponIndex);
        }
    }

    public BaseWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public void RefillCurrentWeaponAmmo()
    {
        if (currentWeapon != null)
        {
            currentWeapon.RefillAmmo();
        }
    }

    public void RefillAllWeaponsAmmo()
    {
        foreach (GameObject weapon in _weapons)
        {
            BaseWeapon baseWeapon = weapon.GetComponentInChildren<BaseWeapon>();
            if (baseWeapon != null)
            {
                baseWeapon.RefillAmmo();
            }
        }
        Debug.Log("Патроны пополнены для всех оружий!");
    }
    
    public void SetWeaponCameras(GameObject weapon, CinemachineVirtualCamera normalCam, CinemachineVirtualCamera aimCam)
    {
        if (weapon == null) return;
        
        BaseWeapon baseWeapon = weapon.GetComponentInChildren<BaseWeapon>();
        if (baseWeapon != null)
        {
            baseWeapon.normalCamera = normalCam;
            baseWeapon.aimCamera = aimCam;
        }
    }
    
    public void UpdateAllWeaponCameras(CinemachineVirtualCamera newNormalCamera, CinemachineVirtualCamera newAimCamera)
    {
        normalCamera = newNormalCamera;
        aimCamera = newAimCamera;
        AssignCamerasToWeapons();
    }
}