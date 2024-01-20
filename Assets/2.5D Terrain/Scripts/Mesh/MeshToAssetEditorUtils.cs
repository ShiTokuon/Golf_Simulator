#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace Kamgam.Terrain25DLib
{
    public static class MeshToAssetEditorUtils
    {
        static string meshNamePart = "25DTerrainMesh";
        static Regex invalidPathCharactersRegex = new Regex(@"[^a-zA-Z0-9-_. ()]+");

        static string sanitizePath(string path)
        {
            return invalidPathCharactersRegex.Replace(path, "");
        }

        public static void SaveMesh(MeshFilter meshFilter)
        {
            SaveMeshAsAsset(meshFilter.sharedMesh, getNewAssetPath(meshFilter));

            // Ensure that modified prefab assets are saved
            if (PrefabUtility.IsPartOfAnyPrefab(meshFilter.gameObject) && !PrefabUtility.IsPartOfImmutablePrefab(meshFilter.gameObject))
            {
                var root = getRoot(meshFilter.gameObject);
                PrefabUtility.SavePrefabAsset(root);

                EditorUtility.SetDirty(meshFilter);
#if UNITY_2020_3_OR_NEWER
                AssetDatabase.SaveAssetIfDirty(meshFilter);
#else
                AssetDatabase.SaveAssets();
#endif

                var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(meshFilter.gameObject);
                AssetDatabase.ImportAsset(assetPath);
            }

#if UNITY_2021_2_OR_NEWER
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
            var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif
            if (prefabStage != null && prefabStage.IsPartOfPrefabContents(meshFilter.gameObject))
            {
                // Make sure the Prefab recognizes the changes
                PrefabUtility.RecordPrefabInstancePropertyModifications(meshFilter);
                EditorSceneManager.MarkSceneDirty(meshFilter.gameObject.scene);

                // Schedule an update to the scene view will rerender (otherwise
                // the change would not be visible unless clicked into the scene view).
                EditorApplication.QueuePlayerLoopUpdate();
            }
        }

        public static GameObject getRoot(GameObject go)
        {
            var result = go.transform;
            while (result.transform.parent != null)
                result = result.transform.parent;

            return result.gameObject;
        }

        public static void SaveMeshAsAsset(Mesh mesh, string assetPath)
        {
            if (mesh == null)
                return;

            // List to remember the affected components.
            List<Component> componentsWithExistingMesh = null;

            // Check if the asset already exists.
            var existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existingMesh != null)
            {
                Undo.RegisterCompleteObjectUndo(existingMesh, "2.5D Terrain mesh updated.");

                // Remember the affected components.
                componentsWithExistingMesh = getComponentsInLoadedScenesWithReferenceToMesh(existingMesh);
            }

            if(mesh == existingMesh)
            {
                Debug.LogWarning("No changes to the mesh detected. Aborting.");
                return;
            }

            AssetDatabase.CreateAsset(mesh, assetPath);
            AssetDatabase.SaveAssets();
            // Important to force the reimport to avoid the "SkinnedMeshRenderer: Mesh has
            // been changed to one which is not compatibile with the expected mesh data size
            // and vertex stride." error.
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            // Sadly "Undo.RegisterCreatedObjectUndo" does not work here. TODO: investigate
            // if (existingMesh == null)
            //    Undo.RegisterCreatedObjectUndo(mesh, "Make Mesh double-sided new asset");

            Debug.Log($"Saved new 2.5D terrain mesh under <color=yellow>'{assetPath}'</color>.");

            // Patch old references in scenes so they now all point to the new mesh.
            if (componentsWithExistingMesh != null)
                setComponentMeshInLoadedScenes(componentsWithExistingMesh, mesh);
        }

        /// <summary>
        /// Returns a list of all the MeshRenderer or SkinnedMeshRenderer components in
        /// the loaded scene (in the hierarchy) which have a reference to the given "mesh".
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        static List<Component> getComponentsInLoadedScenesWithReferenceToMesh(Mesh mesh)
        {
            var components = new List<Component>();

            int numOfScenes = EditorSceneManager.sceneCount;
            for (int i = 0; i < numOfScenes; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                var rootObjects = scene.GetRootGameObjects();
                for (int r = 0; r < rootObjects.Length; r++)
                {
                    var root = rootObjects[r];

                    var meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
                    foreach (var meshFilter in meshFilters)
                    {
                        if (meshFilter.sharedMesh == mesh)
                            components.Add(meshFilter);
                    }

                    var meshRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
                    foreach (var meshRenderer in meshRenderers)
                    {
                        if (meshRenderer.sharedMesh == mesh)
                            components.Add(meshRenderer);
                    }
                }
            }

            return components;
        }

        static void setComponentMeshInLoadedScenes(List<Component> componentsWithExistingMesh, Mesh newMesh)
        {
            foreach (var component in componentsWithExistingMesh)
            {
                if (component is MeshFilter meshFilter)
                {
                    meshFilter.sharedMesh = newMesh;
                }
                if (component is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    skinnedMeshRenderer.sharedMesh = newMesh;
                }
            }
        }

        static string getNewAssetPath(MeshFilter meshFilter)
        {
            string name = "25DMesh";
            var terrain = meshFilter.gameObject.GetComponentInParent<Terrain25D>();
            if(terrain != null)
            {
                name = terrain.name;
            }

            // Try to get the prefab path first.
            string filePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(meshFilter);

            // Maybe it's an object within the prefab stage?
            if (string.IsNullOrEmpty(filePath))
                filePath = getPrefabStageAssetPath();

            // Not a prefab -> get path from the scene.
            if (string.IsNullOrEmpty(filePath))
                filePath = getFilePathForSceneObject(meshFilter.gameObject);

            return getNewAssetPath(filePath, name);
        }

        static string getPrefabStageAssetPath()
        {
#if UNITY_2021_2_OR_NEWER
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
                return prefabStage.assetPath;
#else
            var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
                return prefabStage.prefabAssetPath;
#endif

            return null;
        }

        static string getFilePathForSceneObject(GameObject go)
        {
            string path;
            if (Directory.Exists("Assets/Models"))
                path = "Assets/Models/";
            else
                path = "Assets/";
            string sceneName = go.scene == null ? "" : go.scene.name;
            sceneName = sanitizePath(sceneName);

            string objectName = go.name;
            objectName = sanitizePath(objectName);

            path = path + sceneName + "." + objectName;
            return path;
        }

        static string getNewAssetPath(string filePath, string sharedMeshName)
        {
            // Fallback in case the file path is invalid.
            if (string.IsNullOrEmpty(filePath))
            {
                return "Assets/" + sharedMeshName + ".asset";
            }

            int lastIndex = filePath.LastIndexOf('/');
            string path = filePath.Substring(0, lastIndex + 1);

            sharedMeshName = sanitizePath(sharedMeshName);
            return path + sharedMeshName + "." + meshNamePart + ".asset";
        }
    }
}
#endif