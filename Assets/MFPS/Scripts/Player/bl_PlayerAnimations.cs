//////////////////////////////////////////////////////////////////////////////
// bl_PlayerAnimations.cs
//
// - was ordered to encourage TPS player animations using legacy animations,
//  and heat look controller from Unity technologies.
//
//                           Lovatto Studio
/////////////////////////////////////////////////////////////////////////////
using UnityEngine;

public class bl_PlayerAnimations : bl_MonoBehaviour
{
    [HideInInspector]
    public bool m_Update = true;
    [Header("Animations")]
    public Animator m_animator;
    public bl_FootStepsLibrary FootStepLibrary;

    [HideInInspector]
    public bool grounded = true;
    [HideInInspector]
    public int state = 0;
    [HideInInspector]
    public Vector3 velocity = Vector3.zero;
    [HideInInspector]
    public Vector3 localVelocity = Vector3.zero;
    [HideInInspector]
    public float movementSpeed;
    [HideInInspector]
    public float lastYRotation;
    [HideInInspector] public string UpperState = "Idle";

    private int UpperStateID = 0;
    private bool HitType = false;
    private GunType cacheWeaponType = GunType.Machinegun;
    private float vertical;
    private float horizontal;
    private Transform PlayerRoot;
    private float turnSpeed;
    private bool parent = false;
    private float TurnLerp = 0;
    [HideInInspector] public bl_NetworkGun EditorSelectedGun = null;
    //foot steps
    private AudioSource StepSource;
    private float m_StepCycle;
    private float m_NextStep;
    private float m_StepInterval;
    private float m_RunStepInterval;
    private float m_WalkstepLengh;
    private float m_RunstepLenghten;
    private bool useFootSteps = false;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        PlayerRoot = transform.root;

