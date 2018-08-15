////////////////////////////////////////////////////////////////////////////////
// bl_PlayerSettings.cs
//
// This script configures the required settings for the local and remote player
//
//                        Lovatto Studio
////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bl_PlayerSettings : bl_PhotonHelper
{
    /// <summary>
    /// The tag of Player for default is "Player"
    /// </summary>
    public const string LocalTag = "Player";
    /// <summary>
    /// please if you have this tag in the tag list, add
    /// </summary>
    public string RemoteTag = "Remote";
    public Team m_Team = Team.All;
    [Header("when the player is our disable these scripts")]
    public List<MonoBehaviour> LocalDisabledScripts = new List<MonoBehaviour>();
    [Header("when the player is Not our disable these scripts")]
    public List<MonoBehaviour> RemoteDisabledScripts = new List<MonoBehaviour>();
    [Space(5)]
    public GameObject LocalObjects;
    public GameObject RemoteObjects;
    [Header("Player References")]
    public Camera PlayerCamera;
    public Transform FlagPosition;
    public Mesh RightHandMesh;

    [Header("Hands Textures")]
    public HandsLocal_ m_hands;

    [System.Serializable]
    public class HandsLocal_
    {
        public Material SlevesMat;
        public Material GlovesMat;
        [Space(5)]
        public Texture2D SlevesTeam1;
        public Texture2D GlovesTeam1;
        [Space(5)]
        public Texture2D SlevesTeam2;
        public Texture2D GlovesTeam2;
        [Space(5)]
        public bool useEffect = true;
        public Color HandsInitColor = new Color(1, 1, 1, 1);
        public Color mBettewColor = new Color(0.1f, 0.1f, 1, 1);
    }
   
    //private
    
    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (isMine)
        {
            LocalPlayer();
        }
        else
        {
            RemotePlayer();
        }
    }

    /// <summary>
    /// We call this function only if we are Remote player
    /// </summary>
    public void RemotePlayer()
    {
        foreach (MonoBehaviour script in RemoteDisabledScripts)
        {
            Destroy(script);
        }
        LocalObjects.SetActive(false);
        m_Team = photonView.owner.GetPlayerTeam();
        this.gameObject.tag = RemoteTag;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

    }
    /// <summary>
    /// We call this function only if we are Local player
    /// </summary>
    public void LocalPlayer()
    {
        gameObject.name = PhotonNetwork.player.NickName;
        if (myTeam == Team.Delta.ToString())
        {
            m_Team = Team.Delta;
            if (m_hands.SlevesMat != null)
            {
                m_hands.SlevesMat.mainTexture = m_hands.SlevesTeam1;
            }
            if (m_hands.GlovesMat != null)
            {
                m_hands.GlovesMat.mainTexture = m_hands.GlovesTeam1;
            }
        }
        else if (myTeam == Team.Recon.ToString())
        {
            m_Team = Team.Recon;
            if (m_hands.SlevesMat != null)
            {
                m_hands.SlevesMat.mainTexture = m_hands.SlevesTeam2;
            }
            if (m_hands.GlovesMat != null)
            {
                m_hands.GlovesMat.mainTexture = m_hands.GlovesTeam2;
            }
        }
        else
        {
            m_Team = Team.All;
            if (m_hands.SlevesMat != null)
            {
                m_hands.SlevesMat.mainTexture = m_hands.SlevesTeam2;
            }
            if (m_hands.GlovesMat != null)
            {
                m_hands.GlovesMat.mainTexture = m_hands.GlovesTeam2;
            }
        }
        if (m_hands.GlovesMat != null && m_hands.GlovesMat.HasProperty("_Color")
            && m_hands.SlevesMat != null && m_hands.SlevesMat.HasProperty("_Color") && m_hands.useEffect)
        {
            StartCoroutine(StartEffect());
        }
        foreach (MonoBehaviour script in LocalDisabledScripts)
        {
            Destroy(script);
        }
        RemoteObjects.SetActive(false);
        this.gameObject.tag = LocalTag;
#if GR
        transform.GetComponentInChildren<bl_GunManager>().isGunRace = (GetGameMode == GameMode.GR);
#endif
    }
    /// <summary>
    /// produce an effect of spawn
    /// with a loop 
    /// </summary>
    /// <returns></returns>
    IEnumerator StartEffect()
    {
        int loops = 8;// number of repeats
        for (int i = 0; i < loops; i++)
        {
            yield return new WaitForSeconds(0.25f);
            m_hands.GlovesMat.color = m_hands.mBettewColor;
            m_hands.SlevesMat.color = m_hands.mBettewColor;
            yield return new WaitForSeconds(0.25f);
            m_hands.GlovesMat.color = m_hands.HandsInitColor;
            m_hands.SlevesMat.color = m_hands.HandsInitColor;

        }
    }

    public bool isLocal { get { return photonView.isMine; } }
}