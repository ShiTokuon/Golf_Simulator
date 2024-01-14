#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    public static class MaterialShaderFixer
    {
        public enum RenderPiplelineType
        {
            URP, HDRP, BuiltIn
        }

        private class ShaderFixPackage
        {
            public RenderPiplelineType RenderPipeline;
            public bool UseShaderGraph;
            /// <summary>
            /// If UseShaderGraph = true then this is the major shader graph version.
            /// Otherwise it's the UNITY version of the BuiltIn shader.
            /// </summary>
            public int ShaderGraphVersion;
            public string PackagePath;

            public ShaderFixPackage(RenderPiplelineType renderPipeline, bool useShaderGraph, int shaderGraphVersion, string packagePath)
            {
                RenderPipeline = renderPipeline;
                UseShaderGraph = useShaderGraph;
                ShaderGraphVersion = shaderGraphVersion;
                PackagePath = packagePath;
            }
        }

        static List<ShaderFixPackage> Packages = new List<ShaderFixPackage>()
        {
			new ShaderFixPackage( RenderPiplelineType.URP,     useShaderGraph: true, shaderGraphVersion: 7,  "Assets/2.5D Terrain/Packages/ShaderGraph7-URP.unitypackage" ),
            new ShaderFixPackage( RenderPiplelineType.HDRP,    useShaderGraph: true, shaderGraphVersion: 7,  "Assets/2.5D Terrain/Packages/ShaderGraph7-HDRP.unitypackage" ),
            new ShaderFixPackage( RenderPiplelineType.HDRP,    useShaderGraph: true, shaderGraphVersion: 10, "Assets/2.5D Terrain/Packages/ShaderGraph10-HDRP.unitypackage" ),
            new ShaderFixPackage( RenderPiplelineType.BuiltIn, useShaderGraph: true, shaderGraphVersion: 12, "Assets/2.5D Terrain/Packages/ShaderGraph12-BuiltIn.unitypackage" )
        };

        static ShaderFixPackage getPackageFor(RenderPiplelineType renderPipeline, bool useShaderGraph, int version)
        {
            foreach (var pkg in Packages)
            {
                if (pkg.RenderPipeline == renderPipeline && pkg.UseShaderGraph == useShaderGraph && pkg.ShaderGraphVersion == version)
                    return pkg;
            }

            return null;
        }

        static System.Action _onComplete;

        #region StartFixMaterial delayed
        static double startPackageImportAt;

        public static void FixMaterialsDelayed(System.Action onComplete)
        {
            // Materials may not be loaded at this time. Thus we wait for them to be imported.
            _onComplete = onComplete;
            EditorApplication.update -= onEditorUpdate;
            EditorApplication.update += onEditorUpdate;
            startPackageImportAt = EditorApplication.timeSinceStartup + 3; // wait N seconds
        }

        static void onEditorUpdate()
        {
            // wait for the time to reach startPackageImportAt
            if (startPackageImportAt - EditorApplication.timeSinceStartup < 0)
            {
                EditorApplication.update -= onEditorUpdate;
                FixMaterials();
                return;
            }
        }
        #endregion

        [MenuItem("Tools/2.5D Terrain/Fix Materials", priority = 501)]
        public static void FixMaterials()
        {
            Debug.Log("Importing materials.");
            CheckPackages.CheckForPackageVersion("com.unity.shadergraph", (version) =>
            {
                var createdForRenderPipleline = RenderPiplelineType.BuiltIn;
                var currentRenderPipline = GetCurrentRenderPiplelineType();

                // If the package was not found then 'version' will be NULL.
                bool useShaderGraph = version != null;
                int shaderGraphVersion = 0;
                if (useShaderGraph)
                {
                    // default shadergraph version file are 7
                    shaderGraphVersion = 7;
                    
                    // Use the updated ShaderGraph10 package for HDRP >= 10
                    if (version.Major >= 10 && currentRenderPipline == RenderPiplelineType.HDRP)
                        shaderGraphVersion = 10;

                    // ShaderGraph 12 added support for BuiltIn
                    if (version.Major >= 12 && currentRenderPipline == RenderPiplelineType.BuiltIn)
                        shaderGraphVersion = 12;
                }

                // 2.5D Terrain does not contain HLSL shaders for URP or HDRP.
                // In those cases revert to the standard shader of each render pipeline.
                if (!useShaderGraph && currentRenderPipline != RenderPiplelineType.BuiltIn)
                {
                    // Not shader graph installed -> Fix tri planar shader by applying the default shader.
                    string msg = "Shader Graph not used.\n\nTo use the provided Tri-Planar shader you'll have to either use the BuiltIn (Standard) renderer or install shader graph: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest/ and then call Tools > 2.5D Terrain > Fix Materials afterwards.\n\nFor now the 'Standard' shader will be assigned to all tri-planar materials";
                    EditorUtility.DisplayDialog("Shader Graph Package is not installed!", msg, "Okay");

                    // Revert shadergraph shader to default shader if shadergraph package is not installed
                    var shader = GetDefaultShader();
                    if (shader != null)
                    {
                        var currentRenderPipeline = GetCurrentRenderPiplelineType();
                        var materialPaths = new Dictionary<string, Color>
                        {
                            { "Assets/2.5D Terrain/Materials/2.5D Terrain TriPlanar.mat", new Color(0.1f, 0.600f, 0.1f) },
                            { "Assets/2.5D Terrain/Examples/2.5D Terrain TriPlanar Rock Grass.mat", new Color(0.1f, 0.600f, 0.1f) },
                            { "Assets/2.5D Terrain/Examples/2.5D Terrain TriPlanar Rock Sand.mat", new Color(1f, 0.9f, 0.3f) },
                            { "Assets/2.5D Terrain/Examples/Foliage/Materials/BushPurple.mat", new Color(0.8f, 0.0f, 0.7f) },
                            { "Assets/2.5D Terrain/Examples/Foliage/Materials/BushTeal.mat", new Color(0.0f, 0.9f, 0.9f) },
                            { "Assets/2.5D Terrain/Examples/Foliage/Materials/GrassRed.mat", new Color(0.7f, 0.24f, 0.0f) },
                            { "Assets/2.5D Terrain/Examples/Foliage/Materials/GrassYellow.mat", new Color(0.9f, 0.7f, 0f) },
                            { "Assets/2.5D Terrain/Examples/Foliage/Materials/Sky.mat", new Color(0.4f, 0.9f, 0.9f) },
                            { "Assets/2.5D Terrain/Examples/Foliage/Materials/TreeLeavesGreen.mat", new Color(0.15f, 0.5f, 0.18f) },
                            { "Assets/2.5D Terrain/Examples/Foliage/Materials/TreeLeavesYellow.mat", new Color(0.9f, 0.7f, 0f) },
                            { "Assets/2.5D Terrain/Examples/Foliage/Materials/TreeTrunk.mat", new Color(0.5f, 0.24f, 0.0f) }
                        };

                        foreach (var kv in materialPaths)
                        {
                            var path = kv.Key;
                            var color = kv.Value;
                            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                            if (material != null)
                            {
                                Debug.LogWarning($"Setting material '{path}' to Standard shader.");
                                material.shader = shader;
                                material.color = color;
                            }
                        }
                        AssetDatabase.SaveAssets();
                    }
                    else
                    {
                        Debug.LogError("No default shader found! Please contact support.");
                    }
                }

                var package = getPackageFor(currentRenderPipline, useShaderGraph, shaderGraphVersion);
                if (package == null)
                {
                    Debug.Log("All good.");
                    _onComplete?.Invoke();
                    _onComplete = null;
                }
                else
                {

                    Debug.Log("2.5D Terrain: The materials in this asset have " +
                        "been created with the " + createdForRenderPipleline.ToString() + " Pipeline. " +
                        "You are using '" + currentRenderPipline.ToString() + "'. The materials for your " +
                        "render pipeline will now be imported.");

                    AssetDatabase.importPackageCompleted -= onPackageImported;
                    AssetDatabase.importPackageCompleted += onPackageImported;

                    // import package
                    Debug.Log("2.5D Terrain: Updating materials by importing '" + package.PackagePath + "'.");
                    AssetDatabase.ImportPackage(package.PackagePath, interactive: false);
                    AssetDatabase.SaveAssets();
                }
            });
        }

        static void onPackageImported(string packageName)
        {
            // Check if it is one of our packages.
            // Abort if not.
            bool isFixerPackage = false;
            foreach (var pkg in Packages)
            {
                if (pkg.PackagePath.Contains(packageName))
                    isFixerPackage = true;
            }
            if (!isFixerPackage)
                return;

            AssetDatabase.importPackageCompleted -= onPackageImported;
            AssetDatabase.SaveAssets();
            _onComplete?.Invoke();
            _onComplete = null;

            Debug.Log("Done");
        }

        public static RenderPiplelineType GetCurrentRenderPiplelineType()
        {
            // Assume URP as default
            var renderPipeline = RenderPiplelineType.URP;

            // check if Standard or HDRP
            if (getUsedRenderPipeline() == null)
                renderPipeline = RenderPiplelineType.BuiltIn; // Standard
            else if (!getUsedRenderPipeline().GetType().Name.Contains("Universal"))
                renderPipeline = RenderPiplelineType.HDRP; // HDRP

            return renderPipeline;
        }

        public static Shader GetDefaultShader()
        {
            if (getUsedRenderPipeline() == null)
                return Shader.Find("Standard");
            else
                return getUsedRenderPipeline().defaultShader;
        }

        public static Shader GetDefaultParticleShader()
        {
            if (getUsedRenderPipeline() == null)
                return Shader.Find("Particles/Standard Unlit");
            else
                return getUsedRenderPipeline().defaultParticleMaterial.shader;
        }

        /// <summary>
        /// Returns the current pipline. Returns NULL if it's the standard render pipeline.
        /// </summary>
        /// <returns></returns>
        static UnityEngine.Rendering.RenderPipelineAsset getUsedRenderPipeline()
        {
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            else
                return UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        }

    }
}
#endif