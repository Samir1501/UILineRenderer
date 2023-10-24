using UnityEditor;
using UnityEngine;

public class UILineSettings : ScriptableObject
{
    private const string settingsPath = "Assets/Editor/UILineSettings.asset";
    internal static SerializedObject serializedObject => GetSerializedSettings();
    internal static UILineSettings uiLineSettings => GetOrCreateSettings();

    public CurveHandeSettings curveHandeSettings;
    
    private static UILineSettings GetOrCreateSettings()
    {
        UILineSettings settings = AssetDatabase.LoadAssetAtPath<UILineSettings>(settingsPath);
        if (settings == null)
        {
            settings = CreateInstance<UILineSettings>();
            AssetDatabase.CreateAsset(settings,settingsPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }
    private static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }
}
