using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class bl_AIAnimation : MonoBehaviour
{
    [Header("Ragdoll")]
    public List<Rigidbody> mRigidBody = new List<Rigidbody>();
    public List<bl_AIHitBox> HitBoxes = new List<bl_AIHitBox>();

    private Animator m_animator;
    private Vector3 localVelocity;
    private float vertical;
    private float horizontal;
    private float turnSpeed;
    private bool parent = false;
    private Vector3 velocity;
    private float lastYRotation;
    private float TurnLerp;
    private float movementSpeed;
    private bl_AIShooterAgent AI;
    private Vector3 headTarget;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        AI = transform.parent.GetComponent<bl_AIShooterAgent>();
        SetKinecmatic();
    }

    private void Update()
    {
        ControllerInfo();
        Animate();
    }

    /// <summary>
    /// 
    /// </summary>
    void ControllerInfo()
    {
        velocity = AI.vel;
        localVelocity = transform.InverseTransformDirection(velocity);
        localVelocity.y = 0;

        vertical = Mathf.Lerp(vertical, localVelocity.z, Time.deltaTime * 10);
        horizontal = Mathf.Lerp(horizontal, localVelocity.x, Time.deltaTime * 10);

        parent = !parent;
        if (parent)
        {
            lastYRotation = transform.rotation.eulerAngles.y;
        }
        turnSpeed = Mathf.DeltaAngle(lastYRotation, transform.rotation.eulerAngles.y);
        TurnLerp = Mathf.Lerp(TurnLerp, turnSpeed, 7 * Time.deltaTime);
        movementSpeed = velocity.magnitude;
    }
    /// <summary>
    /// 
    /// </summary>
    void Animate()
    {
        if (m_animator == null)
            return;

        m_animator.SetFloat("Vertical", vertical);
        m_animator.SetFloat("Horizontal", horizontal);
        m_animator.SetFloat("Speed", movementSpeed);
        m_animator.SetFloat("Turn", TurnLerp);
        m_animator.SetBool("isGround", true);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (AI.Target == null)
            return;

        m_animator.SetLookAtWeight(1, 1, 1, 1, 0.5f);
        headTarget = Vector3.Slerp(headTarget, AI.Target.position, Time.deltaTime * 7);
        m_animator.SetLookAtPosition(headTarget);
    }

    public void SetKinecmatic()
    {
        for(int i = 0; i< HitBoxes.Count; i++)
        {
            mRigidBody[i].isKinematic = true;
        }
    }

    public void Ragdolled(Vector3 from)
    {
        m_animator.enabled = false;
        for (int i = 0; i < HitBoxes.Count; i++)
        {
            mRigidBody[i].isKinematic = false;
            mRigidBody[i].detectCollisions = true;
            Vector3 rhs = transform.position - from;
            mRigidBody[i].AddForceAtPosition(rhs * 30, rhs);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Get RigidBodys")]
#endif
    public void GetRigidBodys()
    {
        Rigidbody[] R = this.transform.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in R)
        {
            if (!mRigidBody.Contains(rb))
            {
                mRigidBody.Add(rb);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Set Scripts")]
#endif
    public void SetScripts()
    {
        for(int i = 0; i < mRigidBody.Count; i++)
        {
            mRigidBody[i].gameObject.tag = "AI";
            bl_AIHitBox box = null;
            if(mRigidBody[i].gameObject.GetComponent<bl_AIHitBox>() == null)
            {
                box = mRigidBody[i].gameObject.AddComponent<bl_AIHitBox>();
            }else
            {
                box = mRigidBody[i].gameObject.GetComponent<bl_AIHitBox>();
            }
            box.AI = transform.parent.GetComponent<bl_AIShooterAgent>();
            box.m_Collider = mRigidBody[i].gameObject.GetComponent<Collider>();
            HitBoxes.Add(box);
        }
    }
}