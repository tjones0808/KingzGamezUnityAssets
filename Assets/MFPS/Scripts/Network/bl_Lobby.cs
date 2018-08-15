using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class bl_Lobby : bl_PhotonHelper
{


    private string playerName;
    private string hostName; //Name of room
    [Header("Photon")]
    public string AppVersion = "1.0";
    [SerializeField]private string RoomNamePrefix = "LovattoRoom {0}";
    [SerializeField]private string PlayerNamePrefix = "Guest {0}";
    public float UpdateServerListEach = 2;
    public bool ShowPhotonStatus;
    public bool ShowPhotonStatistics;

    [Header("Room Options")]
    //Max players in game
    public int[] maxPlayers = new int[] { 6, 2, 4, 8 };
    private int players;
    //Room Time in seconds
    public int[] RoomTime = new int[] { 600, 300, 900, 1200 };
    private int r_Time;
    //Room Max Kills
    public int[] RoomKills = new int[] {50,100,150,200 };
    private int CurrentMaxKills = 0;
    //Room Max Ping
    public int[] MaxPing = new int[] { 100, 200, 500, 1000 };
    private int CurrentMaxPing = 0;
    #if BDGM
    public int[] BDRounds = new int[] { 5, 3, 7, 9 };
    private int CurrentBDRound;
    #endif

    private string[] GameModes;
    private int CurrentGameMode = 0;

    [Header("Levels Manager")]
    public List<AllScenes> m_scenes = new List<AllScenes>();

    [Header("References")]
    public List<GameObject> MenusUI = new List<GameObject>();
    public GameObject FadeImage;
    [SerializeField] private Text PhotonStatusText = null;
    [SerializeField] private Text PlayerNameText = null;
    [SerializeField] private GameObject RoomInfoPrefab;
    [SerializeField]private GameObject TopInfoUI;
    [SerializeField]private GameObject PhotonStaticticsUI;
    [SerializeField] private GameObject PhotonGamePrefab;
    [SerializeField] private GameObject EnterPasswordUI;
    [SerializeField] private GameObject PingKickMessageUI;
    [SerializeField] private GameObject AFKKickMessageUI;
    [SerializeField] private GameObject SeekingMatchUI;
    [SerializeField] private Transform RoomListPanel;
    [SerializeField] private CanvasGroup CanvasGroupRoot = null;
    [SerializeField] private Text MaxPlayerText = null;
    [SerializeField] private Text RoundTimeText = null;
    [SerializeField] private Text GameModeText = null;
    [SerializeField] private Text MaxKillsText = null;
    [SerializeField] private Text MaxPingText = null;
    [SerializeField] private Text MapNameText = null;
    [SerializeField] private Text BDRoundText;
    [SerializeField] private Text PasswordLogText = null;
    [SerializeField]private Text NoRoomText;
    [SerializeField] private Slider VolumeSlider;
    [SerializeField] private Slider BackgroundVolumeSlider;
    [SerializeField] private Slider SensitivitySlider;
    [SerializeField] private Slider AimSensitivitySlider;
    [SerializeField] private Slider WeaponFOVSlider;
    [SerializeField] private Dropdown QualityDropdown;
    [SerializeField] private Dropdown AnisoDropdown;
    [SerializeField] private Image MapPreviewImage = null;
    public Image LevelIcon;
    [SerializeField] private InputField PlayerNameField = null;
    [SerializeField] private InputField RoomNameField = null;
    [SerializeField] private InputField PassWordField;
    [SerializeField] private Toggle MuteVoiceToggle;
    [SerializeField] private Toggle PushToTalkToogle;
    [SerializeField] private Toggle InvertMouseXToogle;
    [SerializeField] private Toggle InvertMouseYToogle;
    [SerializeField] private Toggle FrameRateToggle;
    [SerializeField] private Toggle WithBotsToggle;
    [SerializeField] private Dropdown ServersDropdown;
    [SerializeField] private GameObject KickMessageUI;
    public GameObject MaxPingMessageUI;
    [SerializeField] private Button[] ClassButtons;
    public GameObject[] AddonsButtons;
    public LevelUI m_LevelUI;

    [Header("Audio")]
    [SerializeField]private  AudioClip BackgroundSound;
    [SerializeField, Range(0, 1)] private float MaxBackgroundVolume = 0.3f;

    //OPTIONS
    private int m_currentQuality = 3;
    private float m_volume = 1.0f;
    private float BackgroundVolume = 0.3f;
    private float m_sensitive = 15;
    private float AimSensitivity = 7;
    private bool FrameRate = false;
    private bool imh;
    private bool imv;
    private bool MuteVoice;
    private bool PushToTalk;
    private int m_stropic = 0;
    private bool GamePerRounds = false;
    private bool AutoTeamSelection = false;
    private bool FriendlyFire = false;
    private float alpha = 2.0f;
    [Serializable]
    public class AllScenes
    {
        public string m_name;
        public string m_SceneName;
        public Sprite m_Preview;
    }

    private List<GameObject> CacheRoomList = new List<GameObject>();
    private int CurrentScene = 0;
    private bl_LobbyChat Chat;
    private string CachePlayerName = "";
    private AudioSource BackSource;
    private RoomInfo checkingRoom;
    private int PendingRegion = -1;
    private bool FirstConnectionMade = false;
#if ULSP
    private bl_DataBase DataBase;
#endif

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        bl_UtilityHelper.LockCursor(false);
        Application.targetFrameRate = 60;
        PhotonNetwork.autoJoinLobby = true;
        Chat = GetComponent<bl_LobbyChat>();
#if ULSP
        DataBase = FindObjectOfType<bl_DataBase>();
#endif
        hostName = string.Format(RoomNamePrefix, Random.Range(10, 999));
        RoomNameField.text = hostName;
        GameModes = Enum.GetNames(typeof(GameMode));
        if(FindObjectOfType<bl_PhotonGame>() == null) { Instantiate(PhotonGamePrefab); }
        if (BackgroundSound != null)
        {
            BackSource = gameObject.AddComponent<AudioSource>();
            BackSource.clip = BackgroundSound;
            BackSource.volume = 0;
            BackSource.playOnAwake = false;
            BackSource.loop = true;
        }
        GetPlayerName();
        LoadSettings();
        SetUpUI();
        if (BackgroundSound != null) { StartCoroutine(FadeAudioBack(true)); }
        InvokeRepeating("UpdateServerList", 0, UpdateServerListEach);
        FadeImage.SetActive(false);
        if(PhotonStaticticsUI != null) { PhotonStaticticsUI.SetActive(ShowPhotonStatistics); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t_state"></param>
    /// <returns></returns>
    IEnumerator Fade(LobbyState t_state, float t = 2.0f)
    {
        alpha = 0.0f;
        while (alpha < t)
        {
            alpha += Time.deltaTime;
            CanvasGroupRoot.alpha = alpha;
            yield return null;
        }
    }

    IEnumerator FadeAudioBack(bool up)
    {
        if (up)
        {
            BackSource.Play();
            while (BackSource.volume < BackgroundVolume)
            {
                BackSource.volume += Time.deltaTime * 0.01f;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (BackSource.volume > 0)
            {
                BackSource.volume -= Time.deltaTime * 0.5f;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void ConnectPhoton()
    {
        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (!PhotonNetwork.connected || PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
        {
            PhotonNetwork.AuthValues = new AuthenticationValues(PhotonNetwork.playerName);
            if (PendingRegion == -1)
            {
                PhotonNetwork.ConnectUsingSettings(AppVersion);
            }
            else
            {
                FirstConnectionMade = true;
                ChangeServerCloud(PendingRegion);
            }
            ChangeWindow(3);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    void UpdateServerList()
    {
        ServerList();
    }
    /// <summary>
    /// 
    /// </summary>
    void FixedUpdate()
    {
        if (PhotonNetwork.connected)
        {
            if (ShowPhotonStatus)
            {
                PhotonStatusText.text = "<color=#FFE300>STATUS:</color>  " + PhotonNetwork.connectionStateDetailed.ToString().ToUpper();
                PlayerNameText.text = "<color=#FFE300>Welcome,</color>  " + PhotonNetwork.player.NickName + bl_GameData.Instance.RolePrefix;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ServerList()
    {
        //Removed old list
        if (CacheRoomList.Count > 0)
        {
            foreach (GameObject g in CacheRoomList)
            {
                Destroy(g);
            }
            CacheRoomList.Clear();
        }
        //Update List
        RoomInfo[] ri = PhotonNetwork.GetRoomList();
        if (ri.Length > 0)
        {
            NoRoomText.text = string.Empty;
            for (int i = 0; i < ri.Length; i++)
            {
                GameObject r = Instantiate(RoomInfoPrefab) as GameObject;
                CacheRoomList.Add(r);
                r.GetComponent<bl_RoomInfo>().GetInfo(ri[i]);
                r.transform.SetParent(RoomListPanel, false);
            }
           
        }
        else
        {
            NoRoomText.text = bl_GameTexts.NoRoomsCreated;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnterName(InputField field = null)
    {
        if (field == null || string.IsNullOrEmpty(field.text))
            return;

        CachePlayerName = field.text;
        int check = bl_GameData.Instance.CheckPlayerName(CachePlayerName);
        if (check == 1)
        {
            MenusUI[9].SetActive(true);
            return;
        }
        else if (check == 2)
        {
            field.text = string.Empty;
            return;
        }

        playerName = CachePlayerName;
        playerName = playerName.Replace("\n", "");
        StartCoroutine(Fade(LobbyState.MainMenu));

        PhotonNetwork.playerName = playerName;
        ConnectPhoton();
        if (Chat != null && bl_GameData.Instance.UseLobbyChat) { Chat.Connect(AppVersion); }
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnterPassword(InputField field = null)
    {
        if (field == null || string.IsNullOrEmpty(field.text))
            return;

        string pass = field.text;
        if (bl_GameData.Instance.CheckPasswordUse(CachePlayerName, pass))
        {
            playerName = CachePlayerName;
            playerName = playerName.Replace("\n", "");
            StartCoroutine(Fade(LobbyState.MainMenu));

            PhotonNetwork.playerName = playerName;
            ConnectPhoton();
            if (Chat != null && bl_GameData.Instance.UseLobbyChat) { Chat.Connect(AppVersion); }
        }
        else
        {
            field.text = string.Empty;
        }
    }
    public void ClosePassField() { MenusUI[9].SetActive(false); }


    private bool isSeekingMatch = false;
    public void AutoMatch()
    {
        if (isSeekingMatch)
            return;

        isSeekingMatch = true;
        StartCoroutine(AutoMatchIE());
    }

    IEnumerator AutoMatchIE()
    {
        //active the search match UI
        SeekingMatchUI.SetActive(true);
        yield return new WaitForSeconds(2);
        PhotonNetwork.JoinRandomRoom();
         isSeekingMatch = false;
        SeekingMatchUI.SetActive(false);
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        base.OnPhotonRandomJoinFailed(codeAndMsg);
        Debug.Log("Don't found any available room, create one automatically!");

        int propsCount = 11;
        string roomName = string.Format("[PUBLIC] {0}{1}", PhotonNetwork.playerName.Substring(0, 2), Random.Range(0, 9999));
        int scid = Random.Range(0, m_scenes.Count);
        int maxPlayersRandom = Random.Range(0, maxPlayers.Length);
        int timeRandom = Random.Range(0, RoomTime.Length);
        int killsRandom = Random.Range(0, RoomKills.Length);
        int modeRandom = Random.Range(0, GameModes.Length);

        ExitGames.Client.Photon.Hashtable roomOption = new ExitGames.Client.Photon.Hashtable();
        roomOption[PropertiesKeys.TimeRoomKey] = RoomTime[timeRandom];
        roomOption[PropertiesKeys.GameModeKey] = GameModes[modeRandom];
        roomOption[PropertiesKeys.SceneNameKey] = m_scenes[scid].m_SceneName;
        roomOption[PropertiesKeys.RoomRoundKey] = RoundStyle.OneMacht;
        roomOption[PropertiesKeys.TeamSelectionKey] = false;
        roomOption[PropertiesKeys.CustomSceneName] = m_scenes[scid].m_name;
        roomOption[PropertiesKeys.RoomMaxKills] = RoomKills[killsRandom];
        roomOption[PropertiesKeys.RoomFriendlyFire] = false;
        roomOption[PropertiesKeys.MaxPing] = MaxPing[CurrentMaxPing];
        roomOption[PropertiesKeys.RoomPassworld] = string.Empty;
        roomOption[PropertiesKeys.WithBotsKey] = GameModes[modeRandom] == GameMode.FFA.ToString() ? true : false;
#if BDGM
        roomOption[PropertiesKeys.BombDefuseRounds] = BDRounds[CurrentBDRound];
         propsCount = 12;
#endif

        string[] properties = new string[propsCount];
        properties[0] = PropertiesKeys.TimeRoomKey;
        properties[1] = PropertiesKeys.GameModeKey;
        properties[2] = PropertiesKeys.SceneNameKey;
        properties[3] = PropertiesKeys.RoomRoundKey;
        properties[4] = PropertiesKeys.TeamSelectionKey;
        properties[5] = PropertiesKeys.CustomSceneName;
        properties[6] = PropertiesKeys.RoomMaxKills;
        properties[7] = PropertiesKeys.RoomFriendlyFire;
        properties[8] = PropertiesKeys.MaxPing;
        properties[9] = PropertiesKeys.RoomPassworld;
        properties[10] = PropertiesKeys.WithBotsKey;
#if BDGM
        properties[11] = PropertiesKeys.BombDefuseRounds;
#endif

        PhotonNetwork.CreateRoom(roomName, new RoomOptions()
        {
            MaxPlayers = (byte)maxPlayers[maxPlayersRandom],
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            CustomRoomPropertiesForLobby = properties

        }, null);
        FadeImage.SetActive(true);
        FadeImage.GetComponent<Animator>().speed = 2;
        if (BackSource != null) { StartCoroutine(FadeAudioBack(false)); }
    }


    /// <summary>
    /// 
    /// </summary>
    public void CreateRoom()
    {
        int propsCount = 11;
        PhotonNetwork.playerName = playerName;
        //Save Room properties for load in room
        ExitGames.Client.Photon.Hashtable roomOption = new ExitGames.Client.Photon.Hashtable();
        roomOption[PropertiesKeys.TimeRoomKey] = RoomTime[r_Time];
        roomOption[PropertiesKeys.GameModeKey] = GameModes[CurrentGameMode];
        roomOption[PropertiesKeys.SceneNameKey] = m_scenes[CurrentScene].m_SceneName;
        roomOption[PropertiesKeys.RoomRoundKey] = GamePerRounds ? RoundStyle.Rounds : RoundStyle.OneMacht;
        roomOption[PropertiesKeys.TeamSelectionKey] = AutoTeamSelection;
        roomOption[PropertiesKeys.CustomSceneName] = m_scenes[CurrentScene].m_name;
        roomOption[PropertiesKeys.RoomMaxKills] = RoomKills[CurrentMaxKills];
        roomOption[PropertiesKeys.RoomFriendlyFire] = FriendlyFire;
        roomOption[PropertiesKeys.MaxPing] = MaxPing[CurrentMaxPing];
        roomOption[PropertiesKeys.RoomPassworld] = PassWordField.text;
        roomOption[PropertiesKeys.WithBotsKey] = GameModes[CurrentGameMode] == GameMode.FFA.ToString() ? WithBotsToggle.isOn : false;
#if BDGM
        roomOption[PropertiesKeys.BombDefuseRounds] = BDRounds[CurrentBDRound];
         propsCount = 12;
#endif

        string[] properties = new string[propsCount];
        properties[0] = PropertiesKeys.TimeRoomKey;
        properties[1] = PropertiesKeys.GameModeKey;
        properties[2] = PropertiesKeys.SceneNameKey;
        properties[3] = PropertiesKeys.RoomRoundKey;
        properties[4] = PropertiesKeys.TeamSelectionKey;
        properties[5] = PropertiesKeys.CustomSceneName;
        properties[6] = PropertiesKeys.RoomMaxKills;
        properties[7] = PropertiesKeys.RoomFriendlyFire;
        properties[8] = PropertiesKeys.MaxPing;
        properties[9] = PropertiesKeys.RoomPassworld;
        properties[10] = PropertiesKeys.WithBotsKey;
#if BDGM
        properties[11] = PropertiesKeys.BombDefuseRounds;
#endif

        PhotonNetwork.CreateRoom(hostName, new RoomOptions()
        {
            MaxPlayers = (byte)maxPlayers[players],
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            CustomRoomPropertiesForLobby = properties,
            PublishUserId = true,
        }, null);
        FadeImage.SetActive(true);
        FadeImage.GetComponent<Animator>().speed = 2;
        if(BackSource != null) { StartCoroutine(FadeAudioBack(false)); }
    }


#region UGUI

    public void CheckRoomPassword(RoomInfo room)
    {
        checkingRoom = room;
        EnterPasswordUI.SetActive(true);
    }

    public void OnEnterPassworld(InputField pass)
    {
        if (checkingRoom == null)
        {
            Debug.Log("Checking room is not assigned more!");
            return;
        }

        if((string)checkingRoom.CustomProperties[PropertiesKeys.RoomPassworld] == pass.text && checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
        {
            if (PhotonNetwork.GetPing() < (int)checkingRoom.CustomProperties[PropertiesKeys.MaxPing])
            {
                FadeImage.SetActive(true);
                FadeImage.GetComponent<Animator>().speed = 2;
                if (checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
                {
                    PhotonNetwork.JoinRoom(checkingRoom.Name);
                }
            }
        }
        else
        {
            PasswordLogText.text = "Wrong room password";
        }
    }

    /// <summary>
    /// For button can call this 
    /// </summary>
    /// <param name="id"></param>
    public void ChangeWindow(int id)
    {
        ChangeWindow(id, -1);
    }

    public void OnChangeClass(int Class)
    {
        foreach (Button b in ClassButtons) { b.interactable = true; }
        ClassButtons[Class].interactable = false;
        PlayerClass p = (PlayerClass)Class;
        p.SavePlayerClass();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void ChangeWindow(int id, int id2)
    {
        StartCoroutine(Fade(LobbyState.MainMenu, 3f));
        for (int i = 0; i < MenusUI.Count; i++)
        {
            if (i == id || i == id2)
            {
                MenusUI[i].SetActive(true);
            }
            else
            {
                if (i != 1)//1 = main menu buttons
                {
                    if (MenusUI[i] != null)
                    {
                        MenusUI[i].SetActive(false);
                    }
                }
                if (id == 8)
                {
                    MenusUI[1].SetActive(false);
                }
            }
        }
    }

    public void Disconect()
    {
        if (PhotonNetwork.connected)
        { PhotonNetwork.Disconnect(); }
        if (Chat != null) { Chat.Disconnect(); }
    }

    void DelayDisconnect() { PhotonNetwork.Disconnect(); }
    /// <summary>
    /// 
    /// </summary>
    public void ChangeServerCloud(int id)
    {      
        if (PhotonNetwork.connected && FirstConnectionMade)
        {
            ChangeWindow(3);
            MenusUI[1].SetActive(false);
            PendingRegion = id;
            Invoke("DelayDisconnect", 0.2f);
            return;
        }     
        if (!string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppID))
        {
            if (!FirstConnectionMade)
            {
                PendingRegion = id;
                return;
            }
            CloudRegionCode code = CloudRegionCode.usw;
            if(id > 3) { id++; }
            code = (CloudRegionCode)id;
            PhotonNetwork.ConnectToRegion(code, AppVersion);
            PlayerPrefs.SetString(PropertiesKeys.PreferredRegion, code.ToString());
        }
        else
        {
            Debug.LogWarning("Need your AppId for change server, please add it in PhotonServerSettings");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="level"></param>
    public void LoadLocalLevel(string level)
    {
        Disconect();
        bl_UtilityHelper.LoadLevel(level);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="plus"></param>
    public void ChangeMaxPlayer(bool plus)
    {
        if (plus)
        {
            if (players < maxPlayers.Length)
            {
                players++;
                if (players > (maxPlayers.Length - 1)) players = 0;

            }
        }
        else
        {
            if (players < maxPlayers.Length)
            {
                players--;
                if (players < 0) players = maxPlayers.Length - 1;
            }
        }
        MaxPlayerText.text = string.Format("{0} {1}", maxPlayers[players], bl_GameTexts.Players);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="plus"></param>
    public void ChangeRoundTime(bool plus)
    {
        if (!plus)
        {
            if (r_Time < RoomTime.Length)
            {
                r_Time--;
                if (r_Time < 0)
                {
                    r_Time = RoomTime.Length - 1;

                }
            }
        }
        else
        {
            if (r_Time < RoomTime.Length)
            {
                r_Time++;
                if (r_Time > (RoomTime.Length - 1))
                {
                    r_Time = 0;

                }

            }
        }
        RoundTimeText.text = (RoomTime[r_Time] / 60) + " Minutes";
    }


    public void ChangeBDRounds(bool plus)
    {
#if BDGM
        if (plus)
        {
            if (CurrentBDRound < BDRounds.Length)
            {
                CurrentBDRound++;
                if (CurrentBDRound > (BDRounds.Length - 1)) CurrentBDRound = 0;

            }
        }
        else
        {
            if (CurrentBDRound < BDRounds.Length)
            {
                CurrentBDRound--;
                if (CurrentBDRound < 0) CurrentBDRound = BDRounds.Length - 1;
            }
        }
        BDRoundText.text = BDRounds[CurrentBDRound] + " Rounds";
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeMaxKills(bool plus)
    {
        if (!plus)
        {
            if (CurrentMaxKills < RoomKills.Length)
            {
                CurrentMaxKills--;
                if (CurrentMaxKills < 0)
                {
                    CurrentMaxKills = RoomKills.Length - 1;

                }
            }
        }
        else
        {
            if (CurrentMaxKills < RoomKills.Length)
            {
                CurrentMaxKills++;
                if (CurrentMaxKills > (RoomKills.Length - 1))
                {
                    CurrentMaxKills = 0;

                }

            }
        }
        MaxKillsText.text = (RoomKills[CurrentMaxKills]) + " Kills";
    }

    public void ChangeMaxPing(bool fowr)
    {
        if (fowr)
        {
            if (CurrentMaxPing >= MaxPing.Length - 1) { CurrentMaxPing = 0; }
            else
            {
                CurrentMaxPing = CurrentMaxPing + 1 % MaxPing.Length;
            }
        }
        else
        {
            if (CurrentMaxPing > 0) { CurrentMaxPing = CurrentMaxPing - 1 % MaxPing.Length; }
            else { CurrentMaxPing = MaxPing.Length - 1; }
        }
        string f = (MaxPing[CurrentMaxPing] == 0) ? bl_GameTexts.NoLimit : "{0} ms";
        MaxPingText.text = string.Format(f, MaxPing[CurrentMaxPing]);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeGameMode(bool plus)
    {
        if (plus)
        {
            if (CurrentGameMode < GameModes.Length)
            {
                CurrentGameMode++;
                if (CurrentGameMode > (GameModes.Length - 1))
                {
                    CurrentGameMode = 0;
                }
            }
        }
        else
        {
            if (CurrentGameMode < GameModes.Length)
            {
                CurrentGameMode--;
                if (CurrentGameMode < 0)
                {
                    CurrentGameMode = GameModes.Length - 1;

                }
            }
        }
        WithBotsToggle.gameObject.SetActive(GameModes[CurrentGameMode] == GameMode.FFA.ToString());
        GameModeText.text = GameModes[CurrentGameMode];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="plus"></param>
    public void ChangeMap(bool plus)
    {
        if (!plus)
        {
            if (CurrentScene < m_scenes.Count)
            {
                CurrentScene--;
                if (CurrentScene < 0)
                {
                    CurrentScene = m_scenes.Count - 1;

                }
            }
        }
        else
        {
            if (CurrentScene < m_scenes.Count)
            {
                CurrentScene++;
                if (CurrentScene > (m_scenes.Count - 1))
                {
                    CurrentScene = 0;
                }
            }
        }
        MapNameText.text = m_scenes[CurrentScene].m_name;
        MapPreviewImage.sprite = m_scenes[CurrentScene].m_Preview;
    }

    public void OnChangeQuality(int id)
    {
        QualitySettings.SetQualityLevel(id, true);
        m_currentQuality = id;
    }

    public void OnChangeAniso(int id)
    {
        if (id == 0)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        }
        else if (id == 1)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        }
        else
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        }
        m_stropic = id;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    public void QuitGame(bool b)
    {
        if (b)
        {
            Chat.Disconnect();
            Application.Quit();
            Debug.Log("Game Exit, this only work in standalone version");
        }
        else
        {
            StartCoroutine(Fade(LobbyState.MainMenu, 3.2f));
            ChangeWindow(2, 1);
        }
    }

    public void CloseKickMessage()
    {
        KickMessageUI.SetActive(false);
        MaxPingMessageUI.SetActive(false);
        PingKickMessageUI.SetActive(false);
        bl_PhotonGame.Instance.hasPingKick = false;
    }

    public void ChangeAutoTeamSelection(bool b) { AutoTeamSelection = b; }
    public void ChangeFriendlyFire(bool b) { FriendlyFire = b; }
    public void ChangeGamePerRound(bool b) { GamePerRounds = b; }
    public void ChangeRoomName(string t) { hostName = t; }
    public void ChangeVolume(float v) { m_volume = v; }
    public void ChangeSensitivity(float s) { m_sensitive = s; }
    public void ChangeAimSensitivity(float s) { AimSensitivity = s; }
    public void ChangeWeaponFov(float s) { m_WeaponFov = Mathf.FloorToInt(s); }
    public void OnChangeFrameRate(bool b) { FrameRate = b; }
    public void OnChangeIMV(bool b) { imv = b; }
    public void OnChangeIMH(bool b) { imh = b; }
    public void OnChangeMuteVoice(bool b) { MuteVoice = b; }
    public void OnChangePushToTalk(bool b) { PushToTalk = b; }
    public void OnBackgroundVolume(float v) { BackgroundVolume = v; BackSource.volume = v; }

    /// <summary>
    /// 
    /// </summary>
    void SetUpUI()
    {
        if (PhotonNetwork.connected)
        {
            TopInfoUI.SetActive(true);
        }
        MaxPlayerText.text = string.Format("{0} {1}", maxPlayers[players], bl_GameTexts.Players);
        RoundTimeText.text = (RoomTime[r_Time] / 60) + " Minutes";
        GameModeText.text = GameModes[CurrentGameMode];
        MaxKillsText.text = (RoomKills[CurrentMaxKills]) + " Kills";
        MapNameText.text = m_scenes[CurrentScene].m_name;
        MapPreviewImage.sprite = m_scenes[CurrentScene].m_Preview;
        SensitivitySlider.value = m_sensitive;
        AimSensitivitySlider.value = AimSensitivity;
        BackgroundVolumeSlider.maxValue = MaxBackgroundVolume;
        BackgroundVolumeSlider.value = BackgroundVolume;
        VolumeSlider.value = m_volume;
        List<Dropdown.OptionData> od = new List<Dropdown.OptionData>();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            Dropdown.OptionData data = new Dropdown.OptionData();
            data.text = QualitySettings.names[i];
            od.Add(data);
        }
        WithBotsToggle.gameObject.SetActive(false);
        QualityDropdown.AddOptions(od);
        QualityDropdown.value = m_currentQuality;
        PlayerClass p = PlayerClass.Assault; p = p.GetSavePlayerClass();
        OnChangeClass((int)p);
        MaxPingText.text = string.Format("{0} ms", MaxPing[CurrentMaxPing]);
        InvertMouseXToogle.isOn = imh;
        InvertMouseYToogle.isOn = imv;
        PushToTalkToogle.isOn = PushToTalk;
        MuteVoiceToggle.isOn = MuteVoice;
        FrameRateToggle.isOn = FrameRate;
        OnChangeQuality(m_currentQuality);
        OnChangeAniso(m_stropic);
#if BDGM
        BDRoundText.text = BDRounds[CurrentBDRound] + " Rounds";
         BDRoundText.transform.parent.gameObject.SetActive(true);
#else
        BDRoundText.transform.parent.gameObject.SetActive(false);
#endif
#if LM
         LevelIcon.gameObject.SetActive(true);
         LevelIcon.sprite = bl_LevelManager.Instance.GetLevel().Icon;
#else
        LevelIcon.gameObject.SetActive(false);
#endif
        SetRegionDropdown();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Save()
    {
        PlayerPrefs.SetInt(PropertiesKeys.Quality, m_currentQuality);
        PlayerPrefs.SetInt(PropertiesKeys.Aniso, m_stropic);
        PlayerPrefs.SetFloat(PropertiesKeys.Volume, m_volume);
        PlayerPrefs.SetFloat(PropertiesKeys.BackgroundVolume, BackgroundVolume);
        PlayerPrefs.SetFloat(PropertiesKeys.Sensitivity, m_sensitive);
        PlayerPrefs.SetFloat(PropertiesKeys.SensitivityAim, AimSensitivity);
        PlayerPrefs.SetInt(PropertiesKeys.WeaponFov, m_WeaponFov);
        PlayerPrefs.SetInt(PropertiesKeys.Quality, m_currentQuality);
        PlayerPrefs.SetInt(PropertiesKeys.Aniso, m_stropic);
        PlayerPrefs.SetInt(PropertiesKeys.MuteVoiceChat, MuteVoice ? 1 : 0);
        PlayerPrefs.SetInt(PropertiesKeys.PushToTalk, PushToTalk ? 1 : 0);
        PlayerPrefs.SetInt(PropertiesKeys.FrameRate, FrameRate ? 1 : 0);
        PlayerPrefs.SetInt(PropertiesKeys.InvertMouseHorizontal, imh ? 1 : 0);
        PlayerPrefs.SetInt(PropertiesKeys.InvertMouseVertical, imv ? 1 : 0);
    }

    public void ResetSettings()
    {
        LoadSettings();
        SetUpUI();
    }

    void SetRegionDropdown()
    {
        string key = PlayerPrefs.GetString(PropertiesKeys.PreferredRegion, PhotonNetwork.PhotonServerSettings.PreferredRegion.ToString());       
        string[] Regions = Enum.GetNames(typeof(CloudRegionCode));
        for(int i = 0; i < Regions.Length; i++)
        {
            if(key == Regions[i])
            {
                int id = i;
                if(id > 4) { id--; }
                ServersDropdown.value = id;
                break;
            }
        }
        ServersDropdown.RefreshShownValue();
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void GetPlayerName()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.playerName))
        {
#if ULSP
            if (DataBase != null && !DataBase.isGuest)
            {
                playerName = DataBase.LocalUser.NickName;
                PhotonNetwork.playerName = playerName;
                GoToMainMenu();
            }
            else
            {
                playerName = string.Format(PlayerNamePrefix, Random.Range(1, 9999));
                PlayerNameField.text = playerName;
                PhotonNetwork.playerName = playerName;
                ChangeWindow(0);
            }
#else
                playerName = string.Format(PlayerNamePrefix, Random.Range(1, 9999));
                PlayerNameField.text = playerName;
                PhotonNetwork.playerName = playerName;
                ChangeWindow(0);
#endif
        }
        else
        {
            GoToMainMenu();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GoToMainMenu()
    {
        StartCoroutine(Fade(LobbyState.MainMenu, 1.2f));
        if (!PhotonNetwork.connected)
        {
            ConnectPhoton();
        }
        ChangeWindow(2, 1);
        if (Chat != null && bl_GameData.Instance.UseLobbyChat) { Chat.Connect(AppVersion); }
    }

    /// <summary>
    /// 
    /// </summary>
	AudioSource PlayAudioClip(AudioClip clip, Vector3 position, float volume)
    {
        GameObject go = new GameObject("One shot audio");
        go.transform.position = position;
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.Play();
        Destroy(go, clip.length);
        return source;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToGameScene()
    {
        //Wait for check
        Chat.Disconnect();
        while (PhotonNetwork.room == null)
        {
            yield return null;
        }
        PhotonNetwork.isMessageQueueRunning = false;
        bl_UtilityHelper.LoadLevel((string)PhotonNetwork.room.CustomProperties[PropertiesKeys.SceneNameKey]);
    }
    // LOBBY EVENTS

    public override void OnJoinedLobby()
    {
        Debug.Log("We joined the lobby. " + PhotonNetwork.player.UserId);
        StartCoroutine(Fade(LobbyState.MainMenu));
        ChangeWindow(2, 1);
        if(PendingRegion != -1) { }
        TopInfoUI.SetActive(true);
    }

    public override void OnLeftLobby()
    {
        Debug.Log("We left the lobby.");
    }

    // ROOMLIST

    void OnReceivedRoomList()
    {
        Debug.Log("We received a new room list, total rooms: " + PhotonNetwork.GetRoomList().Length);
    }

    public override void OnReceivedRoomListUpdate()
    {
        Debug.Log("We received a room list update, total rooms now: " + PhotonNetwork.GetRoomList().Length);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("We have joined a room.");
        StartCoroutine(MoveToGameScene());
    }

    public override void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        FadeImage.SetActive(false);
        Debug.LogWarning("OnFailedToConnectToPhoton: " + cause);
    }

    public override void OnConnectionFail(DisconnectCause cause)
    {
        FadeImage.SetActive(false);
        Debug.LogWarning("OnConnectionFail: " + cause);
    }

    public override void OnDisconnectedFromPhoton()
    {
        base.OnDisconnectedFromPhoton();
        if (PendingRegion == -1)
        {
            Debug.Log("Disconnect from cloud!");
        }
        else
        {
            Debug.Log("Changing server!");
            ChangeServerCloud(PendingRegion);
        }
    }

    public override void OnConnectedToPhoton()
    {
        base.OnConnectedToPhoton();
        FirstConnectionMade = true;
        Debug.Log("Server connection established to: " + PhotonNetwork.PhotonServerSettings.PreferredRegion);
    }

    /// <summary>
    /// 
    /// </summary>
    void LoadSettings()
    {
        m_currentQuality = PlayerPrefs.GetInt(PropertiesKeys.Quality, 3);
        m_stropic = PlayerPrefs.GetInt(PropertiesKeys.Aniso, 2);
        m_volume = PlayerPrefs.GetFloat(PropertiesKeys.Volume, 1);
        BackgroundVolume = PlayerPrefs.GetFloat(PropertiesKeys.BackgroundVolume, MaxBackgroundVolume);
        BackSource.volume = BackgroundVolume;
        AudioListener.volume = m_volume;
        m_sensitive = PlayerPrefs.GetFloat(PropertiesKeys.Sensitivity, bl_GameData.Instance.DefaultSensitivity);
        AimSensitivity = PlayerPrefs.GetFloat(PropertiesKeys.SensitivityAim, 2);
        m_WeaponFov = PlayerPrefs.GetInt(PropertiesKeys.WeaponFov, 60);
        FrameRate = (PlayerPrefs.GetInt(PropertiesKeys.FrameRate, 0) == 1);
        imv = (PlayerPrefs.GetInt(PropertiesKeys.InvertMouseVertical, 0) == 1);
        imh = (PlayerPrefs.GetInt(PropertiesKeys.InvertMouseHorizontal, 0) == 1);
        MuteVoice = (PlayerPrefs.GetInt(PropertiesKeys.MuteVoiceChat, 0) == 1);
        PushToTalk = (PlayerPrefs.GetInt(PropertiesKeys.PushToTalk, 0) == 1);
        bool km = PlayerPrefs.GetInt(PropertiesKeys.KickKey, 0) == 1;
        KickMessageUI.SetActive(km);
        PingKickMessageUI.SetActive(bl_PhotonGame.Instance.hasPingKick);
        PlayerPrefs.SetInt(PropertiesKeys.KickKey, 0);
        if (bl_PhotonGame.Instance.hasAFKKick) { AFKKickMessageUI.SetActive(true); bl_PhotonGame.Instance.hasAFKKick = false; }

#if LM
        if (bl_LevelManager.Instance.isNewLevel)
        {
            LevelInfo info = bl_LevelManager.Instance.GetLevel();
            m_LevelUI.Icon.sprite = info.Icon;
            m_LevelUI.LevelNameText.text = info.Name;
            m_LevelUI.Root.SetActive(true);
            bl_LevelManager.Instance.Refresh();
        }
#endif
    }

    private int m_WeaponFov;
    [System.Serializable]
    public class LevelUI
    {
        public GameObject Root;
        public Image Icon;
        public Text LevelNameText;
    }
}