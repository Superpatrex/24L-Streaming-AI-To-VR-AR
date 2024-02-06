using System;
using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2015-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// General purpose utility methods for Sticky 3D Controller
    /// </summary>
    public class S3DUtils
    {
        #region Animation

        /// <summary>
        /// Used with avatar muscles
        /// </summary>
        public enum S3DAxis
        {
            x = 0,
            y = 1,
            z = 2
        }

        /// <summary>
        /// Get the (default) min limits of the hand bones in degrees.
        /// </summary>
        /// <param name="humanBodyBone"></param>
        /// <returns></returns>
        public static Vector3 GetHandMuscleMinLimits(HumanBodyBones humanBodyBone)
        {
            // Get the hand limits
            // x-axis is not defined for hands (DoF 0)
            // y-axis is down-up (DoF 1)
            // z-axis is in-out (DoF 2)
            // HumanTrait.MuscleFromBone returns -1 when it doesn't apply to this bone.
            return new Vector3(
                -180f,
                HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone((int)humanBodyBone, 1)),
                HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone((int)humanBodyBone, 2))
                );
        }

        /// <summary>
        /// Get the (default) max limits of the hand bones in degrees
        /// </summary>
        /// <param name="humanBodyBone"></param>
        /// <returns></returns>
        public static Vector3 GetHandMuscleMaxLimits(HumanBodyBones humanBodyBone)
        {
            // Get the hand limits
            // x-axis is not defined for hands (DoF 0)
            // y-axis is down-up (DoF 1)
            // z-axis is in-out (DoF 2)
            // HumanTrait.MuscleFromBone returns -1 when it doesn't apply to this bone.
            return new Vector3(
                180f,
                HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone((int)humanBodyBone, 1)),
                HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone((int)humanBodyBone, 2))
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="axisDoF"></param>
        /// <returns></returns>
        public static int GetMuscleIndex(HumanBodyBones bone, S3DAxis axisDoF)
        {
            return HumanTrait.MuscleFromBone((int)bone, (int)axisDoF);
        }

        /// <summary>
        /// Get a list of the bones for this Avatar rig. Always returns
        /// null if Unity 2018.4 or older.
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        public static HumanBone[] GetHumanBones(Avatar avatar)
        {
            HumanBone[] humanBones = null;

            #if UNITY_2019_1_OR_NEWER
            if (avatar != null && avatar.isHuman)
            {
                HumanDescription humanDescription = avatar.humanDescription;

                humanBones = humanDescription.human;
            }
            #endif

            return humanBones;
        }

        /// <summary>
        /// Attempt to propulate a non-null list for transforms, with the
        /// bone transforms in a humanoid rig, given the animator.
        /// Will return false if this is not a humanoid or the list is null.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="boneTransformList"></param>
        /// <returns></returns>
        public static bool GetHumanBones (Animator animator, List<Transform> boneTransformList)
        {
            if (boneTransformList != null && animator != null && animator.isHuman)
            {
                HumanBodyBones[] boneArray = (HumanBodyBones[])Enum.GetValues(typeof(HumanBodyBones));

                for (int bnIdx = 0; bnIdx < boneArray.Length; bnIdx++)
                {
                    Transform boneTrfm = animator.GetBoneTransform(boneArray[bnIdx]);
                    if (boneTrfm != null) { boneTransformList.Add(boneTrfm); }
                }
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Attempt to propulate a non-null list of S3DHumanBone, with the
        /// bones in a humanoid rig, given the animator.
        /// Will return false if this is not a humanoid or the list is null.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="boneList"></param>
        /// <returns></returns>
        public static bool GetHumanBones (Animator animator, List<S3DHumanBone> boneList)
        {
            if (boneList != null && animator != null && animator.isHuman)
            {
                HumanBodyBones[] boneArray = (HumanBodyBones[])Enum.GetValues(typeof(HumanBodyBones));

                for (int bnIdx = 0; bnIdx < boneArray.Length; bnIdx++)
                {
                    if (boneArray[bnIdx] == HumanBodyBones.LastBone) { break; }

                    Transform boneTrfm = animator.GetBoneTransform(boneArray[bnIdx]);
                    if (boneTrfm != null)
                    {
                        boneList.Add(new S3DHumanBone()
                        {
                            boneTransform = boneTrfm,
                            bone = boneArray[bnIdx],
                            isValid = true
                        });
                    }
                }
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// In the editor console, output a list of bone indexes and bones names
        /// </summary>
        public static void DebugHumanBoneNames()
        {
            string[] boneNames = HumanTrait.BoneName;

            for (int bIdx = 0; bIdx < HumanTrait.BoneCount; bIdx++)
            {
                Debug.Log(bIdx + " " + boneNames[bIdx]);
            }
        }

        /// <summary>
        /// In the editor console, output a list of muscle indexes and muscle names.
        /// The number of muscles seems to be the same as number of bones.
        /// </summary>
        public static void DebugHumanMuscleNames()
        {
            string[] muscleNames = HumanTrait.MuscleName;

            for (int mIdx = 0; mIdx < HumanTrait.MuscleCount; mIdx++)
            {
                Debug.Log(mIdx + " " + muscleNames[mIdx]);
            }
        }

        /// <summary>
        /// In the editor console, output a list of bones and their rotation limits.
        /// Requires U2019.1+. The limits seem to be only non-zero after each one
        /// is changed in the editor model importer under Configure (even in 2020.3.0f1).
        /// Seem to have very limited value. Also are not honoured by OnAnimatorIK().
        /// </summary>
        /// <param name="avatar"></param>
        public static void DebugHumanBoneLimits(Avatar avatar)
        {
            #if UNITY_EDITOR
            HumanBone[] humanBones = GetHumanBones(avatar);

            if (humanBones != null)
            {
                int numBones = humanBones.Length;

                for (int bnIdx = 0; bnIdx < numBones; bnIdx++)
                {
                    HumanBone humanBone = humanBones[bnIdx];
                    Debug.Log(
                    "Muscle ID: " + bnIdx +
                    "\n Rig Bone: " + humanBone.boneName +
                    "\n Mecanim Bone: " + humanBone.humanName + 
                    "\n Rotation Limit (Min) " + humanBone.limit.min +
                    "\n Rotation Limit (Centre) " + humanBone.limit.center +
                    "\n Rotation Limit (Max) " + humanBone.limit.max
                    );
                }
            }
            else { Debug.LogWarning("S3DUtils.DebugHumanBoneLimits - no human bones found"); }

            #endif
        }
        #endregion

        #region Array Methods

        /// <summary>
        /// Sort an array of RaycastHits in ascending order.
        /// Typically used with Physics.RaycastNonAlloc(..).
        /// </summary>
        /// <param name="hits"></param>
        /// <param name="numHits"></param>
        public static void SortHitsAsc(RaycastHit[] hits, int numHits)
        {
            int arrayLen = hits.Length;

            // Physics.RaycastNonAlloc does not reset the structs that are not updated
            // in the array. So, in order to sort the list we need to update those ourselves.
            for (int rIdx = numHits; rIdx < arrayLen; rIdx++)
            {
                hits[rIdx].distance = float.MaxValue;
            }

            Array.Sort(hits, delegate (RaycastHit x, RaycastHit y)
            {
                // If x > y return 1, if x < y return -1,  if x == y return 0 (this is equivalent to x.CompareTo(y))

                if (x.distance > y.distance) { return 1; }
                else if (x.distance < y.distance) { return -1; }
                // With floats, x == y is less likely, so do last.
                else { return 0; }
            });
        }

        #endregion

        #region Colour Methods

        /// <summary>
        /// Update one colour with the other. Same as dest = source.
        /// When ifChanged = true, the update will only occur if the source and dest have different values.
        /// </summary>
        /// <param name="sourceColour"></param>
        /// <param name="destColour"></param>
        /// <param name="destSSCColour"></param>
        /// <param name="ifChanged"></param>
        public static void UpdateColour(ref Color32 sourceColour, ref Color32 destColour, ref S3DColour destSSCColour,  bool ifChanged)
        {
            // if ifChanged is false OR the source and dest values are different, update the dest colour
            if (!ifChanged || destColour.r != sourceColour.r || destColour.g != sourceColour.g || destColour.b != sourceColour.b || destColour.a != sourceColour.a)
            {
                destColour.r = sourceColour.r;
                destColour.g = sourceColour.g;
                destColour.b = sourceColour.b;
                destColour.a = sourceColour.a;

                destSSCColour.Set(destColour.r, destColour.g, destColour.b, destColour.a, true);
            }
        }

        /// <summary>
        /// Update one colour with the other. Same as dest = source without the implicit conversion.
        /// When ifChanged = true, the update will only occur if the source and dest have different values.
        /// </summary>
        /// <param name="sourceColour"></param>
        /// <param name="destColour"></param>
        /// <param name="destSSCColour"></param>
        /// <param name="ifChanged"></param>
        public static void UpdateColour(ref Color sourceColour, ref Color32 destColour, ref S3DColour destSSCColour, bool ifChanged)
        {
            byte srcR = (byte)(sourceColour.r * 255f);
            byte srcG = (byte)(sourceColour.g * 255f);
            byte srcB = (byte)(sourceColour.b * 255f);
            byte srcA = (byte)(sourceColour.a * 255f);

            // if ifChanged is false OR the source and dest values are different, update the dest colour
            if (!ifChanged || destColour.r != srcR || destColour.g != srcG || destColour.b != srcB || destColour.a != srcA)
            {
                destColour.r = srcR;
                destColour.g = srcG;
                destColour.b = srcB;
                destColour.a = srcA;

                destSSCColour.Set(destColour.r, destColour.g, destColour.b, destColour.a, true);
            }
        }

        /// <summary>
        /// Update one colour with the other. Same as dest = source without the implicit conversion.
        /// When ifChanged = true, the update will only occur if the source and dest have different values.
        /// </summary>
        /// <param name="sourceColour"></param>
        /// <param name="destColour"></param>
        /// <param name="destSSCColour"></param>
        /// <param name="ifChanged"></param>
        public static void UpdateColour(ref Color sourceColour, ref Color destColour, ref S3DColour destSSCColour, bool ifChanged)
        {
            // if ifChanged is false OR the source and dest values are different, update the dest colour
            if (!ifChanged || destColour.r != sourceColour.r || destColour.g != sourceColour.g || destColour.b != sourceColour.b || destColour.a != sourceColour.a)
            {
                destColour.r = sourceColour.r;
                destColour.g = sourceColour.g;
                destColour.b = sourceColour.b;
                destColour.a = sourceColour.a;

                destSSCColour.Set(destColour.r, destColour.g, destColour.b, destColour.a, true);
            }
        }

        /// <summary>
        /// Attempt to copy a struct Color32 to a struct Color without allocating any memory
        /// </summary>
        /// <param name="sourceColour"></param>
        /// <param name="destColour"></param>
        public static void Color32toColorNoAlloc(ref Color32 sourceColour, ref Color destColour)
        {
            // Avoid creating a new Color struct by not doing an implicit conversion.
            destColour.r = sourceColour.r / 255f;
            destColour.g = sourceColour.g / 255f;
            destColour.b = sourceColour.b / 255f;
            destColour.a = sourceColour.a / 255f;
        }

        /// <summary>
        /// Attempt to copy a struct Color to a struct Color without allocating any memory
        /// </summary>
        /// <param name="sourceColour"></param>
        /// <param name="destColour"></param>
        public static void ColortoColorNoAlloc(ref Color sourceColour, ref Color destColour)
        {
            // Avoid creating a new Color struct.
            destColour.r = sourceColour.r;
            destColour.g = sourceColour.g;
            destColour.b = sourceColour.b;
            destColour.a = sourceColour.a;
        }

        #endregion

        #region Json Methods

        /// <summary>
        /// Save a list to Json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson<T>(List<T> list)
        {
            ListWrapper<T> listWrapper = new ListWrapper<T>();
            listWrapper.itemList = list;
            return JsonUtility.ToJson(listWrapper);
        }

        /// <summary>
        /// Convert json into a List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static List<T> FromJson<T>(string jsonText)
        {
            ListWrapper<T> listWrapper = JsonUtility.FromJson<ListWrapper<T>>(jsonText);
            return listWrapper.itemList;
        }

        #endregion

        #region Material Methods

        /// <summary>
        /// Configure the colour as transparent with the given colour. Use Fade
        /// to make a hologram kind of appearance.
        /// Assumes the material is not null.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="colour"></param>
        public static void ConfigureFadeMaterial (Material material, Color colour)
        {
            material.SetOverrideTag("RenderType", "Transparent");

            // Configure the blending state
            material.SetFloat(ShaderPropertyID.mode, 2f);
            material.SetInt(ShaderPropertyID.srcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt(ShaderPropertyID.dstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt(ShaderPropertyID.zWrite, 0);

            // Turn off unwanted local shader keywords
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            // Enable alpha blending
            material.EnableKeyword("_ALPHABLEND_ON");

            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            // If URP or HDRP, use baseColor. For Built-in RP use color.
            material.SetColor(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline ? ShaderPropertyID.baseColor : ShaderPropertyID.color, colour);
        }

        /// <summary>
        /// Attempt to get a default built-in material for the current render pipeline
        /// </summary>
        /// <returns></returns>
        public static Material GetDefaultMaterial()
        {
            var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;

            if (rp != null) { return rp.defaultMaterial; }
            else
            {
                // Built in render pipeline
                System.Reflection.MethodInfo methodInfo = ReflectionGetMethod(typeof(Material), "GetDefaultMaterial", true, true);

                if (methodInfo != null)
                {
                    return (Material)methodInfo.Invoke(null, null);
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[ERROR] S3DUtil: Could not get GetDefaultMaterial");
                    #endif

                    return null;
                }
            }
        }

        /// <summary>
        /// Attempt to get a default built-in line material for the current render pipeline
        /// </summary>
        /// <returns></returns>
        public static Material GetDefaultLineMaterial()
        {
            var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;

            if (rp != null) { return rp.defaultLineMaterial; }
            else
            {
                // Built in render pipeline
                System.Reflection.MethodInfo methodInfo = ReflectionGetMethod(typeof(Material), "GetDefaultLineMaterial", true, true);

                if (methodInfo != null)
                {
                    return (Material)methodInfo.Invoke(null, null);
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[ERROR] S3DUtil: Could not get GetDefaultLineMaterial");
                    #endif

                    return null;
                }
            }
        }

        /// <summary>
        /// Attempt to get a default built-in particle material for the current render pipeline
        /// </summary>
        /// <returns></returns>
        public static Material GetDefaultParticleMaterial()
        {
            var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;

            if (rp != null) { return rp.defaultParticleMaterial; }
            else
            {
                // Built in render pipeline
                System.Reflection.MethodInfo methodInfo = ReflectionGetMethod(typeof(Material), "GetDefaultParticleMaterial", true, true);

                if (methodInfo != null)
                {
                    return (Material)methodInfo.Invoke(null, null);
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[ERROR] S3DUtil: Could not get GetDefaultParticleMaterial");
                    #endif

                    return null;
                }
            }
        }

        #endregion

        #region Misc Methods

        /// <summary>
        /// Create a simple sphere at a world space position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="includeCollider"></param>
        /// <returns></returns>
        public static GameObject CreateSphere(Vector3 position, float scale, bool includeCollider)
        {
            GameObject sphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (sphereGO != null)
            {
                if (!includeCollider)
                {
                    Collider goCollider;
                    if (sphereGO.TryGetComponent(out goCollider))
                    {
                        #if UNITY_EDITOR
                        GameObject.DestroyImmediate(goCollider);
                        #else
                        GameObject.Destroy(goCollider);
                        #endif
                    }
                }
                sphereGO.transform.position = position;
                sphereGO.transform.localScale *= scale > Mathf.Epsilon ? scale : 1f;
            }
            return sphereGO;
        }


        /// <summary>
        /// Verify a display (monitor or screen) and if required, activate it.
        /// Multiple displays can be activated (once) and then cannot be de-activated for the current session.
        /// Currently turning it off doesn't work due to a Unity limitation with additional displays...
        /// In the editor Display.displays will always return 1. So as a workaround, go to Game view, click
        /// Add Tab->Game view and move it to the second monitor. Then, on second Game view, set Display to Display 2.
        /// In the editor, with 2+ Game views, Maximise On Play will have no effect.
        /// </summary>
        /// <param name="displayNumber">Range 1 to 8</param>
        /// <param name="activateIfRequired"></param>
        /// <returns></returns>
        public static bool VerifyTargetDisplay(int displayNumber, bool activateIfRequired)
        {
            bool isValid = false;

            #if UNITY_EDITOR
            isValid = displayNumber > 0 && displayNumber < 9;
            #else

            if (displayNumber > 0 && Display.displays.Length >= displayNumber)
            {
                // If the display is not active, activate it now
                if (activateIfRequired && !Display.displays[displayNumber-1].active) { Display.displays[displayNumber-1].Activate(); }
                isValid = true;
            }
            #endif

            return isValid;
        }

        #endregion

        #region Reflection

        /// <summary>
        /// Get a method so that it can be invoked, given the type of class it is in and its method name
        /// </summary>
        /// <param name="t"></param>
        /// <param name="methodName"></param>
        /// <param name="includePrivate"></param>
        /// <param name="includeStatic"></param>
        /// <returns></returns>
        public static System.Reflection.MethodInfo ReflectionGetMethod(Type t, string methodName, bool includePrivate, bool includeStatic)
        {
            if (string.IsNullOrEmpty(methodName)) { return null; }

            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !includeStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (includeStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (includeStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            return t.GetMethod(methodName, bindingFlags);
        }

        /// <summary>
        /// Get a field value of the given type from the class. If the field is not static, an instance of the class must be supplied.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classType"></param>
        /// <param name="fieldName"></param>
        /// <param name="classInstance"></param>
        /// <param name="includePrivate"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        public static T ReflectionGetValue<T>(System.Type classType, string fieldName, UnityEngine.Object classInstance, bool includePrivate, bool isStatic)
        {
            // Some types may not be nullable, so instead set to the default value for that type.
            T value = default(T);

            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !isStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (isStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (isStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            System.Reflection.FieldInfo fieldInfo = typeof(T).GetField(fieldName, bindingFlags);

            if (fieldInfo != null)
            {
                if (isStatic) { value = (T)fieldInfo.GetValue(null); }
                else { value = (T)fieldInfo.GetValue(classInstance); }
            }

            return value;
        }

        /// <summary>
        /// Get a property value of the given type from the class. If the property is not static, an instance of the class must be supplied.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classType"></param>
        /// <param name="propertyName"></param>
        /// <param name="classInstance"></param>
        /// <param name="includePrivate"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        public static T ReflectionGetPropertyValue<T>(System.Type classType, string propertyName, UnityEngine.Object classInstance, bool includePrivate, bool isStatic)
        {
            // Some types may not be nullable, so instead set to the default value for that type.
            T value = default(T);

            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !isStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (isStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (isStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            System.Reflection.PropertyInfo propertyInfo = classType.GetProperty(propertyName, bindingFlags);

            if (propertyInfo != null)
            {
                if (isStatic) { value = (T)propertyInfo.GetValue(null); }
                else { value = (T)propertyInfo.GetValue(classInstance); }
            }

            return value;
        }

        public static void ReflectionOutputMethods(Type t, bool showParmeters, bool includePrivate, bool includeStatic)
        {
            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !includeStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (includeStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (includeStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            System.Reflection.MethodInfo[] methodInfos = t.GetMethods(bindingFlags);
            foreach (System.Reflection.MethodInfo mInfo in methodInfos)
            {
                Debug.Log("S3DUtils: Type: " + t.Name + " Method: " + mInfo.Name);

                if (showParmeters)
                {
                    System.Reflection.ParameterInfo[] parameters = mInfo.GetParameters();
                    if (parameters != null)
                    {
                        foreach (System.Reflection.ParameterInfo parm in parameters)
                        {
                            Debug.Log(" Parm: " + parm.Name + " ParmType: " + parm.ParameterType.Name);
                        }
                    }
                }
            }
        }

        public static void ReflectionOutputFields(Type t, bool includePrivate, bool includeStatic)
        {
            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !includeStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (includeStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (includeStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            // Get the Fields of the type based on the binding flags selected
            System.Reflection.FieldInfo[] fInfos = t.GetFields(bindingFlags);

            foreach (System.Reflection.FieldInfo fInfo in fInfos)
            {
                Debug.Log("S3DUtils: Type: " + t.Name + " field: " + fInfo.Name + " fldtype: " + fInfo.FieldType.Name);
            }
        }

        public static void ReflectionOutputProperties(Type t, bool includePrivate, bool includeStatic)
        {
            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !includeStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (includeStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (includeStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            // Get the Properties of the type based on the binding flags selected
            System.Reflection.PropertyInfo[] piArray = t.GetProperties(bindingFlags);

            foreach (System.Reflection.PropertyInfo pi in piArray)
            {
                Debug.Log("S3DUtils: Type: " + t.Name + " property: " + pi.Name + " proptype: " + pi.PropertyType.Name);
            }
        }

        #endregion

        #region RenderTexture Methods

        /// <summary>
        /// Create a new RenderTexture
        /// </summary>
        /// <param name="texWidth"></param>
        /// <param name="texLength"></param>
        /// <param name="texName"></param>
        /// <returns></returns>
        public static RenderTexture CreateRenderTexture(int texWidth, int texLength, string texName)
        {
            RenderTexture renderTex = null;

            if (texWidth > 0 && texLength > 0)
            {
                renderTex = new RenderTexture(texWidth, texLength, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

                if (renderTex)
                {
                    renderTex.useMipMap = false;
                    renderTex.autoGenerateMips = false;
                    renderTex.hideFlags = HideFlags.DontSave;
                    renderTex.enableRandomWrite = true;
                    renderTex.name = texName;
                    renderTex.Create();

                    if (!renderTex.IsCreated()) { DestroyRenderTexture(ref renderTex); }
                }
            }

            return renderTex;
        }

        /// <summary>
        /// Safely destroy or cleanup a RenderTexture
        /// </summary>
        /// <param name="renderTexture"></param>
        public static void DestroyRenderTexture(ref RenderTexture renderTexture)
        {
            if (renderTexture != null)
            {
                if (renderTexture.IsCreated()) { renderTexture.Release(); }

                #if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(renderTexture);
                #else
                UnityEngine.Object.Destroy(renderTexture);
                #endif

                renderTexture = null;
            }
        }

        #endregion

        #region SSC Integration

        #if SCSM_SSC
        public static bool IsSSCAvailable { get { return true; } }
#else
        public static bool IsSSCAvailable { get { return false; } }
#endif

        #endregion

        #region String Methods

        /// <summary>
        /// Return string text of Friend (1), Foe (-1), Neutral (0) or Unknown (other value).
        /// </summary>
        /// <param name="friendOrFoeInt"></param>
        /// <returns></returns>
        public static string GetFriendOrFoe (int friendOrFoeInt)
        {
            return friendOrFoeInt == -1 ? "Foe" : friendOrFoeInt == 0 ? "Neutral" : friendOrFoeInt == 1 ? "Friend" : "Unknown";
        }

        /// <summary>
        /// Return a string from a value rounded to n (0-3) decimal place.
        /// Option to add a percentage sign to the end of string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimalPlaces"></param>
        /// <param name="isPercentage"></param>
        /// <returns></returns>
        public static string GetNumericString(float value, int decimalPlaces, bool isPercentage)
        {
            // Reduce GC as much as possible by not appending strings to each other
            if (decimalPlaces == 0)
            {
                if (isPercentage) { return (Mathf.RoundToInt(value)/100f).ToString("0%"); }
                else { return Mathf.RoundToInt(value).ToString("0"); }
            }
            else
            {
                double dValue = (float)System.Math.Round(value, decimalPlaces);

                if (isPercentage)
                {
                    dValue /= 100f;
                    if (decimalPlaces == 1)
                    {
                        return dValue.ToString("0.0%");
                    }
                    else if (decimalPlaces == 2)
                    {
                        return dValue.ToString("0.00%");
                    }
                    else
                    {
                        return dValue.ToString("0.000%");
                    }
                }
                else if (decimalPlaces == 1)
                {
                    return dValue.ToString("0.0");
                }
                else if (decimalPlaces == 2)
                {
                    return dValue.ToString("0.00");
                }
                else
                {
                    return dValue.ToString("0.000");
                }
            }
        }

        #endregion

        #region Transform and UI Methods

        /// <summary>
        /// Return the default Unity font
        /// </summary>
        /// <returns></returns>
        public static Font GetDefaultFont()
        {
            #if UNITY_2022_2_OR_NEWER
            return (Font)Resources.GetBuiltinResource(typeof(Font), "LegacyRuntime.ttf");
            #else
            return (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            #endif
        }

        /// <summary>
        /// Get a child RectTransform or UI panel of a parent given the name of the child.
        /// See also GetChildTransform(...)
        /// </summary>
        /// <param name="parentTrfm"></param>
        /// <param name="childName"></param>
        /// <param name="callerName"></param>
        /// <returns></returns>
        public static RectTransform GetChildRectTransform(Transform parentTrfm, string childName, string callerName)
        {
            RectTransform rectTransform = null;
            Transform childTrm = parentTrfm == null || string.IsNullOrEmpty(childName) ? null : parentTrfm.Find(childName);

            if (childTrm != null)
            {
                rectTransform = childTrm.GetComponent<RectTransform>();
            }

            #if UNITY_EDITOR
            if (rectTransform == null)
            {
                Debug.LogWarning(string.Format("ERROR: {0} could not find RectTransform child {1} under {2}", callerName, string.IsNullOrEmpty(childName) ? "[unknown]" : childName, parentTrfm == null ? " no parent" : parentTrfm.name));
            }
            #endif

            return rectTransform;
        }

        /// <summary>
        /// Get a child transform or UI panel of a parent given the name of the child
        /// See also GetChildRectTransform(..)
        /// </summary>
        /// <param name="parentTrfm"></param>
        /// <param name="childName"></param>
        /// <param name="callerName"></param>
        /// <returns></returns>
        public static Transform GetChildTransform(Transform parentTrfm, string childName, string callerName)
        {
            Transform childTrm = parentTrfm == null || string.IsNullOrEmpty(childName) ? null : parentTrfm.Find(childName);

            #if UNITY_EDITOR
            if (childTrm == null)
            {
                Debug.LogWarning(string.Format("ERROR: {0} could not find child {1} under {2}", callerName, string.IsNullOrEmpty(childName) ? "[unknown]" : childName, parentTrfm == null ? " no parent" : parentTrfm.name));
            }
            #endif

            return childTrm;
        }

        /// <summary>
        /// Translate a world space position into a relative local space position.
        /// This is an alternative to transform.InverseTransformPoint which doesn't
        /// work if the parent gameobject has scale not equal to 1,1,1.
        /// </summary>
        /// <param name="trfm"></param>
        /// <param name="wsPosition"></param>
        /// <returns></returns>
        public static Vector3 GetLocalSpacePosition(Transform trfm, Vector3 wsPosition)
        {
            if (trfm != null)
            {
                return Quaternion.Inverse(trfm.rotation) * (wsPosition - trfm.position);
            }
            else { return Vector3.zero; }
        }

        /// <summary>
        /// Get or Create a RectTransform as a child of a given Transform.
        /// panelOffsetX, panelOffsetY range is -1.0 to 1.0.
        /// panelWidth and panelHeight range is 0.0 to 1.0.
        /// </summary>
        /// <param name="parentRectTrfm"></param>
        /// <param name="parentSize"></param>
        /// <param name="childName"></param>
        /// <param name="panelOffsetX"></param>
        /// <param name="panelOffsetY"></param>
        /// <param name="panelWidth"></param>
        /// <param name="panelHeight"></param>
        /// <param name="anchorMinX"></param>
        /// <param name="anchorMinY"></param>
        /// <param name="anchorMaxX"></param>
        /// <param name="anchorMaxY"></param>
        /// <returns></returns>
        public static RectTransform GetOrCreateChildRectTransform
        (
            RectTransform parentRectTrfm,
            Vector2 parentSize,
            string childName,
            float panelOffsetX, float panelOffsetY,
            float panelWidth, float panelHeight,
            float anchorMinX, float anchorMinY,
            float anchorMaxX, float anchorMaxY
        )
        {
            RectTransform childRectTrfm = null;

            if (parentRectTrfm != null && !string.IsNullOrEmpty(childName))
            {
                Transform childTrfm = parentRectTrfm.Find(childName);
                if (childTrfm == null)
                {
                    GameObject _tempGO = new GameObject(childName);
                    if (_tempGO != null)
                    {
                        _tempGO.layer = 5;
                        _tempGO.transform.SetParent(parentRectTrfm);
                        childRectTrfm = _tempGO.AddComponent<RectTransform>();

                        childRectTrfm.anchorMin = new Vector2(anchorMinX, anchorMinY);
                        childRectTrfm.anchorMax = new Vector2(anchorMaxX, anchorMaxY);

                        // Panel width and height are 0.0-1.0 values
                        childRectTrfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth * parentSize.x);
                        childRectTrfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight * parentSize.y);

                        // Force the child to be the same scale as the parent
                        childRectTrfm.localScale = Vector3.one;

                        childRectTrfm.localPosition = new Vector3(panelOffsetX * parentSize.x , panelOffsetY * parentSize.y, parentRectTrfm.localPosition.z);
                    }
                }
                else
                {
                    // If the transform exists but the RectTransform component is missing, it will return null
                    childRectTrfm = childTrfm.GetComponent<RectTransform>();
                }
            }

            return childRectTrfm;
        }


        /// <summary>
        /// Get an existing child transform or create a new one if it does not exit.
        /// </summary>
        /// <param name="parentTrfm"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static Transform GetOrCreateChildTransform(Transform parentTrfm, string childName)
        {
            Transform childTrfm = null;

            if (!string.IsNullOrEmpty(childName))
            {
                childTrfm = parentTrfm.Find(childName);
                if (childTrfm == null)
                {
                    GameObject _tempGO = new GameObject(childName);
                    if (_tempGO != null)
                    {
                        childTrfm = _tempGO.transform;
                        childTrfm.SetParent(parentTrfm);
                    }
                }
            }

            return childTrfm;
        }

        /// <summary>
        /// Get a world space position of a relative offset from a transform
        /// (converts a 3D position from local space to world space).
        /// If the transform is null, returns Vector3.zero.
        /// </summary>
        /// <param name="localSpacePosition"></param>
        /// <returns></returns>
        public static Vector3 GetWorldPosition(Transform trfm, Vector3 localSpacePosition)
        {
            if (trfm != null)
            {
                return trfm.position + (trfm.rotation * localSpacePosition);
            }
            else { return Vector3.zero; }
        }

        #endregion
    }

    #region AnimParm Struct

    /// <summary>
    /// Hold data about an Animation Parameter
    /// </summary>
    public struct S3DAnimParm
    {
        public int hashCode;
        public string paramName;

        public S3DAnimParm(int hashCode, string paramName)
        {
            this.hashCode = hashCode;
            this.paramName = paramName;
        }

        /// <summary>
        /// Populate a non-null empty list with all parameters
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="s3dAnimParmList"></param>
        public static void GetParameterListAll(Animator animator, List<S3DAnimParm> s3dAnimParmList)
        {
            // If the animator is not active, it will raise warnings in the editor and animator.parameters will fail.
            if (s3dAnimParmList != null && IsAnimatorReady(animator))
            {
                s3dAnimParmList.Clear();
                var parms = animator.parameters;
                int numParams = parms == null ? 0 : parms.Length;

                for (int paramIdx = 0; paramIdx < numParams; paramIdx++)
                {
                    AnimatorControllerParameter p = parms[paramIdx];
                    s3dAnimParmList.Add(new S3DAnimParm(p.nameHash, p.name));
                }
            }
        }

        /// <summary>
        /// Populate a non-null empty list with parameters of the given type.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="s3dAnimParmList"></param>
        /// <param name="parmeterType"></param>
        public static void GetParameterList(Animator animator, List<S3DAnimParm> s3dAnimParmList, S3DAnimAction.ParameterType parmeterType)
        {
            // If the animator is not active, it will raise warnings in the editor and animator.parameters will fail.
            if (s3dAnimParmList != null && IsAnimatorReady(animator))
            {
                s3dAnimParmList.Clear();
                var parms = animator.parameters;
                int numParams = parms == null ? 0 : parms.Length;

                s3dAnimParmList.Add(new S3DAnimParm(0, "Not Set"));

                for (int paramIdx = 0; paramIdx < numParams; paramIdx++)
                {
                    AnimatorControllerParameter p = parms[paramIdx];

                    if (p.type == AnimatorControllerParameterType.Bool)
                    {
                        if (parmeterType == S3DAnimAction.ParameterType.Bool)
                        {
                            s3dAnimParmList.Add(new S3DAnimParm(p.nameHash, p.name));
                        }
                    }
                    else if (p.type == AnimatorControllerParameterType.Trigger)
                    {
                        if (parmeterType == S3DAnimAction.ParameterType.Trigger)
                        {
                            s3dAnimParmList.Add(new S3DAnimParm(p.nameHash, p.name));
                        }
                    }
                    else if (p.type == AnimatorControllerParameterType.Float)
                    {
                        if (parmeterType == S3DAnimAction.ParameterType.Float)
                        {
                            s3dAnimParmList.Add(new S3DAnimParm(p.nameHash, p.name));
                        }
                    }
                    else if (p.type == AnimatorControllerParameterType.Int)
                    {
                        if (parmeterType == S3DAnimAction.ParameterType.Integer)
                        {
                            s3dAnimParmList.Add(new S3DAnimParm(p.nameHash, p.name));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get an array of parameter names from a list of supplied parameters
        /// </summary>
        /// <param name="s3dAnimParmList"></param>
        /// <returns></returns>
        public static string[] GetParameterNames(List<S3DAnimParm> s3dAnimParmList)
        {
            int numParams = s3dAnimParmList == null ? 0 : s3dAnimParmList.Count;

            if (numParams == 0) { return new string[] { "Not Set" }; }
            else
            {
                string[] paramNames = new string[numParams+1];

                paramNames[0] = "Not Set";

                for (int paramIdx = 1; paramIdx < numParams; paramIdx++)
                {
                    paramNames[paramIdx] = s3dAnimParmList[paramIdx].paramName;
                }

                return paramNames;
            }
        }

        /// <summary>
        /// Check if the Animator is valid and ready for use.
        /// Avoids the (reoccurring) "Animator is not playing the AnimatorController" warning,
        /// and ensures animator parameters can be fetched correctly.
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        public static bool IsAnimatorReady(Animator animator)
        {
            if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
            {
                // NOTE: This will still raise 1 warning if the animator is not ready.
                if (animator.GetCurrentAnimatorStateInfo(0).length == 0)
                {
                    // Fix the condition that causes reoccurring "Animator is not playing the AnimatorController"
                    animator.Rebind();
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    #endregion

    #region AnimLayer Struct

    /// <summary>
    /// Holds data about an Animation Layer
    /// </summary>
    public struct S3DAnimLayer
    {
        public int hashCode;
        public string layerName;

        public S3DAnimLayer(int hashCode, string layerName)
        {
            this.hashCode = hashCode;
            this.layerName = layerName;
        }
    }

    #endregion

    #region AnimTrans Struct

    /// <summary>
    /// Holds data about an Animation Transition
    /// </summary>
    public struct S3DAnimTrans
    {
        public int layerHashCode;
        public int hashCode;
        public string transitionName;

        public S3DAnimTrans(int layerHashCode, int hashCode, string transitionName)
        {
            this.layerHashCode = layerHashCode;
            this.hashCode = hashCode;
            this.transitionName = transitionName;
        }

        /// <summary>
        /// Get an array of transition names from a list of supplied transitions
        /// </summary>
        /// <param name="s3dAninTransList"></param>
        /// <returns></returns>
        public static string[] GetTransitionNames(List<S3DAnimTrans> s3dAninTransList)
        {
            int numTrans = s3dAninTransList == null ? 0 : s3dAninTransList.Count;

            if (numTrans == 0) { return new string[] { "Not Set" }; }
            else
            {
                string[] transNames = new string[numTrans + 1];

                transNames[0] = "Not Set";

                for (int transIdx = 1; transIdx < numTrans; transIdx++)
                {
                    transNames[transIdx] = s3dAninTransList[transIdx].transitionName;
                }

                return transNames;
            }
        }
    }

    #endregion

    #region AnimClipOverrides Class

    /// <summary>
    /// Class to hold old and new animation clip pairs.
    /// Used with AnimatorOverrideControllers and ApplyOverrides().
    /// </summary>
    public class AnimClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
    {
        public AnimClipOverrides(int capacity) : base(capacity) { }

        public AnimationClip this[string name]
        {
            get { return this.Find(ac => ac.Key.name.Equals(name)).Value; }
            set
            {
                int idx = this.FindIndex(ac => ac.Key.name.Equals(name));
                if (idx != -1)
                {
                    this[idx] = new KeyValuePair<AnimationClip, AnimationClip>(this[idx].Key, value);
                }
            }
        }
    }

    #endregion

    #region CDF Struct

    /// <summary>
    /// A struct used in temporary list of cummulative distribution values pairs.
    /// The cdValue would be calculated by a Cummulative Distribution Function.
    /// </summary>
    public struct S3DCDFItem
    {
        public int index;
        public float cdValue;

        public S3DCDFItem(int i, float value)
        {
            index = i;
            cdValue = value;
        }
    }

    #endregion

    #region Comparer Classes

    /// <summary>
    /// Collider Comparer for use with HashSets.
    /// USAGE:
    /// S3DColliderComparer colliderComparer = new S3DColliderComparer();
    /// HashSet inTriggerColliders = new HashSet<Collider>(colliderComparer);
    /// </summary>
    public class S3DColliderComparer : IEqualityComparer<Collider>
    {
        public bool Equals(Collider c1, Collider c2)
        {
            if (c1 == null && c2 == null) { return true; }
            else if (c1 == null || c2 == null) { return false; }
            else { return c1.GetInstanceID() == c2.GetInstanceID(); }
        }

        public int GetHashCode(Collider collider)
        {
            //return collider.GetHashCode();
            return collider.GetInstanceID();
        }
    }

    #endregion

    #region List Wrapper Class
    /// <summary>
    /// Used with JsonUtility to convert a list to/from json.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ListWrapper<T>
    {
        public List<T> itemList;
    }
    #endregion

    #region Object Hit Parameters Struct

    /// <summary>
    /// Parameters structure for CallbackOnHit (callback for StickyDamageReceiver).
    /// We do not recommend keeping references to any fields within this structure.
    /// Use them immediately, then discard them.
    /// </summary>
    public struct S3DObjectHitParameters
    {
        /// <summary>
        /// Hit information for the raycast hit against the object.
        /// </summary>
        public RaycastHit hitInfo;
        /// <summary>
        /// Prefab for the projectile that hit the object.
        /// </summary>
        public StickyProjectileModule projectilePrefab;
        /// <summary>
        /// Prefab for the beam that hit the object
        /// </summary>
        public StickyBeamModule beamPrefab;
        /// <summary>
        /// Amount of damage done by the projectile or beam.
        /// For projectiles, the ammo Damage Multiplier has been applied.
        /// </summary>
        public float damageAmount;
        /// <summary>
        /// The type of damage done by the projectile or beam
        /// See S3DDamageRegion.DamageType
        /// </summary>
        public int damageTypeInt;
        /// <summary>
        /// The amount of force that is applied when hitting a rigidbody at the point of impact.
        /// For projectiles, the ammo Impact Multiplier has been applied.
        /// </summary>
        public float impactForce;
        /// <summary>
        /// The 0-25 (A-Z) ammo type that was used. -1 = unset.
        /// </summary>
        public int ammoTypeInt;
        /// <summary>
        /// The faction or alliance of the character that fired the projectile or beam belongs to.
        /// </summary>
        public int sourceFactionId;
        /// <summary>
        /// The type, category, or model of the character that fired the projectile or beam.
        /// </summary>
        public int sourceModelId;
        /// <summary>
        /// The ID of the character that fired the projectile or beam.
        /// </summary>
        public int sourceStickyId;
    };

    #endregion

    #region Shader Properties

    public struct ShaderPropertyID
    {
        /// <summary>
        /// URP and HDRP Lit shader colour
        /// </summary>
        public static readonly int baseColor = Shader.PropertyToID("_BaseColor");
        /// <summary>
        /// Built-in RP standard shader colour
        /// </summary>
        public static readonly int color = Shader.PropertyToID("_Color");

        // Blending state properties from the Standard shader
        /// <summary>
        /// Built-in RP 0: Opaque, 1: Cutout, 2: Fade, 3: Transparent
        /// </summary>
        public static readonly int mode = Shader.PropertyToID("_Mode");
        public static readonly int srcBlend = Shader.PropertyToID("_SrcBlend");
        public static readonly int dstBlend = Shader.PropertyToID("_DstBlend");
        public static readonly int zWrite = Shader.PropertyToID("_ZWrite");
    }

    #endregion
}