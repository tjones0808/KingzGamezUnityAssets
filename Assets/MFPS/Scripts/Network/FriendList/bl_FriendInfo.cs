using UnityEngine;
using UnityEngine.UI;

public class bl_FriendInfo : MonoBehaviour {

    public Text NameText = null;
    public Text StatusText = null;
    public Image StatusImage = null;
    public GameObject JoinButton = null;
    [Space(5)]
    public Color OnlineColor = new Color(0, 0.9f, 0, 0.9f);
    public Color OffLineColor = new Color(0.9f, 0, 0, 0.9f);

    private string roomName = string.Empty;
    private FriendInfo cacheInfo;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    public void GetInfo(FriendInfo info)
    {
        cacheInfo = info;
        NameText.text = info.UserId;
        if (StatusText != null) { StatusText.text = (info.IsOnline) ? "[Online]" : "[OffLine]"; }
        StatusImage.color = (info.IsOnline) ? OnlineColor : OffLineColor;
        JoinButton.SetActive((info.IsInRoom) ? true : false);
        roomName = info.Room;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    public void RefreshInfo(FriendInfo[] infos)
    {
        FriendInfo info = FindMe(infos);
        NameText.text = info.UserId;
        if (StatusText != null) { StatusText.text = (info.IsOnline) ? "[Online]" : "[OffLine]"; }
        StatusImage.color = (info.IsOnline) ? OnlineColor : OffLineColor;
        JoinButton.SetActive((info.IsInRoom) ? true : false);
        roomName = info.Room;
    }

    private FriendInfo FindMe(FriendInfo[] info)
    {
        for(int i = 0; i < info.Length; i++)
        {
            if(info[i].UserId == cacheInfo.UserId)
            {
                return info[i];
            }
        }
        Destroy(gameObject);
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    public void JoinRoom()
    {
        if (!string.IsNullOrEmpty(roomName))
        {
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.Log("This room doesn't exits more");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Remove()
    {
        bl_FriendList manager =  FindObjectOfType<bl_FriendList>();
        manager.RemoveFriend(NameText.text);
    }
}