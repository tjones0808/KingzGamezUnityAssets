using EasyBuildSystem.Runtimes;
using EasyBuildSystem.Runtimes.Internal.Builder;
using EasyBuildSystem.Runtimes.Internal.Storage;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EasyBuildSystem.Codes.Editor.Internal.Storage
{
    [CustomEditor(typeof(BuildStorage))]
    public class BuildStorageInspector : UnityEditor.Editor
    {
        #region Private Fields

        private BuildStorage Target;

        private string LoadPath;

        #region Foldout(s)

        private static bool AllIsOpen = false;

        private static bool BaseFoldout;

        private static bool Help;

        #endregion Foldout(s)

        #endregion Private Fields

        #region Public Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(10);

            #region Inspector

            GUILayout.BeginVertical("Easy Build System - Build Storage", "window", GUILayout.Height(10));

            GUILayout.BeginVertical("box");

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.BeginHorizontal();

            GUILayout.Label("Build Storage Settings", EditorStyles.boldLabel);

            #region Build Storage Settings

            if (GUILayout.Button(AllIsOpen ? "Fold In" : "Fold Out", GUILayout.Width(80)))
            {
                BaseFoldout = !BaseFoldout;
                AllIsOpen = !AllIsOpen;
            }

            if (GUILayout.Button(Help ? "Hide Help" : "Show Help", GUILayout.Width(100)))
                Help = !Help;

            GUI.color = Color.white;

            GUILayout.EndHorizontal();

            if (Help)
            {
                EditorGUILayout.HelpBox("This component allows to save and load all the Parts Behaviour of the scene before/after the runtime.\n" +
                    "Note : The larger the number of parts saved in the file, the longer the loading will be.", MessageType.Info);

                EditorGUILayout.HelpBox("The component has not been tested on Mac/Linux.\n" +
                    "The define path button works only under windows, if you do not use windows please define the path directly in the field output path.", MessageType.Info);

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
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoDefine"), new GUIContent("Auto Define :", "This allows to auto define the data path."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoSave"), new GUIContent("Auto Save :", "This allows to enable auto save."));

                if (serializedObject.FindProperty("AutoSave").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoSaveInterval"), new GUIContent("Auto Save Interval (ms) :", "This allows to define the auto save interval."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("SavePrefabs"), new GUIContent("Save The Prefabs :", "This allows to save all the prefabs after have exited the scene."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("LoadPrefabs"), new GUIContent("Load The Prefabs :", "This allows to save all the prefabs at startup of the scene."));

                if (serializedObject.FindProperty("StorageFileName").stringValue.Contains("{date}") || serializedObject.FindProperty("StorageFileName").stringValue.Contains("{time}"))
                {
                    GUI.enabled = serializedObject.FindProperty("LoadPrefabs").boolValue;

                    if (serializedObject.FindProperty("LoadLastSave").boolValue)
                    {
                        EditorGUILayout.HelpBox("Note: This option is useful only if your field (StorageFileName) contains argument(s).\nUse the function PlayerPrefs to save the path.", MessageType.Info);
                    }

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("LoadLastSave"), new GUIContent("Load Last Save File :", "This allows to load the last the save\nThis is only useful if your FileName contains a date/time."));

                    GUI.enabled = false;

                    GUILayout.Label("Last Save Path : " + PlayerPrefs.GetString("lastUsedFile"));

                    GUI.enabled = true;

                    if (GUILayout.Button("Clear Last Save File"))
                    {
                        PlayerPrefs.DeleteKey("lastUsedFile");

                        Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : The last save file has been cleared.");
                    }

                    GUI.enabled = true;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("StorageFileName"), new GUIContent("Storage File Name :", "Name of file on which the data will writing.\nAllow some argument(s) :\n{DATE} Write current date on file name.\n{TIME} Write current time on file name."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("StorageFileExtension"), new GUIContent("Storage File Extension :", "Extension of file on which the data will writing."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("StorageSpecialFolder"), new GUIContent("Storage Special Output :", "Storage output type."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("StorageOutputFile"), new GUIContent("Storage Output Path :", "Output path to save and load the file."));

                GUI.color = MainEditor.GetEditorColor;

                if (GUILayout.Button("Define Save Storage Path ..."))
                {
                    Target.StorageOutputFile = Environment.GetFolderPath((Environment.SpecialFolder)serializedObject.FindProperty("StorageSpecialFolder").intValue) + "\\" + Target.StorageFileName + "." + Target.StorageFileExtension;

                    EditorUtility.SetDirty(target);
                }
                GUI.color = Color.white;

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            #endregion Build Storage Settings

            GUI.color = MainEditor.GetEditorColor;

            GUILayout.Label("Build Storage Editor Loader", EditorStyles.boldLabel);

            #region Build Storage Editor Loader

            GUI.color = Color.white;

            GUILayout.Label("Load Storage File In Edit Mode : " + LoadPath);

            GUI.color = MainEditor.GetEditorColor;

            if (GUILayout.Button("Load Storage File ..."))
            {
                if (EditorUtility.DisplayDialog("Easy Build System - Information", "(Only Large File) Note :\nYour scene will be saved, to avoid the loss of data in case of a crash of the editor.", "Load", "Cancel"))
                {
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

                    LoadPath = EditorUtility.OpenFilePanel("Load Ebs Storage File :", "", "*.*");

                    if (LoadPath != string.Empty)
                    {
                        Target.LoadInEditor(LoadPath);
                    }
                }
            }

            GUI.color = Color.white;

            #endregion Build Storage Editor Loader

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
            Target = (BuildStorage)target;
        }

        #endregion Private Methods
    }
}