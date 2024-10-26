﻿using UnityEngine;

public class Demo_FreeLookCam : Demo_PivotBasedCameraRig
{
    #region Public Fields

    [Header("Free Look Settings")]
    public float MoveSpeed = 1f;

    [Range(0f, 10f)] public float TurnSpeed = 1.5f;
    public float TurnSmoothing = 0.0f;
    public float TiltMax = 75f;
    public float TiltMin = 45f;
    public bool LockCursor = false;
    public bool VerticalAutoReturn = false;

    #endregion Public Fields

    #region Private Fields

    private float LookAngle;
    private float TiltAngle;
    private const float LookDistance = 100f;
    private Vector3 PivotEulers;
    private Quaternion PivotTargetRot;
    private Quaternion TransformTargetRot;

    #endregion Private Fields

    #region Private Methods

    protected override void Awake()
    {
        base.Awake();

        Cursor.lockState = LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !LockCursor;
        PivotEulers = Pivot.rotation.eulerAngles;

        PivotTargetRot = Pivot.transform.localRotation;
        TransformTargetRot = transform.localRotation;
    }

    protected void Update()
    {
        HandleRotationMovement();
        if (LockCursor && Input.GetKeyUp(KeyCode.B))
        {
            Cursor.lockState = LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !LockCursor;
        }
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected override void FollowTarget(float deltaTime)
    {
        if (Target == null)
            return;

        transform.position = Vector3.Lerp(transform.position, Target.position, deltaTime * MoveSpeed);
    }

    private void HandleRotationMovement()
    {
        if (Time.timeScale < float.Epsilon)
            return;

        var x = Input.GetAxis("Mouse X");
        var y = Input.GetAxis("Mouse Y");
        LookAngle += x * TurnSpeed;

        TransformTargetRot = Quaternion.Euler(0f, LookAngle, 0f);

        if (VerticalAutoReturn)
        {
            TiltAngle = y > 0 ? Mathf.Lerp(0, -TiltMin, y) : Mathf.Lerp(0, TiltMax, -y);
        }
        else
        {
            TiltAngle -= y * TurnSpeed;
            TiltAngle = Mathf.Clamp(TiltAngle, -TiltMin, TiltMax);
        }

        PivotTargetRot = Quaternion.Euler(TiltAngle, PivotEulers.y, PivotEulers.z);

        if (TurnSmoothing > 0)
        {
            Pivot.localRotation = Quaternion.Slerp(Pivot.localRotation, PivotTargetRot, TurnSmoothing * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, TransformTargetRot, TurnSmoothing * Time.deltaTime);
        }
        else
        {
            Pivot.localRotation = PivotTargetRot;
            transform.localRotation = TransformTargetRot;
        }
    }

    #endregion Private Methods
}