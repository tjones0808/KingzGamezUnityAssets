using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_AIMananger : bl_PhotonHelper
{

    [SerializeField] private GameObject AIPrefabs;
    [SerializeField, Range(1, 16)] public int NumberOfBots = 5;

    private List<bl_AIShooterAgent> AllBots = new List<bl_AIShooterAgent>();
    private List<Transform> AllBotsTransforms = new List<Transform>();
    private bl_GameManager GameManager;

    private void Awake()
    {
        GameManager = FindObjectOfType<bl_GameManager>();
    }

    private void Start()
    {
        if (PhotonNetwork.isMasterClient && GetGameMode == GameMode.FFA)
        {
            bool with = (bool)PhotonNetwork.room.CustomProperties[PropertiesKeys.WithBotsKey];
            if (with)
            {
                for (int i = 0; i < NumberOfBots; i++)
                {
                    SpawnBot();
                }
            }
        }
    }

    public void SpawnBot(bl_AIShooterAgent agent = null)
    {
        Transform t = GameManager.GetAnSpawnPoint;
        GameObject bot = PhotonNetwork.Instantiate(AIPrefabs.name, t.position, t.rotation, 0);
        bl_AIShooterAgent newAgent = bot.GetComponent<bl_AIShooterAgent>();
        if (agent != null)
        {
            newAgent.AIName = agent.AIName;
        }
        else
        {
            newAgent.AIName = "AI " + Random.Range(0, 999);
        }
        AllBots.Add(newAgent);
        AllBotsTransforms.Add(newAgent.AimTarget);
    }

    public void OnBotDeath(bl_AIShooterAgent agent, bl_AIShooterAgent killer)
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        AllBots.Remove(agent);
        AllBotsTransforms.Remove(agent.AimTarget);
        for (int i = 0; i < AllBots.Count; i++)
        {
            AllBots[i].CheckTargets();
        }
        SpawnBot(agent);
    }

    public List<Transform> GetOtherBots(Transform bot)
    {
        List<Transform> all = new List<Transform>();
        all.AddRange(AllBotsTransforms);
        all.Remove(bot);
        return all;
    }

    public bl_AIShooterAgent GetBot(int viewID)
    {
        foreach(bl_AIShooterAgent agent in AllBots)
        {
            if(agent.photonView.viewID == viewID)
            {
                return agent;
            }
        }
        return null;
    }

}