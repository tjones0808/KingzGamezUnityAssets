using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class bl_FriendListUI : MonoBehaviour
{
    [Header("Settings")]
    public bool SortOnline = true;
    [Header("References")]
    public GameObject FriendUIPrefab = null;
    public Transform PanelList = null;
    public Text FriendsCountText = null;
    [SerializeField]private Image ArrowImage;
    [SerializeField]private Sprite UpArrowSprite;
    [SerializeField]private Sprite BottomArrowSprite;
    [SerializeField]private Text LogText;
    [SerializeField] private Animator m_Anim;
    public GameObject Panel;

    private List<bl_FriendInfo> cacheFriendsInfo = new List<bl_FriendInfo>();
    private int OnlineCount = 0;
    private bool onlineFirst = false;
    private bool isShow = false;
    private bl_FriendList FriendManager;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        FriendManager = FindObjectOfType<bl_FriendList>();
        onlineFirst = SortOnline;
        ArrowImage.sprite = (onlineFirst == true) ? UpArrowSprite : BottomArrowSprite;
        UpdateFriendList(true);
        if (LogText != null)
        {
            LogText.canvasRenderer.SetAlpha(0);
        }
        if (!PhotonNetwork.connected)
        {
            Panel.SetActive(false);
        }
    }

    /// <summary>
    /// Instance UI in Friend panel for each friends
    /// </summary>
    /// <param name="friends"></param>
    public void InstanceFriendList(string[] friends)
    {
        for (int i = 0; i < friends.Length; i++)
        {
            GameObject f = Instantiate(FriendUIPrefab) as GameObject;

            f.transform.SetParent(PanelList, false);
        }
    }

    public void SetShow()
    {
        isShow = !isShow;
        m_Anim.SetBool("show", isShow);
    }

    /// <summary>
    /// This is called each time that Friend list is update
    /// // while updating a friends list, Photon will temporarily set
    //isOnline and isInRoom to false
    // if you update on a timer, you will notice state rapidly
    //switching between offline and online
    // therefore, we will store online state and room in a
    //dictionary and wait until an update is actually received
    // and store the updated value
    /// </summary>
    public void OnUpdatedFriendList()
    {
        if (!PhotonNetwork.connected || PhotonNetwork.Friends == null || !PhotonNetwork.insideLobby)
        {
            if (PhotonNetwork.Friends.Count <= 0) { CleanCacheList(); }
            return;
        }

        if (cacheFriendsInfo.Count <= 0 && PhotonNetwork.Friends != null || FriendManager.WaitForEvent)
        {
            UpdateFriendList(true);
        }
        else
        {
            UpdateFriendList();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateFriendList(bool instance = false)
    {
        if (PhotonNetwork.Friends == null || !PhotonNetwork.insideLobby)
        {
            return;
        }

        FriendInfo[] friends = PhotonNetwork.Friends.ToArray();

        if (instance)
        {
            CleanCacheList();
            if (friends.Length > 0)
            {
                List<FriendInfo> OnlineFriends = new List<FriendInfo>();
                List<FriendInfo> OfflineFriends = new List<FriendInfo>();

                for (int i = 0; i < friends.Length; i++)
                {
                    if (friends[i].UserId != "Null")
                    {
                        if (onlineFirst)
                        {
                            if (friends[i].IsOnline)
                            {
                                OnlineFriends.Add(friends[i]);
                            }
                            else
                            {
                                OfflineFriends.Add(friends[i]);
                            }
                        }
                        else
                        {
                            GameObject f = Instantiate(FriendUIPrefab) as GameObject;
                            bl_FriendInfo info = f.GetComponent<bl_FriendInfo>();
                            info.GetInfo(friends[i]);
                            f.name = friends[i].UserId;
                            cacheFriendsInfo.Add(info);

                            f.transform.SetParent(PanelList, false);
                        }
                    }
                }

                if (onlineFirst)
                {
                    if(OnlineFriends.Count > 0)
                    {
                        InstanceFriend(OnlineFriends.ToArray());
                    }
                    if(OfflineFriends.Count > 0)
                    {
                        InstanceFriend(OfflineFriends.ToArray());
                    }
                }
            }
        }
        else//Just update list
        {
            for (int i = 0; i < cacheFriendsInfo.Count; i++)
            {
                if (cacheFriendsInfo[i] != null)
                {
                    cacheFriendsInfo[i].RefreshInfo(friends);
                }
                else
                {
                    cacheFriendsInfo.RemoveAt(i);
                }
            }
        }
        UpdateCount(friends);
    }

    /// <summary>
    /// 
    /// </summary>
    void InstanceFriend(FriendInfo[] friends)
    {
        for (int i = 0; i < friends.Length; i++)
        {
            GameObject f = Instantiate(FriendUIPrefab) as GameObject;
            bl_FriendInfo info = f.GetComponent<bl_FriendInfo>();
            info.GetInfo(friends[i]);
            cacheFriendsInfo.Add(info);
            f.name = friends[i].UserId;
            f.transform.SetParent(PanelList, false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateCount(FriendInfo[] friends)
    {
        OnlineCount = 0;
        foreach (FriendInfo info in friends)
        {
            if (info.IsOnline)
            {
                OnlineCount++;
            }
        }
        if (FriendsCountText != null)
        {
            FriendsCountText.text = OnlineCount + "/" + friends.Length;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CleanCacheList()
    {
        if (cacheFriendsInfo.Count > 0)
        {
            for (int i = 0; i < cacheFriendsInfo.Count; i++)
            {
                Destroy(cacheFriendsInfo[i].gameObject);
            }
        }
        cacheFriendsInfo.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetOnlineFirst(bool first)
    {
        onlineFirst = first;
        ArrowImage.sprite = (onlineFirst == true) ? UpArrowSprite : BottomArrowSprite;
        UpdateFriendList(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeOnlineFirst()
    {
        onlineFirst = !onlineFirst;
        ArrowImage.sprite = (onlineFirst == true) ? UpArrowSprite : BottomArrowSprite;
        UpdateFriendList(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="log"></param>
    public void ShowLog(string log)
    {
        CancelInvoke("HideLog");
        if (LogText != null)
        {
            LogText.text = log;
            LogText.CrossFadeAlpha(1, 0.7f, true);
        }
        Invoke("HideLog", 3);
    }

    /// <summary>
    /// 
    /// </summary>
    void HideLog()
    {
        if (LogText != null)
        {
            LogText.CrossFadeAlpha(0, 1, true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fname"></param>
    /// <returns></returns>
    public bool hasThisFriend(string fname)
    {
        for(int i = 0; i < cacheFriendsInfo.Count; i++)
        {
            if (cacheFriendsInfo[i].name == fname)
            {
                return true;
            }
        }
        return false;

    }
}