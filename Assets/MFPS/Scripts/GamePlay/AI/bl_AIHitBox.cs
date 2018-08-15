using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_AIHitBox : MonoBehaviour
{

    public bl_AIShooterAgent AI;
    public Collider m_Collider;

    public void DoDamage(int damage, string wn, Vector3 direction, int viewID, bool fromBot)
    {
        if (AI == null)
            return;

        AI.DoDamage(damage, wn, direction, viewID, fromBot);
    }
}