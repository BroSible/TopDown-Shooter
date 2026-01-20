using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BaseWeapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    public float weaponDamage;
    public int magazineSize;
    public int maxSizeMagazine = 30;
    public float distance;
    public float bulletSpeed;
    public float timeBetweenShot = 0.3f;
    
    [Header("Ammo")]
    public bool infiniteAmmo;
    public int currentAmmo;
    public int maxAmmo;
    
    [Header("Shooting")]
    public GameObject bullet;
    public Transform shotPoint;
    public VisualEffect muzzleFlash;
    
    [Header("Audio")]
    public AudioSource audiogun;
    public AudioClip ShootGun;
    public AudioClip reloadingGun;
    
    [Header("Camera & ADS")]
    public Camera playerCamera;
    public bool isAiming = false;
    public float aimFOV = 40f;
    public float normalFOV = 60f;
    public float aimSpeed = 10f;
    
    [Header("Weapon Attachment")]
    [Tooltip("Кость руки персонажа для привязки оружия")]
    public Transform handBone;
    [Tooltip("Имя кости руки для автоматического поиска (например: RightHand, mixamorig:RightHand)")]
    public string handBoneName = "RightHand";
    [Tooltip("Сохраненная локальная позиция оружия (заполняется автоматически)")]
    public Vector3 savedLocalPosition;
    [Tooltip("Сохраненная локальная ротация оружия (заполняется автоматически)")]
    public Vector3 savedLocalRotation;
    [Tooltip("Применять сохраненную позицию при старте")]
    public bool applysavedPosition = true;
    
    [Header("Status")]
    public bool isShooting;
    public bool isReloading;
    
    protected KeyCode reloadKey = KeyCode.R;
    protected float timeSinceLastShot = 0f;
    
    public bool IsReloading
    {
        get { return isReloading; }
        set { isReloading = value; }
    }
    
    public bool IsShooting
    {
        get { return isShooting; }
    }
    
    protected virtual void Awake()
    {
        if (handBone == null)
        {
            handBone = FindHandBone();
        }
        
        if (handBone != null)
        {
            if (transform.parent != handBone)
            {
                if (!applysavedPosition || savedLocalPosition == Vector3.zero)
                {
                    savedLocalPosition = transform.localPosition;
                    savedLocalRotation = transform.localEulerAngles;
                }
                
                transform.SetParent(handBone);
                
                if (applysavedPosition && savedLocalPosition != Vector3.zero)
                {
                    transform.localPosition = savedLocalPosition;
                    transform.localRotation = Quaternion.Euler(savedLocalRotation);
                }
                
                Debug.Log($"[BaseWeapon] Оружие {gameObject.name} привязано к {handBone.name}");
            }
            else
            {
                Debug.Log($"[BaseWeapon] Оружие {gameObject.name} уже привязано к {handBone.name}");
            }
        }
        else
        {
            Debug.LogWarning($"[BaseWeapon] Не удалось найти кость руки для {gameObject.name}. Укажите handBone или handBoneName.");
        }
    }
    
    protected virtual void Start()
    {
        audiogun = GetComponent<AudioSource>();
        currentAmmo = maxAmmo;
        
        if (infiniteAmmo)
        {
            currentAmmo = int.MaxValue;
            maxAmmo = int.MaxValue;
        }
        
        if (playerCamera != null)
        {
            normalFOV = playerCamera.fieldOfView;
        }
    }

    protected virtual Transform FindHandBone()
    {
        Transform current = transform.parent;
        while (current != null)
        {
            if (IsHandBone(current))
            {
                return current;
            }
            current = current.parent;
        }
        
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
            Transform found = FindHandBoneRecursive(root.transform);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    protected virtual Transform FindHandBoneRecursive(Transform parent)
    {
        if (IsHandBone(parent))
        {
            return parent;
        }
        
        foreach (Transform child in parent)
        {
            Transform result = FindHandBoneRecursive(child);
            if (result != null)
            {
                return result;
            }
        }
        
        return null;
    }
    
    protected virtual bool IsHandBone(Transform t)
    {
        string name = t.name.ToLower();
        string searchName = handBoneName.ToLower();
        
        return name.Contains(searchName) || 
               name.Contains("righthand") || 
               name.Contains("right_hand") ||
               name.Contains("hand_r") ||
               name.EndsWith("hand");
    }
    
    public virtual void SaveCurrentPosition()
    {
        savedLocalPosition = transform.localPosition;
        savedLocalRotation = transform.localEulerAngles;
        Debug.Log($"[BaseWeapon] Позиция сохранена: Pos={savedLocalPosition}, Rot={savedLocalRotation}");
    }
    
    public virtual void ApplySavedPosition()
    {
        if (savedLocalPosition != Vector3.zero)
        {
            transform.localPosition = savedLocalPosition;
            transform.localRotation = Quaternion.Euler(savedLocalRotation);
            Debug.Log($"[BaseWeapon] Позиция применена: Pos={savedLocalPosition}, Rot={savedLocalRotation}");
        }
    }
    
    public virtual void SetHandBone(Transform bone)
    {
        handBone = bone;
        if (handBone != null)
        {
            transform.SetParent(handBone);
            if (applysavedPosition && savedLocalPosition != Vector3.zero)
            {
                ApplySavedPosition();
            }
        }
    }

    protected virtual void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        
        HandleAiming();
        
        if (Input.GetButton("Fire1") && !isShooting && !isReloading && magazineSize != 0)
        {
            isShooting = true;
            if (timeSinceLastShot > timeBetweenShot)
            {
                Shoot();
            }
        }
        else if (Input.GetKey(reloadKey) && !isShooting && !isReloading && magazineSize != maxSizeMagazine && currentAmmo > 0)
        {
            StartCoroutine("C_reload");
        }
        else if (magazineSize == 0 && !isReloading && currentAmmo > 0)
        {
            StartCoroutine("C_reload");
        }
        else
        {
            isShooting = false;
        }
    }

    protected virtual void HandleAiming()
    {
        if (playerCamera == null) return;

        if (Input.GetButton("Fire2") && !isReloading)
        {
            isAiming = true;
        }
        else
        {
            isAiming = false;
        }

        float targetFOV = isAiming ? aimFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, aimSpeed * Time.deltaTime);
    }

    protected virtual void Shoot()
    {
        audiogun.PlayOneShot(ShootGun);
        
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        Vector3 direction = GetShootDirection();
        
        GameObject _bullet = Instantiate(bullet, shotPoint.position, Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0));
        _bullet.SetActive(true);

        Rigidbody bulletRb = _bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.velocity = direction * bulletSpeed;
        }
        
        Destroy(_bullet, 5f);

        timeSinceLastShot = 0f;
        magazineSize--;
        
        Debug.Log($"Оставшиеся патроны в магазине: {magazineSize}");
    }

    protected virtual Vector3 GetShootDirection()
    {
        if (playerCamera == null)
        {
            return shotPoint.transform.forward.normalized;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 1000f;
        }

        Vector3 direction = (targetPoint - shotPoint.position).normalized;
        return direction;
    }

    public virtual void RefillAmmo()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Патроны пополнены!");
    }

    protected virtual IEnumerator C_reload()
    {
        isReloading = true;
        audiogun.PlayOneShot(reloadingGun);
        
        yield return new WaitForSeconds(3f);
        
        int requiredAmmo = Mathf.Min(maxSizeMagazine - magazineSize, currentAmmo);
        magazineSize += requiredAmmo;
        currentAmmo -= requiredAmmo;
        
        isReloading = false;
        
        Debug.Log($"Перезарядка завершена! Патроны в магазине: {magazineSize}");
    }
}