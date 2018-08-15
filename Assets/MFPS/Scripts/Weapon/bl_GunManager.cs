/////////////////////////////////////////////////////////////////////////////////
///////////////////////////bl_GunManager.cs//////////////////////////////////////
/////////////Use this to manage all weapons Player///////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
//////////////////////////////Lovatto Studio/////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bl_GunManager : bl_MonoBehaviour {

    [Header("Weapons List")]
    /// <summary>
    /// all the Guns of game
    /// </summary>
    public List<bl_Gun> AllGuns = new List<bl_Gun>();
    /// <summary>
    /// weapons that the player take equipped
    /// </summary>
    [HideInInspector] public List<bl_Gun> PlayerEquip = new List<bl_Gun>() { null, null, null, null };
    [Header("Settings")]
    /// <summary>
    /// ID the weapon to take to start
    /// </summary>
    public int m_Current = 0;
    /// <summary>
    /// time it takes to switch weapons
    /// </summary>
    public float SwichTime = 1;
    public float PickUpTime = 2.5f;

    [HideInInspector] public bl_Gun CurrentGun;
    [HideInInspector] public bool CanSwich;
    [Header("References")]
    public Animator m_HeatAnimator;
    public Transform TrowPoint = null;

    private bl_PickGunManager PUM;
    private int PreviousGun = -1;
    private bool isFastFire = false;
#if GR
    public bool isGunRace { get; set; }
    private bl_GunRace GunRace;
#endif

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        PUM = FindObjectOfType<bl_PickGunManager>();
#if GR
        if (transform.root.GetComponent<PhotonView>().isMine)
        {
            GunRace = FindObjectOfType<bl_GunRace>();
            if (GunRace != null) { GunRace.SetGunManager(this); }
            else { Debug.Log("Gun Race is not integrated in this map, just go to MFPS -> Addons -> Gun Race -> Integrate, with the map scene open)."); }
        }
#endif
        //when player instance select player class select in bl_RoomMenu
        GetClass();
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        //Desactive all weapons in children and take the first
        foreach (bl_Gun g in PlayerEquip) { g.Setup(true); }
        foreach (bl_Gun guns in AllGuns) { guns.gameObject.SetActive(false); }
#if GR
        if (isGunRace)
        {
            PlayerEquip[0] = GunRace.GetGunInfo(AllGuns);
            m_Current = 0;
        }
