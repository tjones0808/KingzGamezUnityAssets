using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_KickVotationUI : MonoBehaviour
{

    [SerializeField] private GameObject Window;
    [SerializeField] private Text TitleText;
    [SerializeField] private Text VotingText;
    [SerializeField] private Text YesText;
    [SerializeField] private Text NoText;
    [SerializeField] private Text VoteConfirmation;
    [SerializeField] private GameObject KeyInfoUI;
    private string CacheTargetName;
    private bl_KickVotation KickManager;

    private void Awake()
    {
        Window.SetActive(false);
        KickManager = FindObjectOfType<bl_KickVotation>();
    }

    public void OpenVotatation(PhotonPlayer again, PhotonPlayer By)
    {
        CancelInvoke("Hide");
        CacheTargetName = again.NickName;
        TitleText.text = string.Format(bl_GameTexts.VoteBy, By.NickName);
        VotingText.text = string.Format(bl_GameTexts.KickQuestion, again.NickName);
        YesText.text = "0";
        NoText.text = "0";
        VoteConfirmation.text = string.Empty;
        if (again.UserId != PhotonNetwork.player.UserId)
        {
            KeyInfoUI.SetActive(true);
        }
        else
        {
            KeyInfoUI.SetActive(false);
        }
        Window.SetActive(true);
    }

    public void OnSendLocalVote(bool yes)
    {
        KeyInfoUI.SetActive(false);
        string vote = yes ? "<color=green>YES</color>" : "<color=red>NO</color>";
        VoteConfirmation.text = string.Format(bl_GameTexts.YouVote, vote);
    }

    public void OnReceiveVote(int yes, int no)
    {
        YesText.text = yes.ToString();
        NoText.text = no.ToString();
    }

    public void OnFinish(bool yes)
    {
        if (yes)
        {
            VotingText.text = string.Format(bl_GameTexts.KickConfirmation, CacheTargetName);
        }
        else
        {
            VotingText.text = string.Format(bl_GameTexts.KickFailed, CacheTargetName);
        }
        KeyInfoUI.SetActive(false);
        Invoke("Hide", 3);
    }

    public void RequestVotation()
    {
        KickManager.RequestKick(bl_UIReferences.Instance.PlayerPopUp);
        bl_UIReferences.Instance.OpenScoreboardPopUp(false);
    }

    void Hide()
    {
        Window.SetActive(false);
    }
}