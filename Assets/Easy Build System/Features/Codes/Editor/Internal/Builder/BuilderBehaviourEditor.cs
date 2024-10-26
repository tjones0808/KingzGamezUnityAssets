﻿using EasyBuildSystem.Runtimes;
using EasyBuildSystem.Runtimes.Internal.Addons;
using EasyBuildSystem.Runtimes.Internal.Builder;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Codes.Editor.Internal.Builder
{
    [CustomEditor(typeof(BuilderBehaviour), true)]
    public class BuilderBehaviourEditor : UnityEditor.Editor
    {
        #region Private Fields

        private BuilderBehaviour Target;

        #region Foldout(s)

        private static bool AllIsOpen = false;

        private static bool BaseFoldout;

        private static bool ModesFoldout;

        private static bool PreviewFoldout;

        private static bool InputsFoldout;

        private static bool AudioFoldout;

        private static bool AddonsFoldout;

        private static bool Help;

        #endregion Foldout(s)

        #endregion Private Fields

        #region Public Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(10);

            #region Inspector

            GUILayout.BeginVertical("Easy Build System - Builder Behaviour", "window", GUILayout.Height(10));

            GUILayout.BeginVertical("box");

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.BeginHorizontal();

            GUILayout.Label("Builder Behaviour Settings", EditorStyles.boldLabel);

            #region Builder Behaviour Settings

            if (GUILayout.Button(AllIsOpen ? "Fold In" : "Fold Out", GUILayout.Width(80)))
            {
                BaseFoldout = !BaseFoldout;
                ModesFoldout = !ModesFoldout;
                PreviewFoldout = !PreviewFoldout;
                InputsFoldout = !InputsFoldout;
                AudioFoldout = !AudioFoldout;
                AddonsFoldout = !AddonsFoldout;
                AllIsOpen = !AllIsOpen;
            }

            if (GUILayout.Button(Help ? "Hide Help" : "Show Help", GUILayout.Width(100)))
                Help = !Help;

            GUI.color = Color.white;

            GUILayout.EndHorizontal();

            if (Help)
            {
                EditorGUILayout.HelpBox("This component contains all the actions behaviours whose can be performed by the player during the runtime.\n" +
                    "It require the Build Manager prefab in the scene with an Parts Collection, otherwise it will be useless.\n" +
                    "Note : Also check that this component is only present once in your scene, otherwise it can cause problems.", MessageType.Info);

                GUI.color = MainEditor.GetEditorColor;

                if (GUILayout.Button("Open Documentation Link"))
                    Application.OpenURL(Constants.DOCS_LINK);

                GUI.color = Color.white;
            }

            GUI.enabled = false;

            EditorGUILayout.ObjectField("Script", target, typeof(BuilderBehaviour), true);

            GUI.enabled = true;

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            BaseFoldout = EditorGUILayout.Foldout(BaseFoldout, "Base Section Settings");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            GUILayout.BeginVertical();

            if (BaseFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CreativeMode"), new GUIContent("Use Creative Mode :", "This allows to build without limit.\nNote : don't use this if you've a Inventory System."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraType"), new GUIContent("Camera Type :", "This allows to define the raycast origin type.\n" +
                    "First Person : The raycast origin come from camera center.\n" +
                    "Top Down : The raycast origin come from mouse position.\n" +
                    "Third Person : The raycast origin come from reference Transform Point."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("FreeLayers"), new GUIContent("Free Layer(s) :", "This allows to define the layer(s) on which the preview will be placed and moved."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("SocketLayers"), new GUIContent("Socket(s) Layer(s) :", "This allows to define the layer(s) that will be detected to snap the preview."));

                if (((RayType)serializedObject.FindProperty("CameraType").enumValueIndex) == RayType.ThirdPerson)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastOriginTransform"), new GUIContent("Camera Origin Transform :", "This allows to define the origin transform where the ray will be sent."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("ActionDistance"), new GUIContent("Placement Distance :", "This allows to define the maximum distance on which the preview can will be moved in allowed status."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("OutOfRangeDistance"), new GUIContent("Out Of Range Distance :", "This allows to define the maximum out of range distance on which the preview can will be moved in denied status. (0 = Use only Placement Distance)"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RayDetection"), new GUIContent("Raycast Detection Type :", "This allows to define the accuracy raycast type.\nIt is advisable to use the following types :\nLine if you've sockets with the type (Attachment).\nOverlap if you've sockets with the type (Point)."));

                if (serializedObject.FindProperty("RayDetection").enumValueIndex == (int)DetectionType.Overlap)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OverlapAngles"), new GUIContent("Overlap Detection Angles :", "This allows to define the maximum angles to detect the sockets."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastOffset"), new GUIContent("Raycast Forward Offset :", "This allows to define the raycast offset on axis (Z)."));
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            #endregion Builder Behaviour Settings

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Builder Preview Settings", EditorStyles.boldLabel);

            #region Builder Preview Settings

            GUI.color = Color.white;

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            PreviewFoldout = EditorGUILayout.Foldout(PreviewFoldout, "Preview Section Settings");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            GUILayout.BeginVertical();

            if (PreviewFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UsePreviewCamera"), new GUIContent("Use Through Preview :", "This allows to use the preview camera, this will allows to see through the mesh like terrain details."));

                if (serializedObject.FindProperty("UsePreviewCamera").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewCamera"), new GUIContent("Preview Camera :", "This allows to define the preview camera will be used."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewLayer"), new GUIContent("Preview Layer :", "This allows to define the preview layer that the camera can see."), true);
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("LockRotation"), new GUIContent("Lock Preview Rotation :", "This allows to make that the preview look the camera rotation."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewMovementType"), new GUIContent("Movement Type :", "This allows to define the preview movement type."));

                if ((MovementType)serializedObject.FindProperty("PreviewMovementType").enumValueIndex == MovementType.Smooth)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewSmoothTime"), new GUIContent("Movement Smooth Time :", "This allows to define the smooth time movement."));

                if ((MovementType)serializedObject.FindProperty("PreviewMovementType").enumValueIndex == MovementType.Grid)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewGridSize"), new GUIContent("Grid Size :", "This allows to define the grid size on which the preview will be moved."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewGridOffset"), new GUIContent("Grid Offset :", "This allows to define the grid offset for the preview position."));
                }
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            #endregion Builder Preview Settings

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Builder Inputs Settings", EditorStyles.boldLabel);

            #region Builder Inputs Settings

            GUI.color = Color.white;

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            InputsFoldout = EditorGUILayout.Foldout(InputsFoldout, "Inputs Section Settings");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            GUILayout.BeginVertical();

            if (InputsFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InputsCollection"), new GUIContent("Inputs Collection :", "This allows define all the inputs relative at Input Manager of your project settings."));
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            #endregion Builder Inputs Settings

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Builder Modes Settings", EditorStyles.boldLabel);

            #region Builder Modes Settings

            GUI.color = Color.white;

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            ModesFoldout = EditorGUILayout.Foldout(ModesFoldout, "Modes Section Settings");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            GUILayout.BeginVertical();

            if (ModesFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UsePlacementMode"), new GUIContent("Use Placement Mode :", "This allows to allow the placement mode."));

                if (serializedObject.FindProperty("UsePlacementMode").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetModeAfterPlacement"), new GUIContent("Reset Mode After Placement :", "This allows to reset the mode to (None) after the placement."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseDestructionMode"), new GUIContent("Use Destruction Mode :", "This allows to allow the destruction mode."));

                if (serializedObject.FindProperty("UseDestructionMode").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetModeAfterDestruction"), new GUIContent("Reset Mode After Destruction :", "This allows to reset the mode to (None) after the destruction."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseEditionMode"), new GUIContent("Use Edition Mode :", "This allows to allow the edition mode."));

                if (serializedObject.FindProperty("UseEditionMode").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetModeAfterEdition"), new GUIContent("Reset Mode After Edition :", "This allows to reset the mode to (None) after the edition."));
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            #endregion Builder Modes Settings

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Builder Audio Settings", EditorStyles.boldLabel);

            #region Builder Audio Settings

            GUI.color = Color.white;

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            AudioFoldout = EditorGUILayout.Foldout(AudioFoldout, "Audio Section Settings");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            GUILayout.BeginVertical();

            GUI.color = Color.white;

            if (AudioFoldout)
            {
                EditorGUILayout.HelpBox("If you want use the placements and destructions sounds, add below your Audio Source component also that the clips.\n" +
                    "If the Audio Source field is empty, no clips will be played during the runtime.", MessageType.Info);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Source"), new GUIContent("Audio Source :", "This source is the source on which the sounds will be played."));

                GUILayout.BeginHorizontal();

                GUILayout.Space(13);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PlacementClips"), new GUIContent("Placement Sound(s) :", "Placement clips at play when a preview is placed (Played randomly)."), true);

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Space(13);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("DestructionClips"), new GUIContent("Destructions Sound(s) :", "Destruction clips at play when a part is destroyed (Played randomly."), true);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            #endregion Builder Audio Settings

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Builder Add-Ons Settings", EditorStyles.boldLabel);

            #region Builder Add-Ons Settings

            GUI.color = Color.white;

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            AddonsFoldout = EditorGUILayout.Foldout(AddonsFoldout, "Add-Ons Section Settings");

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Space(13);

            GUILayout.BeginVertical();

            if (AddonsFoldout)
            {
                MainEditor.DrawAddons(Target, AddOnTarget.BuilderBehaviour);
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            #endregion Builder Add-Ons Settings

            GUILayout.EndVertical();

            GUILayout.EndVertical();

            #endregion Inspector

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);
        }

        #endregion Public Methods

        #region Private Methods

        private void OnEnable()
        {
            Target = (BuilderBehaviour)target;

            MainEditor.LoadAddons(Target, AddOnTarget.BuilderBehaviour);
        }

        #endregion Private Methods
    }
}