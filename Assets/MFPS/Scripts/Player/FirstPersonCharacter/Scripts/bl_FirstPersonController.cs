using UnityEngine;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class bl_FirstPersonController : bl_MonoBehaviour
{
    [Header("Settings")]
    public PlayerState State;
    [SerializeField]
    private float m_WalkSpeed;
    [SerializeField]
    private float m_RunSpeed;
    [SerializeField]
    private float m_CrouchSpeed;
    [SerializeField]
    private float m_ClimbSpeed = 1f;
    [SerializeField]
    private float m_JumpSpeed;
    [SerializeField, Range(0, 2)] private float JumpMinRate = 0.82f;
    [SerializeField]
    private float m_StickToGroundForce;
    [SerializeField]
    private float m_GravityMultiplier;
    public bool FallDamage = true;
    [Range(0.1f, 5f),SerializeField]
    private float FallDamageThreshold = 1;
    [Header("Mouse Look")]
    [SerializeField]
    private MouseLook m_MouseLook;
    [SerializeField]
    private bool m_UseFovKick;
    [SerializeField]
    private FOVKick m_FovKick = new FOVKick();
    [SerializeField]
    private Transform HeatRoot;
    [SerializeField]
    private Transform CameraRoot;
    [Header("HeadBob")]
    [SerializeField]
    private bool m_UseHeadBob;
    [SerializeField]
    private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField]
    private LerpControlledBob m_JumpBob = new LerpControlledBob();
    [SerializeField, Range(1, 15)]
    private float HeadBobLerp = 7;
    public float m_StepInterval;
    public float m_RunStepInterval;
    [Header("FootSteps")]
    [Range(0f, 1f)]
    public float m_WalkstepLengh;
    [Range(0f, 1f)]
    public float m_RunstepLenghten;
    [SerializeField] private bl_FootStepsLibrary FootStepLibrary;
    [SerializeField]
    private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField]
    private AudioClip m_LandSound;           // the sound played when character touches back on ground.
    [Header("UI")]
     [SerializeField]private Sprite StandIcon;
    [SerializeField]private Sprite CrouchIcon;

    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private bool Crounching = false;
    private AudioSource m_AudioSource;
    [HideInInspector]
    public Vector3 Velocity;
    [HideInInspector]
    public float VelocityMagnitude;
    private bl_RoomMenu RoomMenu;
    private bl_GunManager GunManager;
    private bool Finish = false;
    private Vector3 defaultCameraRPosition;
    private bool isClimbing = false;
    private bl_Ladder m_Ladder;
    private bool MoveToStarted = false;
#if MFPSM
    private bl_Joystick Joystick;
#endif
    private float fallTime = 0;
    private float PostGroundVerticalPos = 0;
    private bl_PlayerDamageManager DamageManager;
    private float lastJumpTime = 0;
    private int WeaponWeight = 1;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!photonView.isMine)
            return;

        base.Awake();
        m_CharacterController = GetComponent<CharacterController>();
        RoomMenu = FindObjectOfType<bl_RoomMenu>();
        GunManager = GetComponentInChildren<bl_GunManager>();
        DamageManager = GetComponent<bl_PlayerDamageManager>();
#if MFPSM
        Joystick = FindObjectOfType<bl_Joystick>();
#endif
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        defaultCameraRPosition = CameraRoot.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = gameObject.AddComponent<AudioSource>();
        m_MouseLook.Init(transform, HeatRoot, RoomMenu, GunManager);
        fallTime = Time.time;
        PostGroundVerticalPos = transform.position.y;
        lastJumpTime = Time.time;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        bl_EventHandler.OnRoundEnd += OnRoundEnd;
        bl_EventHandler.OnChangeWeapon += OnChangeWeapon;
#if MFPSM
        bl_TouchHelper.OnCrouch += OnCrouchClicked;
        bl_TouchHelper.OnJump += OnJump;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        bl_EventHandler.OnRoundEnd -= OnRoundEnd;
        bl_EventHandler.OnChangeWeapon -= OnChangeWeapon;
