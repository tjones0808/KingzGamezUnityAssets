﻿using UnityEngine;

[ExecuteInEditMode]
public class bl_HeatLookMecanim : bl_MonoBehaviour {

	public Transform Target;
    [Range(0,1)]public float Weight;
    [Range(0,1)]public float Body;
    [Range(0,1)]public float Head;
    [Range(0,1)]public float Eyes;
    [Range(0,1)]public float Clamp;
    [Range(1,20)]public float Lerp = 8;

    public Vector3 HandOffset;
    public Vector3 AimSightPosition = new Vector3(0.02f, 0.19f, 0.02f);

    private Animator animator;
    private Vector3 target;
    private Vector3 CachePosition;
    private Quaternion CacheRotation;

    private float RighHand = 1;
    private float LeftHand = 1;
    private float RightHandPos = 0;
    private Transform HeatTrans;

    void Start()
    {
        animator = GetComponent<Animator>();
        HeatTrans = animator.GetBoneTransform(HumanBodyBones.Head);
    }

    void OnAnimatorIK(int layer)
    {
        if (Target == null || animator == null)
            return;

        if (layer == 0)
        {
            animator.SetLookAtWeight(Weight, Body, Head, Eyes, Clamp);
            target = Vector3.Slerp(target, Target.position, Time.deltaTime * 8);
            animator.SetLookAtPosition(target);
        }
        else if (layer == 1)//upper body layer
        {
            if (LeftHandTarget != null)
            {
                HandsIK();
            }
            else
            {
                ResetWeightIK();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void HandsIK()
    {
        float weight = (inPointMode) ? 1 : 0;
        float lweight = (PlayerSync.UpperState != "Reloading") ? 1 : 0;
        RighHand = Mathf.Lerp(RighHand, weight, Time.deltaTime * 5);
        LeftHand = Mathf.Lerp(LeftHand, lweight, Time.deltaTime * 5);

        animator.SetIKRotation(AvatarIKGoal.LeftHand, CacheRotation);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, CachePosition);
        if (RighHand > 0)
        {
            Transform arm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Vector3 rhs = Target.position - arm.position;
            Quaternion lookAt = Quaternion.LookRotation(rhs);
            Vector3 v = lookAt.eulerAngles;
            v = v + HandOffset;
            animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.Euler(v));
        }

        float rpw = (PlayerSync.UpperState == "Aimed" || PlayerSync.UpperState == "AimFire") ? 0.5f : 0;
        RightHandPos = Mathf.Lerp(RightHandPos, rpw, Time.deltaTime * 7);
        Vector3 hf = HeatTrans.TransformPoint(AimSightPosition);
        animator.SetIKPosition(AvatarIKGoal.RightHand, hf);

        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, RighHand);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, RightHandPos);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, LeftHand);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, LeftHand);
    }

    void ResetWeightIK()
    {
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.0f);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.0f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
    }

    private bool inPointMode
    {
        get
        {
            return (PlayerSync.UpperState == "Firing" || PlayerSync.UpperState == "Aimed" || PlayerSync.UpperState == "Idle" || PlayerSync.UpperState == "AimFire");
        }
    }

    private Transform LeftHandTarget
    {
        get
        {
            if (Application.isPlaying)
            {
                if (PlayerSync != null && PlayerSync.CurrenGun != null)
                {
                    return PlayerSync.CurrenGun.LeftHandPosition;
                }
            }
            else
            {
                if (PlayerSync != null && PlayerSync.m_PlayerAnimation != null && PlayerSync.m_PlayerAnimation.EditorSelectedGun)
                {
                    return PlayerSync.m_PlayerAnimation.EditorSelectedGun.LeftHandPosition;
                }
            }
            return null;
        }
    }

    public override  void OnUpdate()
     {
         if (LeftHandTarget == null) return;

         CachePosition = LeftHandTarget.position;
         CacheRotation = LeftHandTarget.rotation;
     }

#if UNITY_EDITOR
    void Update()
    {
        if (LeftHandTarget == null || Application.isPlaying) return;

        CachePosition = LeftHandTarget.position;
        CacheRotation = LeftHandTarget.rotation;
    }
#endif

    private void OnDrawGizmos()
    {
        if(animator == null) { animator = GetComponent<Animator>(); }
        if(HeatTrans == null) { HeatTrans = animator.GetBoneTransform(HumanBodyBones.Head); }
        Gizmos.color = Color.yellow;
        Vector3 hf =  HeatTrans.TransformPoint(AimSightPosition);
        Gizmos.DrawLine(HeatTrans.position, hf);
        Gizmos.DrawSphere(hf, 0.03f);
        
    }

    private bl_PlayerSync PSync = null;
    private bl_PlayerSync PlayerSync
    {
        get
        {
            if (PSync == null) { PSync = transform.root.GetComponent<bl_PlayerSync>(); }
            return PSync;
        }
    }
}