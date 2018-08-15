//disable Bullet debugging? Comment out the next line
#define BULLET_DEBUGGER

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ballistics
{
    public class BulletHandler : MonoBehaviour
    {
        /// <summary>
        /// List of all bullets in the scene
        /// </summary>
        [HideInInspector]
        public Queue<BulletData> Bullets = new Queue<BulletData>();

        /// <summary>
        /// maximal amount of bullets getting updated each frame
        /// </summary>
        [Tooltip("Maximal amount of bullet calculations per frame")]
        public int MaxBulletUpdatesPerFrame = 500;

        /// <summary>
        /// time it takes the bullet visualisation to move to the calculated virtual bullet
        /// </summary>
        [Tooltip("Time (s) until the visual representation of the bullet reaches the position of the calculation")]
        public float VisualBulletToRealBulletMovementTime = 0.05f;

        /// <summary>
        /// singelton to bullethandler instance
        /// </summary>
        public static BulletHandler instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BulletHandler>();
                    if (_instance == null)
                    {
                        Debug.LogError("There is no 'BulletHandler' Component in the scene!");
                        //GameObject instanceGO = new GameObject("BulletHandler");
                        //_instance = instanceGO.AddComponent<BulletHandler>();
                    }
                }
                return _instance;
            }
        }
        private static BulletHandler _instance;

        //Settings
        [HideInInspector]
        public BallisticsSettings Settings;

        private PoolManager myPool;

        //private
        private float g;
        private BulletData[] bulletListTmp;

        void Awake()
        {
            g = Physics.gravity.y;
            myPool = PoolManager.instance;
        }

        void LateUpdate()
        {
            UpdateBullets();
        }

        /// <summary>
        /// calculate bullet flights
        /// </summary>
        void UpdateBullets()
        {
            float deltaTime = Time.deltaTime;

            //predefined variables
            BulletData cBullet;
            Weapon cWeapon;
            Ray ray;
            RaycastHit hit;
            Transform hitTrans;
            if (Bullets.Count > MaxBulletUpdatesPerFrame)
            {
                bulletListTmp = Bullets.ToArray();
            }

            //Rigidbody hitRigid;
            //LivingEntityCollider hitLivingEntity;
            BallisticObject hitBallisticObject;

            //is windy?
            bool isWindy = Settings.WindDirection.sqrMagnitude > 0;

            float leftOverFlyTime = 0;
            float myDeltaTime = 0;
            bool processAgain = true;

#if (UNITY_EDITOR && BULLET_DEBUGGER)
            BulletDebugger debugger = null;
#endif

            //Process each Bullet
            for (int i = 0; i < Bullets.Count; i++)
            {
                if (i < MaxBulletUpdatesPerFrame)
                {
                    cBullet = Bullets.Dequeue();

                    cWeapon = cBullet.parentWeapon;

#if (UNITY_EDITOR && BULLET_DEBUGGER) 
                    if (cBullet.BulletTrans != null)
                    {
                        debugger = cBullet.BulletTrans.GetComponent<BulletDebugger>();
                    }
                    else
                    {
                        debugger = null;
                    }
#endif

                    processAgain = true;
                    while (processAgain)
                    {
                        //when current bullet passed processing already but has 'flytime over'
                        if (leftOverFlyTime == 0)
                        {
                            myDeltaTime = deltaTime;
                        }
                        else
                        {
                            myDeltaTime = leftOverFlyTime;
                            leftOverFlyTime = 0;
                        }

                        //when this bullet had to wait in the query for more than 1 frame
                        if (cBullet.timeSinceLastUpdate > 0)
                        {
                            myDeltaTime += cBullet.timeSinceLastUpdate;
                            cBullet.timeSinceLastUpdate = 0;
                        }

                        //decrease Lifetime
                        cBullet.lifeTime -= myDeltaTime;
                        if (cBullet.lifeTime <= 0)
                        {
                            //remove Bullet if dead
                            DeactivateBullet(cWeapon, cBullet.BulletTrans);
                            processAgain = false;
                            continue;
                        }

                        if (Settings.useBulletdrag)
                        {
                            //Air resistence
                            cBullet.Speed -= cWeapon.PreDrag * ((cBullet.Speed * cBullet.Speed)) * myDeltaTime;
                            //Wind Influence
                            if (isWindy)
                            {
                                cBullet.bulletDir += ((Settings.WindDirection * cWeapon.DragCoefficient * Settings.AirDensity) / cBullet.Speed) * myDeltaTime;
                                cBullet.bulletDir.Normalize();
                            }
                        }


                        //Move Bullet
#if (UNITY_EDITOR && BULLET_DEBUGGER)
                        if (debugger != null)
                        {
                            debugger.AddPos(cBullet.bulletPos);
                        }
#endif

                        cBullet.bulletPos += cBullet.bulletDir * cBullet.Speed * myDeltaTime;

                        if (Settings.useBulletdrop)
                        {
                            //Bulletdrop
                            cBullet.ySpeed += g * myDeltaTime;

                            if (Settings.useBulletdrag)
                            {
                                cBullet.ySpeed += cWeapon.PreDrag * ((cBullet.ySpeed * cBullet.ySpeed)) * myDeltaTime;
                            }

                            cBullet.bulletPos.y += cBullet.ySpeed * myDeltaTime;
                        }

                        //Hitcheck------------------
                        Vector3 realBulletDir = cBullet.bulletPos - cBullet.LastPos;
                        float dirMag = realBulletDir.magnitude;
                        //bullet speed + fall speed
                        float rBulletSpeed = dirMag / myDeltaTime;
                        ray = new Ray(cBullet.LastPos, realBulletDir);

                        if (Physics.Raycast(ray, out hit, dirMag, cWeapon.HitMask))
                        {
                            hitTrans = hit.transform;
                            hitBallisticObject = null;

                            hitBallisticObject = Settings.useBallisticMaterials ? hitTrans.GetComponent<BallisticObject>() : null;

                            if ((hitBallisticObject != null) ? hitBallisticObject.MatID >= Settings.MaterialData.Count : true)
                            {
                                //hit object has no ballistic material
#if (UNITY_EDITOR && BULLET_DEBUGGER)
                                if (debugger != null)
                                {
                                    debugger.AddPos(hit.point);
                                }
#endif

                                //Stop Bullet
                                DeactivateBullet(cWeapon, cBullet.BulletTrans);
                                processAgain = false;
                                continue;
                            }
                            else
                            {
                                BallisticObjectData hitData = Settings.MaterialData[hitBallisticObject.MatID];

                                Vector3 bulletDirNormalized = realBulletDir.normalized;

                                //does this bullet ricochet?
                                if (hitData.RicochetPropability.Evaluate(Vector3.Angle(hit.normal, -bulletDirNormalized) / 90f) < Random.Range(0f, 1f))
                                {

                                    //maximal material penetration distance of this bullet
                                    float MaxRange = (0.5f * cWeapon.BulletMass * rBulletSpeed * rBulletSpeed) / hitData.EnergylossPerUnit;

                                    //backtrace the bulletpath to find out whether the bullet went through the material
                                    ray = new Ray(hit.point + bulletDirNormalized * MaxRange, -bulletDirNormalized);
                                    RaycastHit[] hits = Physics.RaycastAll(ray, MaxRange, cWeapon.HitMask);


                                    int OutIndex = -1;
                                    float shortestDist = 0f;
                                    for (int n = 0; n < hits.Length; n++)
                                    {
                                        if (hits[n].transform == hitTrans)
                                        {
                                            //backwards
                                            if (hits[n].distance > shortestDist)
                                            {
                                                OutIndex = n;
                                                shortestDist = hits[n].distance;
                                            }
                                        }
                                    }

                                    if (OutIndex != -1)
                                    {
                                        //Shoot through
                                        RaycastHit outHit = hits[OutIndex];

                                        //slowdown
                                        float dist = (outHit.point - hit.point).magnitude;
                                        float fac = 1 - (dist / MaxRange);
                                        float afterSpeed = rBulletSpeed * (1-fac);
                                        cBullet.ySpeed *= fac;
                                        cBullet.Speed *= fac;

                                        //Spread
                                        cBullet.bulletDir = hitData.RndSpread > 0 ? (Quaternion.AngleAxis(Random.Range(0f, 360f), cBullet.bulletDir) * Quaternion.AngleAxis(Random.Range(0f, hitData.RndSpread), Vector3.Cross(Vector3.up, cBullet.bulletDir)) * cBullet.bulletDir) : cBullet.bulletDir;


                                        //process Bullet again
                                        leftOverFlyTime = ((cBullet.bulletPos - outHit.point).magnitude / dirMag) * myDeltaTime;

                                        //call BulletImpact on the hitBallisticObject
                                        hitBallisticObject.BulletImpact(cWeapon, bulletDirNormalized, rBulletSpeed, afterSpeed, dist, hit, hitData);

                                        //bullet exit
                                        hitBallisticObject.BulletImpact(cWeapon, bulletDirNormalized, afterSpeed, afterSpeed, dist, outHit, hitData);

                                        cBullet.bulletPos = cBullet.LastPos = outHit.point;

#if (UNITY_EDITOR && BULLET_DEBUGGER)
                                        if (debugger != null)
                                        {
                                            debugger.AddPos(cBullet.bulletPos);
                                        }
#endif

                                        processAgain = true;
                                        continue;
                                    }
                                    else
                                    {
                                        //Bullet stuck in object


                                        //call BulletImpact on the hitBallisticObject
                                        hitBallisticObject.BulletImpact(cWeapon, bulletDirNormalized, rBulletSpeed, 0, MaxRange, hit, hitData);

#if (UNITY_EDITOR && BULLET_DEBUGGER)
                                        if (debugger != null)
                                        {
                                            debugger.AddPos(hit.point);
                                        }
#endif

                                        //Stop Bullet
                                        DeactivateBullet(cWeapon, cBullet.BulletTrans);
                                        processAgain = false;
                                        continue;
                                    }
                                }
                                else
                                {
                                    //Ricochet

                                    //Reflect bullet
                                    cBullet.bulletDir = Vector3.Reflect(bulletDirNormalized, hit.normal);

                                    cBullet.bulletDir = hitData.RndSpreadRic > 0 ? (Quaternion.AngleAxis(Random.Range(0f, 360f), cBullet.bulletDir) * Quaternion.AngleAxis(Random.Range(0f, hitData.RndSpreadRic), Vector3.Cross(hit.normal, cBullet.bulletDir)) * cBullet.bulletDir) : cBullet.bulletDir;

                                    //Slowdown
                                    float fac = 1f - (Vector3.Angle(bulletDirNormalized, cBullet.bulletDir) / 180f);
                                    cBullet.Speed = cBullet.Speed * fac;
                                    cBullet.ySpeed = 0;

                                    leftOverFlyTime = ((hit.point - cBullet.bulletPos).magnitude / (myDeltaTime * rBulletSpeed)) * myDeltaTime;

                                    //call BulletImpact on the hitBallisticObject
                                    hitBallisticObject.BulletImpact(cWeapon, -hit.normal * fac, rBulletSpeed, rBulletSpeed * fac, 0.1f, hit, hitData);

                                    //process Bullet again
                                    cBullet.bulletPos = cBullet.LastPos = hit.point;

#if (UNITY_EDITOR && BULLET_DEBUGGER)
                                    if (debugger != null)
                                    {
                                        debugger.AddPos(cBullet.bulletPos);
                                    }
#endif

                                    processAgain = true;
                                    continue;
                                }
                            }
                        }
                        processAgain = false;

                        cBullet.LastPos = cBullet.bulletPos;
                        //Update Transform
                        if (cBullet.BulletTrans != null)
                        {
                            if (cBullet.StartLifeTime - cBullet.lifeTime < VisualBulletToRealBulletMovementTime)
                            {
                                cBullet.VisualOffset = Vector3.Lerp(cBullet.StartOffset, Vector3.zero, (cBullet.StartLifeTime - cBullet.lifeTime) / VisualBulletToRealBulletMovementTime);
                            }
                            else
                            {
                                cBullet.VisualOffset = Vector3.zero;
                            }
                            cBullet.BulletTrans.position = cBullet.bulletPos + cBullet.VisualOffset;

                            cBullet.BulletTrans.rotation = Quaternion.LookRotation(realBulletDir);
                        }
                        //Enqueue at End
                        Bullets.Enqueue(cBullet);
                    }
                }
                else
                {
                    bulletListTmp[i - MaxBulletUpdatesPerFrame].timeSinceLastUpdate += Time.deltaTime;
                }
            }
        }


        /// <summary>
        /// add bullet back to pool and make it invisible
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="bulletTrans"></param>
        void DeactivateBullet(Weapon weapon, Transform bulletTrans)
        {
            if (bulletTrans != null)
            {
                GameObject go = bulletTrans.gameObject;
                go.SetActive(false);
                myPool.AddObject(weapon.BulletPref.gameObject, go);
            }
        }
    }


    public class BulletData
    {
        public Weapon parentWeapon;
        public Vector3 bulletPos;
        public Vector3 LastPos;
        public Vector3 bulletDir;
        public Vector3 VisualOffset;
        public float lifeTime;
        public float StartLifeTime;
        public Vector3 StartOffset;
        public float Speed;
        public float ySpeed;
        public Transform BulletTrans;

        public float timeSinceLastUpdate;

        public BulletData(Weapon weapon, Vector3 pos, Vector3 offset, Vector3 dir, float life, float speed, Transform bC)
        {
            parentWeapon = weapon;
            bulletPos = pos;
            LastPos = pos;
            StartOffset = offset;
            lifeTime = life;
            StartLifeTime = lifeTime;
            VisualOffset = offset;
            bulletDir = dir;
            Speed = speed;
            ySpeed = 0;
            BulletTrans = bC;
        }
    }
}
