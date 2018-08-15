using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client;

public class bl_PlayerVoice : bl_MonoBehaviour
{
    public KeyCode PushButton = KeyCode.P;

    private GameObject RecorderIcon;
#if !UNITY_WEBGL
    private PhotonVoiceRecorder Recorder;
    private PhotonVoiceSpeaker Speaker;
    private bool PushToTalk = false;
#endif
    private PhotonView View;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        RecorderIcon = bl_UIReferences.Instance.SpeakerIcon;
#if !UNITY_WEBGL
        Recorder = GetComponent<PhotonVoiceRecorder>();
        Speaker = GetComponent<PhotonVoiceSpeaker>();
#endif
        View = photonView;
#if !UNITY_WEBGL
        if (View.isMine)
        {
            Recorder.enabled = true;
            PushToTalk = bl_UIReferences.Instance.PushToTalkToggle.isOn;
            if (PushToTalk) { Recorder.Transmit = false; }
            Speaker.enabled = !bl_UIReferences.Instance.MuteVoiceToggle.isOn;
        }
        else
        {
            if (GetGameMode != GameMode.FFA)
            {
                Speaker.enabled = photonView.owner.GetPlayerTeam() == PhotonNetwork.player.GetPlayerTeam();
            }
        }
#else
        RecorderIcon.SetActive(false);
#endif
    }

    protected override void OnEnable()
    {
        base.OnEnable();
#if MFPSM
        if (View.isMine)
        {
            bl_TouchHelper.OnTransmit += OnPushToTalkMobile;
        }
#endif
    }

    protected override void OnDisable()
    {
        base.OnDisable();
#if MFPSM
        if (View.isMine)
        {
            bl_TouchHelper.OnTransmit -= OnPushToTalkMobile;
        }
#endif
    }

    public void OnPushToTalkMobile(bool transmit)
    {
#if !UNITY_WEBGL
        Recorder.Transmit = transmit;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (View.isMine)
        {
#if !UNITY_WEBGL
            RecorderIcon.SetActive(Recorder.IsTransmitting && PhotonVoiceNetwork.ClientState == ExitGames.Client.Photon.LoadBalancing.ClientState.Joined);
            if (PushToTalk)
            {
                Recorder.Transmit = Input.GetKey(PushButton);
            }
#endif
        }
    }

    public void OnMuteChange(bool b)
    {
        if (View.isMine)
        {
#if !UNITY_WEBGL
            Speaker.enabled = !b;
#endif
        }
    }

    public void OnPushToTalkChange(bool b)
    {
        if (View.isMine)
        {
#if !UNITY_WEBGL
            PushToTalk = b;
#endif
#if MFPSM
            if (bl_TouchHelper.Instance != null)
            {
                bl_TouchHelper.Instance.OnPushToTalkChange(b);
            }
#endif
        }
    }
}