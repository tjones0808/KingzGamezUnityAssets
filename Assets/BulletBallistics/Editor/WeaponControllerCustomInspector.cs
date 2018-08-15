using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Ballistics;

[CustomEditor(typeof(BasicWeaponController))]
[CanEditMultipleObjects]
public class WeaponControllerCustomInspector : Editor {
    BasicWeaponController TargetWeaponController;

    private BallisticsSettings Settings;


    void OnEnable()
    {
        TargetWeaponController = (BasicWeaponController)target;

        if(TargetWeaponController.TargetWeapon == null)
        {
            TargetWeaponController.TargetWeapon = TargetWeaponController.GetComponent<Weapon>();
        }
        if (TargetWeaponController.mySpreadController == null)
        {
            TargetWeaponController.mySpreadController = TargetWeaponController.GetComponent<SpreadController>();
        }
        if (TargetWeaponController.myMagazineController == null)
        {
            TargetWeaponController.myMagazineController = TargetWeaponController.GetComponent<MagazineController>();
        }
        BallisticSettingsManager.LoadSettings();
        Settings = BallisticSettingsManager.Settings;
    }

    public override void OnInspectorGUI()
    {
        if (Settings != null)
        {
            EditorGUILayout.LabelField("Weapon Controller Editor", EditorStyles.largeLabel);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("General Settings:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            TargetWeaponController.TargetWeapon = (Weapon) EditorGUILayout.ObjectField("Target Weapon:", TargetWeaponController.TargetWeapon, typeof(Weapon), true);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object obj in targets)
                {
                    ((BasicWeaponController)obj).TargetWeapon = TargetWeaponController.TargetWeapon;
                }
            }

            EditorGUI.BeginChangeCheck();
            TargetWeaponController.ShootDelay = EditorGUILayout.FloatField(new GUIContent("Shootdelay:","Delay between shots"), TargetWeaponController.ShootDelay);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object obj in targets)
                {
                    ((BasicWeaponController)obj).ShootDelay = TargetWeaponController.ShootDelay;
                }
            }

            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("WeaponController Type:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            TargetWeaponController.WeaponType = (ShootingType)EditorGUILayout.EnumPopup(new GUIContent("Shooting Type:","Shooting Mode of the weapon: Single, Auto, Volley or Burst"), TargetWeaponController.WeaponType);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object obj in targets)
                {
                    ((BasicWeaponController)obj).WeaponType = TargetWeaponController.WeaponType;
                }
            }

            EditorGUILayout.Separator();

            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Burst/ Volley Mode:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            TargetWeaponController.BulletsPerBurst = EditorGUILayout.IntField(new GUIContent("Bullets per Burst:", "Bullets shot in one burst"), TargetWeaponController.BulletsPerBurst);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object obj in targets)
                {
                    ((BasicWeaponController)obj).BulletsPerBurst = TargetWeaponController.BulletsPerBurst;
                }
            }

            EditorGUI.BeginChangeCheck();
            TargetWeaponController.BulletsPerVolley = EditorGUILayout.IntField(new GUIContent("Bullets per Volley:","Bullets shot in one volley"), TargetWeaponController.BulletsPerVolley);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object obj in targets)
                {
                    ((BasicWeaponController)obj).BulletsPerVolley = TargetWeaponController.BulletsPerVolley;
                }
            }
            

            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Controller:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            TargetWeaponController.mySpreadController = (SpreadController)EditorGUILayout.ObjectField("Spread Controller:", TargetWeaponController.mySpreadController, typeof(SpreadController), true);
            if (EditorGUI.EndChangeCheck() && TargetWeaponController.mySpreadController != null)
            {
                foreach (Object obj in targets)
                {
                    ((BasicWeaponController)obj).mySpreadController = TargetWeaponController.mySpreadController;
                }
            }

            EditorGUI.BeginChangeCheck();
            TargetWeaponController.myMagazineController = (MagazineController)EditorGUILayout.ObjectField("Magazine Controller:", TargetWeaponController.myMagazineController, typeof(MagazineController), true);
            if (EditorGUI.EndChangeCheck() && TargetWeaponController.myMagazineController != null)
            {
                foreach (Object obj in targets)
                {
                    ((BasicWeaponController)obj).myMagazineController = TargetWeaponController.myMagazineController;
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("No Ballistic Settings found! Check 'Ballistic Settings Manager'");
        }
    }
}
