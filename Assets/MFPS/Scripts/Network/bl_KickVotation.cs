using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_KickVotation : bl_MonoBehaviour
{

    [SerializeField] private KeyCode YesKey = KeyCode.F1;
    [SerializeField] private KeyCode NoKey = KeyCode.F2;

    private PhotonView View;
    private bool IsOpen = false;

    private int YesCount = 0;
    private int NoCount = 0;
    private bl_KickVotationUI UI;
    private PhotonPlayer TargetPlayer;
    private bool isAgainMy = false;
    private bool Voted = false;
    private int AllVoters = 0;

    protected override void Awake()
    {
        base.Awake();
        View = PhotonView.Get(this);
        UI = FindObjectOfType<bl_KickVotationUI>();
    }

    public void RequestKick(PhotonPlayer player)
    {
        if (IsOpen || player == null)
            return;
        if(PhotonNetwork.playerList.Length < 3)
        {
            Debug.Log("there are not enough players.");
            return;
        }
        if (player.ID == PhotonNetwork.player.ID)
        {
            Debug.Log("You can not send a vote for yourself.");
            return;
        }

        View.RPC("RpcRequestKick", PhotonTargets.All, player);
    }

    [PunRPC]
    void RpcRequestKick(PhotonPlayer player, PhotonMessageInfo info)
    {
        if (IsOpen)
            return;

        AllVoters = PhotonNetwork.otherPlayers.Length;
        TargetPlayer = player;
        ResetVotation();
        isAgainMy = (player.UserId == PhotonNetwork.player.UserId);
        UI.OpenVotatation(player, info.sender);
        IsOpen = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void ResetVotation()
    {
        YesCount = 0;
        NoCount = 0;
        isAgainMy = false;
        Voted = false;
    }

    public override void OnUpdate()
    {
        if (!IsOpen || isAgainMy || Voted)
            return;
        if (TargetPlayer == null)
            return;

        if (Input.GetKeyDown(YesKey))
        {
            SendVote(true);
            Voted = true;
        }else if (Input.GetKeyDown(NoKey))
        {
            SendVote(false);
            Voted = true;
        }
    }

    void SendVote(bool yes)
    {
        View.RPC("RPCReceiveVote", PhotonTargets.All, yes);
        UI.OnSendLocalVote(yes);
    }

    [PunRPC]
    void RPCReceiveVote(bool yes, PhotonMessageInfo info)
    {
        if (yes)
        {
            YesCount++;
        }
        else
        {
            NoCount++;
        }
        UI.OnReceiveVote(YesCount, NoCount);
        if (PhotonNetwork.isMasterClient)//master count all votes to determine if kick or not
        {
            CountVotes();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CountVotes()
    {
        int half = (AllVoters / 2);
        bool kicked = false;
        if (YesCount > half)//kick
        {
            bl_PhotonGame.Instance.KickPlayer(TargetPlayer);
            kicked = true;
            View.RPC("EndVotation", PhotonTargets.All, kicked);
        }
        else if (NoCount > half)//no kick
        {
            View.RPC("EndVotation", PhotonTargets.All, kicked);
        }
    }

    [PunRPC]
    void EndVotation(bool finaledKicked)
    {
        IsOpen = false;
        Voted = true;
        UI.OnFinish(finaledKicked);
        TargetPlayer = null;
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        if (TargetPlayer == null)
            return;

       if(otherPlayer.ID == TargetPlayer.ID)
        {
            //cancel voting due player left the room by himself
            UI.OnFinish(true);
        }
    }
}