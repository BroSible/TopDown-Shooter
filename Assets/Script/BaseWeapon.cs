using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BaseWeapon : MonoBehaviour
{
    public float weaponDamage;
    public int magazineSize;
    public int maxSizeMagazine = 100;
    public float distance;
    public bool isShooting;
    public bool isReloading;
    private KeyCode reloadKey = KeyCode.R;
    private RaycastHit hit;
    public GameObject bullet;
    public Transform shotPoint;
    public float shootSpeed;
    public float timeSinceLastShot = 0f;
    public float timeBetweenShot = 0.3f;


    void Start()
    {
        
    }

    void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        Debug.DrawRay(shotPoint.transform.position, shotPoint.transform.forward * distance, Color.red);
        if(Input.GetButton("Fire1") && !isShooting && !isReloading)
        {
            if(timeSinceLastShot > timeBetweenShot)
            {
                Vector3 direction = shotPoint.transform.forward.normalized;  
                GameObject _bullet = Instantiate(bullet, shotPoint.position,Quaternion.LookRotation(shotPoint.forward) * Quaternion.Euler(90, 0, 0));
                _bullet.SetActive(true);
                Rigidbody bulletRb = _bullet.GetComponent<Rigidbody>();
                bulletRb.velocity = direction * 70f; // тут можно изменить скорость полёта пули
                GameObject.Destroy(_bullet, 5f);
                StartCoroutine("C_shoot");
                Shoot();
                timeSinceLastShot = 0f;
            }

        }

        else if(Input.GetKey(reloadKey) && !isShooting && !isReloading && magazineSize != maxSizeMagazine)
        {
            StartCoroutine("C_reload");
            Debug.Log("Перезарядка");
        }

        else if(isShooting && magazineSize == 0)
        {
            StartCoroutine("C_reload");
            Debug.Log("Перезарядка");
        }


    }

    private void Shoot()
    {
        if(Physics.Raycast(shotPoint.transform.position, shotPoint.transform.forward , out hit, distance))
        {
            if(hit.collider.CompareTag("Enemy"))
            {
                Debug.Log("Ты отпиздил дауна");
            }
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
