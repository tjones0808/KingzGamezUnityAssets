using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class bl_AIShooterAgent : bl_MonoBehaviour
{

    public Transform Target;
    [Space(5)]
    [Header("AI")]
    public AIAgentState AgentState = AIAgentState.Idle;
    public float PatrolRadius = 20f; //Radius for get the random point
    public float LookRange = 25.0f;   //when the AI starts to look at the player
    public float FollowRange = 10.0f;       //when the AI starts to chase the player
    public float LosseRange = 50f;
    public float RotationLerp = 6.0f;
    [Range(10, 500)] public int Health = 100;

    [Space(5)]
    [Header("AutoTargets")]
    public List<Transform> PlayersInRoom = new List<Transform>();//All Players in room
    public float UpdatePlayerEach = 5f;

    [Header("References")]
    public Transform AimTarget;
    [SerializeField] private AudioSource FootStepSource;
    [SerializeField] private AudioClip[] FootSteps;

    public int DebuggingState = 0;

    //Privates
    private Vector3 correctPlayerPos = Vector3.zero; // We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; // We lerp towards this
    private NavMeshAgent Agent = null;
    private bool death = false;
    private bool personal = false;

    private Animator Anim;
    public bool playerInFront { get; set; }
    private Vector3 finalPosition;
    private float lastPathTime = 0;
    private float defaultSpeed;
    private bl_AIAnimation AIAnim;
    private float stepTime;
  
    [HideInInspector] public Vector3 vel;
    private bl_AICovertPointManager CoverManager;
    private bl_AIMananger AIManager;
    private bl_AICoverPoint CoverPoint = null;
    private bool ForceCoverFire = false;
    private bool ObstacleBetweenTarget = false;
    private int LastActorEnemy = -1;
    private float CoverTime = 0;
    private bool lookToDirection = false;
    private Vector3 LastHitDirection;
    private int SwitchCoverTimes = 0;
    private float lookTime = 0;
    private bool strafing = false;
    private float strafingTime = 0;
    private Vector2 strafingPosition = Vector3.zero;
    private bool AllOrNothing = false;
    private bl_RoundTime TimeManager;
    private bl_AIShooterWeapon AIWeapon;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        Agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        AIAnim = GetComponentInChildren<bl_AIAnimation>();
        AIWeapon = GetComponent<bl_AIShooterWeapon>();
        defaultSpeed = Agent.speed;
        Anim = GetComponentInChildren<Animator>();
        ObstacleBetweenTarget = false;
        CoverManager = FindObjectOfType<bl_AICovertPointManager>();
        AIManager = CoverManager.GetComponent<bl_AIMananger>();
        TimeManager = FindObjectOfType<bl_RoundTime>();
        InvokeRepeating("UpdateList", 1, UpdatePlayerEach);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(Agent.velocity);
        }
        else
        {
            this.correctPlayerPos = (Vector3)stream.ReceiveNext();
            this.correctPlayerRot = (Quaternion)stream.ReceiveNext();
            vel = (Vector3)stream.ReceiveNext();
        }
    }

    public void DoDamage(int damage, string wn, Vector3 direction, int vi, bool fromBot)
    {
        if (death)
            return;

        photonView.RPC("RpcDoDamage", PhotonTargets.All, damage, wn, direction, vi, fromBot);
    }

    [PunRPC]
    void RpcDoDamage(int damage, string wn, Vector3 direction, int viewID, bool fromBot)
    {
        if (death)
            return;

        Health -= damage;
        if (LastActorEnemy != viewID)
        {
            personal = false;
        }
        LastActorEnemy = viewID;

        if (PhotonNetwork.isMasterClient)
        {
            OnGetHit(direction);
        }
        if (viewID == bl_GameManager.m_view && !fromBot)//if was me that make damage
        {
            bl_UCrosshair.Instance.OnHit();
        }

        if (Health > 0)
        {
            if (Target == null)
            {
                personal = true;
                Target = FindPlayerRoot(viewID).transform;
            }
        }
        else
        {
            death = true;
            Agent.enabled = false;
            bl_AIShooterAgent killerBot = null;
            gameObject.name += " (die)";
            if (viewID == bl_GameManager.m_view && !fromBot)//if was me that kill AI
            {
                bl_EventHandler.KillEvent(base.LocalName, AIName, wn, Team.All.ToString(), 5, 20);
                //Add a new kill and update information
                PhotonNetwork.player.PostKill(1);//Send a new kill

                int score;
                //If heat shot will give you double experience
                /*  if (m_heat)
                  {
                      bl_EventHandler.OnKillEvent(bl_GameTexts.KilledEnemy, bl_GameData.Instance.ScorePerKill);
                      bl_EventHandler.OnKillEvent(bl_GameTexts.HeatShotBonus, bl_GameData.Instance.HeadShotScoreBonus);
                      score = bl_GameData.Instance.ScorePerKill + bl_GameData.Instance.HeadShotScoreBonus;
                  }
                  else
                  {*/
                bl_EventHandler.OnKillEvent(bl_GameTexts.KilledEnemy, bl_GameData.Instance.ScorePerKill);
                score = bl_GameData.Instance.ScorePerKill;
                // }
                //Send to update score to player
                PhotonNetwork.player.PostScore(score);
#if KILL_STREAK
            bl_KillNotifierUtils.GetManager.NewKill();
#endif
            }
            else if (fromBot)
            {
                if (PhotonNetwork.isMasterClient)
                {
                    PhotonView p = PhotonView.Find(viewID);
                    string killer = "Unknown";
                    if (p != null) { killer = p.gameObject.name; }
                    bl_EventHandler.KillEvent(killer, AIName, wn, Team.All.ToString(), 5, 20);

                    bl_AIShooterAgent bot = p.GetComponent<bl_AIShooterAgent>();
                    if (bot != null)
                    {
                        bot.KillTheTarget(transform);
                        killerBot = bot;
                    }
                }
            }
            if (PhotonNetwork.isMasterClient)
            {
                AIManager.OnBotDeath(this, killerBot);
            }
            this.photonView.RPC("DestroyRpc", PhotonTargets.AllBuffered, direction);
        }
    }

    public void CheckTargets()
    {
        if(Target != null && Target.name.Contains("(die)"))
        {
            Target = null;
            photonView.RPC("ResetTarget", PhotonTargets.Others);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnGetHit(Vector3 pos)
    {
        LastHitDirection = pos;
        //if the AI is not covering, will look for a cover point
        if (AgentState != AIAgentState.Covering)
        {
            //if the AI is following and attacking the target he will not look for cover point
            if (AgentState == AIAgentState.Following && TargetDistance <= LookRange)
            {
                lookToDirection = true;
                return;
            }
            Cover(false);
        }
        else
        {
            //if already in a cover and still get shoots from far away will force the AI to fire.
            if (!playerInFront)
            {
                lookToDirection = true;
                Cover(true);
            }
            else
            {
                ForceCoverFire = true;
                lookToDirection = false;
            }
            //if the AI is cover but still get hit, he will search other cover point 
            if (Health <= 50 && Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                Cover(true);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (death)
            return;

        if (!PhotonNetwork.isMasterClient)//if not master client, then get position from server
        {
            transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 7);
            transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * 7);
        }
        else
        {
            vel = Agent.velocity;
            if (TimeManager.isFinish)
            {
                Agent.isStopped = true;
                return;
            }
        }
        if (Target == null)
        {
            //Get the player most near
            SearchPlayers();
            //if target null yet, the patrol         
            RandomPatrol(false);
        }
        else
        {
            CalculateAngle();
            if (AgentState != AIAgentState.Covering)
            {
                TargetControll();
            }
            else
            {
                OnCovering();
            }
        }
        FootStep();
     /*   if (laststate != DebuggingState)
        {
            Debug.Log(DebuggingState);
            laststate = DebuggingState;
        }*/
    }
   // int laststate = 0;

    /// <summary>
    /// 
    /// </summary>
    bool Cover(bool overridePoint, bool toTarget = false)
    {
        Transform t = (!toTarget) ? transform : Target;
        if (overridePoint)
        {
            if (Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                //look for another cover point 
                CoverPoint = CoverManager.GetCloseCover(t, CoverPoint);
            }
        }
        else
        {
            //look for a near cover point
            CoverPoint = CoverManager.GetCloseCover(t);
        }
        if (CoverPoint != null)
        {
            Agent.stoppingDistance = 0.1f;
            Speed = playerInFront ? defaultSpeed : 6;
            Agent.SetDestination(CoverPoint.transform.position);
            AgentState = AIAgentState.Covering;
            CoverTime = Time.time;
            return true;
        }
        else
        {
            //if there are not a near cover point
            if (Target != null)
            {
                //follow target
                Agent.SetDestination(Target.position);
                Speed = playerInFront ? defaultSpeed : 7;
                personal = true;
                AgentState = AIAgentState.Searching;
            }
            else
            {
                CoverPoint = CoverManager.GetCloseCoverForced(transform);
                Agent.SetDestination(CoverPoint.transform.position);
                Speed = defaultSpeed;
            }
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnCovering()
    {
        if (Target != null)
        {
            float Distance = TargetDistance;
            if (Distance <= LookRange && playerInFront)//if in look range and in front, start follow him and shot
            {
                if (!strafing)
                {
                    AgentState = AIAgentState.Following;
                    Agent.SetDestination(Target.position);
                    DebuggingState = 1;
                }
                else
                {
                    AgentState = AIAgentState.Covering;
                    Agent.SetDestination(strafingPosition);
                    DebuggingState = 19;
                }
                AIWeapon.Fire();

            }
            else if (Distance > LosseRange && (Time.time - CoverTime) >= 7)// if out of line of sight, start searching him
            {
                AgentState = AIAgentState.Searching;
                SetCrouch(false);
                DebuggingState = 2;
            }
            else if (ForceCoverFire && !ObstacleBetweenTarget)//if in cover and still get damage, start shoot at him
            {
                AIWeapon.Fire();
                if (CanCover(10)) { SwichCover(); }
                DebuggingState = 3;
            }
            else if (CanCover(10) && Distance >= 7)
            {
                SwichCover();
                DebuggingState = 4;
            }
            else
            {
                if (playerInFront)
                {
                    AIWeapon.Fire();
                    DebuggingState = 5;
                }
                else
                {
                    Look();
                    DebuggingState = 6;
                }
            }
        }
        if (Agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            if (CoverPoint != null && CoverPoint.Crouch) { SetCrouch(true); }
        }
        if (lookToDirection)
        {
            LookToHitDirection();
        }
        else
        {
            Quaternion rotation = Quaternion.LookRotation(Target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * RotationLerp);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SwichCover()
    {
        if (Agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        if (SwitchCoverTimes <= 3)
        {
            Cover(true, true);
            SwitchCoverTimes++;
        }
        else
        {
            AgentState = AIAgentState.Following;
            Agent.SetDestination(TargetPosition);
            SwitchCoverTimes = 0;
            AllOrNothing = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void TargetControll()
    {
        float Distance = Vector3.Distance(Target.position, transform.position);
        if (Distance >= LosseRange)
        {
            if (AgentState == AIAgentState.Following || personal || AgentState == AIAgentState.Searching)
            {
                RandomPatrol(true);
                AgentState = AIAgentState.Searching;
                DebuggingState = 7;
            }
            else
            {
                photonView.RPC("ResetTarget", PhotonTargets.All);
                RandomPatrol(false);
                AgentState = AIAgentState.Patroling;
                DebuggingState = 8;
            }
            Speed = defaultSpeed;
            if (!AIWeapon.isFiring)
            {
                Anim.SetInteger("UpperState", 4);
            }
        }
        else if (Distance > FollowRange && Distance < LookRange)//look range
        {
            OnTargetInSight(false);
        }
        else if (Distance <= FollowRange)
        {
            DebuggingState = 9;
            Follow();
        }
        else if (Distance < LosseRange)
        {
            OnTargetInSight(true);
        }
        else
        {
            Debug.Log("Unknown state: " + Distance);
            DebuggingState = 10;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnTargetInSight(bool overrideCover)
    {
        if (AgentState == AIAgentState.Following || Health <= 50)
        {
            if (!Cover(overrideCover) || CanCover(17) || AllOrNothing)
            {
                Follow();
                AgentState = AIAgentState.Following;
                DebuggingState = 11;
            }
            else
            {
                if (!strafing)
                {
                    strafingTime += Time.deltaTime;
                    if (strafingTime >= 5)
                    {
                        strafing = true;
                        Invoke("ResetStrafing", 4);
                    }
                    SetCrouch(true);
                    DebuggingState = 12;
                }
                else
                {
                    if (strafingTime > 0)
                    {
                        strafingPosition = transform.position + transform.TransformDirection(transform.position + (Vector3.right * Random.Range(-3, 3)));
                        strafingTime = 0;
                    }
                    Agent.destination = strafingPosition;
                    SetCrouch(false);
                    DebuggingState = 18;
                }
            }
        }
        else if (AgentState == AIAgentState.Covering)
        {
            if (CanCover(5) && TargetDistance >= 7)
            {
                Cover(true);
                DebuggingState = 13;
            }
            else
            {
                DebuggingState = 14;
            }
        }
        else
        {
            Look();
            DebuggingState = 15;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SearchPlayers()
    {
        for (int i = 0; i < PlayersInRoom.Count; i++)
        {
            Transform enemy = PlayersInRoom[i];
            if (enemy != null)
            {
                float Distance = Vector3.Distance(enemy.position, transform.position);//if a player in range, get this

                if (Distance < LookRange && !enemy.gameObject.name.Contains("(die)"))//if in range
                {
                    GetTarget(PlayersInRoom[i]);//get this player
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CalculateAngle()
    {
        if (Target == null)
            return;

        Vector3 relative = transform.InverseTransformPoint(Target.position);

        if ((relative.x < 2f && relative.x > -2f) || (relative.x > -2f && relative.x < 2f))
        {
            //target is in front
            playerInFront = true;
        }
        else
        {
            playerInFront = false;
        }
    }

    /// <summary>
    /// If player not in range then the AI patrol in map
    /// </summary>
    void RandomPatrol(bool precision)
    {
        if (death)
            return;

        AgentState = (precision) ? AIAgentState.Searching : AIAgentState.Patroling;
        lookToDirection = false;
        AIWeapon.isFiring = false;
        if (!Agent.hasPath || TargetDistance <= 5.2f || (Time.time - lastPathTime) > 7)
        {
            bool toAnCover = (Random.Range(0, 20) > 18);//probability of get a cover point as random destination
            if (!precision)
            {
                ForceCoverFire = false;
            }
            float pre = precision ? 10 : PatrolRadius;
            Vector3 randomDirection = TargetPosition + (Random.insideUnitSphere * pre);
            if (toAnCover) { randomDirection = CoverManager.GetCoverOnRadius(transform, 20).transform.position; }
            if (Target == null)
            {
                randomDirection += transform.position;
            }
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, pre, 1);
            finalPosition = hit.position;
            lastPathTime = Time.time + Random.Range(0, 5);
            Speed = (Random.Range(0, 5) == 3) ? 4 : 6;
            SetCrouch(false);
            DebuggingState = 16;
        }
        else
        {
            DebuggingState = 17;
        }
        Agent.SetDestination(finalPosition);
    }

    /// <summary>
    /// 
    /// </summary>
    void SetCrouch(bool crouch)
    {
        Anim.SetBool("Crouch", crouch);
        Speed = crouch ? 3.5f : defaultSpeed;
    }
   
    /// <summary>
    /// 
    /// </summary>
    public void KillTheTarget(Transform t)
    {
        photonView.RPC("ResetTarget", PhotonTargets.All);
    }

    /// <summary>
    /// Force AI to look the target
    /// </summary>
    void Look()
    {
        if (AgentState != AIAgentState.Covering)
        {
            if (lookTime >= 5)
            {
                AgentState = AIAgentState.Following;
                lookTime = 0;
                return;
            }
            lookTime += Time.deltaTime;
            AgentState = AIAgentState.Looking;
        }
        Quaternion rotation = Quaternion.LookRotation(TargetPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * RotationLerp);
        AIWeapon.Fire();
        SetCrouch(playerInFront);
        Speed = playerInFront ? defaultSpeed : 7;
        lookToDirection = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void LookToHitDirection()
    {
        if (LastHitDirection == Vector3.zero)
            return;

        Quaternion rotation = Quaternion.LookRotation(Target.position - LastHitDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * RotationLerp);
        SetCrouch(playerInFront);
        Speed = playerInFront ? defaultSpeed : 8;
    }

    /// <summary>
    /// 
    /// </summary>
    void Follow()
    {
        Agent.stoppingDistance = 3;
        lookToDirection = false;
        SetCrouch(false);
        Speed = defaultSpeed;
        Agent.destination = TargetPosition;
        if (Agent.remainingDistance <= 3)
        {
            if (Cover(false, true))
            {
                AgentState = AIAgentState.Covering;
            }
            else { Look(); }
            DebuggingState = 20;
        }
        else
        {
            AgentState = AIAgentState.Following;
            AIWeapon.Fire();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GetTarget(Transform t)
    {
        Target = t;
        PhotonView view = GetPhotonView(Target.root.gameObject);
        if (view != null)
        {
            photonView.RPC("SyncTargetAI", PhotonTargets.Others, view.viewID);
        }
        else
        {
            Debug.Log("This Target " + Target.name + "no have photonview");
        }
    }


    [PunRPC]
    void SyncTargetAI(int view)
    {
        Transform t = FindPlayerRoot(view).transform;
        if (t != null)
        {
            Target = t;
        }
    }

    [PunRPC]
    void ResetTarget()
    {
        Target = null;
    }
    /// <summary>
    /// 
    /// </summary>
    void UpdateList()
    {
        PlayersInRoom = AllPlayers;
    }

    /// <summary>
    /// 
    /// </summary>
    public void FootStep()
    {
        float vel = Agent.velocity.magnitude;
        if (vel < 1)
            return;

        float lenght = 0.6f;
        if (vel > 5)
        {
            lenght = 0.45f;
        }

        if ((Time.time - stepTime) > lenght)
        {
            stepTime = Time.time;
            FootStepSource.clip = FootSteps[Random.Range(0, FootSteps.Length)];
            FootStepSource.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected List<Transform> AllPlayers
    {
        get
        {
            List<Transform> list = new List<Transform>();
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                GameObject g = FindPhotonPlayer(p);
                if (g != null)
                {
                    list.Add(g.transform);
                }
            }
            list.AddRange(AIManager.GetOtherBots(AimTarget));
            return list;
        }
    }

    [PunRPC]
    void RpcSync(int _health)
    {
        Health = _health;
    }

    private float Speed
    {
        get
        {
            return Agent.speed;
        }
        set
        {
            bool cr = Anim.GetBool("Crouch");
            if (cr)
            {
                Agent.speed = 2;
            }
            else
            {
                Agent.speed = value;
            }
        }
    }

    void ResetStrafing() { strafingTime = 0; strafing = false; }

    [PunRPC]
    public IEnumerator DestroyRpc(Vector3 position,PhotonMessageInfo info)
    {
        if ((PhotonNetwork.time - info.timestamp) > 5)
        {
            Destroy(this.gameObject);
            yield break;
        }
        AIAnim.Ragdolled(position);
        yield return new WaitForSeconds(5);
        GameObject.Destroy(this.gameObject);
        yield return 0; // if you allow 1 frame to pass, the object's OnDestroy() method gets called and cleans up references.
        PhotonNetwork.UnAllocateViewID(this.photonView.viewID);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);
        if (PhotonNetwork.isMasterClient && newPlayer.ID != PhotonNetwork.player.ID)
        {
            photonView.RPC("RpcSync", newPlayer, Health);
        }
        /* if (PhotonNetwork.isMasterClient)
         {
             photonView.RequestOwnership();
         }*/
    }

    public Vector3 TargetPosition
    {
        get
        {
            if (Target != null) { return Target.position; }
            return Vector3.zero;
        }
    }
    private string _ainame;
    public string AIName
    {
        get
        {
            return _ainame;
        }
        set
        {
            _ainame = value;
            gameObject.name = value;
        }
    }

    /* private void OnDrawGizmos()
     {
         Gizmos.color = Color.red;
         Gizmos.DrawWireSphere(transform.position + transform.TransformDirection( (Vector3.right * (3))), 1);
     }*/

    public float TargetDistance { get { return Vector3.Distance(transform.position, TargetPosition); } }
    private bool CanCover(float inTimePassed) { return ((Time.time - CoverTime) >= inTimePassed); }
}