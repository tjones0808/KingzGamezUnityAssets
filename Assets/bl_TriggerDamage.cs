using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_TriggerDamage : MonoBehaviour
{
    public int Damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == bl_PlayerSettings.LocalTag)
        {
            bl_PlayerDamageManager pdm = other.GetComponent<bl_PlayerDamageManager>();
            if(pdm != null)
            {
                bl_OnDamageInfo info = new bl_OnDamageInfo();
                info.mDamage = Damage;
                info.mDirection = transform.position;
                info.mWeapon = "Area";

                pdm.GetDamage(info);
            }
        }
    }
}