#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    // Create a new type of Settings Asset.
    public class Terrain25DSettings : ScriptableObject
    {
        public const string Version = "2.0.0";

        public const string SettingsFilePath = "Assets/2.5DTerrainSettings.asset";

        public static Color DefaultAddColor = Color.white;
        public static Color DefaultSubtractColor = new Color(1f, 0.6f, 0.6f);
        public static Color DefaultCombinedColor = Color.green;

        [SerializeField, HideInInspector]
        protected string lastKnownSettingsVersion;

        [SerializeField]
        public Material DefaultMaterial;

        [SerializeField]
        public Color AddColor;

        [SerializeField]
        public Color SubtractColor;

        [SerializeField]
        public Color CombinedColor;

        [SerializeField]
        public bool ShowLabels;

        [SerializeField]
        public KeyCode AddCurvePointKey = KeyCode.N;

        [SerializeField]
        public KeyCode HandleTypeNoneKey = KeyCode.None;

        [SerializeField]
        public KeyCode HandleTypeBrokenKey = KeyCode.None;

        [SerializeField]
        public KeyCode HandleTypeMirroredKey = KeyCode.None;

        [SerializeField]
        public bool ShowIcon = true;

        [SerializeField]
        public bool ShowLogs = false;

        [SerializeField]
        public bool CollapseHierarchyAfterSplineEdit = true;

        static Terrain25DSettings cachedSettings;

        [InitializeOnLoadMethod]
        public static Terrain25DSettings GetOrCreateSettings()
        {
            if (cachedSettings == null)
            {
                cachedSettings = AssetDatabase.LoadAssetAtPath<Terrain25DSettings>(SettingsFilePath);
                
                // Still not found? Then search for it.
                if (cachedSettings == null)
                {
                    string[] results = AssetDatabase.FindAssets("t:Terrain25DSettings");
                    if (results.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(results[0]);
                        cachedSettings = AssetDatabase.LoadAssetAtPath<Terrain25DSettings>(path);
                    }
                }

                // Still not found? Then create settings.
                if (cachedSettings == null)
                {
                    Material defaultMaterial = null;
                    defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/2.5D Terrain/Materials/2.5D Terrain TriPlanar.mat");

                    cachedSettings = ScriptableObject.CreateInstance<Terrain25DSettings>();
                    cachedSettings.DefaultMaterial = defaultMaterial;
                    cachedSettings.AddColor = Terrain25DSettings.DefaultAddColor;
                    cachedSettings.SubtractColor = Terrain25DSettings.DefaultSubtractColor;
                    cachedSettings.CombinedColor = Terrain25DSettings.DefaultCombinedColor;
                    cachedSettings.AddCurvePointKey = KeyCode.N;
                    cachedSettings.HandleTypeNoneKey = KeyCode.None;
                    cachedSettings.HandleTypeBrokenKey = KeyCode.None;
                    cachedSettings.HandleTypeMirroredKey = KeyCode.None;
                    cachedSettings.ShowIcon = true;
                    cachedSettings.ShowLogs = false;
                    cachedSettings.CollapseHierarchyAfterSplineEdit = true;
                    cachedSettings.lastKnownSettingsVersion = Version;

                    AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);
                    AssetDatabase.SaveAssets();

                    MaterialShaderFixer.FixMaterialsDelayed(onSetupComplete);
                }
                else if(cachedSettings.lastKnownSettingsVersion != Version)
                {
                    // Version changed.
                    Debug.Log("2.5D Terrain: New settings version detected ("+cachedSettings.lastKnownSettingsVersion+" > "+Version+").");
                    cachedSettings.lastKnownSettingsVersion = Version;
                    EditorUtility.SetDirty(cachedSettings);
                    AssetDatabase.SaveAssets();

                    MaterialShaderFixer.FixMaterialsDelayed(null); 
                }
            }

            return cachedSettings;
        }

        static void onSetupComplete()
        {
            EditorUtility.DisplayDialog(
                   "Import finished",
                   "The 2.5D Terrain package has been imported.\n\nYou can create a new terrain under:\n" +
                   "Tools > 2.5D Terrain > Create ..." +
                   "\n\n" +
                   "Let's start by showing you the demo scene.\n\n" +
                   "If things appear pink then try:\n" +
                   "Tools > 2.5D Terrain > Fix Materials",
                   "Ok (open demo scene)"
                   );

            EditorSceneManager.OpenScene("Assets/2.5D Terrain/Examples/2.5D Terrain Demo.unity");
            OpenOfflineManual();
        }

        public static void OpenOfflineManual()
        {
            EditorUtility.OpenWithDefaultApp("Assets/2.5D Terrain/2.5DTerrainManual.pdf");
        }

        static UnityEngine.Rendering.RenderPipelineAsset getUsedRenderPipeline()
        {
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            else
                return UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        // settings
        [MenuItem("Tools/2.5D Terrain/Settings", priority = 500)]
        public static void SelectSettings()
        {
            var settings = Terrain25DSettings.GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "2.5D Terrain settings could not be found or created.", "Ok");
            }
        }
    }

    static class Terrain25DSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateTerrain25DSettingsProvider()
        {
            var provider = new SettingsProvider("Project/2.5D Terrain", SettingsScope.Project)
            {
                label = "2.5D Terrain",
                guiHandler = (searchContext) =>
                {
                    var settings = Terrain25DSettings.GetSerializedSettings();

                    EditorGUILayout.LabelField("Version: " + Terrain25DSettings.Version);

                    EditorGUILayout.PropertyField(settings.FindProperty("DefaultMaterial"), new GUIContent("Default Material:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("AddColor"), new GUIContent("Spline add color:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("SubtractColor"), new GUIContent("Spline subtract color:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("CombinedColor"), new GUIContent("Spline combined color:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("ShowLabels"), new GUIContent("Show Labels:"));

                    EditorGUILayout.PropertyField(settings.FindProperty("AddCurvePointKey"), new GUIContent("Add curve point key:"));
                    EditorGUILayout.HelpBox("Press and hold this key to enable the add point mode.", MessageType.None);

                    EditorGUILayout.PropertyField(settings.FindProperty("HandleTypeNoneKey"), new GUIContent("Set handle to 'None' key:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("HandleTypeBrokenKey"), new GUIContent("Set handle to 'Broken' key:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("HandleTypeMirroredKey"), new GUIContent("Set handle to 'Mirrored' key:"));

                    EditorGUILayout.PropertyField(settings.FindProperty("CollapseHierarchyAfterSplineEdit"), new GUIContent("Collapse Hierarchy after spline edit:"));

                    EditorGUILayout.PropertyField(settings.FindProperty("ShowIcon"), new GUIContent("Show the gizmo icon:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("ShowLogs"), new GUIContent("Show log messages:"));

                    settings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting.
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "2.5D", "2.5d", "terrain", "terrain25d", "25d", "2.5d" })
            };

            return provider;
        }
    }
}
#endif