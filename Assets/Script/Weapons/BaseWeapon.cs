using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.VFX;

public class BaseWeapon : MonoBehaviour
{
    public float weaponDamage;
    public int magazineSize;
    public int maxSizeMagazine = 30;
    public float distance;
    public bool isShooting;
    public bool isReloading;
    private KeyCode reloadKey = KeyCode.R;
    public GameObject bullet;
    public Transform shotPoint;
    public float bulletSpeed;
    public float timeSinceLastShot = 0f;
    public float timeBetweenShot = 0.3f;
    public AudioSource audiogun;
    public AudioClip ShootGun;
    public AudioClip reloadingGun;
    public VisualEffect muzzleFlash;

    //публичное свойство для передачи переменной в класс WeaponManager
    public bool IsReloading
    {
        get {return isReloading;}
    }
    
    public bool IsShooting
    {
        get {return isShooting;}
    }
    
    protected virtual void Start()
    {
        audiogun = GetComponent<AudioSource>();
    }

    protected virtual void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        Debug.DrawRay(shotPoint.transform.position, shotPoint.transform.forward * distance, Color.red);
        if(Input.GetButton("Fire1") && !isShooting && !isReloading)
        {
            isShooting = true;
            if(timeSinceLastShot > timeBetweenShot)
            {
                audiogun.PlayOneShot(ShootGun);

                muzzleFlash.Play();

                Vector3 direction = shotPoint.transform.forward.normalized;  
                GameObject _bullet = Instantiate(bullet, shotPoint.position,Quaternion.LookRotation(shotPoint.forward) * Quaternion.Euler(90, 0, 0));
                _bullet.SetActive(true);

                Rigidbody bulletRb = _bullet.GetComponent<Rigidbody>();
                bulletRb.velocity = direction * bulletSpeed; // тут можно изменить скорость полёта пули
                GameObject.Destroy(_bullet, 5f);

                timeSinceLastShot = 0f;
                magazineSize--;
            }
        }

        else if(Input.GetKey(reloadKey) && !isShooting && !isReloading && magazineSize != maxSizeMagazine ) 
        {

            StartCoroutine("C_reload");
            Debug.Log("Перезарядка");
        }

        else if(magazineSize == 0  && !isReloading)
        {
            StartCoroutine("C_reload");
            Debug.Log("У тебя 0 патрон, поэтому Relaod");
        }

        else
        {
            isShooting = false;
        }

        
    }

    protected virtual IEnumerator C_reload()
    {   
        isReloading = true;
        audiogun.PlayOneShot(reloadingGun);
        yield return new WaitForSeconds(3f);
        magazineSize = maxSizeMagazine;
        isReloading = false;
    }



}
