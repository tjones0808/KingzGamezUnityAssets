﻿using UnityEngine;
using System.Collections;

public class testGUI : MonoBehaviour {

    public BasicPlayerWeaponInput myBasicWeaponController;
    public UnityEngine.UI.Text zeroingText;
    public UnityEngine.UI.Text MagsText;
    public UnityEngine.UI.Text BulletsText;
    public UnityEngine.UI.Text WeaponText;
    public UnityEngine.UI.Text Fps;

    private float timer = 0;

    /// <summary>
    /// visualize weapon data
    /// </summary>
    void Update () {
        if (myBasicWeaponController.currentWeapon != -1)
        {
            zeroingText.text = "Zeroing: " + (myBasicWeaponController.Weapons[myBasicWeaponController.currentWeapon].weapon.TargetWeapon.currentBarrelZero == -1 ? "0" : myBasicWeaponController.Weapons[myBasicWeaponController.currentWeapon].weapon.TargetWeapon.BarrelZeroingDistances[myBasicWeaponController.Weapons[myBasicWeaponController.currentWeapon].weapon.TargetWeapon.currentBarrelZero].ToString());
        }
        else {
            zeroingText.text = "Zeroing: 0";
        }
        WeaponText.text = myBasicWeaponController.Weapons[myBasicWeaponController.currentWeapon].weapon.name;
        DefaultMagazineController magController = (DefaultMagazineController)myBasicWeaponController.Weapons[myBasicWeaponController.currentWeapon].weapon.myMagazineController;
        BulletsText.text = magController.getBulletsInMag().ToString();
        MagsText.text = ((int)(magController.StoredBullets / (magController.BulletsPerMag))).ToString();

        timer += Time.deltaTime;

        if (timer > 0.2f)
        {
            timer = 0;
            Fps.text = "fps: " + ((int)((1 / Time.deltaTime)*Time.timeScale)).ToString();
        }
    }
}
