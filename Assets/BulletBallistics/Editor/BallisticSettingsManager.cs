using System.Collections;
using UnityEditor;
using Ballistics;

public static class BallisticSettingsManager {
    public static BallisticsSettings Settings;

    public static void LoadSettings()
    {
        if(Settings == null)
        {
            if (EditorPrefs.HasKey("SettingsPath"))
            {
                string path = EditorPrefs.GetString("SettingsPath");
				BallisticsSettings LoadedSettings = (BallisticsSettings)AssetDatabase.LoadAssetAtPath(path, typeof(BallisticsSettings));

				if(LoadedSettings != null){
					Settings = LoadedSettings;
				}
            }
        }
    }
}