        useFootSteps = bl_GameData.Instance.CalculateNetworkFootSteps;
        if (useFootSteps)
        {
            bl_FirstPersonController fpc = PlayerRoot.GetComponent<bl_FirstPersonController>();
            m_RunStepInterval = fpc.m_RunStepInterval;
            m_WalkstepLengh = fpc.m_WalkstepLengh;
            m_StepInterval = fpc.m_StepInterval;
            m_RunstepLenghten = fpc.m_RunstepLenghten;
            StepSource = gameObject.AddComponent<AudioSource>();
            StepSource.spatialBlend = 1;
            StepSource.maxDistance = 15;
            StepSource.playOnAwake = false;
        }  
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!m_Update)
            return;

        ControllerInfo();
        Animate();
        UpperControll();
        if (useFootSteps)
        {
            ProgressStepCycle(movementSpeed);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    void ControllerInfo()
    {
        localVelocity = PlayerRoot.InverseTransformDirection(velocity);
        localVelocity.y = 0;

        vertical = Mathf.Lerp(vertical, localVelocity.z, Time.deltaTime * 10);
        horizontal = Mathf.Lerp(horizontal, localVelocity.x, Time.deltaTime * 10);

        parent = !parent;
        if (parent)
        {
            lastYRotation = PlayerRoot.rotation.eulerAngles.y;
        }
        turnSpeed = Mathf.DeltaAngle(lastYRotation, PlayerRoot.rotation.eulerAngles.y);
        TurnLerp = Mathf.Lerp(TurnLerp, turnSpeed, 7 * Time.deltaTime);
        movementSpeed = velocity.magnitude;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private float HorizontalAngle(Vector3 direction)
    {
        return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 
    /// </summary>
    void Animate()
    {
        if (m_animator == null)
            return;

        m_animator.SetFloat("Vertical", vertical);
        m_animator.SetFloat("Horizontal", horizontal);
        m_animator.SetFloat("Speed", movementSpeed);
        m_animator.SetFloat("Turn", TurnLerp);
        m_animator.SetBool("isGround", grounded);
        bool isCrouch = (state == 3);
        bool isClimbing = (state == 5);
        m_animator.SetBool("Crouch", isCrouch);
        m_animator.SetBool("isClimbing", isClimbing);
    }

    /// <summary>
    /// 
    /// </summary>
    void UpperControll()
    {
        if (UpperState == "Firing" || UpperState == "AimFire")
        {
            UpperStateID = 1;
        }
        else if (UpperState == "Reloading")
        {
            UpperStateID = 2;
        }
        else if (UpperState == "Aimed")
        {
            UpperStateID = 3;
        }
        else if (UpperState == "Running")
        {
            UpperStateID = 4;
        }
        else
        {
            UpperStateID = 0;
        }
        if (m_animator != null)
        {
            m_animator.SetInteger("UpperState", UpperStateID);
        }
    }

    #region FootSteps
    /// <summary>
    /// 
    /// </summary>
    private void ProgressStepCycle(float speed)
    {
        if (velocity.sqrMagnitude > 1)
        {
            m_StepCycle += (velocity.magnitude + (speed * ((state == (int)PlayerState.Walking) ? m_WalkstepLengh : m_RunstepLenghten))) * Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        if (state == (int)PlayerState.Running)
        {
            m_NextStep = m_StepCycle + m_RunStepInterval;
        }
        else
        {
            m_NextStep = m_StepCycle + m_StepInterval;
        }

        PlayFootStepAudio();
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayFootStepAudio()
    {
        bool isClimbing = (state == 5);
        if (!grounded && !isClimbing)
        {
            return;
        }
        if (!isClimbing)
        {
            RaycastHit hit;
            string _tag = "none";
            int n = 0;
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, 10))
            {
                _tag = hit.collider.transform.tag;
            }

            switch (_tag)
            {
                case "Water":
                    n = Random.Range(1, FootStepLibrary.WatertepSounds.Length);
                    StepSource.clip = FootStepLibrary.WatertepSounds[n];
                    StepSource.PlayOneShot(StepSource.clip);
                    // move picked sound to index 0 so it's not picked next time
                    FootStepLibrary.WatertepSounds[n] = FootStepLibrary.WatertepSounds[0];
                    FootStepLibrary.WatertepSounds[0] = StepSource.clip;
                    break;
                case "Metal":
                    n = Random.Range(1, FootStepLibrary.MetalStepSounds.Length);
                    StepSource.clip = FootStepLibrary.MetalStepSounds[n];
                    StepSource.PlayOneShot(StepSource.clip);
                    // move picked sound to index 0 so it's not picked next time
                    FootStepLibrary.MetalStepSounds[n] = FootStepLibrary.MetalStepSounds[0];
                    FootStepLibrary.MetalStepSounds[0] = StepSource.clip;
                    break;
                default:
                    n = Random.Range(1, FootStepLibrary.m_FootstepSounds.Length);
                    StepSource.clip = FootStepLibrary.m_FootstepSounds[n];
                    StepSource.PlayOneShot(StepSource.clip);
                    // move picked sound to index 0 so it's not picked next time
                    FootStepLibrary.m_FootstepSounds[n] = FootStepLibrary.m_FootstepSounds[0];
                    FootStepLibrary.m_FootstepSounds[0] = StepSource.clip;
                    break;
            }
        }
        else
        {
            int n = Random.Range(1, FootStepLibrary.MetalStepSounds.Length);
            StepSource.clip = FootStepLibrary.MetalStepSounds[n];
            StepSource.PlayOneShot(StepSource.clip);
            // move picked sound to index 0 so it's not picked next time
            FootStepLibrary.MetalStepSounds[n] = FootStepLibrary.MetalStepSounds[0];
            FootStepLibrary.MetalStepSounds[0] = StepSource.clip;
        }
    }
    #endregion

    public void PlayFireKnife()
    {
        m_animator.Play("FireKnife", 1, 0);
    }

    public void HitPlayer()
    {
        if (m_animator != null)
        {
            HitType = !HitType;
            int ht = (HitType) ? 1 : 0;
            m_animator.SetInteger("HitType", ht);
            m_animator.SetTrigger("Hit");
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="weaponType"></param>
    public void SetNetworkWeapon(GunType weaponType)
    {
        if (m_animator != null)
        {
            m_animator.SetInteger("GunType", (int)weaponType);
        }
        cacheWeaponType = weaponType;
    }

    public GunType GetCurretWeaponType() { return cacheWeaponType; }
}