#if MFPSM
        bl_TouchHelper.OnCrouch -= OnCrouchClicked;
        bl_TouchHelper.OnJump -= OnJump;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    void OnRoundEnd()
    {
        Finish = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        Velocity = m_CharacterController.velocity;
        VelocityMagnitude = Velocity.magnitude;
        RotateView();

        if (Finish)
            return;
        if (!bl_UtilityHelper.GetCursorState)
            return;

        // the jump state needs to read here to make sure it is not missed
#if INPUT_MANAGER

        if (!m_Jump && State != PlayerState.Crouching && (Time.time - lastJumpTime) > JumpMinRate)
        {
            m_Jump = bl_Input.GetKeyDown("Jump");
        }
        if (State != PlayerState.Jumping)
        {
            if (bl_Input.GetKeyDown("Crouch"))
            {
                Crounching = !Crounching;
               if (Crounching)
                    {
                        State = PlayerState.Crouching;
                        bl_UIReferences.Instance.PlayerStateIcon.sprite = CrouchIcon;
                    }
                    else
                    {
                        State = PlayerState.Idle;
                        bl_UIReferences.Instance.PlayerStateIcon.sprite = StandIcon;
                    }
            }
        }
#else
        if (!bl_UtilityHelper.isMobile)
        {
            if (!m_Jump && State != PlayerState.Crouching && (Time.time - lastJumpTime) > JumpMinRate)
            {
                m_Jump = Input.GetKeyDown(KeyCode.Space);
            }
            if (State != PlayerState.Jumping && State != PlayerState.Climbing)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    Crounching = !Crounching;
                    if (Crounching)
                    {
                        State = PlayerState.Crouching;
                        bl_UIReferences.Instance.PlayerStateIcon.sprite = CrouchIcon;
                    }
                    else
                    {
                        State = PlayerState.Idle;
                        bl_UIReferences.Instance.PlayerStateIcon.sprite = StandIcon;
                    }
                }
            }
        }
