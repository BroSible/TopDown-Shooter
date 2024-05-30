using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public TMP_Text ammoText;
    public TMP_Text healthText;
    public TMP_Text scoreText;
    public TMP_Text currentAmmo;
    [SerializeField] private BaseWeapon[] _weapons;

    void Update()
    {
        ammoText.text = _weapons[WeaponManager.currentWeaponIndex].magazineSize.ToString();
        healthText.text = Controller.playerHealth.ToString();
        scoreText.text = Controller.score.ToString();
        currentAmmo.text = _weapons[WeaponManager.currentWeaponIndex].currentAmmo.ToString();

        if(Controller.playerHealth  <= 0)
        {
            healthText.text = "0";
        }

        if( WeaponManager.currentWeaponIndex == 0)
        {
            currentAmmo.text = "infinite";
        }
    }
}
