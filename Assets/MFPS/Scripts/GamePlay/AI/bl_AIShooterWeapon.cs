using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_AIShooterWeapon : bl_PhotonHelper
{
    [Header("Settings")]
    public float AttackRate = 3;
    public float Damage = 20; // The damage AI give
    public int BulletsPerClip = 30;
    [Range(0, 5)] public int Grenades = 3;
    [SerializeField, Range(10, 100)] private float GrenadeSpeed = 50;

    [Header("References")]
    [SerializeField] private Transform FirePoint;
    [SerializeField] private GameObject Bullet;
    [SerializeField] private GameObject Grenade;
    [SerializeField] private AudioClip FireAudio;
    [SerializeField] private AudioClip ReloadSound;
    [SerializeField] private AudioSource FireSource;
    [SerializeField] private ParticleSystem MuzzleFlash;

    private int bullets;
    private bool canFire = true;
    private float attackTime;
    private int FollowingShoots = 0;
    public bool isFiring { get; set; }
    private bl_AIShooterAgent AI;
    private Animator Anim;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (PhotonNetwork.isMasterClient)
        {
            bullets = BulletsPerClip;
        }
        AI = GetComponent<bl_AIShooterAgent>();
        Anim = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        attackTime = Time.time;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Fire()
    {
        if (!canFire)
            return;
        if (!AI.playerInFront)
            return;

        if (Time.time >= attackTime)
        {
            if (Grenades > 0 && AI.TargetDistance >= 10 && FollowingShoots > 5)
            {
                if ((Random.Range(0, 70) > 63))
                {
                    StartCoroutine(ThrowGrenade(false, Vector3.zero, Vector3.zero));
                    attackTime = Time.time + 3.3f;
                    return;
                }
            }
            Anim.SetInteger("UpperState", 1);
            attackTime = Time.time + AttackRate;
            GameObject bullet = Instantiate(Bullet, FirePoint.position, transform.root.rotation) as GameObject;
            bullet.transform.LookAt(AI.TargetPosition);
            bl_BulletSettings info = new bl_BulletSettings();
            info.Damage = 10;
            info.isNetwork = false;
            info.Position = transform.position;
            info.WeaponID = 0;
            info.Spread = 2f;
            info.MaxSpread = 3f;
            info.Speed = 200;
            info.LifeTime = 10;
            info.WeaponName = "[Killed]";
            bullet.GetComponent<bl_Bullet>().SetUp(info);
            bullet.GetComponent<bl_Bullet>().AISetUp(AI.AIName, photonView.viewID);
            FireSource.pitch = Random.Range(0.85f, 1.1f);
            FireSource.clip = FireAudio;
            FireSource.Play();
            if (MuzzleFlash != null) { MuzzleFlash.Play(); }
            bullets--;
            FollowingShoots++;
            photonView.RPC("RpcFire", PhotonTargets.Others, FirePoint.position, AI.TargetPosition);
            if (bullets <= 0)
            {
                canFire = false;
                StartCoroutine(Reload());
            }
            else
            {
                if (FollowingShoots > 5)
                {
                    if (Random.Range(0, 15) > 12)
                    {
                        attackTime += Random.Range(0.01f, 5);
                        FollowingShoots = 0;
                    }
                }
            }
            isFiring = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator ThrowGrenade(bool network, Vector3 velocity, Vector3 forward)
    {
        Anim.SetInteger("UpperState", 2);
        Anim.SetTrigger("Grenade");
        attackTime = Time.time + AttackRate;
        yield return new WaitForSeconds(0.7f);
        GameObject bullet = Instantiate(Grenade, FirePoint.position, transform.root.rotation) as GameObject;

        bl_BulletSettings info = new bl_BulletSettings();
        info.Damage = 100;
        info.isNetwork = false;
        info.Position = transform.position;
        info.WeaponID = 0;
        info.Spread = 2f;
        info.MaxSpread = 3f;
        info.Speed = GrenadeSpeed;
        info.LifeTime = 5;
        info.WeaponName = "AI";
        bullet.GetComponent<bl_Grenade>().SetUp(info);
        bullet.GetComponent<bl_Grenade>().AISetUp(photonView.viewID);
        if (!network)
        {
            Rigidbody r = bullet.GetComponent<Rigidbody>();
            velocity = GetVelocity(AI.TargetPosition);
            r.velocity = velocity;
            r.AddRelativeTorque(Vector3.right * -5500.0f);
            forward = AI.TargetPosition - r.transform.position;
            r.transform.forward = forward;
            Grenades--;
            photonView.RPC("FireGrenadeRPC", PhotonTargets.Others, velocity, forward);
        }
        else
        {
            Rigidbody r = bullet.GetComponent<Rigidbody>();
            r.velocity = velocity;
            r.AddRelativeTorque(Vector3.right * -5500.0f);
            r.transform.forward = forward;
        }
    }

    [PunRPC]
    void FireGrenadeRPC(Vector3 velocity, Vector3 forward)
    {
        StartCoroutine(ThrowGrenade(true, velocity, forward));
    }

    private Vector3 GetVelocity(Vector3 target)
    {
        Vector3 velocity = Vector3.zero;
        Vector3 toTarget = target - transform.position;
        float speed = 15;
        // Set up the terms we need to solve the quadratic equations.
        float gSquared = Physics.gravity.sqrMagnitude;
        float b = speed * speed + Vector3.Dot(toTarget, Physics.gravity);
        float discriminant = b * b - gSquared * toTarget.sqrMagnitude;

        // Check whether the target is reachable at max speed or less.
        if (discriminant < 0)
        {
            velocity = toTarget;
            velocity.y = 0;
            velocity.Normalize();
            velocity.y = 0.7f;

            Debug.DrawRay(transform.position, velocity * 3.0f, Color.blue);

            velocity *= speed;
            return velocity;
        }

        float discRoot = Mathf.Sqrt(discriminant);

        // Highest shot with the given max speed:
        float T_max = Mathf.Sqrt((b + discRoot) * 2f / gSquared);

        float T = 0;
        T = T_max;


        // Convert from time-to-hit to a launch velocity:
        velocity = toTarget / T - Physics.gravity * T / 2f;

        return velocity;
    }

    [PunRPC]
    void RpcFire(Vector3 pos, Vector3 look)
    {
        Anim.SetInteger("UpperState", 1);
        GameObject bullet = Instantiate(Bullet, pos, Quaternion.identity) as GameObject;
        bullet.transform.LookAt(look);
        bl_BulletSettings info = new bl_BulletSettings();
        info.Damage = 0;
        info.isNetwork = true;
        info.Position = transform.position;
        info.WeaponID = 0;
        info.Spread = 2f;
        info.MaxSpread = 3f;
        info.Speed = 200;
        info.LifeTime = 10;
        info.WeaponName = "";
        bullet.GetComponent<bl_Bullet>().SetUp(info);
       //  bullet.GetComponent<bl_Bullet>().AIFrom = AIName;
        FireSource.pitch = Random.Range(0.85f, 1.1f);
        FireSource.clip = FireAudio;
        FireSource.Play();
        if (MuzzleFlash != null) { MuzzleFlash.Play(); }
    }

    IEnumerator Reload()
    {
        photonView.RPC("RpcReload", PhotonTargets.Others);
        yield return new WaitForSeconds(0.25f);
        Anim.SetInteger("UpperState", 2);
        FireSource.clip = ReloadSound;
        FireSource.Play();
        yield return new WaitForSeconds(2.7f);
        Anim.SetInteger("UpperState", 0);
        bullets = BulletsPerClip;
        canFire = true;
    }

    [PunRPC]
    IEnumerator RpcReload()
    {
        Anim.SetInteger("UpperState", 2);
        FireSource.clip = ReloadSound;
        FireSource.Play();
        yield return new WaitForSeconds(2.7f);
        Anim.SetInteger("UpperState", 0);
    }
}