#endif
        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            PlayLandingSound();
            if (FallDamage)
            {
                CalculateFall();
            }
            m_MoveDir.y = 0f;
            m_Jumping = false;
            State = PlayerState.Idle;
            bl_EventHandler.OnSmallImpactEvent();
        }
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        Crouch();
        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }

    /// <summary>
    /// 
    /// </summary>
    void CalculateFall()
    {
        float total = Time.time - fallTime;
        float ver = transform.position.y - PostGroundVerticalPos;
        float abs = Mathf.Abs(ver);
        if (total >= FallDamageThreshold && ver < 0 && abs > 3) //if the fall distance is > 3
        {
            DamageManager.GetFallDamage(total);
        }
        fallTime = Time.time;
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (Finish)
            return;
        if (m_CharacterController == null || !m_CharacterController.enabled || MoveToStarted)
            return;


        if (bl_UtilityHelper.GetCursorState)
        {
            float s = 0;
            GetInput(out s);
            speed = s;
        }
        else
        {
            m_Input = Vector2.zero;
        }
        Vector3 desiredMove = Vector2.zero;
        if (isClimbing && m_Ladder != null)
        {
            if (m_Ladder.HasPending)
            {
                if (!MoveToStarted)
                {
                    StartCoroutine(MoveTo(m_Ladder.GetCurrent, false));
                }
            }
            else
            {

                desiredMove = m_Ladder.transform.rotation * Vector3.forward * m_Input.y;
                m_MoveDir.y = desiredMove.y * m_ClimbSpeed;
                m_MoveDir.x = desiredMove.x * m_ClimbSpeed;
                m_MoveDir.z = desiredMove.z * m_ClimbSpeed;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ToggleClimbing();
                    m_Ladder.JumpOut();
                    m_MoveDir.y = m_JumpSpeed;
                    m_MoveDir.z = 30;
                    fallTime = Time.time;
                    PostGroundVerticalPos = transform.position.y;
                    lastJumpTime = Time.time;
                }
                m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
            }
        }
        else
        {
            // always move along the camera forward as it is the direction that it being aimed at
            desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo, m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal);
            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                    State = PlayerState.Jumping;
                    lastJumpTime = Time.time;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
        }
        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * ((State == PlayerState.Walking) ? m_WalkstepLengh : m_RunstepLenghten))) * Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        if (State == PlayerState.Running)
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
        if (!m_CharacterController.isGrounded && !isClimbing)
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
                    n = Random.Range(1,FootStepLibrary.WatertepSounds.Length);
                    m_AudioSource.clip = FootStepLibrary.WatertepSounds[n];
                    m_AudioSource.PlayOneShot(m_AudioSource.clip);
                    // move picked sound to index 0 so it's not picked next time
                    FootStepLibrary.WatertepSounds[n] = FootStepLibrary.WatertepSounds[0];
                    FootStepLibrary.WatertepSounds[0] = m_AudioSource.clip;
                    break;
                case "Metal":
                    n = Random.Range(1, FootStepLibrary.MetalStepSounds.Length);
                    m_AudioSource.clip = FootStepLibrary.MetalStepSounds[n];
                    m_AudioSource.PlayOneShot(m_AudioSource.clip);
                    // move picked sound to index 0 so it's not picked next time
                    FootStepLibrary.MetalStepSounds[n] = FootStepLibrary.MetalStepSounds[0];
                    FootStepLibrary.MetalStepSounds[0] = m_AudioSource.clip;
                    break;
                default:
                    n = Random.Range(1, FootStepLibrary.m_FootstepSounds.Length);
                    m_AudioSource.clip = FootStepLibrary.m_FootstepSounds[n];
                    m_AudioSource.PlayOneShot(m_AudioSource.clip);
                    // move picked sound to index 0 so it's not picked next time
                    FootStepLibrary.m_FootstepSounds[n] = FootStepLibrary.m_FootstepSounds[0];
                    FootStepLibrary.m_FootstepSounds[0] = m_AudioSource.clip;
                    break;
            }
        }
        else
        {
            int n = Random.Range(1, FootStepLibrary.MetalStepSounds.Length);
            m_AudioSource.clip = FootStepLibrary.MetalStepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            FootStepLibrary.MetalStepSounds[n] = FootStepLibrary.MetalStepSounds[0];
            FootStepLibrary.MetalStepSounds[0] = m_AudioSource.clip;
        }
    }

    /// <summary>
    /// When player is in Crouch
    /// </summary>
    void Crouch()
    {
        if (Crounching)
        {
            if (m_CharacterController.height != 1.4f)
            {
                m_CharacterController.height = 1.4f;
            }
            m_CharacterController.center = new Vector3(0, -0.3f, 0);
            Vector3 ch = CameraRoot.transform.localPosition;

            if (CameraRoot.transform.localPosition.y != 0.2f)
            {
                ch.y = Mathf.Lerp(ch.y, 0.2f, Time.deltaTime * 8);
                CameraRoot.transform.localPosition = ch;
            }
        }
        else
        {
            if (m_CharacterController.height != 2f)
            {
                m_CharacterController.height = 2f;
            }
            m_CharacterController.center = Vector3.zero;
            Vector3 ch = CameraRoot.transform.localPosition;
            if (ch.y != defaultCameraRPosition.y)
            {
                ch.y = Mathf.Lerp(ch.y, defaultCameraRPosition.y, Time.deltaTime * 8);
                CameraRoot.transform.localPosition = ch;
            }
        }
    }


    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (!m_UseHeadBob)
        {
            return;
        }
        if (m_CharacterController.velocity.magnitude > 0 && (m_CharacterController.isGrounded || isClimbing))
        {
            HeatRoot.localPosition = m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude + (speed * ((State == PlayerState.Running) ? m_RunstepLenghten : m_WalkstepLengh)));
            newCameraPosition = HeatRoot.localPosition;
            newCameraPosition.y = HeatRoot.localPosition.y - m_JumpBob.Offset();
        }
        else
        {
            newCameraPosition = HeatRoot.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }
        HeatRoot.localPosition = Vector3.Lerp(HeatRoot.localPosition, newCameraPosition, Time.deltaTime * HeadBobLerp);
    }


    private void GetInput(out float speed)
    {
        // Read input
#if INPUT_MANAGER
        float horizontal = bl_Input.HorizontalAxis;
        float vertical = bl_Input.VerticalAxis;
#else
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

#endif
#if MFPSM
        if (bl_UtilityHelper.isMobile)
        {
            horizontal = Joystick.Horizontal;
            vertical = Joystick.Vertical;
        }
#endif

        m_Input = new Vector2(horizontal, vertical);
        PlayerState waswalking = State;

#if !INPUT_MANAGER
        if (State != PlayerState.Climbing)
        {
            if (m_Input.magnitude > 0 && (m_CharacterController.isGrounded))
            {
                if (!bl_UtilityHelper.isMobile)
                {
                    // On standalone builds, walk/run speed is modified by a key press.
                    // keep track of whether or not the character is walking or running
                    if (Input.GetKey(KeyCode.LeftShift) && State != PlayerState.Crouching && vertical > 0)
                    {
                        State = PlayerState.Running;
                    }
                    else if (Input.GetKeyUp(KeyCode.LeftShift) && State != PlayerState.Crouching && vertical > 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Crouching && vertical > 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Idle;
                    }

                }
                else
                {
                    if (vertical > m_WalkSpeed && vertical > 0.05f && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Running;
                    }
                    else if (vertical <= m_WalkSpeed && vertical != 0 && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Crouching && vertical != 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Idle;
                    }
                }
            }
            else if (m_CharacterController.isGrounded)
            {
                if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                {
                    State = PlayerState.Idle;
                }
            }
        }
#else

        if (State != PlayerState.Climbing)
        {
            if (m_Input.magnitude > 0 && (m_CharacterController.isGrounded))
            {
                if (!bl_UtilityHelper.isMobile)
                {
                    // On standalone builds, walk/run speed is modified by a key press.
                    // keep track of whether or not the character is walking or running
                    if (bl_Input.GetKey("Run") && State != PlayerState.Crouching && vertical > 0)
                    {
                        State = PlayerState.Running;
                    }
                    else if (bl_Input.GetKeyUp("Run") && State != PlayerState.Crouching && vertical > 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Crouching && vertical > 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Idle;
                    }

                }
                else
                {
                    if (vertical > m_WalkSpeed && vertical > 0.05f && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Running;
                    }
                    else if (vertical <= m_WalkSpeed && vertical != 0 && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Crouching && vertical != 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Idle;
                    }
                }
            }
            else if (m_CharacterController.isGrounded)
            {
                if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                {
                    State = PlayerState.Idle;
                }
            }
        }       
#endif

        if (Crounching)
        {
            speed = m_CrouchSpeed;
        }
        else
        {
            // set the desired speed to be walking or running
            speed = (State == PlayerState.Running) ? m_RunSpeed : m_WalkSpeed;
        }


        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fov kick is to be used
        if (State != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0 && State == PlayerState.Running)
        {
            StopAllCoroutines();
            StartCoroutine((State == PlayerState.Running) ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }

#if MFPSM
    /// <summary>
    /// 
    /// </summary>
    void OnCrouchClicked()
    {
        Crounching = !Crounching;
        if (Crounching) { State = PlayerState.Crouching; } else { State = PlayerState.Idle; }
     bl_UIReferences.Instance.PlayerStateIcon.sprite = (Crounching) ? CrouchIcon : StandIcon;
    }

    void OnJump()
    {
        if (!m_Jump && State != PlayerState.Crouching)
        {
            m_Jump = true;
        }
    }
#endif
    /// <summary>
    /// 
    /// </summary>
    private void RotateView()
    {
        if (!isClimbing)
        {
            m_MouseLook.LookRotation(transform, HeatRoot);
        }
        else
        {
            m_MouseLook.LookRotation(transform, HeatRoot, m_Ladder.InsertionPoint);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == null)
            return;

        bl_Ladder l = other.transform.parent.GetComponent<bl_Ladder>();
        if (l != null)
        {
            if (!l.CanUse)
                return;

            m_Ladder = l;
            if (other.transform.name == bl_Ladder.BottomColName)
            {
                m_Ladder.InsertionPoint = other.transform;
                if (!isClimbing)
                {
                    m_Ladder.ToBottom();
                    ToggleClimbing();
                }
                else
                {
                    ToggleClimbing();
                    m_Ladder.HasPending = false;
                }
            }
            else if (other.transform.name == bl_Ladder.TopColName)
            {
                m_Ladder.InsertionPoint = other.transform;
                if (isClimbing)
                {
                    m_Ladder.SetToTop();
                    if (!MoveToStarted)
                    {
                        StartCoroutine(MoveTo(m_Ladder.GetCurrent, true));
                    }
                }
                else
                {
                    m_Ladder.ToMiddle();
                }
                ToggleClimbing();
            }
        }
    }


    void OnChangeWeapon(int id)
    {
        WeaponWeight = bl_GameData.Instance.GetWeapon(id).Weight;
    }

    private void ToggleClimbing()
    {
        isClimbing = !isClimbing;
        State = (isClimbing) ? PlayerState.Climbing : PlayerState.Idle;
        bl_UIReferences.Instance.JumpLadder.SetActive(isClimbing);
    }

    IEnumerator MoveTo(Vector3 pos, bool down)
    {
        MoveToStarted = true;
        bool small = false;
        float t = 0;
        while (t < 0.7f)
        {
            t += Time.deltaTime / 1.5f;
            transform.position = Vector3.Lerp(transform.position, pos, t);
            if (t >= 0.6f && !small && down)
            {
                bl_EventHandler.OnSmallImpact();
                small = true;
            }
            yield return new WaitForFixedUpdate();
        }
        if (m_Ladder != null)
        {
            m_Ladder.HasPending = false;
        }
        MoveToStarted = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }

    internal float _speed = 0;
    public float speed
    {
        get
        {
            return _speed;
        }
        set
        {
            _speed = value - WeaponWeight;
            _speed = Mathf.Clamp(_speed, 2, 10);
        }
    }

    public bool isGrounded { get { return m_CharacterController.isGrounded; } }
}