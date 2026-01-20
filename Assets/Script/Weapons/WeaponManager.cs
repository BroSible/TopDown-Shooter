using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{ 
    [Header("Weapons")]
    public GameObject[] _weapons;
    public GameObject _bonusGun;
    public static int currentWeaponIndex = 0;
    
    [Header("UI")]
    public Texture[] weaponIcons;
    public RawImage weaponIcon;
    
    [Header("Camera Reference")]
    public Camera playerCamera; // Назначь главную камеру или Cinemachine Brain камеру
    
    [Header("Bonus Weapon Settings")]
    public float bonusGunDuration = 20f;
    
    private bool isReloading;
    private bool isShooting;
    private BaseWeapon currentWeapon;

    private void Start()
    {
        // Отключаем все оружия кроме первого
        for (int i = 1; i < _weapons.Length; i++)
        {
            _weapons[i].SetActive(false);
        }
        
        // Назначаем камеру всем оружиям
        AssignCameraToWeapons();
        
        // Обновляем иконку
        UpdateWeaponIcon();
    }

    private void Update()
    {
        // Получаем текущее оружие
        currentWeapon = _weapons[currentWeaponIndex].GetComponentInChildren<BaseWeapon>();
        
        // Переключение оружия (1, 2, 3)
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

        // Если подобрали бонусное оружие
        if (Bonus.isPickedMachineGun)
        {
            if (currentWeapon != null)
            {
                currentWeapon.IsReloading = false;
            }
        }

        // Проверяем статус и бонусное оружие
        CheckStatus();
        CheckBonusGun();
    }

    /// <summary>
    /// Назначает камеру всем оружиям при старте
    /// </summary>
    private void AssignCameraToWeapons()
    {
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera не назначена в WeaponManager!");
            return;
        }

        // Назначаем камеру всем обычным оружиям
        foreach (GameObject weapon in _weapons)
        {
            if (weapon != null)
            {
                BaseWeapon baseWeapon = weapon.GetComponentInChildren<BaseWeapon>();
                if (baseWeapon != null)
                {
                    baseWeapon.playerCamera = playerCamera;
                }
            }
        }

        // Назначаем камеру бонусному оружию
        if (_bonusGun != null)
        {
            BaseWeapon bonusWeapon = _bonusGun.GetComponentInChildren<BaseWeapon>();
            if (bonusWeapon != null)
            {
                bonusWeapon.playerCamera = playerCamera;
            }
        }

        Debug.Log("Камера назначена всем оружиям!");
    }

    /// <summary>
    /// Переключение между оружиями
    /// </summary>
    private void SwitchWeapon(int newIndex)
    {
        if (newIndex < 0 || newIndex >= _weapons.Length)
        {
            Debug.LogError("Invalid weapon index!");
            return;
        }
        
        // Отключаем текущее оружие
        _weapons[currentWeaponIndex].SetActive(false);
        
        // Включаем новое оружие
        _weapons[newIndex].SetActive(true);
        currentWeaponIndex = newIndex;
        
        // Обновляем иконку
        UpdateWeaponIcon();
        
        Debug.Log($"Переключено на оружие {newIndex + 1}");
    }

    public void CheckBonusGun()
    {
        if (Bonus.isPickedMachineGun)
        {
            // Включаем бонусное оружие
            _bonusGun.SetActive(true);
            
            // Отключаем текущее оружие
            _weapons[currentWeaponIndex].SetActive(false);
            
            // Запускаем таймер
            StartCoroutine(C_MachineGunTimer());
        }
        else
        {
            // Возвращаем обычное оружие
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
}