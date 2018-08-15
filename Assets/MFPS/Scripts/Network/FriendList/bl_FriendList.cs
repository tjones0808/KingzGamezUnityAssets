using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class bl_FriendList : bl_PhotonHelper {

    [HideInInspector]
    public List<string> Friends = new List<string>();
    [Space(5)]
    [Range(1,60)]
    public float UpdateEvery = 15f;
    [Range(1, 30)] public int MaxFriendsCount = 25;

    public const string FriendSaveKey = "LSFriendList";
    private char splitChar = '/';

    [HideInInspector] public bool WaitForEvent = false;
    private bl_FriendListUI FriendUI;
    #if ULSP
    private bl_DataBase DataBase;
    #endif

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        FriendUI = FindObjectOfType<bl_FriendListUI>();
        #if ULSP
        DataBase = bl_DataBase.Instance;
        #endif
        if (PhotonNetwork.connected)
        {
            GetFriendsStore();
            InvokeRepeating("UpdateList", 1, UpdateEvery);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GetFriendsStore()
    {
        //Get all friends saved 
#if ULSP
        if(DataBase != null)
        {
            Friends.AddRange(DataBase.LocalUser.FriendList.ToArray());
        }
        else
        {
            string cacheFriend = PlayerPrefs.GetString(SaveKey, "Null");
            if (!string.IsNullOrEmpty(cacheFriend))
            {
                string[] splitFriends = cacheFriend.Split(splitChar);
                Friends.AddRange(splitFriends);
            }
        }
#else
        string cacheFriend = PlayerPrefs.GetString(SaveKey, "Null");
        if (!string.IsNullOrEmpty(cacheFriend))
        {
            string[] splitFriends = cacheFriend.Split(splitChar);
            Friends.AddRange(splitFriends);
        }
#endif
        //Find all friends names in photon list.
        if (Friends.Count > 0)
        {
            PhotonNetwork.FindFriends(Friends.ToArray());
            //Update the list UI 
            FriendUI.UpdateFriendList(true);
        }
        else
        {
            Debug.Log("Anyone friend store");
            return;
        }
    }

    /// <summary>
    /// Call For Update List of friends.
    /// </summary>
    void UpdateList()
    {
        if (!PhotonNetwork.connected || PhotonNetwork.Friends == null || PhotonNetwork.room != null || !PhotonNetwork.insideLobby)
            return;
        if (Friends.Count > 0)
        {
            if (Friends.Count > 1 && Friends.Contains("Null"))
            {
                Friends.Remove("Null");
                Friends.Remove("Null");
                SaveFriends();
            }
            if (PhotonNetwork.connectedAndReady)
            {
                PhotonNetwork.FindFriends(Friends.ToArray());
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SaveFriends()
    {
        string allfriends = string.Join(splitChar.ToString(), Friends.ToArray());
#if ULSP
        if(DataBase != null && !DataBase.isGuest)
        {
            StartCoroutine(SaveInDataBase(allfriends));
        }
        else
        {
            PlayerPrefs.SetString(SaveKey, allfriends);
        }
#else
         PlayerPrefs.SetString(SaveKey, allfriends);
#endif
    }

#if ULSP
    IEnumerator SaveInDataBase(string line)
    {
        WWWForm wf = new WWWForm();
        string hash = bl_DataBaseUtils.Md5Sum(DataBase.LocalUser.LoginName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        wf.AddField("name", DataBase.LocalUser.LoginName);
        wf.AddField("flist", line);
        wf.AddField("typ", 5);
        wf.AddField("hash", hash);

        WWW www = new WWW(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.DataBase), wf);

        yield return www;

        if (www.error == null)
        {
            if (www.text.Contains("save"))
            {
                Debug.Log("Friend list save in database!");
                DataBase.LocalUser.SetFriends(line);
            }
            else
            {
                Debug.Log(www.text);
            }
        }
        else
        {
            Debug.LogError(www.error);
        }
        www.Dispose();
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <param name="field"></param>
    public void AddFriend(InputField field)
    {
        if(Friends.Count > MaxFriendsCount)
        {
            FriendUI.ShowLog("Max friends reached!");
            return;
        }
        string t = field.text;
        if (string.IsNullOrEmpty(t))
            return;

        if(FriendUI != null && FriendUI.hasThisFriend(t))
        {
            FriendUI.ShowLog("Already has added this friend.");
            return;
        }
        if(t == PhotonNetwork.playerName)
        {
            FriendUI.ShowLog("You can't add yourself.");
            return;
        }

        Friends.Add(t);
        PhotonNetwork.FindFriends(Friends.ToArray());
        FriendUI.UpdateFriendList(true);
        SaveFriends();
        WaitForEvent = true;

        field.text = string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="field"></param>
    public void AddFriend(string friend)
    {
        Friends.Add(friend);
        PhotonNetwork.FindFriends(Friends.ToArray());
        FriendUI.UpdateFriendList(true);
        SaveFriends();
        WaitForEvent = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="friend"></param>
    public void RemoveFriend(string friend)
    {
        if (Friends.Contains(friend))
        {
            Friends.Remove(friend);
            SaveFriends();
            if (Friends.Count > 0)
            {
                if(Friends.Count > 1 && Friends.Contains("Null"))
                {
                    Friends.Remove("Null");
                    Friends.Remove("Null");
                    SaveFriends();
                }
                PhotonNetwork.FindFriends(Friends.ToArray());
            }
            else
            {
                AddFriend("Null");
                PhotonNetwork.FindFriends(Friends.ToArray());
            }

            FriendUI.UpdateFriendList(true);
            WaitForEvent = true;
        }
        else { Debug.Log("This user doesn't exist"); }
    }
    /// <summary>
    /// 
    /// </summary>
    public override void OnJoinedLobby()
    {
        GetFriendsStore();
        InvokeRepeating("UpdateList", 1, UpdateEvery);
        FriendUI.Panel.SetActive(true);
    }

    /// <summary>
    /// custom key for each player can save multiple friend list in a same device.
    /// </summary>
    private string SaveKey
    {
        get
        {
            return PhotonNetwork.playerName + FriendSaveKey;
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        CancelInvoke("UpdateList");
    }

    public override void OnDisconnectedFromPhoton()
    {
        base.OnDisconnectedFromPhoton();
        CancelInvoke("UpdateList");
    }
}