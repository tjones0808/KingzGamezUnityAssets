using UnityEngine;

public class bl_SpawnPoint : bl_PhotonHelper {

    public Team m_Team = Team.All;
    public float SpawnSpace = 3f;
    // Use this for initialization
    void Start()
    {
        if (this.transform.GetComponent<Renderer>() != null)
        {
            this.GetComponent<Renderer>().enabled = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_UtilityHelper.GetGameManager.RegisterSpawnPoint(this);
    }

    //Debug Spawn Space
    void OnDrawGizmosSelected()
    {
        Color c = (m_Team == Team.Recon) ? bl_GameData.Instance.Team2Color : bl_GameData.Instance.Team1Color;
        if(m_Team == Team.All) { c = Color.white; }
        Gizmos.color = c;
        Gizmos.DrawWireSphere(transform.position, SpawnSpace);
        Gizmos.color = new Color(c.r,c.g,c.b,0.4f);
        Gizmos.DrawSphere(transform.position, SpawnSpace);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(base.transform.position + ((base.transform.forward * this.SpawnSpace)), base.transform.position + (((base.transform.forward * 2f) * this.SpawnSpace)));
    }
}
