using UnityEditor;
using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [InitializeOnLoad]
    public static class StickySetup
    {
        #region Public Static Variables
        public static string s3dFolder = "Assets/SCSM/Sticky3DController";
        public static string texturesFolder = "Assets/SCSM/Sticky3DController/Textures";

        // NOTE: The Demos prefab folders may not exist if the user hadsn't imported them or has deleted them.

        public static string demosBeamsFolder = "Assets/SCSM/Sticky3DController/Demos/Prefabs/Beams";
        public static string demosDecalsFolder = "Assets/SCSM/Sticky3DController/Demos/Prefabs/Decals";
        public static string demosDynamicObjectFolder = "Assets/SCSM/Sticky3DController/Demos/Prefabs/DynamicObjects";
        public static string demosEffectsFolder = "Assets/SCSM/Sticky3DController/Demos/Prefabs/Effects";
        public static string demosProjectilesFolder = "Assets/SCSM/Sticky3DController/Demos/Prefabs/Projectiles";
        public static string demosWeaponsFolder = "Assets/SCSM/Sticky3DController/Demos/Prefabs/Weapons";
        #endregion

        #region Public Structures
        public struct PackageCustomInfo { public string version; }
        #endregion

        #region Private Static variables

        private static SerializedObject tagManager;

        #endregion

        #region Constructor
        static StickySetup()
        {
            DefineSymbols();
            UpgradeProject();
        }
        #endregion

        #region Private Static Methods

        /// <summary>
        /// Add the define for Sticky3D so devs can use the following define in their scripts to call S3D methods
        /// #if SCSM_S3D
        ///    // Call S3D APIs
        /// #endif
        /// </summary>
        private static void DefineSymbols()
        {
            const string S3D_Define = "SCSM_S3D";
            const string SSC_Rewired_Define = "SSC_REWIRED";
            const string SCSM_XR_Define = "SCSM_XR";
            System.Type reInputType = null;
            System.Type xrDeviceType = null;

            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            #if UNITY_2023_1_OR_NEWER
            string defines = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));
            #else
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            #endif

            #region S3D
            if (!defines.Contains(S3D_Define))
            {
                if (string.IsNullOrEmpty(defines)) { defines = S3D_Define; }
                else if (!defines.EndsWith(";")) { defines += ";" + S3D_Define; }

                SetDefineSymbols(buildTargetGroup, defines);
            }
            #endregion

            #region New (Unity) Input System

            // This requires U2019.1, scripting runtime version .NET 4.x Equivalent, and the Package for Input System 1.0+
            // For 2019.2+ could use pInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.inputsystem/package.json");
            // and pInfo.version
            // Share SSC_UIS between Sticky3D Controller and Sci-Fi Ship Controller.
            #if UNITY_2019_1_OR_NEWER
            const string SSC_UnityInputSystem_Define = "SSC_UIS";

            string uisPackageVersion = GetPackageVersion("Packages/com.unity.inputsystem/package.json");
           
            // If the package exists, add the define if it is missing
            if (!string.IsNullOrEmpty(uisPackageVersion))
            {
                if (!defines.Contains(SSC_UnityInputSystem_Define))
                {
                    if (string.IsNullOrEmpty(defines)) { defines = SSC_UnityInputSystem_Define; }
                    else if (!defines.EndsWith(";")) { defines += ";" + SSC_UnityInputSystem_Define; }

                    SetDefineSymbols(buildTargetGroup, defines);
                }
            }
            #endif
            #endregion

            #region Rewired
            // Rewired currently does not have a global define. If it is installed, add one, else remove it if it already exists
            // Share SSC_REWIRED between Sticky3D Controller and Sci-Fi Ship Controller.
            try
            {
                //Debug.Log("type: " + typeof(Rewired.ReInput).AssemblyQualifiedName);
                reInputType = System.Type.GetType("Rewired.ReInput, Rewired_Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true, true);
                if (reInputType != null && !defines.Contains(SSC_Rewired_Define))
                {
                    if (string.IsNullOrEmpty(defines)) { defines = SSC_Rewired_Define; }
                    else if (!defines.EndsWith(";")) { defines += ";" + SSC_Rewired_Define; }

                    SetDefineSymbols(buildTargetGroup, defines);
                }
            }
            catch
            {
                //Debug.LogWarning("S3DSetup.DefineSymbols: it appears that Rewired is not installed in this project.");
            }
            #endregion

            #region Unity XR (new AR/VR system)

            try
            {
                xrDeviceType = System.Type.GetType("UnityEngine.XR.XRDevice, UnityEngine.VRModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", true, true);

                if (xrDeviceType != null && !defines.Contains(SCSM_XR_Define))
                {
                    if (string.IsNullOrEmpty(defines)) { defines = SCSM_XR_Define; }
                    else if (!defines.EndsWith(";")) { defines += ";" + SCSM_XR_Define; }

                    // SCSM is only going to support 2020.3+ although some providers may work in 2019.4 too.
                    #if UNITY_2020_3_OR_NEWER
                    SetDefineSymbols(buildTargetGroup, defines);
                    #endif
                }
            }
            catch { }

            #endregion
        }


        private static void SetDefineSymbols(BuildTargetGroup buildTargetGroup, string defines)
        {
            #if UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), defines);
            #else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            #endif
        }

        // Upgrade project folders, rename and move anything to correct location
        private static void UpgradeProject()
        {

            // This just causes GUID conflicts when importing S3DUIAim01-03 into existing project (instead just leave old ones there)
            //StickySetup.RenameAsset("StickySetup.UpgradeProject", texturesFolder + "/Display", "S3DUIAim1.png", "S3DUIAim01.png");
            //StickySetup.RenameAsset("StickySetup.UpgradeProject", texturesFolder + "/Display", "S3DUIAim2.png", "S3DUIAim02.png");
            //StickySetup.RenameAsset("StickySetup.UpgradeProject", texturesFolder + "/Display", "S3DUIAim3.png", "S3DUIAim03.png");
        }

        #endregion

        #region Public Static Methods

        public static void FindTagAndLayerManager()
        {
            tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        }

        /// <summary>
        /// Create a new Unity Layer. Optionally overwrite existing layer if one already exists.
        /// </summary>
        /// <param name="newLayers"></param>
        /// <param name="newlayerNumbers"></param>
        /// <param name="overwriteExisting"></param>
        public static void CreateLayers(string[] newLayers, int[] newlayerNumbers, bool overwriteExisting = false)
        {
            if (tagManager != null)
            {
                // Get the array of existing layers
                SerializedProperty layers = tagManager.FindProperty("layers");
                if (layers != null)
                {
                    if (layers.isArray)
                    {
                        for (int l = 0; l < newlayerNumbers.Length; l++)
                        {
                            SerializedProperty layerSP = layers.GetArrayElementAtIndex(newlayerNumbers[l]);
                            if ((overwriteExisting || string.IsNullOrEmpty(layerSP.stringValue)) && layerSP.stringValue != newLayers[l])
                            {
                                Debug.Log("Adding layer " + newlayerNumbers[l].ToString() + ": " + newLayers[l]);
                                layerSP.stringValue = newLayers[l];
                            }
                        }

                        // Apply the modifications
                        tagManager.ApplyModifiedProperties();
                    }
                    else { Debug.LogWarning("StickySetup - Layers Serialized Property is not in the expected format (array), so layers could not be created."); }
                }
                else { Debug.LogWarning("StickySetup - Layers Serialized Property is null, so layers could not be created"); }
            }
            else { Debug.LogWarning("StickySetup - TagManager.asset could not be found, so layers could not be created."); }
        }

        /// <summary>
        /// Get the version number for a package. Typically the package can be found in the "Packages"
        /// virtual folder. Will return an empty string if the package does not exist in the project.
        /// USAGE: string packageVersion = GetPackageVersion("Packages/com.unity.inputsystem/package.json");
        /// </summary>
        /// <returns></returns>
        public static string GetPackageVersion(string packagePath)
        {
            #if UNITY_2019_2_OR_NEWER
            UnityEditor.PackageManager.PackageInfo pInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(packagePath);
            if (pInfo != null) { return pInfo.version; }
            else { return string.Empty; }
            #else
            if (System.IO.File.Exists(packagePath))
            {
                string packageJSON = System.IO.File.ReadAllText(packagePath);

                if (!string.IsNullOrEmpty(packageJSON))
                {
                    PackageCustomInfo info = JsonUtility.FromJson<PackageCustomInfo>(packageJSON);
                    return info.version;
                }
                else { return string.Empty; }
            }
            else { return string.Empty;}
            #endif
        }

        /// <summary>
        /// Rename a file if it exists and the new file does not exist
        /// USAGE: StickySetup.RenameAsset("StickySetup.UpgradeProject", "Assets/SciFiShipController/SRP", "SSC_HDRP.unitypackage", "SSC_HDRP_5.13.0.unitypackage");
        /// </summary>
        /// <param name="sourceMethod"></param>
        /// <param name="filePath"></param>
        /// <param name="oldFileName"></param>
        /// <param name="newFilename"></param>
        public static void RenameAsset(string sourceMethod, string filePath, string oldFileName, string newFilename)
        {
            if (!System.IO.File.Exists(filePath + "/" + newFilename) && System.IO.File.Exists(filePath + "/" + oldFileName))
            {
                string status = AssetDatabase.RenameAsset(filePath + "/" + oldFileName, newFilename);
                if (status != "") { Debug.LogWarning(sourceMethod + " - " + status); }
                else { Debug.Log(sourceMethod + " - renamed " + filePath + "/" + oldFileName + " to " + newFilename); }
            }
        }

        #endregion
    }
}