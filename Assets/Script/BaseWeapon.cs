using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public GameObject shotPoint;
    public float shootSpeed;

    void Start()
    {
        
    }

    void Update()
    {
        Debug.DrawRay(shotPoint.transform.position, shotPoint.transform.forward * distance, Color.red);
        if(Input.GetButton("Fire1") && !isShooting && !isReloading)
        {
            StartCoroutine("C_shoot");
            Shoot();
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
