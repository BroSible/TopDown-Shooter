using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public TMP_Text ammoText;
    public TMP_Text healthText;
    [SerializeField] private BaseWeapon weapon;

    void Update()
    {
        ammoText.text = weapon.magazineSize.ToString();
        healthText.text = Controller.playerHealth.ToString();

        if(Controller.playerHealth  <= 0)
        {
            healthText.text = "0";
        }
    }
}
