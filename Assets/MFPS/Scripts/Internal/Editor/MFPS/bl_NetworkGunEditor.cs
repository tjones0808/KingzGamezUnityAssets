using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using MFPSEditor;

[CustomEditor(typeof(bl_NetworkGun))]
public class bl_NetworkGunEditor : Editor
{

    private void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        bl_NetworkGun script = (bl_NetworkGun)target;
        bool allowSceneObjects = !EditorUtility.IsPersistent(script);

        if(script.LocalGun != null)
        {
            script.gameObject.name = bl_GameData.Instance.GetWeapon(script.LocalGun.GunID).Name;
        }

        EditorGUILayout.BeginVertical("box");
        script.LocalGun = EditorGUILayout.ObjectField("Local Weapon", script.LocalGun, typeof(bl_Gun), allowSceneObjects) as bl_Gun;
        EditorGUILayout.EndVertical();

        if (script.LocalGun != null)
        {
            EditorGUILayout.BeginVertical("box");
            if (script.LocalGun.Info.Type != GunType.Knife)
            {
                script.Bullet = EditorGUILayout.ObjectField("Bullet", script.Bullet, typeof(GameObject), allowSceneObjects) as GameObject;

                if(script.LocalGun.Info.Type != GunType.Grenade)
                {
                    script.MuzzleFlash = EditorGUILayout.ObjectField("MuzzleFlash", script.MuzzleFlash, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
                   // script.LeftHandPosition = EditorGUILayout.ObjectField("Left Hand Reference", script.LeftHandPosition, typeof(Transform), allowSceneObjects) as Transform;
                }
            }
            if (script.LocalGun.Info.Type == GunType.Grenade)
            {
                script.DesactiveOnOffAmmo = EditorGUILayout.ObjectField("Desactive On No Ammo", script.DesactiveOnOffAmmo, typeof(GameObject), allowSceneObjects) as GameObject;
            }
                EditorGUILayout.EndVertical();
        }
        if(script.LeftHandPosition != null)
        {
            if(GUILayout.Button("Edit Hand Position"))
            {
                OpenIKWindow(script);
            }
        }
        else if(script.LocalGun.Info.Type != GunType.Grenade && script.LocalGun.Info.Type != GunType.Knife)
        {
            if (GUILayout.Button("SetUp Hand IK"))
            {

                GameObject gobject = new GameObject("LeftHandPoint");
                gobject.transform.parent = script.transform;
                gobject.transform.localPosition = Vector3.zero;
                gobject.transform.localEulerAngles = Vector3.zero;
                script.LeftHandPosition = gobject.transform;

                OpenIKWindow(script);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    void OpenIKWindow(bl_NetworkGun script)
    {
        AnimatorRunner window = (AnimatorRunner)EditorWindow.GetWindow(typeof(AnimatorRunner));
        window.Show();
        bl_PlayerSync pa = script.transform.root.GetComponent<bl_PlayerSync>();
        Animator anim = pa.m_PlayerAnimation.m_animator;
        pa.m_PlayerAnimation.EditorSelectedGun = script;
        bl_HeatLookMecanim hm = pa.m_PlayerAnimation.GetComponentInChildren<bl_HeatLookMecanim>(true);
        if (hm != null) { hm.enabled = true; }
        window.SetAnim(anim);
        Selection.activeObject = script.LeftHandPosition.gameObject;
    }
}