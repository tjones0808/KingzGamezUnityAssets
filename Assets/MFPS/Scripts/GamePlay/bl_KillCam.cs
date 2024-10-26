﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bl_KillCam : bl_MonoBehaviour
{

    /// <summary>
    /// Target to follow
    /// </summary>
    public Transform target = null;
    /// <summary>
    /// Enemys Tag
    /// </summary>
    public string TagTargets = "";
    /// <summary>
    /// Distance from camera to target
    /// </summary>
	public float distance = 10.0f;
    /// <summary>
    /// list with al enemys with same tag
    /// </summary>
    public List<Transform> OtherList = new List<Transform>();
    /// <summary>
    /// target name to follow
    /// </summary>
	public string Follow = "";
    /// <summary>
    /// in case targets is null see this
    /// </summary>
	public Transform Provide;
    /// <summary>
    /// Maxime Distance to target
    /// </summary>
    public float distanceMax = 15f;
    /// <summary>
    /// Min Distance to target
    /// </summary>
	public float distanceMin = 0.5f;
    /// <summary>
    /// X vector speed
    /// </summary>
	public float xSpeed = 120f;
    /// <summary>
    /// maxime y vector Limit
    /// </summary>
	public float yMaxLimit = 80f;
    /// <summary>
    /// minime Y vector limit
    /// </summary>
	public float yMinLimit = -20f;
    /// <summary>
    /// Y vector speed
    /// </summary>
	public float ySpeed = 120f;

    public bool Can_See_Other = true;
    public bool SmoothMovement = true;
    /// <summary>
    /// Smooth motion speed
    /// </summary>
    public float SpeedSmooth = 5;
    public string KillCamTitle = "";
    [Range(0.01f, 5f)]
    public float m_wait;
    public bool isLocal = false;

    private int CurrentOther = 0;
    private bool isSearch = false;
    private bool isWait = false;
    private float x;
    private float y;
    [HideInInspector] public bool StaticCamera = true;

    protected override void OnEnable()
    {
        if (!StaticCamera)
        {
            base.OnEnable();
            this.x = 30f;
            this.y = 30f;
            if (GetComponent<Rigidbody>() != null)
            {
                GetComponent<Rigidbody>().freezeRotation = true;
            }
            if (Can_See_Other)
            {
                InvokeRepeating("UpdateOtherList", 1, 1);
            }
            if (m_wait > 0)
            {
                isWait = true;
                StartCoroutine(Wait());
            }
            else
            {
                this.transform.parent = null;
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        CancelInvoke("UpdateOtherList");
    }

    /// <summary>
    /// update list of targets
    /// </summary>
    void UpdateOtherList()
    {
        OtherList.Clear();
        GameObject[] others = GameObject.FindGameObjectsWithTag(TagTargets);
        foreach (GameObject ot in others)
        {
            OtherList.Add(ot.transform);
        }
    }

    /// <summary>
    /// resfresh all
    /// </summary>
    void Refresh()
    {
        if (GameObject.Find(Follow) != null)
        {
            target = GameObject.Find(Follow).transform;
        }
        else
        {
            if (!Can_See_Other)
            {
                target = Provide;
            }
            else
            {
                OtherList.Clear();
                GameObject[] others = GameObject.FindGameObjectsWithTag(TagTargets);
                foreach (GameObject ot in others)
                {
                    OtherList.Add(ot.transform);
                }

            }
        }

    }

    public override void OnLateUpdate()
    {
        if(target != null)
        {
            bl_UIReferences.Instance.KillCamSpectatingText.text = target.name;
        }
        if (StaticCamera)
            return;

        if (!isWait)
        {
            CamMovements();
            UpdateTarget();
        }
    }

    /// <summary>
    /// update camera movements
    /// </summary>
    void CamMovements()
    {
        if (this.target != null)
        {
            RaycastHit hit;
            this.x += ((Input.GetAxis("Mouse X") * this.xSpeed) * this.distance) * 0.02f;
            this.y -= (Input.GetAxis("Mouse Y") * this.ySpeed) * 0.02f;
            this.y = ClampAngle(this.y, this.yMinLimit, this.yMaxLimit);
            Quaternion quaternion = Quaternion.Euler(this.y, this.x, 0f);
            this.distance = Mathf.Clamp(this.distance - (Input.GetAxis("Mouse ScrollWheel") * 5f), distanceMin, distanceMax);
            if ((Physics.Linecast(target.position, transform.position, out hit) && !hit.transform.IsChildOf(target)) && (target.transform != hit.transform))
            {
                distance = Mathf.Clamp(this.distance - (Input.GetAxis("Mouse ScrollWheel") * 5f), 0f, distanceMax);
                distance = hit.distance - 1.5f;
            }
            Vector3 vector = new Vector3(0f, 0f, -distance);
            Vector3 vector2 = target.position;
            vector2.y = target.position.y + 1f;
            Vector3 vector3 = (quaternion * vector) + vector2;
            transform.rotation = quaternion;
            if (!SmoothMovement)
            {
                transform.position = vector3;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.localPosition, vector3, Time.deltaTime * SpeedSmooth);
            }
        }
    }

    /// <summary>
    /// Update the camera to follow 
    /// </summary>
    void UpdateTarget()
    {
        if ((Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space)) && OtherList.Count > 0)
        {

            isSearch = true;
            if (CurrentOther <= OtherList.Count && CurrentOther >= 0)
            {
                CurrentOther++;
            }
            if (CurrentOther >= OtherList.Count)
            {
                CurrentOther = 0;
            }
        }

        if (!isSearch)
        {
            if (GameObject.Find(Follow) != null)
            {
                target = GameObject.Find(Follow).transform;
            }
            else
            {
                target = Provide;
            }
        }
        else
        {
            if (OtherList.Count > 0 && Can_See_Other)
            {
                target = OtherList[CurrentOther];
            }
            else
            {
                target = Provide;
            }
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
        {
            angle += 360f;
        }
        if (angle > 360f)
        {
            angle -= 360f;
        }
        return Mathf.Clamp(angle, min, max);
    }
    /// <summary>
    /// receive target name to camera follow
    /// </summary>
    /// <param name="t_target"> name of target</param>
    public void Send_Target(string t_target)
    {
        Follow = t_target;
        Refresh();
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(m_wait);
        this.transform.parent = null;
        isWait = false;
    }
}