using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace scsmmedia
{
    [CustomEditor(typeof(LBS_DoorAnimation01))]
    public class LBS_DoorAnimation01Editor : Editor
    {
        #region Private Variables
        private GUIStyle labelFieldRichText;
        private string txtColourName = "black";
        private int tagIdx = 0;
        #endregion

        #region SerializedProperty
        private SerializedProperty isLockedProp;
        private SerializedProperty animationOpeningSpeedProp;
        private SerializedProperty animationClosingSpeedProp;
        private SerializedProperty tagListProp;
        private SerializedProperty tagProp;

        #endregion

        #region GUIContent
        private static readonly GUIContent isLockedContent = new GUIContent("Is Locked", "Is the door or gate locked?");
        private static readonly GUIContent aninOpenSpeedContent = new GUIContent("Open Speed", "The rate at which the door opens (Default is 1)");
        private static readonly GUIContent aninCloseSpeedContent = new GUIContent("Close Speed", "The rate at which the door closes (Default is 1)");
        private static readonly GUIContent tagListContent = new GUIContent("Tag List", "[Optional] list of Unity Tags to detect objects entering or exiting doors");

        #endregion

        #region Event Methods

        public void OnEnable()
        {
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; }

            isLockedProp = serializedObject.FindProperty("_isLocked");
            animationOpeningSpeedProp = serializedObject.FindProperty("_animationOpeningSpeed");
            animationClosingSpeedProp = serializedObject.FindProperty("_animationClosingSpeed");
            tagListProp = serializedObject.FindProperty("tagList");
        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 150f;

            if (labelFieldRichText == null)
            {
                labelFieldRichText = new GUIStyle("Label");
                labelFieldRichText.richText = true;
            }

            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("<color=" + txtColourName + "><b>LB Enviro Pack Samples</b></color>", labelFieldRichText);
            EditorGUILayout.PropertyField(isLockedProp, isLockedContent);
            EditorGUILayout.PropertyField(animationOpeningSpeedProp, aninOpenSpeedContent);
            EditorGUILayout.PropertyField(animationClosingSpeedProp, aninCloseSpeedContent);

            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(tagListProp, tagListContent);
            EditorGUI.indentLevel -= 1;

            if (tagListProp.isExpanded)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Tags: " + tagListProp.arraySize.ToString("00"));
                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    tagListProp.arraySize += 1;
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (tagListProp.arraySize > 0) { tagListProp.arraySize -= 1; }
                }
                GUILayout.EndHorizontal();

                for (tagIdx = 0; tagIdx < tagListProp.arraySize; tagIdx++)
                {
                    tagProp = tagListProp.GetArrayElementAtIndex(tagIdx);
                    EditorGUILayout.PropertyField(tagProp, GUIContent.none);
                }
            }
            else
            {

            }

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();


            // If the base command is called, all public fields not hidden, will be displayed
            //base.OnInspectorGUI();
        }


        #endregion
    }
}
