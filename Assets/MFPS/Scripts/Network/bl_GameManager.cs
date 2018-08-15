/////////////////////////////////////////////////////////////////////////////////
//////////////////////////////bl_GameManager.cs//////////////////////////////////
/////////////////place this in a scene for Spawn Players in Room/////////////////
/////////////////////////////////////////////////////////////////////////////////
////////////////////////////////Briner Games/////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables

public class bl_GameManager : bl_PhotonHelper {

    public static int m_view = -1;
    public static bool isAlive = false;
    public static int SuicideCount = 0;
    public static bool Joined = false;
    [HideInInspector]
    public GameObject OurPlayer;
    [Header("Global")]
    public string OnDisconnectReturn = "MainMenu";
    [Header("References")]

    /// <summary>
    /// Camera Preview
    /// </summary>
    public Camera m_RoomCamera;
    [HideInInspector]public List<Transform> AllSpawnPoints = new List<Transform>();
    private List<Transform> ReconSpawnPoint = new List<Transform>();
    private List<Transform> DeltaSpawnPoint = new List<Transform>();
    /// <summary>
    /// List with all Players in Current Room
    /// </summary>
    public List<PhotonPlayer> connectedPlayerList = new List<PhotonPlayer>();
    private bool EnterInGamePlay = false;
    [HideInInspector] public bool GameFinish = false;
    private bl_ChatRoom Chat;
#if UMM
    private Canvas MiniMapCanvas = null;
#endif

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (!PhotonNetwork.connected)
            return;

