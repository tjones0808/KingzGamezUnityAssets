//////////////////////////////////////////////////////////////////////////////
//  bl_DrawName.cs
//
// Can be attached to a GameObject to show Player Name 
//
//           Lovatto Studio
/////////////////////////////////////////////////////////////////////////////
using UnityEngine;

//[ExecuteInEditMode]
public class bl_DrawName : bl_MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Range(5, 30)]
    private float LargeIconSize = 15;
    [SerializeField, Range(0.1f, 25)] private float HealthBarHeight = 10;
    [SerializeField] private Color HealthBackColor = new Color(0, 0, 0, 0.5f);
    [SerializeField] private Color HealthBarColor = new Color(0, 1, 0, 0.9f);

    [Header("References")]
    public GUIStyle m_Skin;
    /// <summary>
    /// at what distance the name is hiding
    /// </summary>
    public float m_HideDistance;
    /// <summary>
    /// 
    /// </summary>    
    public Texture2D m_HideTexture;
    public Texture2D TalkingIcon;
    [SerializeField] private Texture2D BarTexture;
    // [Range(0, 100)] public float Health = 100;
    public string m_PlayerName { get; set; }
    public Transform m_Target;

    //Private
    private float m_dist;
    private Transform myTransform;
#if !UNITY_WEBGL
    private PhotonVoiceSpeaker PRecorder;
#endif
    private bl_PlayerDamageManager DamagerManager;
    private bool ShowHealthBar = false;

    protected override void Awake()
    {
        base.Awake();
#if !UNITY_WEBGL
        PRecorder = GetComponent<PhotonVoiceSpeaker>();
#endif
        DamagerManager = GetComponent<bl_PlayerDamageManager>();
        ShowHealthBar = bl_GameData.Instance.ShowTeamMateHealthBar;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        this.myTransform = this.transform;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGUI()
    {
        if (bl_UtilityHelper.CameraInUse == null)
            return;

        Vector3 vector = bl_UtilityHelper.CameraInUse.WorldToScreenPoint(m_Target.position);
        /*GUI.Label(new Rect(vector.x - 5, (Screen.height - vector.y) - 7, 10, 11), "LOVATTO", m_Skin);
        GUI.color = HealthBackColor;
        GUI.DrawTexture(new Rect(vector.x - 50, (Screen.height - vector.y) + 7, 100, HealthBarHeight), BarTexture);
        GUI.color = HealthBarColor;
        GUI.DrawTexture(new Rect(vector.x - 50, (Screen.height - vector.y) + 7, Health, HealthBarHeight), BarTexture);
        GUI.color = Color.white;*/
        if (vector.z > 0)
        {
            float spex = 10;
            int vertical = ShowHealthBar ? 15 : 10;
            if (this.m_dist < m_HideDistance)
            {
                float nameSize = m_Skin.CalcSize(new GUIContent(m_PlayerName)).x;
                spex = (nameSize / 2) + 2;
                GUI.Label(new Rect(vector.x - 5, (Screen.height - vector.y) - vertical, 10, 11), m_PlayerName, m_Skin);
                if (ShowHealthBar)
                {
                    GUI.color = HealthBackColor;
                    GUI.DrawTexture(new Rect(vector.x - (DamagerManager.maxHealth / 2), (Screen.height - vector.y), DamagerManager.maxHealth, HealthBarHeight), BarTexture);
                    GUI.color = HealthBarColor;
                    GUI.DrawTexture(new Rect(vector.x - (DamagerManager.maxHealth / 2), (Screen.height - vector.y), DamagerManager.health, HealthBarHeight), BarTexture);
                    GUI.color = Color.white;
                }
            }
            else
            {
                GUI.DrawTexture(new Rect(vector.x - (LargeIconSize / 2), (Screen.height - vector.y) - (LargeIconSize / 2), LargeIconSize, LargeIconSize), m_HideTexture);
            }
#if !UNITY_WEBGL
            //voice chat icon
            if (PRecorder.IsPlaying)
            {
                GUI.DrawTexture(new Rect(vector.x + spex, (Screen.height - vector.y) - vertical, 14, 14), TalkingIcon);
            }
#endif
        }
    }



    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (bl_UtilityHelper.CameraInUse == null)
            return;

        this.m_dist = bl_UtilityHelper.GetDistance(myTransform.position, bl_UtilityHelper.CameraInUse.transform.position);
    }
}