using EasyBuildSystem.Runtimes;
using EasyBuildSystem.Runtimes.Internal.Builder;
using EasyBuildSystem.Runtimes.Internal.Managers.Data;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Codes.Editor.Internal.Manager.Collection
{
    [CustomEditor(typeof(InputsCollection))]
    public class InputsCollectionInspector : UnityEditor.Editor
    {
        #region Private Fields

        private static bool Help;

        #endregion Private Fields

        #region Public Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(10);

            #region Inspector

            GUILayout.BeginVertical("Easy Build System - Inputs Collection", "window", GUILayout.Height(10));

            GUILayout.BeginVertical("box");

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.BeginHorizontal();

            GUILayout.Label("Inputs Collection Settings", EditorStyles.boldLabel);

            #region Inputs Collection Settings

            if (GUILayout.Button(Help ? "Hide Help" : "Show Help", GUILayout.Width(100)))
                Help = !Help;

            GUI.color = Color.white;

            GUILayout.EndHorizontal();

            if (Help)
            {
                EditorGUILayout.HelpBox("This component allows to custom the input by default of system.\n" +
                    "It is also possible to custom the input by the ProjectSettings references. (Recommended for mobile/pad).", MessageType.Info);

                GUI.color = MainEditor.GetEditorColor;

                if (GUILayout.Button("Open Documentation Link"))
                    Application.OpenURL(Constants.DOCS_LINK);

                GUI.color = Color.white;
            }

            GUI.enabled = false;

            EditorGUILayout.ObjectField("Script", target, typeof(BuilderBehaviour), true);

            GUI.enabled = true;

            EditorGUILayout.HelpBox("The section Builder Behaviour contains only the input references that the default script use.\n" +
                "However, if you use your own builder with other inputs ref's you will need to custom this component.", MessageType.Info);

            GUILayout.Label("(General) Builder Behaviour Input(s) :");

            GUILayout.BeginVertical();

            GUI.color = MainEditor.GetEditorColor;

            GUI.color = Color.white;

            GUILayout.BeginVertical("box");

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Switch Input :", EditorStyles.boldLabel);

            GUI.color = Color.white;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("InputSwitchName"), new GUIContent("Custom Input Switch Name :", "This allows to define the switch key who can be used during the runtime.\n(Switch of prefab selection by default)."));

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Action Input :", EditorStyles.boldLabel);

            GUI.color = Color.white;

            if (serializedObject.FindProperty("InputActionIsCustom").boolValue)
            {
                EditorGUILayout.HelpBox("Insert here the names of axes used in the Input Manager of Project settings.", MessageType.Info);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("InputActionName"), new GUIContent("Input Validation Name :", "This allows to define the action key who can be used during the runtime.\n(Place the current preview by default)."));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InputActionKey"), new GUIContent("Input Validation Key :", "This allows to define the action key who can be used during the runtime.\n(Place the current preview by default)."));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("InputActionIsCustom"), new GUIContent("Use Custom Input Validate :", "This allows to define a keyCode or a custom input relative to Input Manager."));

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Cancel Input :", EditorStyles.boldLabel);

            GUI.color = Color.white;

            if (serializedObject.FindProperty("InputCancelIsCustom").boolValue)
            {
                EditorGUILayout.HelpBox("Insert here the names of axes used in the Input Manager of Project settings.", MessageType.Info);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("InputCancelName"), new GUIContent("Input Validation Name :", "This allows to define the cancel key who can be used during the runtime.\n(Cancel the current preview by default)."));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InputCancelKey"), new GUIContent("Input Validation Key :", "This allows to define the cancel key who can be used during the runtime.\n(Cancel the current preview by default)."));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("InputCancelIsCustom"), new GUIContent("Use Custom Input Cancel :", "This allows to define a keyCode or a custom input relative to Input Manager."));

            GUILayout.EndVertical();

            GUILayout.EndVertical();

            GUILayout.Label("(Default) Default Builder Behaviour Input(s) :");

            GUILayout.BeginVertical("box");

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Placement Input :", EditorStyles.boldLabel);

            GUI.color = Color.white;

            if (!serializedObject.FindProperty("InputPlacementIsCustom").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InputPlacementKey"), new GUIContent("Input Placement Key :", "This allows to define the placement key who can be used during the runtime.\n(Change mode to Placement by default)."));
            }
            else
            {
                EditorGUILayout.HelpBox("Insert here the names of axes used in the Input Manager of Project settings.", MessageType.Info);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("InputPlacementName"), new GUIContent("Custom Input Placement Name :", "This allows to define the placement key who can be used during the runtime.\n(Change mode to Placement by default)."));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("InputPlacementIsCustom"), new GUIContent("Use Custom Input Placement :", "This allows to define a keyCode or a custom input relative to Input Manager."));

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Destruction Input :", EditorStyles.boldLabel);

            GUI.color = Color.white;

            if (serializedObject.FindProperty("InputDestructionIsCustom").boolValue)
            {
                EditorGUILayout.HelpBox("Insert here the names of axes used in the Input Manager of Project settings.", MessageType.Info);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("InputDestructionName"), new GUIContent("Input Destruction Name :", "This allows to define the destruction key who can be used during the runtime.\n(Change mode to Destruction by default)."));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InputDestructionKey"), new GUIContent("Input Destruction Key :", "This allows to define the destruction key who can be used during the runtime.\n(Change mode to Destruction by default)."));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("InputDestructionIsCustom"), new GUIContent("Use Custom Input Destruction :", "This allows to define a keyCode or a custom input relative to Input Manager."));

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Edition Input :", EditorStyles.boldLabel);

            GUI.color = Color.white;

            if (serializedObject.FindProperty("InputEditionIsCustom").boolValue)
            {
                EditorGUILayout.HelpBox("Insert here the names of axes used in the Input Manager of Project settings.", MessageType.Info);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("InputEditionName"), new GUIContent("Input Edition Name :", "This allows to define the destruction key who can be used during the runtime.\n(Change mode to Edition by default)."));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InputEditionKey"), new GUIContent("Input Edition Key :", "This allows to define the destruction key who can be used during the runtime.\n(Change mode to Edition by default)."));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("InputEditionIsCustom"), new GUIContent("Use Custom Input Edition :", "This allows to define a keyCode or a custom input relative to Input Manager."));

            GUILayout.EndVertical();

            #endregion Inputs Collection Settings

            GUILayout.EndVertical();

            GUILayout.EndVertical();

            #endregion Inspector

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);
        }

        #endregion Public Methods
    }
}