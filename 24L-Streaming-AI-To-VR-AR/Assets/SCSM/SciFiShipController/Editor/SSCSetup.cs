using UnityEditor;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    // Script that performs miscellaneous setup tasks for Sci-Fi Ship Controller - runs when the project is opened in the Unity editor
    // See also SSCDemoSetup

    [InitializeOnLoad]
    public static class SSCSetup
    {
        #region Public Static Variables

        public static string sscFolder = "Assets/SCSM/SciFiShipController";
        public static string materialsFolder = "Assets/SCSM/SciFiShipController/Materials";
        public static string texturesFolder = "Assets/SCSM/SciFiShipController/Textures";
        public static string modelsFolder = "Assets/SCSM/SciFiShipController/Models";
        public static string effectsFolder = "Assets/SCSM/SciFiShipController/Prefabs/Effects";
        public static string destructFolder = "Assets/SCSM/SciFiShipController/Prefabs/Destructs";
        public static string projectilesFolder = "Assets/SCSM/SciFiShipController/Prefabs/Projectiles";
        public static string beamsFolder = "Assets/SCSM/SciFiShipController/Prefabs/Beams";

        public static string demoMaterialsFolder = "Assets/SCSM/SciFiShipController/Demos/Materials";
        public static string demoModelsFolder = "Assets/SCSM/SciFiShipController/Demos/Models";
        public static string demoScriptsFolder = "Assets/SCSM/SciFiShipController/Demos/Scripts";

        #endregion

        #region Public Structures
        public struct PackageCustomInfo { public string version; }
        #endregion

        #region Private Static variables

        private static SerializedObject tagManager;

        #endregion

        #region Constructor
        static SSCSetup()
        {
            // See SSCDemoSetup for adding the SSC Celestials layer.
            // This was moved into SSCDemoSetup for the scenario when the Demo folder is not imported.
            //int[] layerNumbersToAdd = { Celestials.celestialsUnityLayer };
            //string[] layersToAdd = { "SSC Celestials" };

            FindTagAndLayerManager();
            //CreateLayers(layersToAdd, layerNumbersToAdd);

            DefineSymbols();

            UpgradeProject();
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Add the define for SSC so devs can use the following define in their scripts to call SSC methods
        /// #if SCSM_SSC
        ///    // Call SSC APIs
        /// #endif
        /// </summary>
        private static void DefineSymbols()
        {
            const string SSC_Define = "SCSM_SSC";
            const string SSC_Rewired_Define = "SSC_REWIRED";
            const string SSC_OVR_Define = "SSC_OVR";  // Oculus VR
            const string SCSM_XR_Define = "SCSM_XR";
            System.Type reInputType = null;
            System.Type ovrInputType = null;
            System.Type xrDeviceType = null;

            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            #if UNITY_2023_1_OR_NEWER
            string defines = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));
            #else
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            #endif

            #region SSC
            if (!defines.Contains(SSC_Define))
            {
                if (string.IsNullOrEmpty(defines)) { defines = SSC_Define; }
                else if (!defines.EndsWith(";")) { defines += ";" + SSC_Define; }

                SetDefineSymbols(buildTargetGroup, defines);
            }
            #endregion

            #region Rewired
            // Rewired currently does not have a global define. If it is installed, add one, else remove it if it already exists

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
                //Debug.LogWarning("SSCSetup.DefineSymbols: it appears that Rewired is not installed in this project.");
            }
            #endregion

            #region Oculus API
            //Debug.Log("OVR: " + typeof(OVRInput).AssemblyQualifiedName);

            // There are currently 2 known Oculus API versions.
            //  Assembly-CSharp, Version=1.0.0.0
            //  Oculus.VR, Version=0.0.0.0

            try
            {
                ovrInputType = System.Type.GetType("OVRinput, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true, true);

                if (ovrInputType != null && !defines.Contains(SSC_OVR_Define))
                {
                    if (string.IsNullOrEmpty(defines)) { defines = SSC_OVR_Define; }
                    else if (!defines.EndsWith(";")) { defines += ";" + SSC_OVR_Define; }

                    SetDefineSymbols(buildTargetGroup, defines);
                }
            }
            catch { }

            try
            {
                if (ovrInputType == null)
                {
                    ovrInputType = System.Type.GetType("OVRinput, Oculus.VR, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", true, true);
                    if (ovrInputType != null && !defines.Contains(SSC_OVR_Define))
                    {
                        if (string.IsNullOrEmpty(defines)) { defines = SSC_OVR_Define; }
                        else if (!defines.EndsWith(";")) { defines += ";" + SSC_OVR_Define; }

                        SetDefineSymbols(buildTargetGroup, defines);
                    }
                }
            }
            catch { }
            #endregion

            // Rewired is not installed, but there is a SSC-created global define, so remove it
            // NOTE: Unfortunately this doesn't work because now the PlayerInputModuleEditor won't compile
            if (reInputType == null && defines.Contains(SSC_Rewired_Define))
            {
                defines = defines.Replace(";" + SSC_Rewired_Define, "");
                defines = defines.Replace(SSC_Rewired_Define + ";", "");
                defines = defines.Replace(SSC_Rewired_Define, "");
                SetDefineSymbols(buildTargetGroup, defines);
            }

            #region New (Unity) Input System

            // This requires U2019.1, scripting runtime version .NET 4.x Equivalent, and the Package for Input System 1.0+
            // For 2019.2+ could use pInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.inputsystem/package.json");
            // and pInfo.version
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

            #region Entities U2019.1 or newer

            #if UNITY_2019_1_OR_NEWER
            // Due to changes in Entities, this requires Unity 2019.1 or newer
            const string SSC_Entity_Define = "SSC_ENTITIES";
            string entitiesPackageVersion = GetPackageVersion("Packages/com.unity.entities/package.json");
            
            // ECS 1.0.0 renamed Hybrid Renderer V2 to Entities Graphics and moved it to the Entities namespace.
            // ECS 1.0.0 replaced 0.51.x in U2022.2.
            #if UNITY_2022_2_OR_NEWER
            string entitiesRendererPackageVersion = GetPackageVersion("Packages/com.unity.entities.graphics/package.json");
            #else
            string entitiesRendererPackageVersion = GetPackageVersion("Packages/com.unity.rendering.hybrid/package.json");
            #endif

            // If editor scripts exist, add the Editor define if it is missing
            if (!string.IsNullOrEmpty(entitiesPackageVersion) && !string.IsNullOrEmpty(entitiesRendererPackageVersion))
            {
                // Debug.Log("[DEBUG] entities: " + entitiesPackageVersion + " entities renderer: " + entitiesRendererPackageVersion);

                if (!defines.Contains(SSC_Entity_Define))
                {
                    if (string.IsNullOrEmpty(defines)) { defines = SSC_Entity_Define; }
                    else if (!defines.EndsWith(";")) { defines += ";" + SSC_Entity_Define; }

                    SetDefineSymbols(buildTargetGroup, defines);
                }
            }
            #endif
            #endregion

            #region Unity.Physics U2019.3 or newer
            #if UNITY_2019_3_OR_NEWER
            const string SSC_Physics_Define = "SSC_PHYSICS";
            string physicsPackageVersion = GetPackageVersion("Packages/com.unity.physics/package.json");

            // If the package exists, add the define if it is missing (currently only support v0.2.4.
            // Version 0.5.1-preview.32 needs an upgrade from IJobForEach to IJobEntity. See ProjectileSystem.cs
            if (!string.IsNullOrEmpty(physicsPackageVersion) && SSCUtils.CompareVersionNumbers(physicsPackageVersion, "0.2.5") < 0)
            {
                if (!defines.Contains(SSC_Physics_Define))
                {
                    if (string.IsNullOrEmpty(defines)) { defines = SSC_Physics_Define; }
                    else if (!defines.EndsWith(";")) { defines += ";" + SSC_Physics_Define; }

                    SetDefineSymbols(buildTargetGroup, defines);
                }
            }
            #endif
            #endregion

            #region URP 7.3.1 or newer
#if UNITY_2019_3_OR_NEWER
            // URP 7.3.1+ with U2019.4+ supports stacked cameras which is used in Celestials.
            const string SSC_URP73_Define = "SSC_URP";

            string urpPackageVersion = GetPackageVersion("Packages/com.unity.render-pipelines.universal/package.json");

            if (!string.IsNullOrEmpty(urpPackageVersion))
            {
                if (!defines.Contains(SSC_URP73_Define) && SSCUtils.CompareVersionNumbers(urpPackageVersion, "7.3.1") >= 0)
                {
                    if (string.IsNullOrEmpty(defines)) { defines = SSC_URP73_Define; }
                    else if (!defines.EndsWith(";")) { defines += ";" + SSC_URP73_Define; }

                    SetDefineSymbols(buildTargetGroup, defines);
                }
            }
#endif
            #endregion

            #region HDRP 7.3.1 or newer
#if UNITY_2019_3_OR_NEWER
            // HDRP 7.3.1+ support with U2019.4 is used in Celestials.
            const string SSC_HDRP73_Define = "SSC_HDRP";

            string hdrpPackageVersion = GetPackageVersion("Packages/com.unity.render-pipelines.high-definition/package.json");

            if (!string.IsNullOrEmpty(hdrpPackageVersion))
            {
                if (!defines.Contains(SSC_HDRP73_Define) && SSCUtils.CompareVersionNumbers(hdrpPackageVersion, "7.3.1") >= 0)
                {
                    if (string.IsNullOrEmpty(defines)) { defines = SSC_HDRP73_Define; }
                    else if (!defines.EndsWith(";")) { defines += ";" + SSC_HDRP73_Define; }

                    SetDefineSymbols(buildTargetGroup, defines);
                }
            }
#endif
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

        // Upgrade project folders and move anything to correct location
        private static void UpgradeProject()
        {
            // Move stars into Environment folder
            MoveAsset("SSCSetup.UpgradeProject", demoModelsFolder, demoModelsFolder + "/Environment", "StarLowPolyFBX.fbx");

            // Move demo models from main models folder to demos
            MoveAsset("SSCSetup.UpgradeProject", modelsFolder + "/Environment", demoModelsFolder + "/Environment", "AGTrack1.fbx");
            MoveAsset("SSCSetup.UpgradeProject", modelsFolder + "/Environment", demoModelsFolder + "/Environment", "FlatCityscape1.fbx");
            MoveAsset("SSCSetup.UpgradeProject", modelsFolder + "/Environment", demoModelsFolder + "/Environment", "RingCityscape1.fbx");
            MoveAsset("SSCSetup.UpgradeProject", modelsFolder + "/Environment", demoModelsFolder + "/Environment", "SpaceRingsTrack1.fbx");

            // Move stars material into Environment folder
            MoveAsset("SSCSetup.UpgradeProject", demoMaterialsFolder, demoMaterialsFolder + "/Environment", "LBStar.mat");

            // Move demo materials from main materials folder to demos
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Building_Wall_1.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Building_Wall_2.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Building_Window.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Floating_City_Base.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Ground_sand.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Metal_Tech.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Sign_Colour_1.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Sign_Colour_2.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Sign_Post.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_SSC_Skybox.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Track_Blue.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Track_Glass_Barrier.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Track_Grey.mat");
            MoveAsset("SSCSetup.UpgradeProject", materialsFolder + "/Environment", demoMaterialsFolder + "/Environment", "SCSM_Track_Orange.mat");

            DeleteAsset(demoScriptsFolder + "/EndlessFlierDemoScript.cs");

            RenameAsset("SSCSetup.UpgradeProject", sscFolder + "/SRP", "SSC_HDRP.unitypackage", "SSC_HDRP_5.13.0.unitypackage");
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
        /// <param name=""></param>
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
                    else { Debug.LogWarning("Layers Serialized Property is not in the expected format (array), so layers could not be created."); }
                }
                else { Debug.LogWarning("Layers Serialized Property is null, so layers could not be created"); }
            }
            else { Debug.LogWarning("TagManager.asset could not be found, so layers could not be created."); }
        }

        /// <summary>
        /// Move a file from one folder to another in the Project Hierarchy
        /// Keep the meta-data the same so as not to break linked assets in scenes
        /// USAGE: SSCSetup.MoveAsset("SSCSetup.UpgradeProject", materialsFolder, materialsFolder + "/Resources", "LBMoon.mat");
        /// </summary>
        /// <param name="sourceMethod"></param>
        /// <param name="sourceFolder"></param>
        /// <param name="destFolder"></param>
        /// <param name="fileName"></param>
        public static void MoveAsset(string sourceMethod, string sourceFolder, string destFolder, string fileName)
        {
            string sourcePath = sourceFolder + (sourceFolder.EndsWith("/") ? "" : "/") + fileName;
            string destPath = destFolder + (destFolder.EndsWith("/") ? "" : "/") + fileName;

            //Debug.Log("[DEBUG] Source: " + sourcePath + " dest: " + destPath);

            if (System.IO.File.Exists(sourcePath))
            {
                CheckFolder(destFolder);

                if (!AssetDatabase.IsValidFolder(destFolder))
                {
                    Debug.Log(sourceMethod + " could not move " + fileName + " because the destination folder isn't in asset database: " + destFolder);
                }
                else if (!System.IO.File.Exists(destPath))
                {
                    Debug.Log(sourceMethod + " - moving " + fileName + " from " + sourceFolder + " to " + destFolder + "...");
                    string status = AssetDatabase.MoveAsset(sourcePath, destPath);
                    if (status != "") { Debug.LogWarning("SSCSetup.UpgradeProject - " + status); }
                    else { Debug.Log(sourceMethod + " - moved " + fileName + " from " + sourceFolder + " to " + destFolder + "."); }
                }
            }
        }

        /// <summary>
        /// Delete a file from the Project Hierarchy
        /// USAGE: SSCSetup.DeleteAsset("Assets/SCSM/SciFiShipController/Demos/Scripts/myfile.cs");
        /// </summary>
        /// <param name="filepath"></param>
        public static void DeleteAsset(string filepath)
        {
            if (System.IO.File.Exists(filepath))
            {
                AssetDatabase.DeleteAsset(filepath);
            }
        }

        /// <summary>
        /// Rename a file if it exists and the new file does not exist
        /// USAGE: SSCSetup.RenameAsset("SSCSetup.UpgradeProject", "Assets/SciFiShipController/SRP", "SSC_HDRP.unitypackage", "SSC_HDRP_5.13.0.unitypackage");
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

        // Check folder is valid. If it is missing, create it.
        // Does not check for multiple folder levels. Just the last one
        public static void CheckFolder(string folderPath)
        {
            // CreateFolder doesn't work immediately. This can result it it seeming to be invalid immediately after calling CreateFolder.
            // Check the filesystem too, so that we don't create multiple folders with a the next sequential number appended to the folder name.
            if (!AssetDatabase.IsValidFolder(folderPath) && !System.IO.Directory.Exists(folderPath))
            {
                int i = folderPath.LastIndexOf('/');

                if (i >= 0 && i < folderPath.Length - 2)
                {
                    Debug.Log("INFO SSCSetup - Creating new folder " + folderPath.Substring(i + 1) + " in " + folderPath.Substring(0, i));

                    // NOTE: This doesn't have an immediate effect. SaveAssets() and Refresh() have no effect.
                    AssetDatabase.CreateFolder(folderPath.Substring(0, i), folderPath.Substring(i + 1));
                }
            }
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


        #endregion

    }
}
