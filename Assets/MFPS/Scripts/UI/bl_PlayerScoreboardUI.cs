using UnityEngine;
using UnityEngine.UI;

public class bl_PlayerScoreboardUI : MonoBehaviour
{
    [SerializeField]private Text NameText;
    [SerializeField]private Text KillsText;
    [SerializeField]private Text DeathsText;
    [SerializeField]private Text ScoreText;
    [SerializeField] private GameObject KickButton;
    [SerializeField] private Image LevelIcon;

    private PhotonPlayer cachePlayer = null;
    private bl_UIReferences UIReference;
    private bool isInitializated = false;
    private Image BackgroundImage;
    private Team InitTeam = Team.None;

    public void Init(PhotonPlayer player, bl_UIReferences uir)
    {
        cachePlayer = player;
        gameObject.name = player.NickName + player.ID;
        UIReference = uir;

        BackgroundImage = GetComponent<Image>();
        if(player.ID == PhotonNetwork.player.ID)
        {
            Color c = BackgroundImage.color;
            c.a = 0.35f;
            BackgroundImage.color = c;
        }
        InitTeam = player.GetPlayerTeam();
        NameText.text = player.NickNameAndRole();
        KillsText.text = player.CustomProperties[PropertiesKeys.KillsKey].ToString();
        DeathsText.text = player.CustomProperties[PropertiesKeys.DeathsKey].ToString();
        ScoreText.text = player.CustomProperties[PropertiesKeys.ScoreKey].ToString();
        KickButton.SetActive(PhotonNetwork.isMasterClient && player.ID != PhotonNetwork.player.ID && bl_GameData.Instance.MasterCanKickPlayers);
#if LM
         LevelIcon.gameObject.SetActive(true);
         LevelIcon.sprite = bl_LevelManager.Instance.GetPlayerLevelInfo(cachePlayer).Icon;
#else
        LevelIcon.gameObject.SetActive(false);
#endif
    }

    public void Refresh()
    {
        if(cachePlayer == null || cachePlayer.GetPlayerTeam() != InitTeam)
        {
            UIReference.RemoveUIPlayer(this);
            Destroy(gameObject);
        }

        NameText.text = cachePlayer.NickNameAndRole();
        KillsText.text = cachePlayer.CustomProperties[PropertiesKeys.KillsKey].ToString();
        DeathsText.text = cachePlayer.CustomProperties[PropertiesKeys.DeathsKey].ToString();
        ScoreText.text = cachePlayer.CustomProperties[PropertiesKeys.ScoreKey].ToString();
#if LM
         LevelIcon.sprite = bl_LevelManager.Instance.GetPlayerLevelInfo(cachePlayer).Icon;
#endif
    }

    public void Kick()
    {
        if (PhotonNetwork.isMasterClient)
        {
            bl_PhotonGame.Instance.KickPlayer(cachePlayer);
        }
    }

    public void OnClick()
    {
        if (cachePlayer.ID != PhotonNetwork.player.ID)
        {
            bl_UIReferences.Instance.OpenScoreboardPopUp(true, cachePlayer);
        }
    }

    void OnEnable()
    {
        if (cachePlayer == null && isInitializated)
        {
          //  UIReference.RemoveUIPlayer(this);
            Destroy(gameObject);
            isInitializated = true;
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

}