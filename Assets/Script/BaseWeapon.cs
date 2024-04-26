using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.VFX;

public class BaseWeapon : MonoBehaviour
{
    public float weaponDamage;
    public int magazineSize;
    public int maxSizeMagazine = 100;
    public float distance;
    public bool isShooting;
    public bool isReloading;
    private KeyCode reloadKey = KeyCode.R;
    public GameObject bullet;
    public Transform shotPoint;
    public float shootSpeed;
    public float timeSinceLastShot = 0f;
    public float timeBetweenShot = 0.3f;
    public AudioSource audiogun;
    public AudioClip ShootGun;
    public AudioClip reloadingGun;
    public VisualEffect muzzleFlash;
    
    

    void Start()
    {
        audiogun = GetComponent<AudioSource>();
    }

    void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        Debug.DrawRay(shotPoint.transform.position, shotPoint.transform.forward * distance, Color.red);
        if(Input.GetButton("Fire1") && !isShooting && !isReloading)
        {
            if(timeSinceLastShot > timeBetweenShot)
            {
                audiogun.PlayOneShot(ShootGun);

                muzzleFlash.Play();

                Vector3 direction = shotPoint.transform.forward.normalized;  
                GameObject _bullet = Instantiate(bullet, shotPoint.position,Quaternion.LookRotation(shotPoint.forward) * Quaternion.Euler(90, 0, 0));
                _bullet.SetActive(true);

                Rigidbody bulletRb = _bullet.GetComponent<Rigidbody>();
                bulletRb.velocity = direction * 70f; // тут можно изменить скорость полёта пули
                GameObject.Destroy(_bullet, 5f);

                StartCoroutine("C_shoot");
                timeSinceLastShot = 0f;
            }

        }

        else if(Input.GetKey(reloadKey) && !isShooting && !isReloading && magazineSize != maxSizeMagazine)
        {
            audiogun.PlayOneShot(reloadingGun);
            StartCoroutine("C_reload");
            Debug.Log("Перезарядка");
        }

        else if(isShooting && magazineSize == 0)
        {
            audiogun.PlayOneShot(reloadingGun);
            StartCoroutine("C_reload");
            Debug.Log("Перезарядка");
        }
        

    }


    private IEnumerator C_shoot()
    {
        isShooting = true;

        magazineSize--;
        yield return new WaitForSeconds(shootSpeed);
        isShooting = false;
    }

    private IEnumerator C_reload()
    {   
        
        isReloading = true;
        yield return new WaitForSeconds(3f);
        magazineSize = maxSizeMagazine;
        isReloading = false;
    }



}
