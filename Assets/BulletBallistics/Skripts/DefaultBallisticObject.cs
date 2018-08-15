using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ballistics;

public class DefaultBallisticObject : BallisticObject
{
    private Rigidbody myRigidbody;
    private LivingEntity myLivingEntity;
    private LivingEntityCollider myLECollider;
    private bool hasRigidbody, hasLivingEntity;

    [Range(0f, 1f)]
    public float forceTransferEfficiency = .5f;

    private void Awake()
    {
        //is static object?
        if (gameObject.isStatic)
        {
            return;
        }

        //cache rigidbody and livingentity collider if attached
        myLECollider = GetComponent<LivingEntityCollider>();
        myLivingEntity = myLECollider != null ? myLECollider.ParentLivingEntity : null;
        hasLivingEntity = myLivingEntity != null;

        myRigidbody = GetComponent<Rigidbody>();
        hasRigidbody = myRigidbody != null;
    }

    public override void BulletImpact(Weapon weapon, Vector3 bulletDir, float bulletSpeedBefore, float bulletSpeedAfter, float penetrationDepth, RaycastHit rayHit, BallisticObjectData myBallisticObjData)
    {
        float deltaSpeed = bulletSpeedBefore - bulletSpeedAfter;

        if (hasLivingEntity)
        {
            //damage dependent on bullet speed
            myLivingEntity.Health -= (Mathf.Pow(deltaSpeed, 2f) / Mathf.Pow(weapon.MaxBulletSpeed, 2f)) * weapon.MuzzleDamage * myLECollider.DamageMultiplier;
        }

        //apply force
        if (hasRigidbody)
        {
            float force = ((Mathf.Pow(deltaSpeed, 2f) * 0.5f * weapon.BulletMass) / penetrationDepth) * forceTransferEfficiency;
            myRigidbody.AddForceAtPosition(bulletDir * force, rayHit.point, ForceMode.Impulse);
        }

        if (myBallisticObjData.impactObject != null)
        {
            //get instance of impactObject
            GameObject impactGO = PoolManager.instance.GetNextGameObject(myBallisticObjData.impactObject.gameObject);

            Transform impact;
            if (impactGO == null)
            {
                impact = Instantiate(myBallisticObjData.impactObject.gameObject).transform;
                //impact.SetParent(PoolManager.instance.transform);
            }
            else
            {
                impact = impactGO.transform;
            }
            impact.SetParent(rayHit.transform, false);

            impact.gameObject.SetActive(true);

            impact.position = rayHit.point;

            ImpactObject myImpact = impact.GetComponent<ImpactObject>();
            if (myImpact != null)
            {
                myImpact.Hit(myBallisticObjData, rayHit);
            }
        }
    }
}
