using UnityEngine;
using System.Collections;
using System;
using Ballistics;

public class BasicWeaponController : MonoBehaviour {

    //General

    /// <summary>
    /// the weapon being controlled
    /// </summary>
    public Weapon TargetWeapon;

    /// <summary>
    /// delay between shots
    /// </summary>
    public float ShootDelay = 0.25f;


    /// <summary>
    /// Defines the type of weapon.
    /// </summary>
    public ShootingType WeaponType = ShootingType.SingleShot;


    //Savles / Burst

    /// <summary>
    /// amount of bullets per shot in volley- / burst- mode
    /// </summary>
    public int BulletsPerVolley = 8;
    public int BulletsPerBurst = 3;

    //Spread
    public SpreadController mySpreadController;
    //--

    //MagazineController
    public MagazineController myMagazineController;
    //--

    //private variables
    private bool shootReset = true;
    private int BurstBulletCounter;
    private float CooldownTimer = 0;

    //--

    public bool isAiming;

    public Action OnShoot;

    void Update()
    {
        CooldownTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Is the Weapon Aiming
    /// </summary>
    /// <param name="active"></param>
    public void Aim(bool active)
    {
        isAiming = active;
    }

    /// <summary>
    /// Fire the gun. Call this every frame the trigger is held down.
    /// </summary>
    public void Shoot()
    {
        if (myMagazineController.isBulletAvailable() && CooldownTimer <= 0)
        {
            switch (WeaponType)
            {
                case ShootingType.Auto:
                    TargetWeapon.ShootBullet(mySpreadController.GetCurrentSpread(TargetWeapon.PhysicalBulletSpawnPoint));
                    CallOnShoot();
                    break;
                case ShootingType.Volley:
                    if (shootReset)
                    {
                        for (int i = 0; i < BulletsPerVolley; i++)
                        {
                            TargetWeapon.ShootBullet(mySpreadController.GetCurrentSpread(TargetWeapon.PhysicalBulletSpawnPoint));
                        }
                        CallOnShoot();
                        shootReset = false;
                    }
                    break;
                case ShootingType.Burst:

                    if (shootReset)
                    {
                        TargetWeapon.ShootBullet(mySpreadController.GetCurrentSpread(TargetWeapon.PhysicalBulletSpawnPoint));
                        BurstBulletCounter++;
                        CallOnShoot();
                        shootReset = false;
                        StartCoroutine(ShootBurst());
                    }
                    break;
                case ShootingType.SingleShot:
                    if (shootReset)
                    {
                        TargetWeapon.ShootBullet(mySpreadController.GetCurrentSpread(TargetWeapon.PhysicalBulletSpawnPoint));
                        CallOnShoot();
                        shootReset = false;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Coroutine to Spawn Bullets for shooting in salves
    /// </summary>
    IEnumerator ShootBurst()
    {
        while (BurstBulletCounter < BulletsPerBurst)
        {
            yield return new WaitForSeconds(ShootDelay);
            TargetWeapon.ShootBullet(mySpreadController.GetCurrentSpread(TargetWeapon.PhysicalBulletSpawnPoint));
            BurstBulletCounter++;
            CallOnShoot();
            if (BurstBulletCounter >= BulletsPerBurst)
            {
                CooldownTimer = ShootDelay;
                BurstBulletCounter = 0;
                break;
            }
        }
    }

    private void CallOnShoot()
    {
        CooldownTimer = ShootDelay;
        ((DefaultMagazineController)myMagazineController).onShoot();
        mySpreadController.onShoot();
        if (OnShoot != null)
        {
            OnShoot();
        }
    }

    /// <summary>
    /// Tells the Weapon, that the fire button has been released to be able to shoot again ( when not in Auto - Mode )
    /// </summary>
    public void StopShoot()
    {
        shootReset = true;
    }
}

public enum ShootingType
{
    SingleShot,
    Volley,
    Auto,
    Burst
}
