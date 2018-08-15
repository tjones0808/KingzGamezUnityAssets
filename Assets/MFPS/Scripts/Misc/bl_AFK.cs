using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_AFK : bl_MonoBehaviour
{

    private float lastInput;
    private Vector3 oldMousePosition = Vector3.zero;
    private bool Leaving = false;
    private bool Watching = false;
    private bl_UIReferences UIReferences;
    private float AFKTimeLimit = 60;

    protected override void Awake()
    {
        base.Awake();
        UIReferences = bl_UIReferences.Instance;
        AFKTimeLimit = bl_GameData.Instance.AFKTimeLimit;
        if (!bl_GameData.Instance.DetectAFK)
        {
            this.enabled = false;
        }
    }

    public override void OnUpdate()
    {
        //if no movement or action of the player is detected, then start again
        if ((PhotonNetwork.player == null || Input.anyKey) || ((oldMousePosition != Input.mousePosition)))
        {
            lastInput = Time.time;
            if (Watching)
            {
                UIReferences.AFKText.gameObject.SetActive(false);
                Watching = false;
            }
        }
        else if ((Time.time - lastInput) > AFKTimeLimit / 2)
        {
            Watching = true;
        }
        oldMousePosition = Input.mousePosition;
        if (((lastInput + AFKTimeLimit) - 10f) < Time.time)
        {
            float t = AFKTimeLimit - (Time.time - lastInput);
            if (t >= 0)
            {
                UIReferences.SetAFKCount(t);
            }
        }
        //If the maximum time is AFK then meets back to the lobby.
        if ((lastInput + AFKTimeLimit) < Time.time && !Leaving)
        {
            bl_UtilityHelper.LockCursor(false);
            bl_PhotonGame.Instance.hasAFKKick = true;
            LeaveMatch();
            Leaving = true;
        }
    }


    public void LeaveMatch()
    {
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            bl_UtilityHelper.LoadLevel(GetComponent<bl_RoomMenu>().LeftRoomReturnScene);
        }
    }

}