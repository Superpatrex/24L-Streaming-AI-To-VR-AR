using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The custom inspector for the StickyShapesModule class
    /// </summary>
    [CustomEditor(typeof(StickyShapesModule))]
    public class StickyShapesModuleEditor : Editor
    {
        #region Custom Editor private or protected variables
        private StickyShapesModule stickyShapesModule = null;
        private List<SkinnedMeshRenderer> tempSMRenList = new List<SkinnedMeshRenderer>(10);

        protected bool isStylesInitialised = false;
        protected bool isSceneModified = false;
        protected string labelText;
        protected GUIStyle labelFieldRichText;
        protected GUIStyle headingFieldRichText;
        protected GUIStyle helpBoxRichText;
        protected GUIStyle buttonCompact;
        protected GUIStyle foldoutStyleNoLabel;
        protected GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        protected GUIStyle toggleCompactButtonStyleToggled = null;
        protected GUIStyle toggleCompactButtonStyleToggledB = null; // Toggled with blue text
        protected GUIStyle toggleButtonStyleNormal12 = null; // Toggle button fontsize 12
        protected GUIStyle toggleButtonStyleToggledB12 = null; // Toggled button fontsize 12 with blue text
        protected Color separatorColor = new Color();
        protected float defaultEditorLabelWidth = 0f;
        protected float defaultEditorFieldWidth = 0f;
        protected bool isDebuggingEnabled = false;
        protected bool isBlinkingEnabled = false;
        protected bool isBlinkingModified = false;

        private int emotionDeletePos = -1;
        private int emotionMoveDownPos = -1;
        private int emotionInsertPos = -1;

        private int emotionShapeDeletePos = -1;
        private int emotionShapeMoveDownPos = -1;

        private int blkemShapeDeletePos = -1;
        private int blkemShapeMoveDownPos = -1;

        private int phonemeDeletePos = -1;
        private int phonemeMoveDownPos = -1;
        private int phonemeInsertPos = -1;

        private int reactDeletePos = -1;
        private int reactMoveDownPos = -1;
        private int reactInsertPos = -1;

        private int speechAudioDeletePos = -1;
        private int speechAudioMoveDownPos = -1;
        private int speechAudioInsertPos = -1;

        private int phShapeDeletePos = -1;
        private int phShapeMoveDownPos = -1;

        private int reactEmotionDeletePos = -1;
        private int reactEmotionMoveDownPos = -1;

        private int reactSpchADeletePos = -1;
        private int reactSpchAMoveDownPos = -1;

        protected string[] modelBlendShapeNames;
        protected int[] modelBlendShapeNameHashes;
        private List<string> tempNameList = null;

        protected string[] emotionNames;
        protected int[] emotionNameHashes;

        protected string[] speechAudioNames;
        protected int[] speechAudioNameHashes;
        protected bool isSpeechAudioNamesStale = true;

        private bool isShowEmotionDebugging = false;
        private List<S3DEmotion> emotionList = null;

        private bool isShowEngageDebugging = false;

        #endregion

        #region GUIContent - Headers
        protected readonly static GUIContent headerContent = new GUIContent("This module lets you configure blend shapes, typically, for facial expressions.");
        #endregion

        #region GUIContent - Tabs

        protected static readonly GUIContent[] baseTabTexts = { new GUIContent("General"), new GUIContent("BlendShapes"), new GUIContent("Emotions"), new GUIContent("Phonetics"), new GUIContent("Engage") };
        protected GUIContent[] tabTexts = null;

        protected static readonly GUIContent[] reactTabTexts = { new GUIContent("General"), new GUIContent("Emotions"), new GUIContent("Events") };

        #endregion

        #region GUIContent - General
        protected readonly static GUIContent initialiseOnStartContent = new GUIContent("Initialise On Start", "If enabled, the " +
                                "Initialise() will be called as soon as Start() runs. This should be disabled if you are instantiating the zone through code.");
        protected readonly static GUIContent isNonS3DControllerContent = new GUIContent("Non S3D Controller", "The character is NOT using a Sticky3DController. i.e., it is using a 3rd party character controller.");
        protected readonly static GUIContent skinnedMRenListContent = new GUIContent("Skinned Mesh Renderers", "The mesh renderers that contain the blendshapes" );
        protected readonly static GUIContent getSkinnedMRenContent = new GUIContent("Get Skinned Mesh Renderers", "Attempt to find the mesh renderers that contain the blendshapes" );
        protected readonly static GUIContent getEyesContent = new GUIContent("Get Eyes", "Attempt to find the eye bone transforms on the humanoid model" );
        protected readonly static GUIContent leftEyeTrfmContent = new GUIContent("Left Eye Transform", "The left eye bone transforms on the humanoid model" );
        protected readonly static GUIContent rightEyeTrfmContent = new GUIContent("Right Eye Transform", "The right eye bone transforms on the humanoid model" );
        protected readonly static GUIContent newBtnContent = new GUIContent("New");
        #endregion

        #region GUIContent - Blendshapes
        protected readonly static GUIContent blendShapeListContent = new GUIContent("Blend Shapes", "The list of blendshapes on the model");
        protected readonly static GUIContent getBlendShapesContent = new GUIContent("Get Blendshapes", "Attempt to find the blendshapes based on the list of Skinned Mesh Renderers from the General tab.");

        #endregion

        #region GUIContent - Blink
        protected readonly static GUIContent blinkSettingsContent = new GUIContent("Blink Settings");
        protected readonly static GUIContent blinkSyncContent = new GUIContent("SYN", "Attempt to (re)synchronize all blink blendshapes with the defined blendshapes");
        protected readonly static GUIContent blinkIsEnableOnInitContent = new GUIContent(" Enable on Init", "Should the eyes start blinking when this component is initialised?");
        protected readonly static GUIContent blinkIsSimpleContent = new GUIContent(" Simple Blink", "A lower-quality simplified blink action that is more performant");
        protected readonly static GUIContent blinkDurationContent = new GUIContent(" Blink Duration", "The time, in seconds, the blink takes to occur.");
        protected readonly static GUIContent blinkMinIntervalContent = new GUIContent(" Blink Min. Interval", "The minimum time, in seconds, between each blink");
        protected readonly static GUIContent blinkMaxIntervalContent = new GUIContent(" Blink Max. Interval", "The maximum time, in seconds, between each blink");
        #endregion

        #region GUIContent - Eye Movement
        protected readonly static GUIContent eyeMovementSettingsContent = new GUIContent("Eye Movement Settings");
        protected readonly static GUIContent isEyeMoveEnableOnInitContent = new GUIContent(" Enable on Init", "Should the eyes start moving when this component is initialised?");
        protected readonly static GUIContent eyeMoveXContent = new GUIContent(" Move Left-Right", "The angle to move left or right. Lower absolute values typically look better.");
        protected readonly static GUIContent eyeMoveYContent = new GUIContent(" Move Down-Up", "The angle to move down and up. Lower absolute values typically look better.");
        protected readonly static GUIContent eyeMoveGazeDurationContent = new GUIContent(" Gaze Duration", "When Eye Movement is enabled, the time, in seconds, the character is gazing in the same direction.");
        protected readonly static GUIContent eyeMoveSpeedContent = new GUIContent(" Move Speed", "Human eyes can turn at around 700 degrees per second! However, this is over very short distances.");

        #endregion

        #region GUIContent - Emotions
        protected readonly static GUIContent emotionSyncContent = new GUIContent("SYN", "Attempt to (re)synchronize all emotions with the defined blendshapes");
        protected readonly static GUIContent emotionListContent = new GUIContent("Emotions", "The list of emotions to be used with this character");
        protected readonly static GUIContent emPrevBtnContent = new GUIContent("Pre", "Preview emotion on the character");
        protected readonly static GUIContent emNameContent = new GUIContent(" Name of Emotion", "The descriptive name of the emotion");
        protected readonly static GUIContent emFadeInDurationContent = new GUIContent(" Fade-In Duration", "The time, in seconds, it takes the emotion to reach its full strength.");
        protected readonly static GUIContent emFadeOutDurationContent = new GUIContent(" Fade-Out Duration", "The time, in seconds, it takes the emotion to stop affecting the character.");
        protected readonly static GUIContent emVOXEmotionContent = new GUIContent(" VOX Emotion", "Is this emotion randomly used while a speech audio clip is playing?");
        protected readonly static GUIContent emAngerIntensityContent = new GUIContent(" Anger Intensity", "The emotion's anger intensity ranging from fear (-1) to anger (1). Default is neither fear nor anger (0)");
        protected readonly static GUIContent emJoyIntensityContent = new GUIContent(" Joy Intensity", "The emotion's joy intensity ranging from sad (-1) to joy (1). Default is neither sad nor joy (0)");
        protected readonly static GUIContent emSurpriseIntensityContent = new GUIContent(" Surprise Intensity", "The emotion's surprise intensity ranging from anticipation (-1) to surprise (1). Default is neither anticipation nor surprise (0)");
        protected readonly static GUIContent emTrustIntensityContent = new GUIContent(" Trust Intensity", "The emotion's anger intensity ranging from disgust (-1) to trust (1). Default is neither disgust nor trust (0)");
        protected readonly static GUIContent emShapeListContent = new GUIContent(" Blend Shapes", "The list of blend shapes to be used with this emotion");
        protected readonly static GUIContent emShapeMaxWeightContent = new GUIContent(" Max Weight", "The normalised weight to be applied to the blendshape");
        #endregion

        #region GUIContent - Phonemes
        protected readonly static GUIContent isAudioSettingsExpandedContent = new GUIContent("Phonetic Settings", "General Phonetic settings");
        protected readonly static GUIContent phonemeAudioSourceContent = new GUIContent(" Audio Source", "The audio source used to play speech autoclips");
        protected readonly static GUIContent phonemeNewAudioContent = new GUIContent("New", "Create a new child audio source on the character");
        protected readonly static GUIContent phonemeSyncContent = new GUIContent("SYN", "Attempt to (re)synchronize all phonemes with the defined blendshapes");
        protected readonly static GUIContent phonemeListContent = new GUIContent("Phonemes", "The list of phonemes to be used with this character");
        protected readonly static GUIContent phPrevBtnContent = new GUIContent("Pre", "Preview viseme on the character");
        protected readonly static GUIContent phNameContent = new GUIContent(" Name of Phoneme", "The name or title of the phoneme");
        protected readonly static GUIContent phShapeListContent = new GUIContent(" Blend Shapes", "The list of blend shapes to be used with this phoneme");
        protected readonly static GUIContent phShapeMaxWeightContent = new GUIContent(" Max Weight", "The normalised weight to be applied to the blendshape");
        protected readonly static GUIContent phShapeDurationContent = new GUIContent(" Duration", "The time, in seconds, the phoneme will be active on the character. A typical short phoneme is 40-80ms.");
        protected readonly static GUIContent phShapeFrequencyContent = new GUIContent(" Frequency", "The number of times the phoneme would be spoken in a sequence of 100 phonemes.");
        protected readonly static GUIContent speechAudioListContent = new GUIContent("Speech Audio Clips", "The list of speech audio clips used for this character");
        protected readonly static GUIContent saAudioClipContent = new GUIContent(" Audio Clip", "Audio clip that holds the sound of the character speaking");
        protected readonly static GUIContent saPrevBtnContent = new GUIContent("Pre", "Play audio and preview visemes and VOX-enabled emotions on the character");
        protected readonly static GUIContent saSpeechSpeedContent = new GUIContent(" Speech Speed", "The speed to play back the phonemes, relative to the phoneme timings set on the character.");
        protected readonly static GUIContent saVolumeContent = new GUIContent(" Volume", "The relative playback volume of the audio clip");
        protected readonly static GUIContent saSilenceTHresholdContent = new GUIContent(" Silence Threshold", "Volume below this level will be considered as silence");
        protected readonly static GUIContent saEmotionsIgnoreSilenceContent = new GUIContent(" Emotions Ignore Silence", "Do emotions with VOX Emotion enabled, play through silent sections of the audio clip?");
        protected readonly static GUIContent saEmotionIntervalContent = new GUIContent(" Emotion Interval", "The interval between VOX emotions playing when Emotions Ignore Silence is true");
        protected readonly static GUIContent saEmotionFadeOnSilenceContent = new GUIContent(" Emotion Fade on Silence", "Should the emotion immediately start fading out when there is silence in the audio and “Emotions Ignore Silence” is false? Emotions may never reach their maximum set weight, however, it may look a little more random.");
        #endregion

        #region GUIContent - Engage (React)
        protected readonly static GUIContent reactSettingsContent = new GUIContent("React Settings", "Settings used to configure how the character reacts to others");
        protected readonly static GUIContent reactEnterSettingsContent = new GUIContent("Enter Settings", "Settings used to configure reactions when other character come nearby");
        protected readonly static GUIContent reactStaySettingsContent = new GUIContent("Stay Settings", "Settings used to configure reactions when other characters remain nearby");
        protected readonly static GUIContent reactExitSettingsContent = new GUIContent("Exit Settings", "Settings used to configure reactions when other characters stop being nearby");
        protected readonly static GUIContent isReactEnableOnInitContent = new GUIContent(" Enable on Init", "Should the character get ready to react when this component is initialised?");
        protected readonly static GUIContent reactNoNotifyDurationContent = new GUIContent(" No Notify Duration", "The number of seconds, after initialisation, that reactions will not be triggered by a character entering or exiting the area. This can be useful if you do not want a character within the collider area to immediately trigger reactions when the component is initialised.");
        protected readonly static GUIContent proximityColliderContent = new GUIContent(" Proximity Collider", "The trigger collider used to detect nearby characters. It must be on a child gameobject within the prefab.");
        protected readonly static GUIContent reactDistanceContent = new GUIContent(" React Distance", "The distance at which the character starts to react to others in the scene");

        protected readonly static GUIContent onPreEnterContent = new GUIContent(" On Pre Enter", "These are triggered immediately before reacting to when another character come nearby");
        protected readonly static GUIContent onPostEnterContent = new GUIContent(" On Post Enter", "These are triggered immediately after reacting to when another character come nearby");
        protected readonly static GUIContent onPreExitContent = new GUIContent(" On Pre Exit", "These are triggered immediately before reacting to when other characters stop being nearby");
        protected readonly static GUIContent onPostExitContent = new GUIContent(" On Post Exit", "These are triggered immediately after reacting to when other characters stop being nearby");

        protected readonly static GUIContent reactSyncContent = new GUIContent("SYN", "Attempt to (re)synchronize all reactions with the Emotions tab.");
        protected readonly static GUIContent reactListContent = new GUIContent("Reactions", "The list of reaction to be used with this character");
        protected readonly static GUIContent reactAutoBtnContent = new GUIContent("Auto", "Automatically populate the list of Enter/Exit from the Emotions tab based on emotion intensities and the “React To” setting of the reaction.");
        protected readonly static GUIContent reactPrevBtnContent = new GUIContent("Pre", "Preview reaction on the character");
        protected readonly static GUIContent reNameContent = new GUIContent(" Name of Reaction", "The descriptive name of the reaction");
        protected readonly static GUIContent reToContent = new GUIContent(" React To", "Which other characters should this character respond to based on the factions of both characters");
        protected readonly static GUIContent reModelsToFilterContent = new GUIContent(" Models To Include (qty)", "An optional array of model IDs that will further limit which characters to react to. The quantity can be changed to increase or decrease the size of the array. Model ID of a Sticky3D character is found on the Engage tab of the StickyControlModule, under Identification Settings.");
        protected readonly static GUIContent enterReEmListContent = new GUIContent(" Enter Emotions", "The list of emotions to be used with this reaction when another character enters the proximity area.");
        protected readonly static GUIContent enterReEvtsContent = new GUIContent(" Enter Events", "The events that can be triggered when another character enters the proximity area.");
        protected readonly static GUIContent enterReSpchAListContent = new GUIContent(" Enter Speech Audio", "A speech audio clip from this list will be randomly played when another character enters the proximity area.");
        protected readonly static GUIContent exitReEmListContent = new GUIContent(" Exit Emotions", "The list of emotions to be used with this reaction when another character leaves the proximity area.");
        protected readonly static GUIContent exitReEvtsContent = new GUIContent(" Exit Events", "The events that can be triggered when another character stops being in the proximity area.");
        protected readonly static GUIContent exitReSpchAListContent = new GUIContent(" Exit Speech Audio", "A speech audio clip from this list will be randomly played when another character leaves the proximity area.");
        protected readonly static GUIContent reEmHoldDurationContent = new GUIContent("Hold Duration", "The length of time, in seconds, the emotion stays at fully active before fading back out.");

        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent(" Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent(" Is Initialised", "Has the module been initialised?");
        private readonly static GUIContent debugNumValidBShapesContent = new GUIContent(" Valid Blendshapes", "The number of valid blendshapes when last checked.");
        private readonly static GUIContent debugIsBlinkingEnabledContent = new GUIContent(" Is Blinking Enabled", "Is the blinking action enabled?");

        private readonly static GUIContent debugIsShowEmotionContent = new GUIContent(" Emotions", "Show Emotion debug data");
        private readonly static GUIContent debugIsEmotionEnabledContent = new GUIContent(" Is Enabled", "Is the emotion enabled?");
        private readonly static GUIContent debugEmotionIdContent = new GUIContent(" Emotion Id", "The unique identifier for this emotion");

        private readonly static GUIContent debugIsShowEngageContent = new GUIContent(" Engage", "Show Engage debug data");
        private readonly static GUIContent debugIsReactInitContent = new GUIContent("  Is React Initialised", "Is the character ready to react to others?");
        private readonly static GUIContent debugIsReactEnabledContent = new GUIContent("  Is React Enabled", "Is the character ready to react to others with emotions or speech?");

        #endregion

        #region Serialized Properties - General
        protected SerializedProperty selectedTabIntProp;
        protected SerializedProperty initialiseOnStartProp;
        protected SerializedProperty isNonS3DControllerProp;
        private SerializedProperty isSkinnedMRenListExpandedProp;
        protected SerializedProperty skinnedMRenListProp;
        protected SerializedProperty leftEyeProp;
        protected SerializedProperty leftEyeTfrmProp;
        protected SerializedProperty rightEyeProp;
        protected SerializedProperty rightEyeTfrmProp;
        #endregion

        #region Serialized Properties - BlendShapes
        private SerializedProperty isBlendShapeListExpandedProp;
        protected SerializedProperty blendShapeListProp;
        protected SerializedProperty blendShapeProp;
        protected SerializedProperty bsIsNameMatchedProp;
        #endregion

        #region Serialized Properties - Blink
        protected SerializedProperty blinkProp;
        protected SerializedProperty blkEmShapeListProp;
        protected SerializedProperty blkShowInEditorProp;
        protected SerializedProperty blkIsSimpleBlinkProp;

        #endregion

        #region Serialized Properties - Eye Movement
        protected SerializedProperty isEyeMovementExpandedProp;
        protected SerializedProperty isEyeMoveEnableOnInitProp;
        protected SerializedProperty eyeMoveMinXProp;
        protected SerializedProperty eyeMoveMaxXProp;
        protected SerializedProperty eyeMoveMinYProp;
        protected SerializedProperty eyeMoveMaxYProp;
        protected SerializedProperty eyeMoveMinGazeDurationProp;
        protected SerializedProperty eyeMoveMaxGazeDurationProp;
        protected SerializedProperty eyeMoveSpeedProp;

        #endregion

        #region Serialized Properties - Emotions
        private SerializedProperty isEmotionListExpandedProp;
        protected SerializedProperty emotionListProp;
        protected SerializedProperty emotionProp;
        protected SerializedProperty emNameProp;
        protected SerializedProperty emShowInEditorProp;
        protected SerializedProperty emFadeInDurationProp;
        protected SerializedProperty emFadeOutDurationProp;
        protected SerializedProperty emPreviewModeProp;
        protected SerializedProperty emVOXEmotionProp;
        protected SerializedProperty emAngerIntensityProp;
        protected SerializedProperty emJoyIntensityProp;
        protected SerializedProperty emSurpriseIntensityProp;
        protected SerializedProperty emTrustIntensityProp;
        protected SerializedProperty emShapeListProp;
        protected SerializedProperty emShapeProp;
        protected SerializedProperty emShapeBlendShapeIdProp;
        protected SerializedProperty emShapeIsSyncedProp;
        protected SerializedProperty emShapeBlendShapeNameHashProp;
        #endregion

        #region Serialized Properties - Phonemes
        private SerializedProperty isAudioSettingsExpandedProp;
        private SerializedProperty isPhonemeListExpandedProp;
        private SerializedProperty isSpeechAudioListExpandedProp;
        protected SerializedProperty phonemeAudioSourceProp;
        protected SerializedProperty phonemeListProp;
        protected SerializedProperty phonemeProp;
        protected SerializedProperty phNameProp;
        protected SerializedProperty phShowInEditorProp;
        protected SerializedProperty phPreviewModeProp;
        protected SerializedProperty phShapeListProp;
        protected SerializedProperty phShapeProp;
        protected SerializedProperty phShapeBlendShapeIdProp;
        protected SerializedProperty phShapeIsSyncedProp;
        protected SerializedProperty phShapeBlendShapeNameHashProp;
        protected SerializedProperty phShapeFrequencyProp;
        protected SerializedProperty phShapeDurationProp;
        protected SerializedProperty speechAudioListProp;
        protected SerializedProperty speechAudioProp;
        protected SerializedProperty saShowInEditorProp;
        protected SerializedProperty saPreviewModeProp;
        protected SerializedProperty saAudioClipProp;
        protected SerializedProperty saSpeechSpeedProp;
        protected SerializedProperty saSpeechAudioIdProp;
        protected SerializedProperty saVolumeProp;
        protected SerializedProperty saSilenceThresholdProp;
        protected SerializedProperty saEmotionsIgnoreSilenceProp;
        protected SerializedProperty saEmotionMinIntervalProp;
        protected SerializedProperty saEmotionMaxIntervalProp;
        protected SerializedProperty saEmotionFadeOnSilenceProp;
        #endregion

        #region Serialized Properties - Engage (React)
        protected SerializedProperty isReactSettingsExpandedProp;
        protected SerializedProperty isReactEnableOnInitProp;
        protected SerializedProperty reactNoNotifyDurationProp;
        protected SerializedProperty proximityColliderProp;
        protected SerializedProperty reactDistanceProp;

        private SerializedProperty isReactListExpandedProp;
        protected SerializedProperty reactListProp;
        protected SerializedProperty reactProp;
        protected SerializedProperty reactSelectedTabIntProp;
        protected SerializedProperty reactNameProp;
        protected SerializedProperty reactToProp;
        protected SerializedProperty reactModelsToIncludeProp;
        protected SerializedProperty reactShowInEditorProp;
        protected SerializedProperty isAutoEnterEmotionsProp;
        protected SerializedProperty isAutoExitEmotionsProp;
        protected SerializedProperty reactEnterPreviewModeProp;
        protected SerializedProperty reactExitPreviewModeProp;
        protected SerializedProperty enterReEmListProp;
        protected SerializedProperty enterReSpchAListProp;
        protected SerializedProperty exitReEmListProp;
        protected SerializedProperty exitReSpchAListProp;
        protected SerializedProperty isReEnterEmListExpandedProp;
        protected SerializedProperty isReEnterEvtsExpandedProp;
        protected SerializedProperty isReEnterSpchAListExpandedProp;
        protected SerializedProperty isReStayEmListExpandedProp;
        protected SerializedProperty isReStayEvtsExpandedProp;
        protected SerializedProperty isReStaySpchAListExpandedProp;
        protected SerializedProperty isReExitEmListExpandedProp;
        protected SerializedProperty isReExitEvtsExpandedProp;
        protected SerializedProperty isReExitSpchAListExpandedProp;
        protected SerializedProperty isReactEnterExpandedProp;
        protected SerializedProperty isReactStayExpandedProp;       
        protected SerializedProperty isReactExitExpandedProp;       
        protected SerializedProperty reEmProp;
        protected SerializedProperty reEmEmotionIdProp;
        protected SerializedProperty reEmIsSyncedProp;
        protected SerializedProperty reEmEmotionNameHashProp;
        protected SerializedProperty reSAProp;
        protected SerializedProperty reSASpeechAudioIdProp;
        protected SerializedProperty reSAIsSyncedProp;
        protected SerializedProperty reSANameHashProp;
        protected SerializedProperty reOnPreEnterProp;
        protected SerializedProperty reOnPostEnterProp;
        protected SerializedProperty reOnPreExitProp;
        protected SerializedProperty reOnPostExitProp;

        #endregion

        #region Events

        protected virtual void OnEnable()
        {
            stickyShapesModule = (StickyShapesModule)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            // Reset GUIStyles
            isStylesInitialised = false;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            toggleCompactButtonStyleToggledB = null;
            toggleButtonStyleNormal12 = null;
            toggleButtonStyleToggledB12 = null;
            foldoutStyleNoLabel = null;

            #region Find Properties - General
            selectedTabIntProp = serializedObject.FindProperty("selectedTabInt");
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            isNonS3DControllerProp = serializedObject.FindProperty("isNonS3DController");
            isSkinnedMRenListExpandedProp = serializedObject.FindProperty("isSkinnedMRenListExpanded");
            skinnedMRenListProp = serializedObject.FindProperty("skinnedMRenList");

            leftEyeProp = serializedObject.FindProperty("leftEye");
            rightEyeProp = serializedObject.FindProperty("rightEye");

            #endregion

            #region Find Properties - Blendshapes
            isBlendShapeListExpandedProp = serializedObject.FindProperty("isBlendShapeListExpanded");
            blendShapeListProp = serializedObject.FindProperty("blendShapeList");
            #endregion

            #region Find Properties - Blink
            blinkProp = serializedObject.FindProperty("blink");
            #endregion

            #region Find Properties - Eye Movement
            isEyeMovementExpandedProp = serializedObject.FindProperty("isEyeMovementExpanded");
            isEyeMoveEnableOnInitProp = serializedObject.FindProperty("isEyeMoveEnableOnInit");
            eyeMoveMinXProp = serializedObject.FindProperty("eyeMoveMinX");
            eyeMoveMaxXProp = serializedObject.FindProperty("eyeMoveMaxX");
            eyeMoveMinYProp = serializedObject.FindProperty("eyeMoveMinY");
            eyeMoveMaxYProp = serializedObject.FindProperty("eyeMoveMaxY");
            eyeMoveMinGazeDurationProp = serializedObject.FindProperty("eyeMoveMinGazeDuration");
            eyeMoveMaxGazeDurationProp = serializedObject.FindProperty("eyeMoveMaxGazeDuration");
            eyeMoveSpeedProp = serializedObject.FindProperty("eyeMoveSpeed");

            #endregion

            #region Find Properties - Emotions
            isEmotionListExpandedProp = serializedObject.FindProperty("isEmotionListExpanded");
            emotionListProp = serializedObject.FindProperty("emotionList");
            #endregion

            #region Find Properties - Phonemes
            isAudioSettingsExpandedProp = serializedObject.FindProperty("isAudioSettingsExpanded");
            isPhonemeListExpandedProp = serializedObject.FindProperty("isPhonemeListExpanded");
            isSpeechAudioListExpandedProp = serializedObject.FindProperty("isSpeechAudioListExpanded");
            phonemeAudioSourceProp = serializedObject.FindProperty("phonemeAudioSource");
            phonemeListProp = serializedObject.FindProperty("phonemeList");
            speechAudioListProp = serializedObject.FindProperty("speechAudioList");
            #endregion

            #region Find Properties - Engage
            isReactSettingsExpandedProp = serializedObject.FindProperty("isReactSettingsExpanded");
            isReactListExpandedProp = serializedObject.FindProperty("isReactListExpanded");
            isReactEnableOnInitProp = serializedObject.FindProperty("isReactEnableOnInit");
            reactNoNotifyDurationProp = serializedObject.FindProperty("reactNoNotifyDuration");
            proximityColliderProp = serializedObject.FindProperty("proximityCollider");
            reactDistanceProp = serializedObject.FindProperty("reactDistance");
            reactListProp = serializedObject.FindProperty("reactList");
            #endregion

            tabTexts = baseTabTexts;

            ValidateBlendShapes();

            AddTabs();
        }

        /// <summary>
        /// Gets called automatically 10 times per second
        /// Comment out if not required
        /// </summary>
        private void OnInspectorUpdate()
        {
            // OnInspectorGUI() only registers events when the mouse is positioned over the custom editor window
            // This code forces OnInspectorGUI() to run every frame, so it registers events even when the mouse
            // is positioned over the scene view
            if (stickyShapesModule.allowRepaint) { Repaint(); }
        }

        #endregion

        #region Private and Protected Methods

        /// <summary>
        /// Add a tab in the inspector
        /// </summary>
        /// <param name="newTab"></param>
        protected void AddTab(string newTab)
        {
            if (!string.IsNullOrEmpty(newTab))
            {
                ArrayUtility.Add(ref tabTexts, new GUIContent(newTab));
            }
        }

        /// <summary>
        /// If not in play mode, check if the scene has been modified and needs
        /// marking accordingly.
        /// </summary>
        protected void CheckMarkSceneDirty()
        {
            if (isSceneModified && !Application.isPlaying)
            {
                isSceneModified = false;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        /// <summary>
        /// Set up the buttons and styles used in OnInspectorGUI.
        /// Call this near the top of OnInspectorGUI.
        /// </summary>
        protected void ConfigureButtonsAndStyles()
        {
            // Set up rich text GUIStyles
            if (!isStylesInitialised)
            {
                helpBoxRichText = new GUIStyle("HelpBox");
                helpBoxRichText.richText = true;

                labelFieldRichText = new GUIStyle("Label");
                labelFieldRichText.richText = true;

                headingFieldRichText = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
                headingFieldRichText.richText = true;
                headingFieldRichText.normal.textColor = helpBoxRichText.normal.textColor;

                // Overide default styles
                EditorStyles.foldout.fontStyle = FontStyle.Bold;

                // When using a no-label foldout, don't forget to set the global
                // EditorGUIUtility.fieldWidth to a small value like 15, then back
                // to the original afterward.
                foldoutStyleNoLabel = new GUIStyle(EditorStyles.foldout);
                foldoutStyleNoLabel.fixedWidth = 0.01f;

                buttonCompact = StickyEditorHelper.GetCompactBtn(10);
                toggleCompactButtonStyleNormal = StickyEditorHelper.GetToggleCompactBtnStyleNormal(10);
                toggleCompactButtonStyleToggled = StickyEditorHelper.GetToggleCompactBtnStyleToggled(10);
                toggleCompactButtonStyleToggledB = StickyEditorHelper.GetToggleCompactBtnStyleToggledB(10);
                toggleButtonStyleNormal12 = StickyEditorHelper.GetToggleCompactBtnStyleNormal(12);
                toggleButtonStyleToggledB12 = StickyEditorHelper.GetToggleCompactBtnStyleToggledB(12);

                isStylesInitialised = true;
            }
        }

        /// <summary>
        /// Draw the audio source, including a new button, in the inspector.
        /// </summary>
        protected void DrawAudioSource()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(phonemeAudioSourceContent, GUILayout.Width(defaultEditorLabelWidth - 53f));

            if (GUILayout.Button(phonemeNewAudioContent, buttonCompact, GUILayout.Width(50f)) && phonemeAudioSourceProp.objectReferenceValue == null)
            {
                serializedObject.ApplyModifiedProperties();

                Undo.SetCurrentGroupName("New Audio Source");
                int undoGroup = UnityEditor.Undo.GetCurrentGroup();

                Undo.RecordObject(stickyShapesModule, string.Empty);

                GameObject audioGameObject = new GameObject("Phonetics");
                if (audioGameObject != null)
                {
                    Undo.RegisterCreatedObjectUndo(audioGameObject, string.Empty);
                    AudioSource _newAudioSource = Undo.AddComponent(audioGameObject, typeof(AudioSource)) as AudioSource;
                    _newAudioSource.playOnAwake = false;
                    _newAudioSource.maxDistance = 50f;
                    _newAudioSource.spatialBlend = 1f;
                    stickyShapesModule.SetPhonemeAudioSource(_newAudioSource);

                    audioGameObject.transform.SetParent(stickyShapesModule.transform, false);

                    // Set to approx mouth height. We could calc from height of character but that's prob overkill.
                    audioGameObject.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                }

                Undo.CollapseUndoOperations(undoGroup);

                // Should be non-scene objects but is required to force being set as dirty
                EditorUtility.SetDirty(stickyShapesModule);

                GUIUtility.ExitGUI();
            }

            EditorGUILayout.PropertyField(phonemeAudioSourceProp, GUIContent.none);

            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck() && phonemeAudioSourceProp.objectReferenceValue != null)
            {
                if (!((AudioSource)phonemeAudioSourceProp.objectReferenceValue).transform.IsChildOf(stickyShapesModule.transform))
                {
                    phonemeAudioSourceProp.objectReferenceValue = null;
                    Debug.LogWarning("The audio source transform must be a child of the parent Sticky3D Controller gameobject or part of the prefab.");
                }
            }
        }

        /// <summary>
        /// Draw Debugging in the inspector. Automatically calls DrawDebugging() which can be overridden.
        /// </summary>
        protected void DrawBaseDebugging()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && stickyShapesModule != null)
            {
                float rightLabelWidth = 175f;

                StickyEditorHelper.PerformanceImpact();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyShapesModule.IsShapesModuleInitialised ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugNumValidBShapesContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyShapesModule.NumValidBlendShapes.ToString("0"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsBlinkingEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyShapesModule.IsBlinkingEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                #region Emotion Debugging

                isShowEmotionDebugging = EditorGUILayout.Toggle(debugIsShowEmotionContent, isShowEmotionDebugging);

                if (isShowEmotionDebugging)
                {
                    if (emotionList == null) { emotionList = stickyShapesModule.GetEmotionList; }

                    int numEmotions = emotionList.Count;
                    float emIndent;

                    for (int emIdx = 0; emIdx < numEmotions; emIdx++)
                    {
                        S3DEmotion emotion = emotionList[emIdx];
                        if (emotion != null)
                        {
                            StickyEditorHelper.DrawUILine(separatorColor, 1, 3);
                            emIndent = 3f;

                            EditorGUILayout.BeginHorizontal();
                            StickyEditorHelper.DrawLabelIndent(emIndent, (emIdx + 1).ToString("00 ") + (string.IsNullOrEmpty(emotion.emotionName) ? "no name" : emotion.emotionName));
                            EditorGUILayout.EndHorizontal();

                            emIndent += 15f;

                            EditorGUILayout.BeginHorizontal();
                            StickyEditorHelper.DrawLabelIndent(emIndent, debugIsEmotionEnabledContent, defaultEditorLabelWidth- emIndent);
                            EditorGUILayout.LabelField(emotion.isEmotionEnabled ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            StickyEditorHelper.DrawLabelIndent(emIndent, debugEmotionIdContent, defaultEditorLabelWidth - emIndent);
                            EditorGUILayout.LabelField(emotion.GetEmotionId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                #endregion

                #region Engage Debugging

                isShowEngageDebugging = EditorGUILayout.Toggle(debugIsShowEngageContent, isShowEngageDebugging);

                if (isShowEngageDebugging)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsReactInitContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(stickyShapesModule.IsReactInitialised ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugIsReactEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(stickyShapesModule.IsReactingEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                    EditorGUILayout.EndHorizontal();
                }

                #endregion

                DrawDebugging();
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw an editable list of S3DEmotionShapes.
        /// </summary>
        /// <param name="emotionShapeListProp"></param>
        protected void DrawEmotionShapeList(SerializedProperty emotionShapeListProp, ref int deletePos, ref int moveDownPos)
        {
            #region Display Emotion Shape List

            int numEmotionShapes = emotionShapeListProp.arraySize;

            deletePos = -1;
            moveDownPos = -1;

            if (numEmotionShapes > 0 && modelBlendShapeNames == null)
            {
                RefreshBlendshapeModelNames();
            }

            EditorGUI.indentLevel++;
            for (int esIdx = 0; esIdx < numEmotionShapes; esIdx++)
            {
                emShapeProp = emotionShapeListProp.GetArrayElementAtIndex(esIdx);

                // Find the matching S3DBlendshape
                emShapeBlendShapeIdProp = emShapeProp.FindPropertyRelative("s3dBlendShapeId");
                emShapeIsSyncedProp = emShapeProp.FindPropertyRelative("isSynced");
                emShapeBlendShapeNameHashProp = emShapeProp.FindPropertyRelative("blendShapeNameHash");

                // Get blendshape hashed name from the S3DBlendShape being referenced from this emotion shape.
                int nameHash = stickyShapesModule.GetBlendShapeNameHash(emShapeBlendShapeIdProp.intValue);

                int modelNameIndex = -1;

                // If we couldn't find the blendshape it most like means it is out of sync
                if (nameHash == 0 && emShapeIsSyncedProp.boolValue)
                {
                    emShapeIsSyncedProp.boolValue = false;
                }

                if (!emShapeIsSyncedProp.boolValue)
                {
                    EditorGUILayout.HelpBox("Out of Sync with BlendShapes tab", MessageType.Warning);
                }
                else
                {
                    modelNameIndex = ArrayUtility.FindIndex(modelBlendShapeNameHashes, h => h == nameHash);
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                int nameListIndex = EditorGUILayout.Popup(" Blendshape " + (esIdx + 1).ToString("00"), modelNameIndex, modelBlendShapeNames, EditorStyles.toolbarPopup);
                if (EditorGUI.EndChangeCheck())
                {
                    string selectBlendShapeName = modelBlendShapeNames[nameListIndex];
                    int selectedBlendShapeHash = modelBlendShapeNameHashes[nameListIndex];

                    int selectedBlendShapeId = stickyShapesModule.GetBlendShapeIdByNameHash(selectedBlendShapeHash);

                    // Update the link to the S3DBlendShape
                    emShapeBlendShapeIdProp.intValue = selectedBlendShapeId;
                    // Update the name hash in case we get unlinked from the S3DBlendShape and need to re-sync.
                    emShapeBlendShapeNameHashProp.intValue = selectedBlendShapeHash;

                    emShapeIsSyncedProp.boolValue = selectedBlendShapeId != 0;
                }

                if (GUILayout.Button("X", GUILayout.MaxWidth(20f))) { deletePos = esIdx; }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(emShapeProp.FindPropertyRelative("maxWeight"), emShapeMaxWeightContent);

                if (esIdx < numEmotionShapes - 1)
                {
                    StickyEditorHelper.DrawHorizontalGap(4f);
                }
            }
            EditorGUI.indentLevel--;

            #endregion
        }

        /// <summary>
        /// Draw an editable list of S3DReactEmotions.
        /// </summary>
        /// <param name="reactEmotionListProp"></param>
        /// <param name="reactionStage"></param>
        /// <param name="deletePos"></param>
        /// <param name="moveDownPos"></param>
        protected void DrawReactEmotionList(SerializedProperty reactEmotionListProp, S3DReact.ReactionStage reactionStage, ref int deletePos, ref int moveDownPos)
        {
            #region Display React Emotion List

            int numReactEmotions = reactEmotionListProp.arraySize;

            bool isEnter = reactionStage == S3DReact.ReactionStage.Enter;
            bool isExit = reactionStage == S3DReact.ReactionStage.Exit;

            deletePos = -1;
            moveDownPos = -1;

            if (numReactEmotions > 0 && emotionNames == null)
            {
                RefreshEmotionNames();
            }

            //EditorGUI.indentLevel++;
            for (int reIdx = 0; reIdx < numReactEmotions; reIdx++)
            {
                // Get the react emotion property
                reEmProp = reactEmotionListProp.GetArrayElementAtIndex(reIdx);

                // Find the matching S3DEmotion
                reEmEmotionIdProp = reEmProp.FindPropertyRelative("s3dEmotionId");
                reEmIsSyncedProp = reEmProp.FindPropertyRelative("isSynced");
                reEmEmotionNameHashProp = reEmProp.FindPropertyRelative("emotionNameHash");

                // Currently need to fetch the emotion for the nameHash and the duration.
                // ** WARNING ** With lots of emotions and many reactions expanded, this might
                // get a bit laggy. If so, might need to consider some caching option.
                S3DEmotion _s3dEmotion = stickyShapesModule.GetEmotion(reEmEmotionIdProp.intValue);

                bool isEmotionFound = _s3dEmotion != null;

                // Get emotion hashed name from the S3DEmotion being referenced from this react emotion.
                //int nameHash = stickyShapesModule.GetEmotionNameHash(reEmEmotionIdProp.intValue);
                int nameHash = !isEmotionFound ? 0 : _s3dEmotion.GetEmotionNameHash;

                int emotionNameIndex = -1;

                // If we couldn't find the emotion it most like means it is out of sync
                if (nameHash == 0 && reEmIsSyncedProp.boolValue)
                {
                    reEmIsSyncedProp.boolValue = false;
                }

                if (!reEmIsSyncedProp.boolValue)
                {
                    EditorGUILayout.HelpBox("Out of Sync with Emotions tab", MessageType.Warning);
                }
                else
                {
                    emotionNameIndex = ArrayUtility.FindIndex(emotionNameHashes, h => h == nameHash);
                }

                EditorGUILayout.BeginHorizontal();
                StickyEditorHelper.DrawLabelIndent(20f, "Emotion " + (reIdx + 1).ToString("00"), defaultEditorLabelWidth-22f);
                EditorGUI.BeginChangeCheck();
                int nameListIndex = EditorGUILayout.Popup(emotionNameIndex, emotionNames, EditorStyles.toolbarPopup);
                if (EditorGUI.EndChangeCheck())
                {
                    string selectEmotionName = emotionNames[nameListIndex];
                    int selectedEmotionHash = emotionNameHashes[nameListIndex];                    

                    int selectedEmotionId = stickyShapesModule.GetEmotionIdByNameHash(selectedEmotionHash);

                    // Update the link to the S3DEmotion
                    reEmEmotionIdProp.intValue = selectedEmotionId;
                    // Update the name hash in case we get unlinked from the S3DEmotion and need to re-sync.
                    reEmEmotionNameHashProp.intValue = selectedEmotionHash;

                    reEmIsSyncedProp.boolValue = selectedEmotionId != 0;
                }

                if (GUILayout.Button("X", GUILayout.MaxWidth(20f))) { deletePos = reIdx; }
                EditorGUILayout.EndHorizontal();

                if (isEmotionFound && reEmIsSyncedProp.boolValue)
                {
                    float holdDuration = isEnter ? _s3dEmotion.holdEnterDuration : isExit ? _s3dEmotion.holdExitDuration : _s3dEmotion.holdStayDuration;

                    EditorGUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawLabelIndent(20f, reEmHoldDurationContent, defaultEditorLabelWidth - 22f);
                    float newValue = EditorGUILayout.Slider(holdDuration, 0f, 60f);
                    EditorGUILayout.EndHorizontal();

                    if (newValue != holdDuration)
                    {
                        serializedObject.ApplyModifiedProperties();
                        if (isEnter)
                        {
                            Undo.RecordObject(stickyShapesModule, "Change Enter Duration");
                            _s3dEmotion.holdEnterDuration = newValue;
                        }
                        else if (isExit)
                        {
                            Undo.RecordObject(stickyShapesModule, "Change Exit Duration");
                            _s3dEmotion.holdExitDuration = newValue;
                        }
                        else
                        {
                            Undo.RecordObject(stickyShapesModule, "Change Stay Duration");
                            _s3dEmotion.holdStayDuration = newValue;
                        }
                        serializedObject.Update();
                    }
                }

                //EditorGUILayout.PropertyField(emShapeProp.FindPropertyRelative("maxWeight"), emShapeMaxWeightContent);

                if (reIdx < numReactEmotions - 1)
                {
                    StickyEditorHelper.DrawHorizontalGap(4f);
                }
            }
            //EditorGUI.indentLevel--;

            #endregion
        }

        /// <summary>
        /// Draw an editable list of S3DReactSpeechAudios.
        /// </summary>
        /// <param name="reactSpeechAudioListProp"></param>
        /// <param name="reactionStage"></param>
        /// <param name="deletePos"></param>
        /// <param name="moveDownPos"></param>
        protected void DrawReactSpeechAudioList(SerializedProperty reactSpeechAudioListProp, S3DReact.ReactionStage reactionStage, ref int deletePos, ref int moveDownPos)
        {
            #region Display React Speech Audio List

            int numReactSpeechAudios = reactSpeechAudioListProp.arraySize;

            bool isEnter = reactionStage == S3DReact.ReactionStage.Enter;
            bool isExit = reactionStage == S3DReact.ReactionStage.Exit;

            deletePos = -1;
            moveDownPos = -1;

            if (numReactSpeechAudios > 0 && (isSpeechAudioNamesStale || speechAudioNames == null))
            {
                RefreshSpeechAudioNames();
                isSpeechAudioNamesStale = false;
            }

            for (int saIdx = 0; saIdx < numReactSpeechAudios; saIdx++)
            {
                // Get the react emotion property
                reSAProp = reactSpeechAudioListProp.GetArrayElementAtIndex(saIdx);

                // Find the matching S3DSpeechAudio
                reSASpeechAudioIdProp = reSAProp.FindPropertyRelative("s3dSpeechAudioId");
                reSAIsSyncedProp = reSAProp.FindPropertyRelative("isSynced");
                reSANameHashProp = reSAProp.FindPropertyRelative("speechAudioNameHash");

                // Currently need to fetch the speech audio for the nameHash.
                // ** WARNING ** With lots of speech audios and many reactions expanded, this might
                // get a bit laggy. If so, might need to consider some caching option.
                S3DSpeechAudio _s3dSpeechAudio = stickyShapesModule.GetSpeechAudio(reSASpeechAudioIdProp.intValue);

                bool isSpeechAudioFound = _s3dSpeechAudio != null;

                // Get speech audio hashed name from the S3DSpeechAudio being referenced from this react speechAudio.
                int nameHash = !isSpeechAudioFound ? 0 : _s3dSpeechAudio.GetClipNameHash;

                int clipNameIndex = -1;

                // If we couldn't find the emotion it most like means it is out of sync
                if (nameHash == 0 && reSAIsSyncedProp.boolValue)
                {
                    reSAIsSyncedProp.boolValue = false;
                }

                if (!reSAIsSyncedProp.boolValue)
                {
                    EditorGUILayout.HelpBox("Out of Sync with Phonemes tab", MessageType.Warning);
                }
                else
                {
                    clipNameIndex = ArrayUtility.FindIndex(speechAudioNameHashes, h => h == nameHash);
                }

                EditorGUILayout.BeginHorizontal();
                StickyEditorHelper.DrawLabelIndent(20f, "Speech Audio " + (saIdx + 1).ToString("00"), defaultEditorLabelWidth - 22f);
                EditorGUI.BeginChangeCheck();
                int nameListIndex = EditorGUILayout.Popup(clipNameIndex, speechAudioNames, EditorStyles.toolbarPopup);
                if (EditorGUI.EndChangeCheck())
                {
                    string selectSpeechAudioName = speechAudioNames[nameListIndex];
                    int selectedSpeechAudioHash = speechAudioNameHashes[nameListIndex];

                    int selectedSpeechAudioId = stickyShapesModule.GetSpeechAudioIdByNameHash(selectedSpeechAudioHash);

                    // Update the link to the S3DEmotion
                    reSASpeechAudioIdProp.intValue = selectedSpeechAudioId;
                    // Update the name hash in case we get unlinked from the S3DSpeechAudio and need to re-sync.
                    reSANameHashProp.intValue = selectedSpeechAudioHash;

                    reSAIsSyncedProp.boolValue = selectedSpeechAudioId != 0;
                }
                if (GUILayout.Button("X", GUILayout.MaxWidth(20f))) { deletePos = saIdx; }
                EditorGUILayout.EndHorizontal();

            }

            #endregion
        }

        /// <summary>
        /// Attempt to refresh the array of blendshapes names and hashed names from the model
        /// </summary>
        protected bool RefreshBlendshapeModelNames()
        {
            bool isEmpty = true;

            if (tempNameList == null) { tempNameList = new List<string>(20); }

            if (tempNameList != null)
            {
                tempNameList.Clear();

                if (stickyShapesModule.GetBlendShapeNamesFromModel(tempNameList))
                {
                    isEmpty = false;
                    modelBlendShapeNames = tempNameList.ToArray();

                    modelBlendShapeNameHashes = S3DMath.GetHashCodes(modelBlendShapeNames);
                }
            }
            
            if (isEmpty)
            {
                modelBlendShapeNames = new string[] { "None" };
                modelBlendShapeNameHashes = new int[] { -1 };
            }

            return !isEmpty;
        }

        /// <summary>
        /// Attempt to refresh the array of emotion names and hashed names from the Emotions tab.
        /// </summary>
        /// <returns></returns>
        protected bool RefreshEmotionNames()
        {
            bool isEmpty = true;

            if (tempNameList == null) { tempNameList = new List<string>(20); }

            if (tempNameList != null)
            {
                tempNameList.Clear();

                if (stickyShapesModule.GetEmotionNames(tempNameList))
                {
                    isEmpty = false;
                    emotionNames = tempNameList.ToArray();

                    emotionNameHashes = S3DMath.GetHashCodes(emotionNames);
                }
            }
            
            if (isEmpty)
            {
                emotionNames = new string[] { "None" };
                emotionNameHashes = new int[] { -1 };
            }

            return !isEmpty;
        }

        /// <summary>
        /// Attempt to refresh the array of speech audio names and hashed names from the Phonenes tab.
        /// </summary>
        /// <returns></returns>
        protected bool RefreshSpeechAudioNames()
        {
            bool isEmpty = true;

            if (tempNameList == null) { tempNameList = new List<string>(20); }

            if (tempNameList != null)
            {
                tempNameList.Clear();

                if (stickyShapesModule.GetSpeechAudioNames(tempNameList))
                {
                    isEmpty = false;
                    speechAudioNames = tempNameList.ToArray();

                    speechAudioNameHashes = S3DMath.GetHashCodes(speechAudioNames);
                }
            }
            
            if (isEmpty)
            {
                speechAudioNames = new string[] { "None" };
                speechAudioNameHashes = new int[] { -1 };
            }

            return !isEmpty;
        }

        /// <summary>
        /// Attempt to validate the blendshapes
        /// </summary>
        protected void ValidateBlendShapes()
        {
            bool hasChanged;
            stickyShapesModule.ValidateBlendShapes(out hasChanged);

            if (hasChanged && !Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }


        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Override this method to add new tabs to the toolbar.
        /// Then override DrawOtherSettings().
        /// </summary>
        protected virtual void AddTabs()
        {
            //AddTab("Test");
        }

        /// <summary>
        /// This function overides what is normally seen in the inspector window
        /// This allows stuff like buttons to be drawn there
        /// </summary>
        protected virtual void DrawBaseInspector()
        {
            #region Initialise
            stickyShapesModule.allowRepaint = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            isSceneModified = false;
            #endregion

            ConfigureButtonsAndStyles();

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            StickyEditorHelper.DrawStickyVersionLabel(labelFieldRichText);
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawGetHelpButtons(buttonCompact);
            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.BeginVertical("HelpBox");

            DrawBaseSettings();

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            DrawBaseDebugging();

            stickyShapesModule.allowRepaint = true;
        }

        /// <summary>
        /// Draw the base settings that should be common to most StickyShapes modules.
        /// </summary>
        protected virtual void DrawBaseSettings()
        {
            StickyEditorHelper.DrawS3DToolbar(selectedTabIntProp, tabTexts);

            if (selectedTabIntProp.intValue == 0)
            {
                DrawGeneralSettings();
            }
            else if (selectedTabIntProp.intValue == 1)
            {
                DrawBlendShapeSettings();
            }
            else if (selectedTabIntProp.intValue == 2)
            {
                DrawBlinkSettings();
                DrawEyeMovementSettings();
                DrawEmotionsSettings();
            }
            else if (selectedTabIntProp.intValue == 3)
            {
                DrawPhoneticSettings();
            }
            else if (selectedTabIntProp.intValue == 4)
            {
                DrawEngageSettings();
            }
        }

        /// <summary>
        /// Draw the blendshape tab settings in the inspector
        /// </summary>
        protected virtual void DrawBlendShapeSettings()
        {
            if (GUILayout.Button(getBlendShapesContent, GUILayout.MaxWidth(120f)))
            {
                serializedObject.ApplyModifiedProperties();

                // Force changes to be noted
                Undo.RecordObject(stickyShapesModule, "Update Blendshape List");

                if (stickyShapesModule.GetSkinnedMeshRendererList.Count < 1)
                {
                    stickyShapesModule.GetSkinnedMeshRenderers(stickyShapesModule.GetSkinnedMeshRendererList);
                }

                stickyShapesModule.GetBlendShapesFromModel(stickyShapesModule.GetBlendShapeList);
                GUIUtility.ExitGUI();
                return;
            }

            //StickyEditorHelper.DrawList(blendShapeListProp, isBlendShapeListExpandedProp, blendShapeListContent, 40f, "", buttonCompact, foldoutStyleNoLabel, defaultEditorFieldWidth);

            #region Add/Remove Buttons
            GUILayout.BeginHorizontal();

            EditorGUI.indentLevel += 1;
            EditorGUIUtility.fieldWidth = 15f;
            isBlendShapeListExpandedProp.boolValue = EditorGUILayout.Foldout(isBlendShapeListExpandedProp.boolValue, "", foldoutStyleNoLabel);
            EditorGUI.indentLevel -= 1;

            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

            // Create a revised GUIContent and don't include a width so that the Buttons right align
            EditorGUILayout.LabelField(new GUIContent(blendShapeListContent.text + blendShapeListProp.arraySize.ToString(": 00"), blendShapeListContent.tooltip));

            if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
            {
                blendShapeListProp.arraySize++;
                serializedObject.ApplyModifiedProperties();
                stickyShapesModule.GetBlendShapeList[blendShapeListProp.arraySize - 1].SetClassDefaults();
                serializedObject.Update();
            }

            if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
            {
                if (blendShapeListProp.arraySize > 0) { blendShapeListProp.arraySize--; }
            }

            GUILayout.EndHorizontal();
            #endregion

            int deleteItem = -1;
            //float labelWidth = 40f;
            string elementName = "";

            if (isBlendShapeListExpandedProp.boolValue)
            {
                #region Blendshape List
                EditorGUI.indentLevel++;
                StickyEditorHelper.DrawHorizontalGap(2f);
                for (int arrayIdx = 0; arrayIdx < blendShapeListProp.arraySize; arrayIdx++)
                {
                    blendShapeProp = blendShapeListProp.GetArrayElementAtIndex(arrayIdx);

                    bsIsNameMatchedProp = blendShapeProp.FindPropertyRelative("isNameMatched");

                    GUILayout.BeginHorizontal();
                    elementName = GetBlendShapeName(blendShapeProp);
                    EditorGUILayout.LabelField((arrayIdx + 1).ToString("00") + (bsIsNameMatchedProp.boolValue ? "" : " [Name Mismatch] ") + (string.IsNullOrEmpty(elementName) ? " **missing**" : " " + elementName));
                    if (GUILayout.Button("X", GUILayout.MaxWidth(20f))) { deleteItem = arrayIdx; }
                    GUILayout.EndHorizontal();

                    //EditorGUILayout.PropertyField(blendShapeListProp.GetArrayElementAtIndex(arrayIdx), GUIContent.none);
                }
                EditorGUI.indentLevel--;
                #endregion

                if (deleteItem >= 0)
                {
                    blendShapeListProp.DeleteArrayElementAtIndex(deleteItem);
                }
            }
        }

        /// <summary>
        /// Overrideable method to draw the blink settings in the editor
        /// </summary>
        protected virtual void DrawBlinkSettings()
        {
            // Set a custom field width for these settings
            float origDefELWidth = defaultEditorLabelWidth;
            defaultEditorLabelWidth = 120f;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;

            blkShowInEditorProp = blinkProp.FindPropertyRelative("showInEditor");
            blkEmShapeListProp = blinkProp.FindPropertyRelative("emotionShapeList");
            blkIsSimpleBlinkProp = blinkProp.FindPropertyRelative("isSimpleBlink");

            StickyEditorHelper.DrawS3DFoldout(blkShowInEditorProp, blinkSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (blkShowInEditorProp.boolValue)
            {
                isBlinkingModified = false;
                if (Application.isPlaying) { isBlinkingEnabled = stickyShapesModule.IsBlinkingEnabled; }

                EditorGUILayout.PropertyField(blinkProp.FindPropertyRelative("isEnableOnInitialise"), blinkIsEnableOnInitContent);
                EditorGUILayout.PropertyField(blkIsSimpleBlinkProp, blinkIsSimpleContent);
                EditorGUILayout.PropertyField(blinkProp.FindPropertyRelative("blinkDuration"), blinkDurationContent);

                StickyEditorHelper.DrawMinMaxSliders(blinkProp.FindPropertyRelative("minBlinkInterval"), blinkProp.FindPropertyRelative("maxBlinkInterval"), blinkMinIntervalContent, blinkMaxIntervalContent);

                StickyEditorHelper.DrawHorizontalGap(3f);

                #region Add/Remove Emotion Shapes
                GUILayout.BeginHorizontal();
                // Create a revised GUIContent and don't include a width so that the Buttons right align
                EditorGUILayout.LabelField(new GUIContent(emShapeListContent.text + blkEmShapeListProp.arraySize.ToString(": 00"), emShapeListContent.tooltip));

                if (GUILayout.Button(blinkSyncContent, GUILayout.MaxWidth(40f)))
                {
                    Undo.RecordObject(stickyShapesModule, "Sync with BlendShapes tab");
                    stickyShapesModule.ResyncBlinkShapes();
                    GUIUtility.ExitGUI();
                    return;
                }

                if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
                {
                    blkEmShapeListProp.arraySize++;
                    serializedObject.ApplyModifiedProperties();

                    if (isBlinkingEnabled) { stickyShapesModule.StopBlinking(); isBlinkingModified = true; }

                    stickyShapesModule.GetBlink.emotionShapeList[blkEmShapeListProp.arraySize - 1].SetClassDefaults();

                    serializedObject.Update();
                }

                if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                {
                    if (blkEmShapeListProp.arraySize > 0)
                    {
                        if (isBlinkingEnabled) { stickyShapesModule.StopBlinking(); isBlinkingModified = true; }
                        blkEmShapeListProp.arraySize--;
                    }
                }

                GUILayout.EndHorizontal();
                #endregion

                blkemShapeDeletePos = -1;
                blkemShapeMoveDownPos = -1;

                DrawEmotionShapeList(blkEmShapeListProp, ref blkemShapeDeletePos, ref blkemShapeMoveDownPos);

                if (isBlinkingEnabled && (blkemShapeDeletePos >= 0 || blkemShapeMoveDownPos >= 0)) { stickyShapesModule.StopBlinking(); isBlinkingModified = true; }

                StickyEditorHelper.DrawDeleteMoveInList(blkEmShapeListProp, ref blkemShapeDeletePos, ref blkemShapeMoveDownPos);

                if (isBlinkingModified && isBlinkingEnabled)
                {
                    // Ensure the new list is applied before restarting blinking
                    serializedObject.ApplyModifiedProperties();
                    stickyShapesModule.StartBlinking();
                    serializedObject.Update();
                }
            }

            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            // Restore the field width
            defaultEditorLabelWidth = origDefELWidth;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
        }

        /// <summary>
        /// Overrideable method for adding more debugging output
        /// </summary>
        protected virtual void DrawDebugging()
        {

        }

        /// <summary>
        /// Draw the eye movement settings in the inspector
        /// </summary>
        protected virtual void DrawEyeMovementSettings()
        {
            // Set a custom field width for these settings
            float origDefELWidth = defaultEditorLabelWidth;
            defaultEditorLabelWidth = 120f;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;

            StickyEditorHelper.DrawS3DFoldout(isEyeMovementExpandedProp, eyeMovementSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (isEyeMovementExpandedProp.boolValue)
            {
                EditorGUILayout.PropertyField(isEyeMoveEnableOnInitProp, isEyeMoveEnableOnInitContent);

                EditorGUI.BeginChangeCheck();
                StickyEditorHelper.DrawMinMaxSlider(eyeMoveMinXProp, eyeMoveMaxXProp, eyeMoveXContent, -30f, 30f, defaultEditorLabelWidth);
                StickyEditorHelper.DrawMinMaxSlider(eyeMoveMinYProp, eyeMoveMaxYProp, eyeMoveYContent, -30f, 30f, defaultEditorLabelWidth);
                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    if (stickyShapesModule.IsEyeMovementEnabled)
                    {
                        stickyShapesModule.StopEyeMovement();
                        stickyShapesModule.StartEyeMovement();
                    }
                }

                StickyEditorHelper.DrawMinMaxSlider(eyeMoveMinGazeDurationProp, eyeMoveMaxGazeDurationProp, eyeMoveGazeDurationContent, 0f, 10f, defaultEditorLabelWidth);
                EditorGUILayout.PropertyField(eyeMoveSpeedProp, eyeMoveSpeedContent);
            }

            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            // Restore the field width
            defaultEditorLabelWidth = origDefELWidth;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
        }

        /// <summary>
        /// Draw the emotions tab settings in the inspector
        /// </summary>
        protected virtual void DrawEmotionsSettings()
        {
            #region Emotion Buttons

            GUILayout.BeginHorizontal();
            StickyEditorHelper.DrawS3DFoldout(isEmotionListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);

            // Create a revised GUIContent and don't include a width so that the Buttons right align
            EditorGUILayout.LabelField(new GUIContent(emotionListContent.text + emotionListProp.arraySize.ToString(": 0"), emotionListContent.tooltip));

            emotionDeletePos = -1;
            emotionMoveDownPos = -1;
            emotionInsertPos = -1;

            if (GUILayout.Button(emotionSyncContent, GUILayout.MaxWidth(40f)))
            {
                Undo.RecordObject(stickyShapesModule, "Sync with BlendShapes tab");
                stickyShapesModule.ResyncEmotions();
                GUIUtility.ExitGUI();
                return;
            }

            if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
            {
                //Undo.RecordObject(stickyShapesModule, "Add Emotion");
                emotionListProp.arraySize++;
                serializedObject.ApplyModifiedProperties();
                stickyShapesModule.GetEmotionList[emotionListProp.arraySize - 1].SetClassDefaults();
                serializedObject.Update();
                if (!isEmotionListExpandedProp.boolValue) { isEmotionListExpandedProp.boolValue = true; }
            }

            if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
            {
                if (emotionListProp.arraySize > 0)
                {
                    emotionDeletePos = emotionListProp.arraySize - 1;
                }
            }

            GUILayout.EndHorizontal();
            #endregion

            string elementName = "";
            bool hasToggled = false;

            if (isEmotionListExpandedProp.boolValue)
            {
                #region Emotion List
                StickyEditorHelper.DrawHorizontalGap(2f);
                int numEmotions = emotionListProp.arraySize;

                if (numEmotions > 0) { StickyEditorHelper.DrawHorizontalGap(2f); }

                for (int emIdx = 0; emIdx < numEmotions; emIdx++)
                {                    
                    emotionProp = emotionListProp.GetArrayElementAtIndex(emIdx);

                    #region Find Properties
                    emNameProp = emotionProp.FindPropertyRelative("emotionName");
                    emShowInEditorProp = emotionProp.FindPropertyRelative("showInEditor");
                    emPreviewModeProp = emotionProp.FindPropertyRelative("isPreviewMode");
                    #endregion

                    #region Emotion Buttons
                    elementName = (emIdx + 1).ToString("00 ") + (string.IsNullOrEmpty(emNameProp.stringValue) ? "**NO NAME**" : emNameProp.stringValue);

                    StickyEditorHelper.DrawUILine(separatorColor, 1, 6);

                    GUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawS3DFoldout(emShowInEditorProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                    EditorGUILayout.LabelField(elementName);
                    // Preview Mode button
                    hasToggled = StickyEditorHelper.DrawToggleButton(emPreviewModeProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled, emPrevBtnContent, 30f);
                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numEmotions > 1) { emotionMoveDownPos = emIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { emotionInsertPos = emIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { emotionDeletePos = emIdx; }
                    GUILayout.EndHorizontal();

                    if (hasToggled)
                    {
                        if (EditorApplication.isPlaying)
                        {
                            if (emPreviewModeProp.boolValue) { stickyShapesModule.StartEmotion(emIdx + 1); }
                            else { stickyShapesModule.StopEmotion(emIdx + 1); }
                            serializedObject.ApplyModifiedProperties();
                            GUIUtility.ExitGUI();
                            return;
                        }
                        else
                        {
                            StickyEditorHelper.PromptGotIt("Preview", "Run the scene to use the preview button");
                        }
                    }

                    StickyEditorHelper.DrawHorizontalGap(2f);
                    #endregion

                    if (emShowInEditorProp.boolValue)
                    {
                        #region Find Emotion Properties
                        emFadeInDurationProp = emotionProp.FindPropertyRelative("fadeInDuration");
                        emFadeOutDurationProp = emotionProp.FindPropertyRelative("fadeOutDuration");
                        emVOXEmotionProp = emotionProp.FindPropertyRelative("isVoxEmotion");
                        emAngerIntensityProp = emotionProp.FindPropertyRelative("angerIntensity");
                        emJoyIntensityProp = emotionProp.FindPropertyRelative("joyIntensity");
                        emSurpriseIntensityProp = emotionProp.FindPropertyRelative("surpriseIntensity");
                        emTrustIntensityProp = emotionProp.FindPropertyRelative("trustIntensity");
                        emShapeListProp = emotionProp.FindPropertyRelative("emotionShapeList");
                        #endregion

                        EditorGUILayout.PropertyField(emNameProp, emNameContent);
                        EditorGUILayout.PropertyField(emFadeInDurationProp, emFadeInDurationContent);
                        EditorGUILayout.PropertyField(emFadeOutDurationProp, emFadeOutDurationContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(emVOXEmotionProp, emVOXEmotionContent);
                        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                        {
                            serializedObject.ApplyModifiedProperties();
                            stickyShapesModule.RefreshVOXEmotions();
                            serializedObject.Update();
                        }

                        EditorGUILayout.PropertyField(emAngerIntensityProp, emAngerIntensityContent);
                        EditorGUILayout.PropertyField(emJoyIntensityProp, emJoyIntensityContent);
                        EditorGUILayout.PropertyField(emSurpriseIntensityProp, emSurpriseIntensityContent);
                        EditorGUILayout.PropertyField(emTrustIntensityProp, emTrustIntensityContent);

                        #region Add/Remove Emotion Shapes
                        StickyEditorHelper.DrawHorizontalGap(2f);
                        GUILayout.BeginHorizontal();
                        // Create a revised GUIContent and don't include a width so that the Buttons right align
                        EditorGUILayout.LabelField(new GUIContent(emShapeListContent.text + emShapeListProp.arraySize.ToString(": 00"), emShapeListContent.tooltip));

                        if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
                        {
                            emShapeListProp.arraySize++;
                            serializedObject.ApplyModifiedProperties();
                            stickyShapesModule.GetEmotionList[emIdx].emotionShapeList[emShapeListProp.arraySize - 1].SetClassDefaults();
                            serializedObject.Update();
                        }

                        if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                        {
                            if (emShapeListProp.arraySize > 0)
                            {
                                if (emPreviewModeProp.boolValue)
                                {
                                    // If deleting a single emotionShape, stop previewing the whole emotion
                                    stickyShapesModule.StopEmotionInstantly(emIdx + 1);
                                    emPreviewModeProp.boolValue = false;
                                }
                                emShapeListProp.arraySize--;
                            }
                        }

                        GUILayout.EndHorizontal();
                        #endregion

                        DrawEmotionShapeList(emShapeListProp, ref emotionShapeDeletePos, ref emotionShapeMoveDownPos);

                        if (emotionShapeDeletePos >=0 && emPreviewModeProp.boolValue)
                        {
                            // If deleting a single emotionShape, stop previewing the whole emotion
                            stickyShapesModule.StopEmotionInstantly(emIdx+1);
                            emPreviewModeProp.boolValue = false;
                        }

                        StickyEditorHelper.DrawDeleteMoveInList(emShapeListProp, ref emotionShapeDeletePos, ref emotionShapeMoveDownPos);
                    }
                }
                #endregion

                #region Move/Insert/Delete Emotions
                if (emotionMoveDownPos >= 0 || emotionInsertPos >= 0 || emotionDeletePos >= 0)
                {
                    GUI.FocusControl(null);
                    // Don't permit multiple operations in the same pass
                    if (emotionMoveDownPos >= 0)
                    {
                        if (emotionListProp.arraySize > 1)
                        {
                            List<S3DEmotion> emList = stickyShapesModule.GetEmotionList;

                            // Apply property changes before potential list changes
                            serializedObject.ApplyModifiedProperties();

                            // Force changes to be noted
                            Undo.RecordObject(stickyShapesModule, "Move Emotion");

                            // Move down one position, or wrap round to 1st position in list
                            if (emotionMoveDownPos < emotionListProp.arraySize - 1)
                            {
                                emList.Insert(emotionMoveDownPos + 2, emList[emotionMoveDownPos]);
                                emList.RemoveAt(emotionMoveDownPos);

                                //emotionListProp.MoveArrayElement(emotionMoveDownPos, emotionMoveDownPos + 1);
                            }
                            else
                            {
                                emList.Insert(0, emList[emotionMoveDownPos]);
                                emList.RemoveAt(emotionMoveDownPos+1);

                                //emotionListProp.MoveArrayElement(emotionMoveDownPos, 0);
                            }

                            emotionMoveDownPos = -1;
                            GUIUtility.ExitGUI();
                            return;
                        }
                        else
                        {
                            emotionMoveDownPos = -1;
                        }
                    }
                    else if (emotionInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        // Force changes to be noted
                        Undo.RecordObject(stickyShapesModule, "Insert Emotion");

                        List<S3DEmotion> emList = stickyShapesModule.GetEmotionList;

                        S3DEmotion insertedEmotion = new S3DEmotion(emList[emotionInsertPos]);
                        insertedEmotion.showInEditor = true;
                        // Generate a new hashcode for the duplicated emotion
                        insertedEmotion.guidHash = S3DMath.GetHashCodeFromGuid();

                        emList.Insert(emotionInsertPos, insertedEmotion);

                        // Hide original Emotion
                        emList[emotionInsertPos + 1].showInEditor = false;

                        emotionInsertPos = -1;
                        GUIUtility.ExitGUI();
                        return;                        
                    }
                    else if (emotionDeletePos >= 0)
                    {
                        stickyShapesModule.StopEmotionInstantly(emotionDeletePos + 1);

                        // Modify the list rather than emotionListProp.DeleteArrayElementAtIndex(..)
                        // which seems to reset all non-serialised field in other list members...
                        serializedObject.ApplyModifiedProperties();
                        // Force changes to be noted
                        Undo.RecordObject(stickyShapesModule, "Delete Emotion");
                        List<S3DEmotion> emList = stickyShapesModule.GetEmotionList;
                        emList.RemoveAt(emotionDeletePos);
                        emotionDeletePos = -1;
                        stickyShapesModule.RefreshVOXEmotions();

                        GUIUtility.ExitGUI();
                        return;
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Draw the general settings in the inspector
        /// </summary>
        protected virtual void DrawGeneralSettings()
        {
            EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);
            EditorGUILayout.PropertyField(isNonS3DControllerProp, isNonS3DControllerContent);

            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            if (GUILayout.Button(getSkinnedMRenContent, GUILayout.MaxWidth(175f)))
            {
                FindSkinnedMeshRenderers();
            }

            StickyEditorHelper.DrawList(skinnedMRenListProp, isSkinnedMRenListExpandedProp, skinnedMRenListContent, 40f, "", buttonCompact, foldoutStyleNoLabel, defaultEditorFieldWidth);

            #region Eye Transforms

            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            if (GUILayout.Button(getEyesContent, GUILayout.MaxWidth(100f)))
            {
                serializedObject.ApplyModifiedProperties();

                Undo.RecordObject(stickyShapesModule, "Get Eye Transforms");

                stickyShapesModule.ReinitialiseEyes();
                serializedObject.Update();
            }

            leftEyeTfrmProp = leftEyeProp.FindPropertyRelative("eyeTfrm");
            rightEyeTfrmProp = rightEyeProp.FindPropertyRelative("eyeTfrm");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(leftEyeTfrmProp, leftEyeTrfmContent);
            if (EditorGUI.EndChangeCheck() && leftEyeTfrmProp.objectReferenceValue != null)
            {
                StickyControlModule stickyControlModule = stickyShapesModule.GetStickyControlModule();

                if (!((Transform)leftEyeTfrmProp.objectReferenceValue).IsChildOf(stickyControlModule.transform))
                {
                    leftEyeTfrmProp.objectReferenceValue = null;
                    Debug.LogWarning("The left eye must be a child of the Sticky3D Controller gameobject or part of the prefab.");
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(rightEyeTfrmProp, rightEyeTrfmContent);
            if (EditorGUI.EndChangeCheck() && rightEyeTfrmProp.objectReferenceValue != null)
            {
                StickyControlModule stickyControlModule = stickyShapesModule.GetStickyControlModule();

                if (!((Transform)rightEyeTfrmProp.objectReferenceValue).IsChildOf(stickyControlModule.transform))
                {
                    rightEyeTfrmProp.objectReferenceValue = null;
                    Debug.LogWarning("The right eye must be a child of the Sticky3D Controller gameobject or part of the prefab.");
                }
            }

            #endregion
        }

        /// <summary>
        /// Place your own code here for new tabs you have added
        /// </summary>
        protected virtual void DrawOtherSettings()
        {
            // Start from the first new tab added
            //int firstNewTabIndex = baseTabTexts.Length;
            //if (selectedTabIntProp.intValue == firstNewTabIndex)
            //{

            //}
            //else if (selectedTabIntProp.intValue == firstNewTabIndex+1)
            //{

            //}
        }

        /// <summary>
        /// Draw the phonetic tab settings in the inspector
        /// </summary>
        protected virtual void DrawPhoneticSettings()
        {
            #region Audio or Phonetic Settings

            StickyEditorHelper.DrawS3DFoldout(isAudioSettingsExpandedProp, isAudioSettingsExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (isAudioSettingsExpandedProp.boolValue)
            {
                DrawAudioSource();
            }

            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            #endregion

            #region Phoneme Buttons
            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel += 1;
            EditorGUIUtility.fieldWidth = 15f;
            isPhonemeListExpandedProp.boolValue = EditorGUILayout.Foldout(isPhonemeListExpandedProp.boolValue, "", foldoutStyleNoLabel);
            EditorGUI.indentLevel -= 1;

            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

            // Create a revised GUIContent and don't include a width so that the Buttons right align
            EditorGUILayout.LabelField(new GUIContent(phonemeListContent.text + phonemeListProp.arraySize.ToString(": 0"), phonemeListContent.tooltip));

            phonemeDeletePos = -1;
            phonemeMoveDownPos = -1;
            phonemeInsertPos = -1;

            if (GUILayout.Button(phonemeSyncContent, GUILayout.MaxWidth(40f)))
            {
                Undo.RecordObject(stickyShapesModule, "Sync with BlendShapes tab");
                stickyShapesModule.ResyncPhonemes();
                GUIUtility.ExitGUI();
                return;
            }

            if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
            {
                phonemeListProp.arraySize++;
                serializedObject.ApplyModifiedProperties();
                stickyShapesModule.GetPhonemeList[phonemeListProp.arraySize - 1].SetClassDefaults();
                serializedObject.Update();
                if (!isPhonemeListExpandedProp.boolValue) { isPhonemeListExpandedProp.boolValue = true; }
            }

            if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
            {
                if (phonemeListProp.arraySize > 0)
                {
                    phonemeDeletePos = phonemeListProp.arraySize - 1;
                }
            }

            GUILayout.EndHorizontal();
            #endregion

            string elementName = "";
            bool hasToggled = false;

            if (isPhonemeListExpandedProp.boolValue)
            {
                #region Phoneme List
                StickyEditorHelper.DrawHorizontalGap(2f);
                int numPhonemes = phonemeListProp.arraySize;

                if (numPhonemes > 0) { StickyEditorHelper.DrawHorizontalGap(2f); }

                for (int phIdx = 0; phIdx < numPhonemes; phIdx++)
                {
                    phonemeProp = phonemeListProp.GetArrayElementAtIndex(phIdx);

                    phNameProp = phonemeProp.FindPropertyRelative("phonemeName");
                    phShowInEditorProp = phonemeProp.FindPropertyRelative("showInEditor");
                    phPreviewModeProp = phonemeProp.FindPropertyRelative("isPreviewMode");
                    phShapeFrequencyProp = phonemeProp.FindPropertyRelative("frequency");
                    phShapeDurationProp = phonemeProp.FindPropertyRelative("duration");

                    elementName = (phIdx + 1).ToString("00 ") + (string.IsNullOrEmpty(phNameProp.stringValue) ? "**NO NAME**" : phNameProp.stringValue + " [" + StickyEditorHelper.GetFloatText(phShapeDurationProp.floatValue * 1000f, 0) + "ms]");

                    StickyEditorHelper.DrawUILine(separatorColor, 1, 6);

                    GUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawS3DFoldout(phShowInEditorProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                    EditorGUILayout.LabelField(elementName);
                    // Preview Mode button
                    hasToggled = StickyEditorHelper.DrawToggleButton(phPreviewModeProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled, phPrevBtnContent, 30f);
                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numPhonemes > 1) { phonemeMoveDownPos = phIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { phonemeInsertPos = phIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { phonemeDeletePos = phIdx; }
                    GUILayout.EndHorizontal();

                    if (hasToggled)
                    {
                        if (EditorApplication.isPlaying)
                        {
                            if (phPreviewModeProp.boolValue) { stickyShapesModule.StartPhoneme(phIdx + 1); }
                            else { stickyShapesModule.StopPhoneme(phIdx + 1); }
                            serializedObject.ApplyModifiedProperties();
                            GUIUtility.ExitGUI();
                            return;
                        }
                        else
                        {
                            StickyEditorHelper.PromptGotIt("Preview", "Run the scene to use the preview button");
                        }
                    }

                    StickyEditorHelper.DrawHorizontalGap(2f);

                    if (phShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(phNameProp, phNameContent);

                        phShapeListProp = phonemeProp.FindPropertyRelative("emotionShapeList");

                        if (phShapeDurationProp.floatValue < S3DPhoneme.MinDuration)
                        {
                            phShapeDurationProp.floatValue = S3DPhoneme.MinDuration;
                        }

                        EditorGUILayout.PropertyField(phShapeDurationProp, phShapeDurationContent);
                        EditorGUILayout.PropertyField(phShapeFrequencyProp, phShapeFrequencyContent);

                        #region Add/Remove Emotion Shapes
                        StickyEditorHelper.DrawHorizontalGap(2f);
                        GUILayout.BeginHorizontal();
                        // Create a revised GUIContent and don't include a width so that the Buttons right align
                        EditorGUILayout.LabelField(new GUIContent(phShapeListContent.text + phShapeListProp.arraySize.ToString(": 00"), phShapeListContent.tooltip));

                        if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
                        {
                            phShapeListProp.arraySize++;
                            serializedObject.ApplyModifiedProperties();
                            stickyShapesModule.GetPhonemeList[phIdx].emotionShapeList[phShapeListProp.arraySize - 1].SetClassDefaults();
                            serializedObject.Update();
                        }

                        if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                        {
                            if (phShapeListProp.arraySize > 0)
                            {
                                if (phPreviewModeProp.boolValue)
                                {
                                    // If deleting a single emotionShape, stop previewing the whole phoneme
                                    stickyShapesModule.StopPhonemeInstantly(phIdx + 1);
                                    phPreviewModeProp.boolValue = false;
                                }
                                phShapeListProp.arraySize--;
                            }
                        }

                        GUILayout.EndHorizontal();
                        #endregion

                        DrawEmotionShapeList(phShapeListProp, ref phShapeDeletePos, ref phShapeMoveDownPos);

                        if (phShapeDeletePos >= 0 && phPreviewModeProp.boolValue)
                        {
                            // If deleting a single emotionShape, stop previewing the whole phoneme
                            stickyShapesModule.StopPhonemeInstantly(phIdx + 1);
                            phPreviewModeProp.boolValue = false;
                        }

                        StickyEditorHelper.DrawDeleteMoveInList(phShapeListProp, ref phShapeDeletePos, ref phShapeMoveDownPos);
                    }
                }
                #endregion

                #region Move/Insert/Delete Phonemes
                if (phonemeMoveDownPos >= 0 || phonemeInsertPos >= 0 || phonemeDeletePos >= 0)
                {
                    GUI.FocusControl(null);
                    // Don't permit multiple operations in the same pass
                    if (phonemeMoveDownPos >= 0)
                    {
                        if (phonemeListProp.arraySize > 1)
                        {
                            List<S3DPhoneme> phList = stickyShapesModule.GetPhonemeList;

                            // Apply property changes before potential list changes
                            serializedObject.ApplyModifiedProperties();

                            // Force changes to be noted
                            Undo.RecordObject(stickyShapesModule, "Move Phoneme");

                            // Move down one position, or wrap round to 1st position in list
                            if (phonemeMoveDownPos < phonemeListProp.arraySize - 1)
                            {
                                phList.Insert(phonemeMoveDownPos + 2, phList[phonemeMoveDownPos]);
                                phList.RemoveAt(phonemeMoveDownPos);
                            }
                            else
                            {
                                phList.Insert(0, phList[phonemeMoveDownPos]);
                                phList.RemoveAt(phonemeMoveDownPos + 1);
                            }

                            phonemeMoveDownPos = -1;
                            GUIUtility.ExitGUI();
                            return;
                        }
                        else
                        {
                            phonemeMoveDownPos = -1;
                        }
                    }
                    else if (phonemeInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        // Force changes to be noted
                        Undo.RecordObject(stickyShapesModule, "Insert Phoneme");

                        List<S3DPhoneme> phList = stickyShapesModule.GetPhonemeList;

                        S3DPhoneme insertedPhoneme = new S3DPhoneme(phList[phonemeInsertPos]);
                        insertedPhoneme.showInEditor = true;
                        // Generate a new hashcode for the duplicated phoneme
                        insertedPhoneme.guidHash = S3DMath.GetHashCodeFromGuid();

                        phList.Insert(phonemeInsertPos, insertedPhoneme);

                        // Hide original Phoneme
                        phList[phonemeInsertPos + 1].showInEditor = false;

                        phonemeInsertPos = -1;
                        GUIUtility.ExitGUI();
                        return;
                    }
                    else if (phonemeDeletePos >= 0)
                    {
                        stickyShapesModule.StopPhonemeInstantly(phonemeDeletePos + 1);

                        // Modify the list rather than phonemeListProp.DeleteArrayElementAtIndex(..)
                        // which seems to reset all non-serialised field in other list members...
                        serializedObject.ApplyModifiedProperties();
                        // Force changes to be noted
                        Undo.RecordObject(stickyShapesModule, "Delete Phoneme");
                        List<S3DPhoneme> phList = stickyShapesModule.GetPhonemeList;
                        phList.RemoveAt(phonemeDeletePos);
                        phonemeDeletePos = -1;

                        GUIUtility.ExitGUI();
                        return;
                    }
                }
                #endregion
            }

            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            #region Speech Audio Buttons
            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel += 1;
            EditorGUIUtility.fieldWidth = 15f;
            isSpeechAudioListExpandedProp.boolValue = EditorGUILayout.Foldout(isSpeechAudioListExpandedProp.boolValue, "", foldoutStyleNoLabel);
            EditorGUI.indentLevel -= 1;

            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

            // Create a revised GUIContent and don't include a width so that the Buttons right align
            EditorGUILayout.LabelField(new GUIContent(speechAudioListContent.text + speechAudioListProp.arraySize.ToString(": 0"), speechAudioListContent.tooltip));

            speechAudioDeletePos = -1;
            speechAudioMoveDownPos = -1;
            speechAudioInsertPos = -1;

            if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
            {
                speechAudioListProp.arraySize++;
                serializedObject.ApplyModifiedProperties();
                stickyShapesModule.GetSpeechAudioList[speechAudioListProp.arraySize - 1].SetClassDefaults();
                serializedObject.Update();

                if (!isSpeechAudioListExpandedProp.boolValue) { isSpeechAudioListExpandedProp.boolValue = true; }

                // React Speech Audio will need updating
                isSpeechAudioNamesStale = true;
            }

            if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
            {
                if (phonemeListProp.arraySize > 0)
                {
                    speechAudioDeletePos = speechAudioListProp.arraySize - 1;
                }
            }

            GUILayout.EndHorizontal();
            #endregion

            if (isSpeechAudioListExpandedProp.boolValue)
            {
                #region Speech Audio List

                StickyEditorHelper.DrawHorizontalGap(2f);
                int numSpeechAudios = speechAudioListProp.arraySize;

                if (numSpeechAudios > 0) { StickyEditorHelper.DrawHorizontalGap(2f); }

                for (int saIdx = 0; saIdx < numSpeechAudios; saIdx++)
                {
                    speechAudioProp = speechAudioListProp.GetArrayElementAtIndex(saIdx);

                    saShowInEditorProp = speechAudioProp.FindPropertyRelative("showInEditor");
                    saPreviewModeProp = speechAudioProp.FindPropertyRelative("isPreviewMode");
                    saSpeechAudioIdProp = speechAudioProp.FindPropertyRelative("guidHash");
                    saSpeechSpeedProp = speechAudioProp.FindPropertyRelative("speechSpeed");
                    saAudioClipProp = speechAudioProp.FindPropertyRelative("audioClip");
                    saVolumeProp = speechAudioProp.FindPropertyRelative("volume");
                    saSilenceThresholdProp = speechAudioProp.FindPropertyRelative("silenceThreshold");
                    saEmotionsIgnoreSilenceProp = speechAudioProp.FindPropertyRelative("isEmotionsIgnoreSilence");
                    saEmotionMinIntervalProp = speechAudioProp.FindPropertyRelative("emotionMinInterval");
                    saEmotionMaxIntervalProp = speechAudioProp.FindPropertyRelative("emotionMaxInterval");
                    saEmotionFadeOnSilenceProp = speechAudioProp.FindPropertyRelative("isEmotionFadeOnSilence");

                    elementName = "Audio " + (saIdx + 1).ToString("00 ");

                    // Display the audio clip name when collapsed
                    if (!saShowInEditorProp.boolValue)
                    {
                        S3DSpeechAudio speechAudio = stickyShapesModule.GetSpeechAudio(saSpeechAudioIdProp.intValue);

                        if (speechAudio != null)
                        {
                            string clipName = speechAudio.GetClipName;

                            if (clipName.Length > 32) { elementName += clipName.Substring(0, 30) + ".."; }
                            else { elementName += clipName; }
                        }
                    }

                    StickyEditorHelper.DrawUILine(separatorColor, 1, 6);

                    GUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawS3DFoldout(saShowInEditorProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                    EditorGUILayout.LabelField(elementName);

                    // Preview Mode button
                    hasToggled = StickyEditorHelper.DrawToggleButton(saPreviewModeProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled, saPrevBtnContent, 30f);

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numSpeechAudios > 1) { speechAudioMoveDownPos = saIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { speechAudioInsertPos = saIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { speechAudioDeletePos = saIdx; }
                    GUILayout.EndHorizontal();

                    if (hasToggled)
                    {
                        if (EditorApplication.isPlaying)
                        {
                            if (saPreviewModeProp.boolValue) { stickyShapesModule.PlaySpeechAudio(saIdx + 1); }
                            else { stickyShapesModule.StopSpeechAudio(saIdx + 1); }
                            serializedObject.ApplyModifiedProperties();
                            GUIUtility.ExitGUI();
                            return;
                        }
                        else
                        {
                            StickyEditorHelper.PromptGotIt("Preview", "Run the scene to use the preview button");
                        }
                    }

                    if (saShowInEditorProp.boolValue)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(saAudioClipProp, saAudioClipContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            // React Speech Audio will need updating
                            isSpeechAudioNamesStale = true;
                        }

                        EditorGUILayout.PropertyField(saSpeechSpeedProp, saSpeechSpeedContent);
                        EditorGUILayout.PropertyField(saVolumeProp, saVolumeContent);
                        EditorGUILayout.PropertyField(saSilenceThresholdProp, saSilenceTHresholdContent);
                        EditorGUILayout.PropertyField(saEmotionsIgnoreSilenceProp, saEmotionsIgnoreSilenceContent);

                        if (saEmotionsIgnoreSilenceProp.boolValue)
                        {
                            StickyEditorHelper.DrawMinMaxSlider(saEmotionMinIntervalProp, saEmotionMaxIntervalProp, saEmotionIntervalContent, 0f, 10f, defaultEditorLabelWidth);
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(saEmotionFadeOnSilenceProp, saEmotionFadeOnSilenceContent);
                        }

                        if (saSpeechSpeedProp.floatValue < S3DSpeechAudio.MinSpeechSpeed)
                        {
                            saSpeechSpeedProp.floatValue = S3DSpeechAudio.MinSpeechSpeed;
                        }
                    }
                }

                #endregion

                #region Move/Insert/Delete Speech Audio clips
                if (speechAudioMoveDownPos >= 0 || speechAudioInsertPos >= 0 || speechAudioDeletePos >= 0)
                {
                    GUI.FocusControl(null);

                    // React Speech Audio will need updating
                    isSpeechAudioNamesStale = true;

                    // Don't permit multiple operations in the same pass
                    if (speechAudioMoveDownPos >= 0)
                    {
                        if (speechAudioListProp.arraySize > 1)
                        {
                            List<S3DSpeechAudio> saList = stickyShapesModule.GetSpeechAudioList;

                            // Apply property changes before potential list changes
                            serializedObject.ApplyModifiedProperties();

                            // Force changes to be noted
                            Undo.RecordObject(stickyShapesModule, "Move Speech Audio");

                            // Move down one position, or wrap round to 1st position in list
                            if (speechAudioMoveDownPos < speechAudioListProp.arraySize - 1)
                            {
                                saList.Insert(speechAudioMoveDownPos + 2, saList[speechAudioMoveDownPos]);
                                saList.RemoveAt(speechAudioMoveDownPos);
                            }
                            else
                            {
                                saList.Insert(0, saList[speechAudioMoveDownPos]);
                                saList.RemoveAt(speechAudioMoveDownPos + 1);
                            }

                            speechAudioMoveDownPos = -1;
                            GUIUtility.ExitGUI();
                            return;
                        }
                        else
                        {
                            speechAudioMoveDownPos = -1;
                        }
                    }
                    else if (speechAudioInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        // Force changes to be noted
                        Undo.RecordObject(stickyShapesModule, "Insert Speech Audio");

                        List<S3DSpeechAudio> saList = stickyShapesModule.GetSpeechAudioList;

                        S3DSpeechAudio insertedSpeechAudio = new S3DSpeechAudio(saList[speechAudioInsertPos]);
                        insertedSpeechAudio.showInEditor = true;
                        // Generate a new hashcode for the duplicated speech audio
                        insertedSpeechAudio.guidHash = S3DMath.GetHashCodeFromGuid();

                        saList.Insert(speechAudioInsertPos, insertedSpeechAudio);

                        // Hide original Speech Audio
                        saList[speechAudioInsertPos + 1].showInEditor = false;

                        speechAudioInsertPos = -1;
                        GUIUtility.ExitGUI();
                        return;
                    }
                    else if (speechAudioDeletePos >= 0)
                    {
                        stickyShapesModule.StopSpeechAudio(speechAudioDeletePos + 1);

                        // Modify the list rather than phonemeListProp.DeleteArrayElementAtIndex(..)
                        // which seems to reset all non-serialised field in other list members...
                        serializedObject.ApplyModifiedProperties();
                        // Force changes to be noted
                        Undo.RecordObject(stickyShapesModule, "Delete Speech Audio");
                        List<S3DSpeechAudio> saList = stickyShapesModule.GetSpeechAudioList;
                        saList.RemoveAt(speechAudioDeletePos);
                        speechAudioDeletePos = -1;

                        GUIUtility.ExitGUI();
                        return;
                    }
                }

                #endregion
            }
        }

        /// <summary>
        /// Overrideable method to draw the engage settings in the editor
        /// </summary>
        protected virtual void DrawEngageSettings()
        {
            #region React Settings

            //StickyEditorHelper.InTechPreview();

            StickyEditorHelper.DrawS3DFoldout(isReactSettingsExpandedProp, reactSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (isReactSettingsExpandedProp.boolValue)
            {
                #region General Engage Settings
                EditorGUILayout.PropertyField(isReactEnableOnInitProp, isReactEnableOnInitContent);
                EditorGUILayout.PropertyField(reactNoNotifyDurationProp, reactNoNotifyDurationContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(reactDistanceProp, reactDistanceContent);
                if (EditorGUI.EndChangeCheck() && proximityColliderProp.objectReferenceValue != null)
                {
                    serializedObject.ApplyModifiedProperties();
                    Undo.SetCurrentGroupName("Change Proximity Distance");
                    int undoGroup = UnityEditor.Undo.GetCurrentGroup();
                    Undo.RecordObject(stickyShapesModule, string.Empty);
                    Undo.RecordObject(proximityColliderProp.objectReferenceValue, string.Empty);
                    stickyShapesModule.SetReactDistance(reactDistanceProp.floatValue);
                    Undo.CollapseUndoOperations(undoGroup);
                    GUIUtility.ExitGUI();
                    return;
                }

                #region Proximity Collider
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(proximityColliderContent, GUILayout.Width(defaultEditorLabelWidth - 53f));
                if (GUILayout.Button(StickyEditorHelper.btnNewContent, buttonCompact, GUILayout.MaxWidth(50f)) && proximityColliderProp.objectReferenceValue == null)
                {
                    serializedObject.ApplyModifiedProperties();

                    Undo.SetCurrentGroupName("Add Engage Proximity");
                    int undoGroup = UnityEditor.Undo.GetCurrentGroup();
                    Undo.RecordObject(stickyShapesModule, string.Empty);

                    GameObject proxGameObject = new GameObject("Proximity");
                    Undo.RegisterCreatedObjectUndo(proxGameObject, string.Empty);
                    proxGameObject.transform.SetParent(stickyShapesModule.transform, false);

                    SphereCollider sphereCollider = Undo.AddComponent<SphereCollider>(proxGameObject);
                    StickyShapesProximity shapesProximity = Undo.AddComponent<StickyShapesProximity>(proxGameObject);

                    sphereCollider.isTrigger = true;
                    sphereCollider.radius = reactDistanceProp.floatValue;

                    stickyShapesModule.SetProximityCollider(sphereCollider);
                    Undo.CollapseUndoOperations(undoGroup);

                    // Should be non-scene objects but is required to force being set as dirty
                    EditorUtility.SetDirty(stickyShapesModule);

                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.PropertyField(proximityColliderProp, GUIContent.none);
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyShapesModule.ProximityCollider = proximityColliderProp.objectReferenceValue as Collider;
                    serializedObject.Update();
                }

                #endregion

                #endregion

                #region React List Buttons
                GUILayout.BeginHorizontal();
                StickyEditorHelper.DrawS3DFoldout(isReactListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);

                // Create a revised GUIContent and don't include a width so that the Buttons right align
                EditorGUILayout.LabelField(new GUIContent(reactListContent.text + reactListProp.arraySize.ToString(": 0"), reactListContent.tooltip));

                reactDeletePos = -1;
                reactMoveDownPos = -1;
                reactInsertPos = -1;

                if (GUILayout.Button(reactSyncContent, GUILayout.MaxWidth(40f)))
                {
                    Undo.RecordObject(stickyShapesModule, "Sync with Emotions tab");
                    stickyShapesModule.ResyncReactions();
                    GUIUtility.ExitGUI();
                    return;
                }

                if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
                {
                    //Undo.RecordObject(stickyShapesModule, "Add Reaction");
                    reactListProp.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                    stickyShapesModule.GetReactList[reactListProp.arraySize - 1].SetClassDefaults();
                    if (EditorApplication.isPlaying)
                    {
                        stickyShapesModule.ReinitialiseReactions();
                    }
                    serializedObject.Update();

                    if (!isReactListExpandedProp.boolValue) { isReactListExpandedProp.boolValue = true; }
                }

                if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                {
                    if (reactListProp.arraySize > 0)
                    {
                        reactDeletePos = reactListProp.arraySize - 1;
                    }
                }

                GUILayout.EndHorizontal();
                #endregion

                #region React List
                if (isReactListExpandedProp.boolValue)
                {
                    string elementName = string.Empty;
                    bool hasPrevToggled = false;
                    bool hasAutoToggled = false;
                    bool isRefreshCache = false;

                    StickyEditorHelper.DrawHorizontalGap(2f);
                    int numReactions = reactListProp.arraySize;

                    if (numReactions > 0) { StickyEditorHelper.DrawHorizontalGap(2f); }

                    for (int reIdx=0; reIdx < numReactions; reIdx++)
                    {
                        reactProp = reactListProp.GetArrayElementAtIndex(reIdx);

                        #region Find Properties
                        reactNameProp = reactProp.FindPropertyRelative("reactName");
                        reactShowInEditorProp = reactProp.FindPropertyRelative("showInEditor");
                        reactEnterPreviewModeProp = reactProp.FindPropertyRelative("isEnterPreviewMode");
                        reactExitPreviewModeProp = reactProp.FindPropertyRelative("isExitPreviewMode");
                        reactToProp = reactProp.FindPropertyRelative("reactTo");
                        reactModelsToIncludeProp = reactProp.FindPropertyRelative("modelsToInclude");
                        #endregion

                        #region React Buttons
                        elementName = (reIdx + 1).ToString("00 ") + (string.IsNullOrEmpty(reactNameProp.stringValue) ? "**NO NAME**" : reactNameProp.stringValue);

                        StickyEditorHelper.DrawUILine(separatorColor, 1, 6);
                        
                        GUILayout.BeginHorizontal();
                        StickyEditorHelper.DrawS3DFoldout(reactShowInEditorProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                        EditorGUILayout.LabelField(elementName);
                        // Move down button
                        if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numReactions > 1) { reactMoveDownPos = reIdx; }
                        // Create duplicate button
                        if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { reactInsertPos = reIdx; }
                        // Delete button
                        if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { reactDeletePos = reIdx; }
                        GUILayout.EndHorizontal();
                        
                        StickyEditorHelper.DrawHorizontalGap(2f);
                        #endregion

                        if (reactShowInEditorProp.boolValue)
                        {
                            #region Find React Properties
                            //reFadeInDurationProp = reactProp.FindPropertyRelative("fadeInDuration");
                            //reFadeOutDurationProp = reactProp.FindPropertyRelative("fadeOutDuration");
                            isAutoEnterEmotionsProp = reactProp.FindPropertyRelative("isAutoEnterEmotions");
                            isAutoExitEmotionsProp = reactProp.FindPropertyRelative("isAutoExitEmotions");
                            enterReEmListProp = reactProp.FindPropertyRelative("enterReactEmotionList");
                            enterReSpchAListProp = reactProp.FindPropertyRelative("enterReactSpeechAudioList");
                            exitReEmListProp = reactProp.FindPropertyRelative("exitReactEmotionList");
                            exitReSpchAListProp = reactProp.FindPropertyRelative("exitReactSpeechAudioList");

                            isReactEnterExpandedProp = reactProp.FindPropertyRelative("isReactEnterExpanded");
                            isReactStayExpandedProp = reactProp.FindPropertyRelative("isReactStayExpanded");
                            isReactExitExpandedProp = reactProp.FindPropertyRelative("isReactExitExpanded");

                            //reactSelectedTabIntProp = reactProp.FindPropertyRelative("selectedTabInt");
                            #endregion

                            //StickyEditorHelper.DrawS3DToolbar(reactSelectedTabIntProp, reactTabTexts);

                            EditorGUILayout.PropertyField(reactNameProp, reNameContent);

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(reactToProp, reToContent);
                            isRefreshCache = EditorGUI.EndChangeCheck();

                            EditorGUI.BeginChangeCheck();
                            StickyEditorHelper.DrawArray(reactModelsToIncludeProp, reModelsToFilterContent, defaultEditorLabelWidth, "Model");
                            isRefreshCache = isRefreshCache || EditorGUI.EndChangeCheck();

                            // At runtime in editor, if required, update the cache
                            if (EditorApplication.isPlaying && isRefreshCache)
                            {
                                serializedObject.ApplyModifiedProperties();
                                stickyShapesModule.GetReactList[reIdx].CacheData();
                                isRefreshCache = false;
                                serializedObject.Update();
                            }

                            #region Enter Emotions, Speech and Events

                            #region Find Enter Properties
                            isReEnterEmListExpandedProp = reactProp.FindPropertyRelative("isEnterEmotionListExpanded");
                            isReEnterEvtsExpandedProp = reactProp.FindPropertyRelative("isEnterEventsExpanded");
                            isReEnterSpchAListExpandedProp = reactProp.FindPropertyRelative("isEnterSpeechAudioListExpanded");
                            #endregion

                            #region Enter foldout and Preview Button
                            GUILayout.BeginHorizontal();
                            EditorGUI.indentLevel += 1;
                            EditorGUIUtility.fieldWidth = 15f;
                            isReactEnterExpandedProp.boolValue = EditorGUILayout.Foldout(isReactEnterExpandedProp.boolValue, GUIContent.none, foldoutStyleNoLabel);
                            EditorGUI.indentLevel -= 1;
                            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                            EditorGUILayout.LabelField(reactEnterSettingsContent);                            
                            // Preview Mode button
                            hasPrevToggled = StickyEditorHelper.DrawToggleButton(reactEnterPreviewModeProp, toggleButtonStyleNormal12, toggleButtonStyleToggledB12, reactPrevBtnContent, 30f);
                            GUILayout.EndHorizontal();

                            if (hasPrevToggled)
                            {
                                if (EditorApplication.isPlaying)
                                {
                                    if (reactEnterPreviewModeProp.boolValue)
                                    {
                                        // update previewMode in stickyShapeModule before
                                        // calling StartEnterReaction
                                        serializedObject.ApplyModifiedProperties();
                                        stickyShapesModule.PlayEnterReaction(reIdx + 1);
                                    }
                                    else
                                    {
                                        serializedObject.ApplyModifiedProperties();
                                        stickyShapesModule.StopEnterReaction(reIdx + 1);
                                    }
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                                else
                                {
                                    StickyEditorHelper.PromptGotIt("Preview", "Run the scene to use the preview button");
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                            }
                            #endregion

                            if (isReactEnterExpandedProp.boolValue)
                            {
                                #region Enter Reactions
                                int _numEnterReEmotions = enterReEmListProp.arraySize;

                                reactEmotionDeletePos = -1;
                                reactEmotionMoveDownPos = -1;

                                #region Add/Remove Enter Reaction Emotion
                                StickyEditorHelper.DrawHorizontalGap(2f);
                                GUILayout.BeginHorizontal();
                                StickyEditorHelper.DrawS3DFoldout(isReEnterEmListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                                // Create a revised GUIContent and don't include a width so that the Buttons right align
                                EditorGUILayout.LabelField(new GUIContent(enterReEmListContent.text + enterReEmListProp.arraySize.ToString(": 00"), enterReEmListContent.tooltip));

                                hasAutoToggled = StickyEditorHelper.DrawToggleButton(isAutoEnterEmotionsProp, toggleButtonStyleNormal12, toggleButtonStyleToggledB12, reactAutoBtnContent, 40f);

                                if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
                                {
                                    enterReEmListProp.arraySize++;
                                    serializedObject.ApplyModifiedProperties();
                                    stickyShapesModule.GetReactList[reIdx].enterReactEmotionList[enterReEmListProp.arraySize - 1].SetClassDefaults();
                                    serializedObject.Update();
                                    if (!isReEnterEmListExpandedProp.boolValue) { isReEnterEmListExpandedProp.boolValue = true; }
                                }

                                if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                                {
                                    if (enterReEmListProp.arraySize > 0)
                                    {
                                        if (reactEnterPreviewModeProp.boolValue)
                                        {
                                            // If deleting a single react emotion, stop previewing the whole reaction
                                            stickyShapesModule.StopEnterReactionInstantly(reIdx + 1);
                                            reactEnterPreviewModeProp.boolValue = false;
                                        }
                                        enterReEmListProp.arraySize--;
                                    }
                                }

                                GUILayout.EndHorizontal();

                                #endregion

                                if (isReEnterEmListExpandedProp.boolValue)
                                {
                                    DrawReactEmotionList(enterReEmListProp, S3DReact.ReactionStage.Enter, ref reactEmotionDeletePos, ref reactEmotionMoveDownPos);
                                }

                                if (reactEmotionDeletePos >= 0)
                                {
                                    // If deleting a single reactEmotion, stop previewing the whole reaction
                                    if (reactEnterPreviewModeProp.boolValue)
                                    {
                                        stickyShapesModule.StopEnterReactionInstantly(reIdx + 1);
                                        reactEnterPreviewModeProp.boolValue = false;
                                    }
                                }

                                StickyEditorHelper.DrawDeleteMoveInList(enterReEmListProp, ref reactEmotionDeletePos, ref reactEmotionMoveDownPos);

                                // If the num of emotions gets changed at runtime in editor, update the cache.
                                if (EditorApplication.isPlaying)
                                {
                                    if (isRefreshCache || enterReEmListProp.arraySize != _numEnterReEmotions)
                                    {
                                        serializedObject.ApplyModifiedProperties();
                                        stickyShapesModule.GetReactList[reIdx].CacheData();
                                        isRefreshCache = false;
                                        serializedObject.Update();
                                    }
                                }

                                #endregion

                                #region Enter Events

                                StickyEditorHelper.DrawS3DFoldout(isReEnterEvtsExpandedProp, enterReEvtsContent, foldoutStyleNoLabel, defaultEditorLabelWidth);

                                if (isReEnterEvtsExpandedProp.boolValue)
                                {
                                    reOnPreEnterProp = reactProp.FindPropertyRelative("onPreEnter");
                                    reOnPostEnterProp = reactProp.FindPropertyRelative("onPostEnter");

                                    StickyEditorHelper.DrawHorizontalGap(3f);
                                    StickyEditorHelper.DrawEventPropertyIndent(20f, reOnPreEnterProp, onPreEnterContent);
                                    StickyEditorHelper.DrawHorizontalGap(6f);
                                    StickyEditorHelper.DrawEventPropertyIndent(20f, reOnPostEnterProp, onPostEnterContent);
                                    StickyEditorHelper.DrawHorizontalGap(3f);

                                }

                                #endregion

                                #region Enter Speech Audio
                                int _numEnterReSpchAudio = enterReSpchAListProp.arraySize;

                                reactSpchADeletePos = -1;
                                reactSpchAMoveDownPos = -1;

                                #region Add/Remove Enter Reaction Speech Audio
                                StickyEditorHelper.DrawHorizontalGap(2f);
                                GUILayout.BeginHorizontal();
                                StickyEditorHelper.DrawS3DFoldout(isReEnterSpchAListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                                // Create a revised GUIContent and don't include a width so that the Buttons right align
                                EditorGUILayout.LabelField(new GUIContent(enterReSpchAListContent.text + enterReSpchAListProp.arraySize.ToString(": 00"), enterReSpchAListContent.tooltip));

                                if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
                                {
                                    enterReSpchAListProp.arraySize++;
                                    serializedObject.ApplyModifiedProperties();
                                    stickyShapesModule.GetReactList[reIdx].enterReactSpeechAudioList[enterReSpchAListProp.arraySize - 1].SetClassDefaults();
                                    serializedObject.Update();
                                    if (!isReEnterSpchAListExpandedProp.boolValue) { isReEnterSpchAListExpandedProp.boolValue = true; }
                                }

                                if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                                {
                                    if (enterReSpchAListProp.arraySize > 0)
                                    {
                                        if (reactEnterPreviewModeProp.boolValue)
                                        {
                                            // If deleting a single react speech audio, stop previewing the whole reaction
                                            stickyShapesModule.StopEnterReactionInstantly(reIdx + 1);
                                            reactEnterPreviewModeProp.boolValue = false;
                                        }
                                        enterReSpchAListProp.arraySize--;
                                    }
                                }

                                GUILayout.EndHorizontal();

                                #endregion

                                if (isReEnterSpchAListExpandedProp.boolValue)
                                {
                                    DrawReactSpeechAudioList(enterReSpchAListProp, S3DReact.ReactionStage.Enter, ref reactSpchADeletePos, ref reactSpchAMoveDownPos);
                                }

                                if (reactSpchADeletePos >= 0)
                                {
                                    // If deleting a single reactSpeechAudio, stop previewing the whole reaction
                                    if (reactEnterPreviewModeProp.boolValue)
                                    {
                                        stickyShapesModule.StopEnterReactionInstantly(reIdx + 1);
                                        reactEnterPreviewModeProp.boolValue = false;
                                    }
                                }

                                StickyEditorHelper.DrawDeleteMoveInList(enterReSpchAListProp, ref reactSpchADeletePos, ref reactSpchAMoveDownPos);

                                // If the num of speech audio gets changed at runtime in editor, update the cache.
                                if (EditorApplication.isPlaying)
                                {
                                    if (isRefreshCache || enterReSpchAListProp.arraySize != _numEnterReSpchAudio)
                                    {
                                        serializedObject.ApplyModifiedProperties();
                                        stickyShapesModule.GetReactList[reIdx].CacheData();
                                        isRefreshCache = false;
                                        serializedObject.Update();
                                    }
                                }

                                #endregion
                            }

                            #endregion

                            #region Stay Emotions, Speech and Events

                            //StickyEditorHelper.DrawHorizontalGap(8f);

                            #region Find Stay Properties
                            isReStayEmListExpandedProp = reactProp.FindPropertyRelative("isStayEmotionListExpanded");
                            isReStayEvtsExpandedProp = reactProp.FindPropertyRelative("isStayEventsExpanded");
                            isReStaySpchAListExpandedProp = reactProp.FindPropertyRelative("isStaySpeechAudioListExpanded");
                            #endregion

                            //StickyEditorHelper.DrawS3DFoldout(isReactStayExpandedProp, reactStaySettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                            if (isReactStayExpandedProp.boolValue)
                            {
                                #region Stay Emotions


                                #endregion

                                #region Stay Events

                                #endregion

                                #region Stay Speech


                                #endregion
                            }
                            #endregion

                            #region Exit Emotions, Speech and Events

                            StickyEditorHelper.DrawHorizontalGap(8f);

                            #region Find Exit Properties
                            isReExitEmListExpandedProp = reactProp.FindPropertyRelative("isExitEmotionListExpanded");
                            isReExitEvtsExpandedProp = reactProp.FindPropertyRelative("isExitEventsExpanded");
                            isReExitSpchAListExpandedProp = reactProp.FindPropertyRelative("isExitSpeechAudioListExpanded");
                            #endregion

                            #region Exit foldout and Preview Button
                            GUILayout.BeginHorizontal();
                            EditorGUI.indentLevel += 1;
                            EditorGUIUtility.fieldWidth = 15f;
                            isReactExitExpandedProp.boolValue = EditorGUILayout.Foldout(isReactExitExpandedProp.boolValue, GUIContent.none, foldoutStyleNoLabel);
                            EditorGUI.indentLevel -= 1;
                            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                            EditorGUILayout.LabelField(reactExitSettingsContent);
                            // Preview Mode button
                            hasPrevToggled = StickyEditorHelper.DrawToggleButton(reactExitPreviewModeProp, toggleButtonStyleNormal12, toggleButtonStyleToggledB12, reactPrevBtnContent, 30f);
                            GUILayout.EndHorizontal();

                            if (hasPrevToggled)
                            {
                                if (EditorApplication.isPlaying)
                                {
                                    if (reactExitPreviewModeProp.boolValue)
                                    {
                                        // update previewMode in stickyShapeModule before
                                        // calling StartEnterReaction
                                        serializedObject.ApplyModifiedProperties();
                                        stickyShapesModule.PlayExitReaction(reIdx + 1);
                                    }
                                    else
                                    {
                                        serializedObject.ApplyModifiedProperties();
                                        stickyShapesModule.StopExitReaction(reIdx + 1);
                                    }
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                                else
                                {
                                    StickyEditorHelper.PromptGotIt("Preview", "Run the scene to use the preview button");
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                            }
                            #endregion

                            if (isReactExitExpandedProp.boolValue)
                            {
                                #region Exit Reactions
                                int _numExitReEmotions = exitReEmListProp.arraySize;

                                reactEmotionDeletePos = -1;
                                reactEmotionMoveDownPos = -1;

                                #region Add/Remove Exit Reaction Emotion
                                StickyEditorHelper.DrawHorizontalGap(2f);
                                GUILayout.BeginHorizontal();
                                StickyEditorHelper.DrawS3DFoldout(isReExitEmListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                                // Create a revised GUIContent and don't include a width so that the Buttons right align
                                EditorGUILayout.LabelField(new GUIContent(exitReEmListContent.text + exitReEmListProp.arraySize.ToString(": 00"), exitReEmListContent.tooltip));

                                hasAutoToggled = StickyEditorHelper.DrawToggleButton(isAutoExitEmotionsProp, toggleButtonStyleNormal12, toggleButtonStyleToggledB12, reactAutoBtnContent, 40f);

                                if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
                                {
                                    exitReEmListProp.arraySize++;
                                    serializedObject.ApplyModifiedProperties();
                                    stickyShapesModule.GetReactList[reIdx].exitReactEmotionList[exitReEmListProp.arraySize - 1].SetClassDefaults();
                                    serializedObject.Update();
                                    if (!isReExitEmListExpandedProp.boolValue) { isReExitEmListExpandedProp.boolValue = true; }
                                }

                                if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                                {
                                    if (exitReEmListProp.arraySize > 0)
                                    {
                                        if (reactExitPreviewModeProp.boolValue)
                                        {
                                            // If deleting a single react emotion, stop previewing the whole reaction
                                            stickyShapesModule.StopExitReactionInstantly(reIdx + 1);
                                            reactEnterPreviewModeProp.boolValue = false;
                                        }
                                        exitReEmListProp.arraySize--;
                                    }
                                }

                                GUILayout.EndHorizontal();

                                #endregion

                                if (isReExitEmListExpandedProp.boolValue)
                                {
                                    DrawReactEmotionList(exitReEmListProp, S3DReact.ReactionStage.Exit, ref reactEmotionDeletePos, ref reactEmotionMoveDownPos);
                                }

                                if (reactEmotionDeletePos >= 0)
                                {
                                    // If deleting a single reactEmotion, stop previewing the whole reaction
                                    if (reactExitPreviewModeProp.boolValue)
                                    {
                                        stickyShapesModule.StopExitReactionInstantly(reIdx + 1);
                                        reactExitPreviewModeProp.boolValue = false;
                                    }
                                }

                                StickyEditorHelper.DrawDeleteMoveInList(exitReEmListProp, ref reactEmotionDeletePos, ref reactEmotionMoveDownPos);

                                // If the num of emotions gets changed at runtime in editor, update the cache.
                                if (EditorApplication.isPlaying)
                                {
                                    if (isRefreshCache || exitReEmListProp.arraySize != _numExitReEmotions)
                                    {
                                        serializedObject.ApplyModifiedProperties();
                                        stickyShapesModule.GetReactList[reIdx].CacheData();
                                        isRefreshCache = false;
                                        serializedObject.Update();
                                    }
                                }

                                #endregion

                                #region Exit Events

                                StickyEditorHelper.DrawS3DFoldout(isReExitEvtsExpandedProp, exitReEvtsContent, foldoutStyleNoLabel, defaultEditorLabelWidth);

                                if (isReExitEvtsExpandedProp.boolValue)
                                {
                                    reOnPreExitProp = reactProp.FindPropertyRelative("onPreExit");
                                    reOnPostExitProp = reactProp.FindPropertyRelative("onPostExit");

                                    StickyEditorHelper.DrawHorizontalGap(3f);
                                    StickyEditorHelper.DrawEventPropertyIndent(20f, reOnPreExitProp, onPreExitContent);
                                    StickyEditorHelper.DrawHorizontalGap(6f);
                                    StickyEditorHelper.DrawEventPropertyIndent(20f, reOnPostExitProp, onPostExitContent);
                                    StickyEditorHelper.DrawHorizontalGap(3f);
                                }

                                #endregion

                                #region Exit Speech Audio
                                int _numExitReSpchAudio = exitReSpchAListProp.arraySize;

                                reactSpchADeletePos = -1;
                                reactSpchAMoveDownPos = -1;

                                #region Add/Remove Exit Reaction Speech Audio
                                StickyEditorHelper.DrawHorizontalGap(2f);
                                GUILayout.BeginHorizontal();
                                StickyEditorHelper.DrawS3DFoldout(isReExitSpchAListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
                                // Create a revised GUIContent and don't include a width so that the Buttons right align
                                EditorGUILayout.LabelField(new GUIContent(exitReSpchAListContent.text + exitReSpchAListProp.arraySize.ToString(": 00"), exitReSpchAListContent.tooltip));

                                if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
                                {
                                    exitReSpchAListProp.arraySize++;
                                    serializedObject.ApplyModifiedProperties();
                                    stickyShapesModule.GetReactList[reIdx].exitReactSpeechAudioList[exitReSpchAListProp.arraySize - 1].SetClassDefaults();
                                    serializedObject.Update();
                                    if (!isReExitSpchAListExpandedProp.boolValue) { isReExitSpchAListExpandedProp.boolValue = true; }
                                }

                                if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                                {
                                    if (exitReSpchAListProp.arraySize > 0)
                                    {
                                        if (reactExitPreviewModeProp.boolValue)
                                        {
                                            // If deleting a single react speech audio, stop previewing the whole reaction
                                            stickyShapesModule.StopExitReactionInstantly(reIdx + 1);
                                            reactExitPreviewModeProp.boolValue = false;
                                        }
                                        exitReSpchAListProp.arraySize--;
                                    }
                                }

                                GUILayout.EndHorizontal();

                                #endregion

                                if (isReExitSpchAListExpandedProp.boolValue)
                                {
                                    DrawReactSpeechAudioList(exitReSpchAListProp, S3DReact.ReactionStage.Exit, ref reactSpchADeletePos, ref reactSpchAMoveDownPos);
                                }

                                if (reactSpchADeletePos >= 0)
                                {
                                    // If deleting a single reactSpeechAudio, stop previewing the whole reaction
                                    if (reactEnterPreviewModeProp.boolValue)
                                    {
                                        stickyShapesModule.StopEnterReactionInstantly(reIdx + 1);
                                        reactEnterPreviewModeProp.boolValue = false;
                                    }
                                }

                                StickyEditorHelper.DrawDeleteMoveInList(exitReSpchAListProp, ref reactSpchADeletePos, ref reactSpchAMoveDownPos);

                                // If the num of speech audio gets changed at runtime in editor, update the cache.
                                if (EditorApplication.isPlaying)
                                {
                                    if (isRefreshCache || exitReSpchAListProp.arraySize != _numExitReSpchAudio)
                                    {
                                        serializedObject.ApplyModifiedProperties();
                                        stickyShapesModule.GetReactList[reIdx].CacheData();
                                        isRefreshCache = false;
                                        serializedObject.Update();
                                    }
                                }

                                #endregion
                            }

                            #endregion
                        }
                    }
                }

                #endregion

                #region Move/Insert/Delete reactions
                if (reactMoveDownPos >= 0 || reactInsertPos >= 0 || reactDeletePos >= 0)
                {
                    GUI.FocusControl(null);
                    // Don't permit multiple operations in the same pass
                    if (reactMoveDownPos >= 0)
                    {
                        if (reactListProp.arraySize > 1)
                        {
                            List<S3DReact> reList = stickyShapesModule.GetReactList;

                            // Apply property changes before potential list changes
                            serializedObject.ApplyModifiedProperties();

                            // Force changes to be noted
                            Undo.RecordObject(stickyShapesModule, "Move Reaction");

                            // Move down one position, or wrap round to 1st position in list
                            if (reactMoveDownPos < reactListProp.arraySize - 1)
                            {
                                reList.Insert(reactMoveDownPos + 2, reList[reactMoveDownPos]);
                                reList.RemoveAt(reactMoveDownPos);
                            }
                            else
                            {
                                reList.Insert(0, reList[reactMoveDownPos]);
                                reList.RemoveAt(reactMoveDownPos + 1);
                            }

                            reactMoveDownPos = -1;
                            GUIUtility.ExitGUI();
                            return;
                        }
                        else
                        {
                            reactMoveDownPos = -1;
                        }
                    }
                    else if (reactInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        // Force changes to be noted
                        Undo.RecordObject(stickyShapesModule, "Insert Reaction");

                        List<S3DReact> reList = stickyShapesModule.GetReactList;

                        S3DReact insertedReact = new S3DReact(reList[reactInsertPos]);
                        insertedReact.showInEditor = true;
                        // Generate a new hashcode for the duplicated reaction
                        insertedReact.guidHash = S3DMath.GetHashCodeFromGuid();

                        reList.Insert(reactInsertPos, insertedReact);

                        // Hide original reaction
                        reList[reactInsertPos + 1].showInEditor = false;

                        reactInsertPos = -1;

                        if (EditorApplication.isPlaying)
                        {
                            stickyShapesModule.ReinitialiseReactions();
                        }

                        GUIUtility.ExitGUI();
                        return;
                    }
                    else if (reactDeletePos >= 0)
                    {
                        stickyShapesModule.StopEnterReactionInstantly(reactDeletePos + 1);

                        // Modify the list rather than reactListProp.DeleteArrayElementAtIndex(..)
                        // which seems to reset all non-serialised field in other list members...
                        serializedObject.ApplyModifiedProperties();
                        // Force changes to be noted
                        Undo.RecordObject(stickyShapesModule, "Delete Reaction");
                        List<S3DReact> reList = stickyShapesModule.GetReactList;
                        reList.RemoveAt(reactDeletePos);
                        reactDeletePos = -1;
                        //stickyShapesModule.RefreshVOXEmotions();

                        if (EditorApplication.isPlaying)
                        {
                            stickyShapesModule.ReinitialiseReactions();
                        }

                        GUIUtility.ExitGUI();
                        return;
                    }
                }
                #endregion
            }

            StickyEditorHelper.DrawHorizontalGap(2);


            #endregion
        }

        /// <summary>
        /// Find all the skinned mesh renderers on the model that contain blendshapes
        /// </summary>
        protected virtual void FindSkinnedMeshRenderers()
        {
            if (skinnedMRenListProp.arraySize < 1 || StickyEditorHelper.PromptYesNo("Skinned Mesh Renderers", "Do you wish to replace the current list with ones on the model that contain blendshapes"))
            {
                stickyShapesModule.gameObject.GetComponentsInChildren(true, tempSMRenList);

                int numMRen = tempSMRenList.Count;

                skinnedMRenListProp.ClearArray();

                for (int sIdx = 0; sIdx < numMRen; sIdx++)
                {
                    SkinnedMeshRenderer sMRen = tempSMRenList[sIdx];

                    Mesh mesh = sMRen.sharedMesh;

                    if (mesh != null && mesh.blendShapeCount > 0)
                    {
                        skinnedMRenListProp.arraySize += 1;
                        SerializedProperty sMRenProp = skinnedMRenListProp.GetArrayElementAtIndex(sIdx);

                        sMRenProp.objectReferenceValue = sMRen;
                    }
                }

                serializedObject.ApplyModifiedProperties();
                GUIUtility.ExitGUI();
            }
        }

        /// <summary>
        /// Attempt to get the name of a blendshape, given a S3DBlendShape SerializedProperty
        /// </summary>
        /// <param name="serializedProperty"></param>
        /// <returns></returns>
        protected string GetBlendShapeName(SerializedProperty serializedProperty)
        {
            SerializedProperty smRenProp = serializedProperty.FindPropertyRelative("skinnedMeshRenderer");
            int blendShapeIndex = serializedProperty.FindPropertyRelative("blendShapeIndex").intValue;

            if (smRenProp != null && smRenProp.objectReferenceValue != null)
            {
                SkinnedMeshRenderer smRen = (SkinnedMeshRenderer)smRenProp.objectReferenceValue;
                Mesh mesh = smRen.sharedMesh;
                if (mesh != null && mesh.blendShapeCount > blendShapeIndex)
                {
                    return mesh.GetBlendShapeName(blendShapeIndex);
                }
                else { return string.Empty; }
            }
            else { return string.Empty; }
        }

        #endregion

        #region OnInspectorGUI

        public override void OnInspectorGUI()
        {
            DrawBaseInspector();
        }

        #endregion
    }
}