#endif
        TakeWeapon(PlayerEquip[m_Current].gameObject);

        bl_EventHandler.ChangeWeaponEvent(PlayerEquip[m_Current].GunID);
    }

    /// <summary>
    /// 
    /// </summary>
    void GetClass()
    {
#if CLASS_CUSTOMIZER
        //Get info for class
        bl_RoomMenu.PlayerClass = bl_ClassManager.m_Class;
        bl_ClassManager.SetUpClasses(this);     
#else
        //when player instance select player class select in bl_RoomMenu
        switch (bl_RoomMenu.PlayerClass)
        {
            case PlayerClass.Assault:
                PlayerEquip[0] = AllGuns[m_AssaultClass.primary];
                PlayerEquip[1] = AllGuns[m_AssaultClass.secondary];
                PlayerEquip[2] = AllGuns[m_AssaultClass.Special];
                PlayerEquip[3] = AllGuns[m_AssaultClass.Knife];
                break;
            case PlayerClass.Recon:
                PlayerEquip[0] = AllGuns[m_ReconClass.primary];
                PlayerEquip[1] = AllGuns[m_ReconClass.secondary];
                PlayerEquip[2] = AllGuns[m_ReconClass.Special];
                PlayerEquip[3] = AllGuns[m_ReconClass.Knife];
                break;
            case PlayerClass.Engineer:
                PlayerEquip[0] = AllGuns[m_EngineerClass.primary];
                PlayerEquip[1] = AllGuns[m_EngineerClass.secondary];
                PlayerEquip[2] = AllGuns[m_EngineerClass.Special];
                PlayerEquip[3] = AllGuns[m_EngineerClass.Knife];
                break;
            case PlayerClass.Support:
                PlayerEquip[0] = AllGuns[m_SupportClass.primary];
                PlayerEquip[1] = AllGuns[m_SupportClass.secondary];
                PlayerEquip[2] = AllGuns[m_SupportClass.Special];
                PlayerEquip[3] = AllGuns[m_SupportClass.Knife];
                break;
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.OnPickUpGun += this.PickUpGun;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.OnPickUpGun -= this.PickUpGun;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!bl_UtilityHelper.GetCursorState)
            return;

        InputControl();
        CurrentGun = PlayerEquip[m_Current];
    }

    /// <summary>
    /// 
    /// </summary>
    void InputControl()
    {
#if GR
        if (isGunRace)
            return;
#endif

#if !INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Alpha1) && CanSwich && m_Current != 0)
        {
            StartCoroutine(ChangeGun(m_Current, PlayerEquip[0].gameObject, 0));
            m_Current = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && CanSwich && m_Current != 1)
        {
            StartCoroutine(ChangeGun(m_Current, PlayerEquip[1].gameObject, 1));
            m_Current = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && CanSwich && m_Current != 2)
        {
            StartCoroutine(ChangeGun(m_Current, PlayerEquip[2].gameObject, 2));
            m_Current = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && CanSwich && m_Current != 3)
        {
            StartCoroutine(ChangeGun(m_Current, PlayerEquip[3].gameObject, 3));
            m_Current = 3;
        }

        //fast fire knife
        if (Input.GetKeyDown(KeyCode.V) && CanSwich && m_Current != 3 && !isFastFire)
        {
            PreviousGun = m_Current;
            isFastFire = true;
            m_Current = 3; // 3 = knife position in list
            PlayerEquip[PreviousGun].gameObject.SetActive(false);
            PlayerEquip[m_Current].gameObject.SetActive(true);
            PlayerEquip[m_Current].FastKnifeFire(OnReturnWeapon);
            CanSwich = false;
        }

#else
        InputManagerControll();
#endif
        //change gun with Scroll mouse
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            SwitchNext();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            SwitchPrevious();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public int SwitchNext()
    {
#if GR
        if (isGunRace)
            return 0;
#endif

        int next = (this.m_Current + 1) % this.PlayerEquip.Count;
        StartCoroutine(ChangeGun(m_Current, PlayerEquip[(this.m_Current + 1) % this.PlayerEquip.Count].gameObject, next));
        m_Current = next;
        return m_Current;
    }

    /// <summary>
    /// 
    /// </summary>
    public int SwitchPrevious()
    {
#if GR
        if (isGunRace)
            return 0;
#endif

        if (this.m_Current != 0)
        {
            int next = (this.m_Current - 1) % this.PlayerEquip.Count;
            StartCoroutine(ChangeGun(m_Current, PlayerEquip[(this.m_Current - 1) % this.PlayerEquip.Count].gameObject, next));
            this.m_Current = next;
        }
        else
        {
            StartCoroutine(ChangeGun(m_Current, PlayerEquip[this.PlayerEquip.Count - 1].gameObject, PlayerEquip.Count - 1));
            this.m_Current = PlayerEquip.Count - 1;
        }
        return m_Current;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnReturnWeapon()
    {
        PlayerEquip[m_Current].gameObject.SetActive(false);
        m_Current = PreviousGun;
        PlayerEquip[m_Current].gameObject.SetActive(true);
        CanSwich = true;
        isFastFire = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void TakeWeapon(GameObject t_weapon)
    {
        t_weapon.SetActive(true);
        CanSwich = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_Gun GetCurrentWeapon()
    {
        if (CurrentGun == null)
        {
            return PlayerEquip[m_Current];
        }
        else
        {
            return CurrentGun;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeTo(int AllWeaponsIndex)
    {
        StartCoroutine(ChangeGun(m_Current, AllGuns[AllWeaponsIndex].gameObject, m_Current));
        PlayerEquip[m_Current] = AllGuns[AllWeaponsIndex];
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeToInstant(int AllWeaponsIndex)
    {
        PlayerEquip[m_Current].gameObject.SetActive(false);
        AllGuns[AllWeaponsIndex].gameObject.SetActive(true);
        bl_EventHandler.ChangeWeaponEvent(PlayerEquip[m_Current].GunID);
        PlayerEquip[m_Current] = AllGuns[AllWeaponsIndex];
    }

    /// <summary>
    /// Coroutine to Change of Gun
    /// </summary>
    /// <returns></returns>
    public IEnumerator ChangeGun(int IDfrom, GameObject t_next, int newID)
    {
        CanSwich = false;
        if (m_HeatAnimator != null)
        {
            m_HeatAnimator.Play("SwichtGun", 0, 0);
        }
        PlayerEquip[IDfrom].DisableWeapon();
        yield return new WaitForSeconds(SwichTime);
        foreach (bl_Gun guns in AllGuns)
        {
            if (guns.gameObject.activeSelf == true)
            {
                guns.gameObject.SetActive(false);
            }
        }
        TakeWeapon(t_next);
        bl_EventHandler.ChangeWeaponEvent(PlayerEquip[newID].GunID);
        if (m_HeatAnimator != null)
        {
            m_HeatAnimator.SetBool("Swicht", false);
            m_HeatAnimator.speed = SwichTime;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PickUpGun(bl_OnPickUpInfo e)
    {
        if (PUM == null)
        {
            Debug.LogError("Need a 'Pick Up Manager' in scene!");
            return;
        }
        //If not already equip
        if (!PlayerEquip.Exists(x => x.GunID == e.ID))
        {

            int actualID = PlayerEquip[m_Current].GunID;
            int nextID = AllGuns.FindIndex(x => x.GunID == e.ID);
            //Get Info
            int[] info = new int[2];
            int clips = PlayerEquip[m_Current].numberOfClips;
            info[0] = clips;
            info[1] = PlayerEquip[m_Current].bulletsLeft;
            PlayerEquip[m_Current] = AllGuns[nextID];
            //Send Info
            AllGuns[nextID].numberOfClips = e.Clips;
            AllGuns[nextID].bulletsLeft = e.Bullets;
            StartCoroutine(PickUpGun((PlayerEquip[m_Current].gameObject), AllGuns[nextID].gameObject, actualID, info));
        }
        else
        {
            foreach (bl_Gun g in PlayerEquip)
            {
                if (g == AllGuns[e.ID])
                {
                    bl_EventHandler.OnAmmo(3);//Add 3 clips
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public IEnumerator PickUpGun(GameObject t_current, GameObject t_next, int id, int[] info)
    {
        CanSwich = false;
        if (m_HeatAnimator != null)
        {
            m_HeatAnimator.SetBool("Swicht", true);
        }
        t_current.GetComponent<bl_Gun>().DisableWeapon();
        yield return new WaitForSeconds(PickUpTime);
        foreach (bl_Gun guns in AllGuns)
        {
            if (guns.gameObject.activeSelf == true)
            {
                guns.gameObject.SetActive(false);
            }
        }
        TakeWeapon(t_next);
        if (m_HeatAnimator != null)
        {
            m_HeatAnimator.SetBool("Swicht", false);
        }
        PUM.TrownGun(id, TrowPoint.position, info);
    }

    /// <summary>
    /// Throw the current gun
    /// </summary>
    public void ThrwoCurrent()
    {
        int actualID = PlayerEquip[m_Current].GunID;
        int[] info = new int[2];
        info[0] = PlayerEquip[m_Current].numberOfClips;
        info[1] = PlayerEquip[m_Current].bulletsLeft;
        PUM.TrownGun(actualID, TrowPoint.position, info);
    }

    public  bl_Gun GetGunOnListById(int id)
    {
        bl_Gun gun = null;
        if(AllGuns.Exists( x=> x.GunID == id))
        {
            gun = AllGuns.Find(x => x.GunID == id);
        }
        else
        {
            Debug.LogError("Gun: " + id + " has not been added on this player list.");
        }
        return gun;
    }

#if INPUT_MANAGER
    void InputManagerControll()
    {
        if (bl_Input.GetKeyDown("Weapon1") && CanSwich && m_Current != 0)
        {

            StartCoroutine(ChangeGun(m_Current, PlayerEquip[0].gameObject, 0));
            m_Current = 0;
        }
        if (bl_Input.GetKeyDown("Weapon2") && CanSwich && m_Current != 1)
        {

            StartCoroutine(ChangeGun(m_Current, PlayerEquip[1].gameObject, 1));
            m_Current = 1;
        }
        if (bl_Input.GetKeyDown("Weapon3") && CanSwich && m_Current != 2)
        {
            StartCoroutine(ChangeGun(m_Current, PlayerEquip[2].gameObject, 2));
            m_Current = 2;
        }
        if (bl_Input.GetKeyDown("Weapon4") && CanSwich && m_Current != 3)
        {
            StartCoroutine(ChangeGun(m_Current, PlayerEquip[3].gameObject, 3));
            m_Current = 3;
        }

        //fast fire knife
        if (bl_Input.GetKeyDown("FastKnife") && CanSwich && m_Current != 3 && !isFastFire)
        {
            PreviousGun = m_Current;
            m_Current = 3; // 3 = knife position in list
            PlayerEquip[PreviousGun].gameObject.SetActive(false);
            PlayerEquip[m_Current].gameObject.SetActive(true);
            PlayerEquip[m_Current].FastKnifeFire(OnReturnWeapon);
            CanSwich = false;
            isFastFire = true;
        }
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public void HeadAnimation(int state, float speed)
    {
        if (m_HeatAnimator == null)
            return;
    
        switch (state)
        {
            case 0:
                m_HeatAnimator.SetInteger("Reload", 0);
                break;
            case 1:
                m_HeatAnimator.SetInteger("Reload", 1);
                break;
            case 2:
                m_HeatAnimator.SetInteger("Reload", 2);
                break;
            case 3:
                m_HeatAnimator.CrossFade("Insert", 0.2f, 0, 0);
                break;
        }
        m_HeatAnimator.speed = m_HeatAnimator.GetCurrentAnimatorStateInfo(0).length / speed;
    }

    
   [System.Serializable]
   public class AssaultClass
   {
       //ID = the number of Gun in the list AllGuns
       /// <summary>
       /// the ID of the first gun Equipped
       /// </summary>
       public int primary = 0;
       /// <summary>
       /// the ID of the secondary Gun Equipped
       /// </summary>
       public int secondary = 1;
       /// <summary>
       /// 
       /// </summary>
       public int Knife = 3;
       /// <summary>
       /// the ID the a special weapon
       /// </summary>
       public int Special = 2;
   }
    [Header("Player Class")]
    public AssaultClass m_AssaultClass;

   [System.Serializable]
   public class EngineerClass
   {
       //ID = the number of Gun in the list AllGuns
       /// <summary>
       /// the ID of the first gun Equipped
       /// </summary>
       public int primary = 0;
       /// <summary>
       /// the ID of the secondary Gun Equipped
       /// </summary>
       public int secondary = 1;
       /// <summary>
       /// 
       /// </summary>
       public int Knife = 3;
       /// <summary>
       /// the ID the a special weapon
       /// </summary>
       public int Special = 2;
   }
   public EngineerClass m_EngineerClass;
    //
   [System.Serializable]
   public class ReconClass
   {
       //ID = the number of Gun in the list AllGuns
       /// <summary>
       /// the ID of the first gun Equipped
       /// </summary>
       public int primary = 0;
       /// <summary>
       /// the ID of the secondary Gun Equipped
       /// </summary>
       public int secondary = 1;
       /// <summary>
       /// 
       /// </summary>
       public int Knife = 3;
       /// <summary>
       /// the ID the a special weapon
       /// </summary>
       public int Special = 2;
   }
   public ReconClass m_ReconClass;
    //
   [System.Serializable]
   public class SupportClass
   {
       //ID = the number of Gun in the list AllGuns
       /// <summary>
       /// the ID of the first gun Equipped
       /// </summary>
       public int primary = 0;
       /// <summary>
       /// the ID of the secondary Gun Equipped
       /// </summary>
       public int secondary = 1;
       /// <summary>
       /// 
       /// </summary>
       public int Knife = 3;
       /// <summary>
       /// the ID the a special weapon
       /// </summary>
       public int Special = 2;
   }
   public SupportClass m_SupportClass;
}