        PhotonNetwork.isMessageQueueRunning = true;
        Joined = false;
        SuicideCount = 0;
        bl_UCrosshair.Instance.Show(false);
        Chat = GetComponent<bl_ChatRoom>();
        Chat.AddLine(bl_GameTexts.OpenChatStart);

#if UMM
        MiniMapCanvas = FindObjectOfType<bl_MiniMap>().m_Canvas;
        MiniMapCanvas.enabled = false;
#endif
    }

    /// <summary>
    /// Spawn Player Function
    /// </summary>
    /// <param name="t_team"></param>
    public void SpawnPlayer(Team t_team)
    {
        if (!this.GetComponent<bl_RoomMenu>().SpectatorMode)
        {
            if (OurPlayer != null)
            {
                PhotonNetwork.Destroy(OurPlayer);
            }
            if (!GameFinish)
            {
                Hashtable PlayerTeam = new Hashtable();
                PlayerTeam.Add(PropertiesKeys.TeamKey, t_team.ToString());
                PhotonNetwork.player.SetCustomProperties(PlayerTeam, null, true);

                //spawn the player model
#if !PSELECTOR
                SpawnPlayerModel(t_team);

                Chat.Refresh();
                m_RoomCamera.gameObject.SetActive(false);
                StartCoroutine(bl_RoomMenu.FadeOut(1));
                bl_UtilityHelper.LockCursor(true);
                Joined = true;
#else

                bl_PlayerSelector ps = FindObjectOfType<bl_PlayerSelector>();
                if (ps.IsSelected)
                {
                    ps.SpawnSelected();
                }
                else
                {
                    ps.OpenSelection(t_team);
                }
#endif
#if UMM
    MiniMapCanvas.enabled = true;
#endif
            }
            else
            {
                m_RoomCamera.gameObject.SetActive(true);
            }
        }
        else
        {
            this.GetComponent<bl_RoomMenu>().WaitForSpectator = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="point"></param>
    public void RegisterSpawnPoint(bl_SpawnPoint point)
    {
        switch (point.m_Team)
        {
            case Team.Delta:
                DeltaSpawnPoint.Add(point.transform);
                break;
            case Team.Recon:
                ReconSpawnPoint.Add(point.transform);
                break;
        }
        AllSpawnPoints.Add(point.transform);
    }

    /// <summary>
    /// If Player exist, them destroy
    /// </summary>
    public void DestroyPlayer(bool ActiveCamera)
    {
        if (OurPlayer != null)
        {
            PhotonNetwork.Destroy(OurPlayer);
        }
        m_RoomCamera.gameObject.SetActive(ActiveCamera);
    } 

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void SpawnPlayerModel(Team t_team)
    {
        Vector3 pos;
        Quaternion rot;
        if (t_team == Team.Recon)
        {
            GetSpawn(ReconSpawnPoint.ToArray(),out pos,out rot);
            OurPlayer = PhotonNetwork.Instantiate(bl_GameData.Instance.Player1.name, pos, rot, 0);
        }
        else if (t_team == Team.Delta)
        {
            GetSpawn(DeltaSpawnPoint.ToArray(), out pos, out rot);
            OurPlayer = PhotonNetwork.Instantiate(bl_GameData.Instance.Player2.name, pos, rot, 0);
        }
        else
        {
            GetSpawn(AllSpawnPoints.ToArray(), out pos, out rot);
            OurPlayer = PhotonNetwork.Instantiate(bl_GameData.Instance.Player1.name, pos, rot, 0);
        }
        EnterInGamePlay = true;
        bl_EventHandler.PlayerLocalSpawnEvent();
        bl_UCrosshair.Instance.Show(true);
    }

#if PSELECTOR
     /// <summary>
    /// 
    /// </summary>
    public void SpawnSelectedPlayer(bl_PlayerSelectorInfo info,Team t_team)
    {
        Vector3 pos;
        Quaternion rot;
        if (t_team == Team.Recon)
        {
            GetSpawn(ReconSpawnPoint.ToArray(), out pos, out rot);
            OurPlayer = PhotonNetwork.Instantiate(info.Prefab.name, pos, rot, 0);
        }
        else if (t_team == Team.Delta)
        {
            GetSpawn(DeltaSpawnPoint.ToArray(), out pos, out rot);
            OurPlayer = PhotonNetwork.Instantiate(info.Prefab.name, pos, rot, 0);
        }
        else
        {
            GetSpawn(AllSpawnPoints.ToArray(), out pos, out rot);
            OurPlayer = PhotonNetwork.Instantiate(info.Prefab.name, pos, rot, 0);
        }

        this.GetComponent<bl_ChatRoom>().Refresh();
        m_RoomCamera.gameObject.SetActive(false);
        StartCoroutine(bl_RoomMenu.FadeOut(1));
        bl_UtilityHelper.LockCursor(true);
        Joined = true;

        EnterInGamePlay = true;
        bl_EventHandler.PlayerLocalSpawnEvent();
        bl_UCrosshair.Instance.Show(true);
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public void GetSpawn(Transform[] list, out Vector3 position, out Quaternion Rotation)
    {
       int random = Random.Range(0, list.Length);
       Vector3 s = Random.insideUnitSphere * list[random].GetComponent<bl_SpawnPoint>().SpawnSpace;
       position = list[random].position + new Vector3(s.x, 0, s.z);
       Rotation = list[random].rotation;
    }

    public Transform GetAnSpawnPoint
    {
        get { return AllSpawnPoints[Random.Range(0, AllSpawnPoints.Count)]; }
    }

    //This is called only when the current gameobject has been Instantiated via PhotonNetwork.Instantiate
    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        base.OnPhotonInstantiate(info);
        Debug.Log("New object instantiated by " + info.sender);
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMaster)
    {
        base.OnMasterClientSwitched(newMaster);
        Debug.Log("The old masterclient left, we have a new masterclient: " + newMaster);
        this.GetComponent<bl_ChatRoom>().AddLine("We have a new masterclient: " + newMaster);
    }

    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
       // Debug.Log(playerAndUpdatedProps[1].ToString());
    }

    public override void OnDisconnectedFromPhoton()
    {
        base.OnDisconnectedFromPhoton();
        Debug.Log("Clean up a bit after server quit");

        /* 
        * To reset the scene we'll just reload it:
        */
        PhotonNetwork.isMessageQueueRunning = false;
        bl_UtilityHelper.LoadLevel(OnDisconnectReturn);
    }
    //PLAYER EVENTS
    public override void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        base.OnPhotonPlayerConnected(player);
        Debug.Log("Player connected: " + player);
    }

    public override void OnReceivedRoomListUpdate()
    {
        base.OnReceivedRoomListUpdate();
    }
    public override void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        base.OnPhotonPlayerDisconnected(player);
        Debug.Log("Player disconnected: " + player);

    }
    public override void OnFailedToConnectToPhoton(DisconnectCause Cause)
    {
        base.OnFailedToConnectToPhoton(Cause);
        Debug.Log("OnFailedToConnectToPhoton "+Cause);

        // back to main menu or first scene       
        bl_UtilityHelper.LoadLevel(OnDisconnectReturn);
    }

    public bool isEnterinGamePlay
    {
        get
        {
            return EnterInGamePlay;
        }
    }

}		