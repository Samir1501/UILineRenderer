using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class UILineRendererPreferences : SettingsProvider
{
    private new const string settingsPath = "Assets/Editor/UILineSettings.asset";

    private static SerializedObject uiLineSettings;
    private UILineRendererPreferences(string path, SettingsScope scopes = SettingsScope.User, IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }
    private static bool isSettingsAvailable => File.Exists(settingsPath);

    public override void OnGUI(string searchContext)
    {
        MyNewPrefCode();
        EditorGUILayout.PropertyField(uiLineSettings.FindProperty("curveHandeSettings"));
        uiLineSettings.ApplyModifiedProperties();
    }
    
    [SettingsProvider]
    static SettingsProvider MyNewPrefCode()
    {
        uiLineSettings = UILineSettings.serializedObject;
        return isSettingsAvailable ? new UILineRendererPreferences("Preferences/UI Line Settings") : null;
    }
}
