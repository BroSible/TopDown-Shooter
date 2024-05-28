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
    [SerializeField] private BaseWeapon[] _weapons;

    void Update()
    {
        ammoText.text = _weapons[WeaponManager.currentWeaponIndex].magazineSize.ToString();
        healthText.text = Controller.playerHealth.ToString();
        scoreText.text = Controller.score.ToString();
        if(Controller.playerHealth  <= 0)
        {
            healthText.text = "0";
        }
    }
}
