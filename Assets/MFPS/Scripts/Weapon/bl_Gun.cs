////////////////////////////////////////////////////////////////////////////////
// bl_Gun.cs
//
// Weapon's logic script
// 
//                        Lovatto Studio
////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class bl_Gun : bl_MonoBehaviour
{
    [HideInInspector]
    public bool CanFire = true;

    public bl_GunInfo Info;
    public int GunID;

    public float CrossHairScale = 8;
    // basic weapon variables all guns have in common
    public bool SoundReloadByAnim = false;
    public AudioClip TakeSound;
    public AudioClip FireSound;
    public AudioClip DryFireSound;
    public AudioClip ReloadSound;
    public AudioClip ReloadSound2 = null;
    public AudioClip ReloadSound3 = null;
    public AudioSource DelaySource = null;
    // Objects, effects and tracers
    public GameObject bullet = null;        // the weapons bullet object
    public GameObject grenade = null;       // the grenade style round... this can also be used for arrows or similar rounds
    public GameObject rocket = null;        // the rocket round
    public ParticleSystem muzzleFlash = null;     // the muzzle flash for this weapon
    public Transform muzzlePoint = null;    // the muzzle point of this weapon
    public ParticleSystem shell = null;          // the weapons empty shell particle
    public GameObject impactEffect = null;  // impact effect, used for raycast bullet types 
    public Vector3 AimPosition; //position of gun when is aimed
    private Vector3 DefaultPos;
    private Vector3 CurrentPos;
    [HideInInspector]
    public bool isAmed;
    private bool CanAim;
    public bool useSmooth = true;
    public float AimSmooth;
    public float ShakeIntense = 0.03f;
    [Range(0, 179)]
    public float AimFog = 50;
    private float DefaultFog;
    private float CurrentFog;
    private float DeafultSmoothSway_;
    private float DefaultAmountSway;

    public float AimSway = 0.0f;
    private float DefaultSway;

    public bool CanAuto = true;
    public bool CanSemi = true;
    public bool CanSingle = true;

    //Machine gun Vars
    [HideInInspector]
    public bool isFiring = false;          // is the machine gun firing?  used for decreasing accuracy while sustaining fire

    //Shotgun Specific Vars
    public int pelletsPerShot = 10;         // number of pellets per round fired for the shotgun
    public float delayForSecondFireSound = 0.45f;

    //Burst Specific Vars
    public int roundsPerBurst = 3;          // number of rounds per burst fire
    public float lagBetweenShots = 0.5f;    // time between each shot in a burst
    private bool isBursting = false;
    //Launcher Specific Vars
    public List<GameObject> OnAmmoLauncher = new List<GameObject>();

    public int impactForce = 50;            // how much force applied to a rigid body
    public float bulletSpeed = 200.0f;      // how fast are your bullets
    public bool AutoReload = true;
    public bool SplitReloadAnimation = false;
    public int bulletsPerClip = 50;         // number of bullets in each clip
    public int numberOfClips = 5;           // number of clips you start with
    public int maxNumberOfClips = 10;       // maximum number of clips you can hold
    [HideInInspector]public int bulletsLeft;                // bullets in the gun-- current clip
    public float DelayFire = 0.85f;
    public float baseSpread = 1.0f;         // how accurate the weapon starts out... smaller the number the more accurate
    public float maxSpread = 4.0f;          // maximum inaccuracy for the weapon
    public float spreadPerSecond = 0.2f;    // if trigger held down, increase the spread of bullets
    public float spread = 0.0f;             // current spread of the gun
    public float decreaseSpreadPerSec = 0.5f;// amount of accuracy regained per frame when the gun isn't being fired 
    public float AimSwayAmount = 0.01f;
    private float DefaultSpreat;
    private float DefaultMaxSpread;
    [HideInInspector] public bool isReloading = false;       // am I in the process of reloading
    // used for tracer rendering
    public int shotsFired = 0;              // shots fired since last tracer round
    public int roundsPerTracer = 1;         // number of rounds per tracer
    private float nextFireTime = 0.0f;      // able to fire again on this frame
    // Recoil
    public float RecoilAmount = 5.0f;
    public float RecoilSpeed = 2;

    private bool m_enable = true;
    private bl_GunBob GunBob;
    private bl_DelaySmooth SwayGun = null;
    private bl_SyncWeapon Sync;
    private bl_Recoil RecoilManager;
    private bl_RoomMenu RoomMenu;
    private bl_UCrosshair Crosshair;
#if MFPSM
    private bl_TouchHelper TouchHelper;
    private bl_AutoWeaponFire AutoFire;
#endif
    
    private bool activeGrenade = true;
    private bool alreadyKnife = false;
    private AudioSource Source;
    private Camera WeaponCamera;
    private Text FireTypeText;
    private bool inReloadMode = false;
    private AmmunitionType AmmoType = AmmunitionType.Bullets;
    private Camera PlayerCamera;
    private AudioSource FireSource = null;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        GunBob = transform.root.GetComponentInChildren<bl_GunBob>();
        SwayGun = this.transform.root.GetComponentInChildren<bl_DelaySmooth>();
        Sync = this.transform.root.GetComponentInChildren<bl_SyncWeapon>();
        RecoilManager = transform.root.GetComponentInChildren<bl_Recoil>();
        if(FireSource == null) { FireSource = gameObject.AddComponent<AudioSource>(); FireSource.playOnAwake = false; }
        Crosshair = bl_UCrosshair.Instance;
        RoomMenu = FindObjectOfType<bl_RoomMenu>();
        Source = GetComponent<AudioSource>();
        if (bl_UIReferences.Instance != null)
        {
            FireTypeText = bl_UIReferences.Instance.FireTypeText;
        }
        PlayerCamera = transform.root.GetComponent<bl_PlayerSettings>().PlayerCamera;
#if MFPSM
         TouchHelper = bl_TouchHelper.Instance;
         AutoFire = FindObjectOfType<bl_AutoWeaponFire>();
#endif
        DefaultSpreat = baseSpread;
        DefaultMaxSpread = maxSpread;
        AmmoType = bl_GameData.Instance.AmmoType;
        Setup();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        Source.clip = TakeSound;
        Source.Play();
        if (Animat)
        {
            Animat.DrawWeapon();
        }
        CanFire = true;
        CanAim = true;
        bl_EventHandler.OnKitAmmo += this.OnPickUpAmmo;
        bl_EventHandler.OnRoundEnd += this.OnRoundEnd;
#if MFPSM
        if (bl_UtilityHelper.isMobile)
        {
            bl_TouchHelper.OnFireClick += OnFire;
            bl_TouchHelper.OnReload += OnReload;
        }
#endif
        if (Info.Type == GunType.Grenade || Info.Type == GunType.Shotgun)
        {
            Crosshair.Change(2);
        }
        else if (Info.Type == GunType.Knife)
        {
            Crosshair.Change(1);
        }
        else
        {
            Crosshair.Change(0);
        }
        if (inReloadMode) { StartCoroutine(reload(0.2f)); }
        SetFireTypeName(); 
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.OnKitAmmo -= this.OnPickUpAmmo;
        bl_EventHandler.OnRoundEnd -= this.OnRoundEnd;
#if MFPSM
        if (bl_UtilityHelper.isMobile)
        {
            bl_TouchHelper.OnFireClick -= OnFire;
            bl_TouchHelper.OnReload -= OnReload;
        }
#endif
        isAmed = false;
        if (PlayerCamera == null) { PlayerCamera = transform.root.GetComponent<bl_PlayerSettings>().PlayerCamera; }
        PlayerCamera.fieldOfView = DefaultFog;
        StopAllCoroutines();
        if (isReloading) { inReloadMode = true; isReloading = false; }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Setup(bool initial = false)
    {
        bulletsLeft = bulletsPerClip; // load gun on startup
        DefaultPos = transform.localPosition;
        if(PlayerCamera == null) { PlayerCamera = transform.root.GetComponent<bl_PlayerSettings>().PlayerCamera; }
        WeaponCamera = PlayerCamera.transform.GetChild(0).GetComponent<Camera>();
        DefaultFog = PlayerCamera.fieldOfView;
        if (!initial)
        {
            DefaultSway = GunBob.bobbingAmount;
            DefaultAmountSway = SwayGun.amount;
            DeafultSmoothSway_ = SwayGun.smooth;
            if (AmmoType == AmmunitionType.Bullets)
            {
                numberOfClips = bulletsPerClip * numberOfClips;
            }
        }
        CanAim = true;
        Info = bl_GameData.Instance.GetWeapon(GunID);
    }

    /// <summary>
    /// check what the player is doing every frame
    /// </summary>
    /// <returns></returns>
    public override void OnUpdate()
    {
        if (!bl_UtilityHelper.GetCursorState)
        {
            if (RoomMenu != null && WeaponCamera != null)
            {
                WeaponCamera.fieldOfView = RoomMenu.WeaponCameraFog;
            }
            return;
        }
        if (!m_enable)
            return;

        InputUpdate();
        Aim();
        SyncState();

        if (isFiring) // if the gun is firing
        {
            spread += spreadPerSecond; // gun is less accurate with the trigger held down
        }
        else
        {
            spread -= decreaseSpreadPerSec; // gun regains accuracy when trigger is released
        }

        if (Info.Type == GunType.Grenade)
        {
            OnLauncherNotAmmo();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnLateUpdate()
    {
        if (spread >= maxSpread)
        {
            spread = maxSpread;  //if current spread is greater then max... set to max
        }
        else
        {
            if (spread <= baseSpread)
            {
                spread = baseSpread; //if current spread is less then base, set to base
            }
        }
    }

    /// <summary>
    /// All Input events 
    /// </summary>
    void InputUpdate()
    {
        // Did the user press fire.... and what kind of weapon are they using ?  ===============
        if (bl_UtilityHelper.isMobile)
        {
#if MFPSM
            if (bl_GameData.Instance.AutoWeaponFire && AutoFire != null)
            {
                HandleAutoFire();
            }
            else
            {
                if (Info.Type == GunType.Machinegun && TouchHelper != null)
                {
                    if (TouchHelper.FireDown && m_CanFire)
                    {
                        MachineGun_Fire();   // fire machine gun                 
                    }
                }
            }
#endif
        }
        else
        {
#if MFPSM
            if (bl_GameData.Instance.AutoWeaponFire && AutoFire != null)
            {
                HandleAutoFire();
            }
#endif
            if (Input.GetMouseButtonDown(0) && !m_CanFire)
            {
                if (Info.Type != GunType.Knife && DryFireSound != null && !isReloading)
                {
                    Source.clip = DryFireSound;
                    Source.Play();
                }
            }
            switch (Info.Type)
            {
                case GunType.Shotgun:
                    if (FireButtonDown && m_CanFire)
                    {
                        ShotGun_Fire();  // fire shotgun
                    }
                    break;
                case GunType.Machinegun:
                    if (FireButton && m_CanFire)
                    {
                        MachineGun_Fire();   // fire machine gun                 
                    }
                    break;
                case GunType.Burst:
                    if (FireButtonDown && m_CanFire && !isBursting)
                    {
                        StartCoroutine(Burst_Fire()); // fire off a burst of rounds                   
                    }
                    break;

                case GunType.Grenade:
                    if (FireButtonDown && m_CanFire && !grenadeFired)
                    {
                        GrenadeFire();
                    }
                    break;
                case GunType.Pistol:
                    if (FireButtonDown && m_CanFire)
                    {
                        MachineGun_Fire();   // fire Pistol gun     
                    }
                    break;
                case GunType.Sniper:
                    if (FireButtonDown && m_CanFire)
                    {
                        Sniper_Fire();
                    }
                    break;
                case GunType.Knife:
                    if (FireButtonDown && m_CanFire && !alreadyKnife)
                    {
                        Knife_Fire();
                    }
                    break;
                default:
                    Debug.LogWarning("Unknown gun type");
                    break;
            }
        }

        if (bl_UtilityHelper.isMobile)
        {
#if MFPSM
            isAmed = TouchHelper.isAim && m_CamAim;
#endif
        }
        else
        {
            isAmed = AimButton && m_CamAim;
        }

        if (bl_UtilityHelper.GetCursorState)
        {
            Crosshair.OnAim(isAmed);
        }
        bool inputReload = Input.GetKeyDown(KeyCode.R);
#if INPUT_MANAGER
        if(bl_Input.Instance.m_InputType == Lovatto.Asset.InputManager.InputType.Xbox)
        {
            inputReload = bl_Input.GetKeyDown("Reload");
        }
#endif
        if (inputReload && m_CanReload)
        {
            StartCoroutine(reload());
        }
        if (Info.Type == GunType.Machinegun || Info.Type == GunType.Burst || Info.Type == GunType.Pistol)
        {
            ChangeTypeFire();
        }
        //used to decrease weapon accuracy as long as the trigger remains down =====================
        if (Info.Type != GunType.Grenade && Info.Type != GunType.Knife)
        {
            if (bl_UtilityHelper.isMobile)
            {
#if MFPSM
                if (!bl_GameData.Instance.AutoWeaponFire)
                {
                    isFiring = (TouchHelper.FireDown && m_CanFire);
                }
#endif
            }
            else
            {
                isFiring = (FireButton && m_CanFire); // fire is down, gun is firing
            }
        }
    }
    /// <summary>
    /// change the type of gun gust
    /// </summary>
    void ChangeTypeFire()
    {
        bool inp = Input.GetKeyDown(KeyCode.B);
#if INPUT_MANAGER
        inp = ((bl_Input.GetKeyDown("FireType")));
#endif 
        if (inp)
        {
            switch (Info.Type)
            {
                case GunType.Machinegun:
                    if (CanSemi)
                    {
                        Info.Type = GunType.Burst;
                    }else if (CanSingle)
                    {
                        Info.Type = GunType.Pistol;
                    }
                    break;
                case GunType.Burst:
                    if (CanSingle)
                    {
                        Info.Type = GunType.Pistol;
                    }
                    else if (CanAuto)
                    {
                        Info.Type = GunType.Machinegun;
                    }
                    break;
                case GunType.Pistol:
                    if (CanAuto)
                    {
                        Info.Type = GunType.Machinegun;
                    }
                    else if (CanSemi)
                    {
                        Info.Type = GunType.Burst;
                    }
                    break;
            }
            if(Source != null && Source.enabled)
            Source.clip = ReloadSound;//create a custom switch sound
            Source.Play();
            SetFireTypeName();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnReload()
    {
        if (m_CanReload)
        {
            StartCoroutine(reload());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnFire()
    {
        if (!m_CanFire)
        {
            if (Info.Type != GunType.Knife && DryFireSound != null && !isReloading)
            {
                Source.clip = DryFireSound;
                Source.Play();
            }
        }
        if (!CanFire)
            return;

        switch (Info.Type)
        {
            case GunType.Shotgun:
                ShotGun_Fire();  // fire shotgun
                break;
            case GunType.Burst:
                if (!isBursting)
                {
                    StartCoroutine(Burst_Fire()); // fire off a burst of rounds                   
                }
                break;
            case GunType.Grenade:
                if (!grenadeFired && m_CanFire)
                {
                    GrenadeFire();
                }
                break;
            case GunType.Pistol:
                MachineGun_Fire();   // fire Pistol gun                    
                break;
            case GunType.Sniper:
                Sniper_Fire();
                break;
            case GunType.Knife:
                Knife_Fire();
                break;
            default:
                if (Info.Type != GunType.Machinegun)
                {
                    Debug.LogWarning("Unknown gun type");
                }
                break;
        }
    }


    void HandleAutoFire()
    {
#if MFPSM
        bool fireDown = AutoFire.Fire();
        isFiring = fireDown;
        if (fireDown)
        {
            switch (Info.Type)
            {
                case GunType.Shotgun:
                    if (m_CanFire)
                    {
                        ShotGun_Fire();  // fire shotgun
                    }
                    break;
                case GunType.Machinegun:
                    if (m_CanFire)
                    {
                        MachineGun_Fire();   // fire machine gun                 
                    }
                    break;
                case GunType.Burst:
                    if (m_CanFire && !isBursting)
                    {
                        StartCoroutine(Burst_Fire()); // fire off a burst of rounds                   
                    }
                    break;

                case GunType.Grenade:
                    //grenades should throw manually :)
                    break;
                case GunType.Pistol:
                    if (m_CanFire)
                    {
                        MachineGun_Fire();   // fire Pistol gun     
                    }
                    break;
                case GunType.Sniper:
                    if (m_CanFire)
                    {
                        Sniper_Fire();
                    }
                    break;
                case GunType.Knife:
                    if (m_CanFire && !alreadyKnife)
                    {
                        Knife_Fire();
                    }
                    break;
                default:
                    Debug.LogWarning("Unknown gun type");
                    break;
            }
        }
#endif
    }

    /// <summary>
    /// Sync Weapon state for Upper animations
    /// </summary>
    void SyncState()
    {
        if (PlayerSync == null)
            return;

        if (isFiring && !isReloading)
        {
            PlayerSync.WeaponState = (isAmed) ?  "AimFire" : "Firing";
        }
        else if (isAmed && !isFiring && !isReloading)
        {
            PlayerSync.WeaponState = "Aimed";
        }
        else if (isReloading)
        {
            PlayerSync.WeaponState = "Reloading";
        }
        else if (controller.State == PlayerState.Running && !isReloading && !isFiring && !isAmed)
        {
            PlayerSync.WeaponState = "Running";
        }
        else
        {
            PlayerSync.WeaponState = "Idle";
        }
    }

    /// <summary>
    /// determine the status of the launcher ammo
    /// to decide whether to show or hide the mesh grenade
    /// </summary>
    void OnLauncherNotAmmo()
    {
        foreach (GameObject go in OnAmmoLauncher)
        {
            // if not have more ammo for launcher
            //them desactive the grenade in hands
            if (bulletsLeft <= 0 && !isReloading)// if not have ammo
            {
                go.SetActive(false);
                if (activeGrenade)
                {
                    Sync.SyncOffAmmoGrenade(false);
                    activeGrenade = false;
                }
            }
            else
            {
                go.SetActive(true);
                if (!activeGrenade)
                {
                    Sync.SyncOffAmmoGrenade(true);
                    activeGrenade = true;
                }
            }
        }
    }
    /// <summary>
    /// fire the machine gun
    /// </summary>
    void MachineGun_Fire()
    {

        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            StartCoroutine(FireOneShot());  // fire a physical bullet

            if (Animat != null)
            {
                if (isAmed)
                {
                    Animat.AimFire();
                }
                else
                {
                    Animat.Fire();
                }
            }
            if (Sync)
            {
                Vector3 position = (Info.Type == GunType.Knife) ? PlayerCamera.transform.position : muzzlePoint.position;
                Sync.Firing(GunType.Machinegun.ToString(), spread, position, transform.parent.rotation);
            }
            PlayFireAudio();
            shotsFired++;
            bulletsLeft--;
            nextFireTime += Info.FireRate;
            EjectShell();
            Kick();
            bl_EventHandler.OnLocalPlayerShake(ShakeIntense, 0.25f, 0.03f, isAmed);
            if (muzzleFlash) { muzzleFlash.Play(); }
            //is Auto reload
            if (bulletsLeft <= 0 && numberOfClips > 0 && AutoReload)
            {
                StartCoroutine(reload());
            }
        }

    }
    /// <summary>
    /// fire the sniper gun
    /// </summary>
    void Sniper_Fire()
    {

        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            StartCoroutine(FireOneShot());  // fire a physical bullet

            if (Animat != null)
            {
                Animat.Fire();
            }
            if (Sync)
            {
                Vector3 position = (Info.Type == GunType.Knife) ? PlayerCamera.transform.position : muzzlePoint.position;
                Sync.Firing(GunType.Sniper.ToString(), spread, position, transform.parent.rotation);
            }
            StartCoroutine(DelayFireSound());
            shotsFired++;
            bulletsLeft--;
            nextFireTime += Info.FireRate;
            EjectShell();
            Kick();
            bl_EventHandler.OnLocalPlayerShake(ShakeIntense, 0.25f, 0.03f, isAmed);
            if (!isAmed)
            {
                if (muzzleFlash) { muzzleFlash.Play(); }
            }
            //is Auto reload
            if (bulletsLeft <= 0 && numberOfClips > 0 && AutoReload)
            {
                StartCoroutine(reload(delayForSecondFireSound + 0.2f));
            }
        }

    }

    public void FastKnifeFire(System.Action callBack)
    {
        StartCoroutine(IEFastKnife(callBack));
    }

    IEnumerator IEFastKnife(System.Action callBack)
    {
        float tt = Knife_Fire();
        yield return new WaitForSeconds(tt * 0.7f);
        DisableWeapon(true);
        yield return new WaitForSeconds(0.5f);
        callBack();
    }

    /// <summary>
    /// 
    /// </summary>
    private float Knife_Fire()
    {
        // If there is more than one shot  between the last and this frame
        // Reset the nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        float time = 0;
        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            isFiring = true; // fire is down, gun is firing
            alreadyKnife = true;
            StartCoroutine(KnifeSendFire());

            Vector3 position = PlayerCamera.transform.position;
            Vector3 direction = PlayerCamera.transform.TransformDirection(Vector3.forward);

            RaycastHit hit;
            if (Physics.Raycast(position, direction,out hit, Info.Range))
            {
                if(hit.transform.tag == "BodyPart")
                {
                    if (hit.transform.GetComponent<bl_BodyPart>() != null)
                    {
                        hit.transform.GetComponent<bl_BodyPart>().GetDamage(Info.Damage, PhotonNetwork.player.NickName, Info.Name, transform.position, GunID);
                    }
                }
            }
        
            if (Animat != null)
            {
                time = Animat.KnifeFire();
            }
            else { }
            if (Sync)
            {
                
                Sync.Firing(GunType.Knife.ToString(), 0, position, transform.parent.rotation);
            }
            PlayFireAudio();
            nextFireTime += Info.FireRate;
            Kick();
            bl_EventHandler.OnLocalPlayerShake(ShakeIntense, 0.25f, 0.03f, isAmed);
            Crosshair.OnFire();
            isFiring = false;
        }
        return time;
    }

    /// <summary>
    /// burst shooting
    /// </summary>
    /// <returns></returns>
    IEnumerator Burst_Fire()
    {
        int shotCounter = 0;

        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            while (shotCounter < roundsPerBurst)
            {
                isBursting = true;

                StartCoroutine(FireOneShot());  // fire a physical bullet

                shotCounter++;
                shotsFired++;
                bulletsLeft--; // subtract a bullet 
                Kick();
                EjectShell();
                bl_EventHandler.OnLocalPlayerShake(ShakeIntense, 0.25f, 0.03f, isAmed);
                if (muzzleFlash) { muzzleFlash.Play(); }
                if (Sync)
                {
                    Vector3 position = (Info.Type == GunType.Knife) ? PlayerCamera.transform.position : muzzlePoint.position;
                    Sync.Firing(GunType.Burst.ToString(), spread, position, transform.parent.rotation);
                }
                if (Animat != null)
                {
                    Animat.Fire();
                }
                PlayFireAudio();

                yield return new WaitForSeconds(lagBetweenShots);

            }

            nextFireTime += Info.FireRate;
            //is Auto reload
            if (bulletsLeft <= 0 && numberOfClips > 0 && AutoReload)
            {
                StartCoroutine(reload());
            }
        }
        isBursting = false;
    }

    /// <summary>
    /// fire the shotgun
    /// </summary>
    void ShotGun_Fire()
    {
        int pelletCounter = 0;  // counter used for pellets per round
        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            do
            {
                StartCoroutine(FireOneShot());  // fire a physical bullet

                if (Sync)
                {
                    Vector3 position = (Info.Type == GunType.Knife) ? PlayerCamera.transform.position : muzzlePoint.position;
                    Sync.Firing(GunType.Shotgun.ToString(), spread, position, transform.parent.rotation);
                }
                pelletCounter++; // add another pellet
                shotsFired++; // another shot was fired                
            } while (pelletCounter < pelletsPerShot); // if number of pellets fired is less then pellets per round... fire more pellets

            StartCoroutine(DelayFireSound());
            if (Animat != null)
            {
                Animat.Fire();
            }
            bl_EventHandler.OnLocalPlayerShake(ShakeIntense, 0.25f, 0.03f, isAmed);
            EjectShell(); // eject 1 shell 
            nextFireTime += Info.FireRate;  // can fire another shot in "fire rate" number of frames
            bulletsLeft--; // subtract a bullet
            Kick();
            //is Auto reload
            if (bulletsLeft <= 0 && numberOfClips > 0 && AutoReload)
            {
                StartCoroutine(reload(delayForSecondFireSound + 0.3f));
            }
        }
    }
    /// <summary>
    /// most shotguns have the sound of shooting and then reloading
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayFireSound()
    {
        PlayFireAudio();
        yield return new WaitForSeconds(delayForSecondFireSound);
        if (DelaySource != null)
        {
            DelaySource.clip = ReloadSound3;
            DelaySource.Play();
        }
        else
        {
            Source.clip = ReloadSound3;
            Source.Play();
        }
    }

    void GrenadeFire()
    {
        if (grenadeFired)
            return;

       if(bulletsLeft == 0 && numberOfClips > 0)
        {
            StartCoroutine(reload(1)); // if out of ammo, reload
            return;
        }
        isFiring = true;
        grenadeFired = true;
        StartCoroutine(Launcher_Fire());
    }

    /// <summary>
    /// fire your launcher
    /// </summary>
    private bool grenadeFired = false;
    IEnumerator Launcher_Fire()
    {
        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;
        bool already = false;
        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            if (!already)
            {
                nextFireTime += Info.FireRate;  // can fire another shot in "fire rate" number of frames
                if (Animat != null)
                {
                    Animat.Fire();
                }
                yield return new WaitForSeconds(DelayFire);
                Vector3 angular = (Random.onUnitSphere * 10f);
                StartCoroutine(FireOneProjectile(angular)); // fire 1 round            
                bulletsLeft--; // subtract a bullet
                Kick();
                if (Sync)
                {
                    Vector3 position = (Info.Type == GunType.Knife) ? PlayerCamera.transform.position : muzzlePoint.position;
                    Sync.FiringGrenade(spread, position, transform.parent.rotation, angular);
                }
                PlayFireAudio();
                bl_EventHandler.OnLocalPlayerShake(ShakeIntense, 0.75f, 0.07f, isAmed);
                isFiring = false;
                //is Auto reload
                if (bulletsLeft <= 0 && numberOfClips > 0 && AutoReload)
                {
                    StartCoroutine(reload(1.2f));
                }
                else if (bulletsLeft <= 0 && numberOfClips <= 0)
                {

                }
                already = true;
            }
            else
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// Create and fire a bullet
    /// </summary>
    /// <returns></returns>
    IEnumerator FireOneShot()
    {
        Vector3 position = PlayerCamera.transform.position; // position to spawn bullet is at the muzzle point of the gun       

        // set the gun's info into an array to send to the bullet
        bl_BulletSettings t_info = new bl_BulletSettings();
        t_info.Damage = Info.Damage;
        t_info.ImpactForce = impactForce;
        t_info.MaxSpread = maxSpread;
        t_info.Spread = spread;
        t_info.Speed = bulletSpeed;
        t_info.WeaponName = Info.Name;
        t_info.Position = this.transform.root.position;
        t_info.WeaponID = this.GunID;
        t_info.isNetwork = false;
        t_info.LifeTime = Info.Range;

        //bullet info is set up in start function
        GameObject newBullet = Instantiate(bullet, position, transform.parent.rotation) as GameObject; // create a bullet
        newBullet.GetComponent<bl_Bullet>().SetUp(t_info);// send the gun's info to the bullet
        Crosshair.OnFire();

        if (!(Info.Type == GunType.Grenade))
        {
            if (shotsFired >= roundsPerTracer) // tracer round every so many rounds fired... is there a tracer this round fired?
            {
                if (newBullet.GetComponent<Renderer>() != null)
                {
                    newBullet.GetComponent<Renderer>().enabled = true; // turn on tracer effect
                }
                shotsFired = 0;                    // reset tracer counter
            }
            else
            {
                if (newBullet.GetComponent<Renderer>() != null)
                {
                    newBullet.GetComponent<Renderer>().enabled = false; // turn off tracer effect
                }
            }
            Source.clip = FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }

        if ((bulletsLeft == 0))
        {
            StartCoroutine(reload());  // if out of bullets.... reload
            yield break;
        }

    }


    /// <summary>
    /// Create and Fire 1 launcher projectile
    /// </summary>
    /// <returns></returns>
    IEnumerator FireOneProjectile(Vector3 angular)
    {
        Vector3 position = muzzlePoint.position; // position to spawn rocket / grenade is at the muzzle point of the gun

        bl_BulletSettings t_info = new bl_BulletSettings();
        t_info.Damage = Info.Damage;
        t_info.ImpactForce = impactForce;
        t_info.MaxSpread = maxSpread;
        t_info.Spread = spread;
        t_info.Speed = bulletSpeed;
        t_info.WeaponName = Info.Name;
        t_info.Position = this.transform.root.position;
        t_info.WeaponID = GunID;
        t_info.isNetwork = false;

        //Instantiate grenade
        GameObject newNoobTube = Instantiate(grenade, position, transform.parent.rotation) as GameObject;
        if (newNoobTube.GetComponent<Rigidbody>() != null)//if grenade have a rigidbody,then apply velocity
        {
            newNoobTube.GetComponent<Rigidbody>().angularVelocity = angular;
        }
        newNoobTube.GetComponent<bl_Grenade>().SetUp(t_info);// send the gun's info to the grenade    

        if ((bulletsLeft == 0 && numberOfClips > 0))
        {
            StartCoroutine(reload());  // if out of bullets.... reload
            yield break;
        }
        grenadeFired = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void EjectShell()
    {
        if (shell != null)
        {
            shell.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void PlayFireAudio()
    {
        FireSource.clip = FireSound;
        FireSource.spread = Random.Range(1.0f, 1.5f);
        FireSource.pitch = Random.Range(1.0f, 1.05f);
        FireSource.Play();
    }

    // tracer rounds for raycast bullets
    void FireOneTracer(bl_BulletSettings info)
    {
        Vector3 position = muzzlePoint.position;
        GameObject newTracer = Instantiate(bullet, position, transform.parent.rotation) as GameObject; // create a bullet
        newTracer.GetComponent<bl_Bullet>().SetUp(info);
        newTracer.GetComponent<bl_Bullet>().SetTracer();  // tell the bullet it is only a tracer
    }

    /// <summary>
    /// effects for raycast bullets
    /// </summary>
    /// <param name="hit"></param>
    void ShowHits(RaycastHit hit,bool isKinf)
    {
        switch (hit.transform.tag)
        {
            case "bullet":
                // do nothing if 2 bullets collide
                break;
            case "BodyPart":
                if (hit.transform.GetComponent<bl_BodyPart>() != null)
                {
                    hit.transform.GetComponent<bl_BodyPart>().GetDamage(Info.Damage, PhotonNetwork.player.NickName, Info.Name, this.transform.root.position, GunID);
                }
                break;
            case "Wood":
                // add wood impact effects
                break;
            case "Concrete":
                // add concrete impact effect
                break;
            case "Dirt":
                // add dirt or ground  impact effect
                break;
            default: // default impact effect and bullet hole
                if (!isKinf)
                {
                    Instantiate(impactEffect, hit.point + 0.1f * hit.normal, Quaternion.FromToRotation(Vector3.up, hit.normal));
                }
                break;
        }
    }
    /// <summary>
    /// ADS
    /// </summary>
    void Aim()
    {
        if (isAmed && !isReloading)
        {
            CurrentPos = AimPosition; //Place in the center ADS
            CurrentFog = AimFog; //create a zoom camera
            GunBob.transform.localPosition = Vector3.zero; //Fix position of gun
            GunBob.bobbingAmount = AimSway; //setting the sway of weapons
            SwayGun.smooth = DeafultSmoothSway_ * 2.5f;
            SwayGun.amount = AimSwayAmount;
            baseSpread = Info.Type == GunType.Sniper ? 0.01f : DefaultSpreat / 2f;//if sniper more accuracy
            maxSpread = Info.Type == GunType.Sniper ? 0.01f : DefaultMaxSpread / 2; //add more accuracy when is aimed
        }
        else // if not aimed
        {
            CurrentPos = DefaultPos; //return to default gun position       
            CurrentFog = DefaultFog; //return to default fog
            GunBob.bobbingAmount = DefaultSway; //enable the gun bob
            SwayGun.smooth = DeafultSmoothSway_;
            SwayGun.amount = DefaultAmountSway;
            baseSpread = DefaultSpreat; //return to default spread
            maxSpread = DefaultMaxSpread; //return to default max spread
        }
        //apply position
        transform.localPosition = useSmooth ? Vector3.Lerp(transform.localPosition, CurrentPos, Time.deltaTime * AimSmooth) : //with smooth effect
        Vector3.MoveTowards(transform.localPosition, CurrentPos, Time.deltaTime * AimSmooth); // with snap effect

        if (PlayerCamera != null)
        {
            PlayerCamera.fieldOfView = useSmooth ? Mathf.Lerp(PlayerCamera.fieldOfView, CurrentFog, Time.deltaTime * (AimSmooth * 3)) : //apply fog distance
             Mathf.Lerp(PlayerCamera.fieldOfView, CurrentFog, Time.deltaTime * AimSmooth);
        }
    }

    public void SetToAim()
    {
        transform.localPosition = AimPosition;
    }
    /// <summary>
    /// send kick back to mouse look
    /// when is fire
    /// </summary>
    void Kick()
    {
        RecoilManager.SetRecoil(RecoilAmount, RecoilSpeed);
    }

    /// <summary>
    /// start reload weapon
    /// deduct the remaining bullets in the cartridge of a new clip
    /// as this happens, we disable the options: fire, aim and run
    /// </summary>
    /// <returns></returns>
    IEnumerator reload(float waitTime = 0.2f)
    {
        isAmed = false;
        CanFire = false;

            if (isReloading)
                yield break; // if already reloading... exit and wait till reload is finished


        yield return new WaitForSeconds(waitTime);

        if (numberOfClips > 0 || inReloadMode)//if have at least one cartridge
        {
            isReloading = true; // we are now reloading
            if (Animat != null)
            {
                if (Info.Type == GunType.Shotgun || SplitReloadAnimation)
                {                  
                    int t_repeat = bulletsPerClip - bulletsLeft; //get the number of spent bullets
                    int add = (numberOfClips >= t_repeat) ? t_repeat : numberOfClips;
                    Animat.ShotgunReload(Info.ReloadTime, add);
                    yield break;
                }
                else
                {
                    Animat.Reload(Info.ReloadTime);
                }
            }
            if (!SoundReloadByAnim)
            {
                StartCoroutine(ReloadSoundIE());
            }
            if (!inReloadMode)// take away a clip
            {
                if (AmmoType == AmmunitionType.Clips)
                {
                    numberOfClips--;
                }
            }
            yield return new WaitForSeconds(Info.ReloadTime); // wait for set reload time
            if (AmmoType == AmmunitionType.Clips)
            {
                bulletsLeft = bulletsPerClip; // fill up the gun
            }
            else
            {
                int need = bulletsPerClip - bulletsLeft;
                int add = (numberOfClips >= need) ? need : numberOfClips;
                bulletsLeft += add;
                numberOfClips -= add;
            }
        }
        isReloading = false; // done reloading
        CanAim = true;
        CanFire = true;
        inReloadMode = false;
    }

    public void AddBullet(int bullet)
    {
        if(AmmoType == AmmunitionType.Bullets)
        {
            numberOfClips -= bullet;
        }
        bulletsLeft += bullet;
    }

    /// <summary>
    /// 
    /// </summary>
    public void FinishReload()
    {
        if (AmmoType == AmmunitionType.Clips)
        {
            numberOfClips--;
        }
        isReloading = false; // done reloading
        CanAim = true;
        CanFire = true;
        inReloadMode = false;
    }

    /// <summary>
    /// use this method to various sounds reload.
    /// if you have only 1 sound, them put only one in inspector
    /// and leave empty other box
    /// </summary>
    /// <returns></returns>
    IEnumerator ReloadSoundIE()
    {
        float t_time = Info.ReloadTime / 3;
        if (ReloadSound != null)
        {
            Source.clip = ReloadSound;
            Source.Play();
            GManager.HeadAnimation(1, t_time);
        }
        if (ReloadSound2 != null)
        {
            if (Info.Type == GunType.Shotgun)
            {
                int t_repeat = bulletsPerClip - bulletsLeft;
                for (int i = 0; i < t_repeat; i++)
                {
                    yield return new WaitForSeconds(t_time / t_repeat + 0.025f);
                    Source.clip = ReloadSound2;
                    Source.Play();

                }
            }
            else
            {
                yield return new WaitForSeconds(t_time);
                Source.clip = ReloadSound2;
                Source.Play();
            }
        }
        if (ReloadSound3 != null)
        {
            yield return new WaitForSeconds(t_time);
            Source.clip = ReloadSound3;
            Source.Play();
            if (GManager != null)
            {
                GManager.HeadAnimation(2, t_time);
            }
        }
        yield return new WaitForSeconds(0.65f);
        if (GManager != null)
        {
            GManager.HeadAnimation(0, t_time);
        }
    }

    

    IEnumerator KnifeSendFire()
    {
        yield return new WaitForSeconds(0.5f);
        isFiring = false;
        alreadyKnife = false;
    }
 
    /// <summary>
    /// When we disable the gun ship called the animation
    /// and disable the basic functions
    /// </summary>
    public void DisableWeapon(bool isFastKill = false)
    {
        CanAim = false;
        if (isReloading) { inReloadMode = true; isReloading = false; }
        CanFire = false;
        if (Animat)
        {
            Animat.HideWeapon();
        }
        if (GManager != null)
        {
            GManager.HeadAnimation(0, 1);
        }
        if(PlayerCamera == null) { PlayerCamera = transform.root.GetComponent<bl_PlayerSettings>().PlayerCamera; }
        PlayerCamera.fieldOfView = DefaultFog;
        if (!isFastKill) { StopAllCoroutines(); }
    }


    /// <summary>
    /// 
    /// </summary>
    void SetFireTypeName()
    {
        string n = "";
        switch (Info.Type)
        {
            case GunType.Machinegun:
                n = bl_GameTexts.FireTypeAuto;
                break;
            case GunType.Burst:
                n = bl_GameTexts.FireTypeSemi;
                break;
            case GunType.Pistol:
            case GunType.Shotgun:
            case GunType.Sniper:
                n = bl_GameTexts.FireTypeSingle;
                break;
            default:
                n = "--";
                break;
        }
        if (FireTypeText != null)
        {
            FireTypeText.text = n;
        }
    }

    /// <summary>
    /// When round is end we can't fire
    /// </summary>
    void OnRoundEnd()
    {
        m_enable = false;
    }

    public void OnPickUpAmmo(int t_clips)
    {
        if (AmmoType == AmmunitionType.Clips)
        {
            if (numberOfClips < maxNumberOfClips)
            {
                numberOfClips += t_clips;
                if (numberOfClips > maxNumberOfClips)
                {
                    numberOfClips = maxNumberOfClips;
                }
            }
        }
        else
        {
            numberOfClips += (bulletsPerClip * t_clips);
        }
    }

    public bool FireButtonDown
    {
        get
        {
#if !INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#else
            return ((bl_Input.GetKey("Fire")));
#endif
        }
    }

    public bool FireButton
    {
        get
        {
#if !INPUT_MANAGER
             return Input.GetMouseButton(0);
#else
            return ((bl_Input.GetKey("Fire")));
#endif
        }
    }

    public bool AimButton
    {
        get
        {
#if !INPUT_MANAGER
            return (Input.GetMouseButton(1));
#else
            return ((bl_Input.GetKey("Aim")));
#endif       
        }
    }

    public bl_WeaponAnin Animat
    {
        get
        {
            return this.GetComponentInChildren<bl_WeaponAnin>();
        }
    }
    private bl_FirstPersonController _controller;
    public bl_FirstPersonController  controller
    {
        get
        {
            if(_controller == null) { _controller = transform.root.GetComponent<bl_FirstPersonController>(); }
            return _controller;
        }
    }

    private bl_PlayerSync _Sync;
    public bl_PlayerSync PlayerSync
    {
        get
        {
            if(_Sync == null) { _Sync = transform.root.GetComponent<bl_PlayerSync>(); }
            return _Sync;
        }
    }

    public int GetCompactClips { get { return (numberOfClips / bulletsPerClip); } }
    /// <summary>
    /// determine if we are ready to shoot
    /// TIP: if you want to have to shoot when running
    /// just remove "!controller.run" of the condition
    /// </summary>
    public bool m_CanFire
    {
        get
        {
            bool can = false;
            if (bulletsLeft > 0 && CanFire && !isReloading && FireWhileRun)
            {
                can = true;
            }
            return can;
        }
    }
    /// <summary>
    /// determine if we can Aim
    /// </summary>
    public bool m_CamAim
    {
        get
        {
            bool can = false;
            if (CanAim && controller.State != PlayerState.Running)
            {
                can = true;
            }
            return can;
        }
    }
    /// <summary>_
    /// determine is we can reload
    /// TIP: if you want to have to shoot when running
    /// just remove "!controller.run" of the condition
    /// </summary>
    bool m_CanReload
    {
        get
        {
            bool can = false;
            if (bulletsLeft < bulletsPerClip && numberOfClips > 0 && controller.State != PlayerState.Running && !isReloading)
            {
                can = true;
            }
            if(Info.Type == GunType.Knife && nextFireTime < Time.time)
            {
                can = false;
            }
            return can;
        }
    }

    bool FireWhileRun
    {
        get
        {
            if (bl_GameData.Instance.CanFireWhileRunning)
            {
                return true;
            }
            if(controller.State != PlayerState.Running)
            {
                return true;
            }else
            {
                return false;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private bl_GunManager GManager
    {
        get
        {
            return this.transform.root.GetComponentInChildren<bl_GunManager>();
        }
    }
}