using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// An shapes module action that a character can perform
    /// </summary>
    [System.Serializable]
    public class S3DReact
    {
        #region Enumerations

        /// <summary>
        /// A single reaction is made up of different stages or phases.
        /// </summary>
        public enum ReactionStage
        {
            Enter = 0,
            Stay = 1,
            Exit = 2,
        }

        /// <summary>
        /// Which characters should this reaction apply to?
        /// Nobody can be used to disable the reaction.
        /// </summary>
        public enum ReactTo
        {
            Any = 0,
            Friendly = 1,
            FriendOnly = 2,
            Foe = 3,
            NeutralOnly = 4,
            Nobody = 99
        }

        #endregion

        #region Public Static Variables

        public static readonly int ReactStageEnterInt = (int)ReactionStage.Enter;
        public static readonly int ReactStageExitInt = (int)ReactionStage.Exit;
        public static readonly int ReactStageStayInt = (int)ReactionStage.Stay;

        public static readonly int ReactToAnyInt = (int)ReactTo.Any;
        public static readonly int ReactToFriendlyInt = (int)ReactTo.Friendly;
        public static readonly int ReactToFriendOnlyInt = (int)ReactTo.FriendOnly;
        public static readonly int ReactToFoeInt = (int)ReactTo.Foe;
        public static readonly int ReactToNeutralOnlyInt = (int)ReactTo.NeutralOnly;
        public static readonly int ReactToNobodyInt = (int)ReactTo.Nobody;

        #endregion

        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// Unique identifier
        /// </summary>
        public int guidHash;

        /// <summary>
        /// The descriptive name of the reaction
        /// </summary>
        public string reactName;

        /// <summary>
        /// A list of potential emotions that can be used when another
        /// character comes near.
        /// </summary>
        public List<S3DReactEmotion> enterReactEmotionList;

        /// <summary>
        /// A list of potential speech audios that can be used when another
        /// character comes near.
        /// </summary>
        public List<S3DReactSpeechAudio> enterReactSpeechAudioList;

        /// <summary>
        /// A list of potential emotions that can be used when another
        /// character stops being nearby.
        /// </summary>
        public List<S3DReactEmotion> exitReactEmotionList;

        /// <summary>
        /// A list of potential speech audios that can be used when another
        /// character stops being nearby.
        /// </summary>
        public List<S3DReactSpeechAudio> exitReactSpeechAudioList;

        /// <summary>
        /// Does the list of enter emotions get automatically populated
        /// from the Emotions tab based on emotion intensities and the
        /// react to setting?
        /// </summary>
        public bool isAutoEnterEmotions;

        /// <summary>
        /// Does the list of exit emotions get automatically populated
        /// from the Emotions tab based on emotion intensities and the
        /// react to setting of the reaction?
        /// </summary>
        public bool isAutoExitEmotions;

        /// <summary>
        /// [EDITOR ONLY] Is this react enter being previewed on the model?
        /// </summary>
        public bool isEnterPreviewMode;

        /// <summary>
        /// [EDITOR ONLY] Is this react exit being previewed on the model?
        /// </summary>
        public bool isExitPreviewMode;

        /// <summary>
        /// [EDITOR ONLY] Is this react stay being previewed on the model?
        /// </summary>
        public bool isStayPreviewMode;

        /// <summary>
        /// Editor only, is this reaction expanded in the Inspector?
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Which other characters should this character respond to based on the factions of both characters?
        /// Once the StickyShapesModule is initialised, call CacheData() after modifying this at runtime.
        /// </summary>
        public ReactTo reactTo;

        /// <summary>
        /// An optional array of Model IDs that will further limit which characters to react to.
        /// Once the StickyShapesModule is initialised, call CacheData() after modifying this at runtime.
        /// </summary>
        public int[] modelsToInclude;

        /// <summary>
        /// Don't modify directly. Call CacheData() instead.
        /// </summary>
        public int numModelsToInclude;

        /// <summary>
        /// There should only be at most one enter emotion playing for a reaction
        /// at any one time.
        /// </summary>
        [System.NonSerialized] public S3DReactEmotion enabledEnterReactEmotion;

        /// <summary>
        /// There should only be at most one enter speechAudio playing for a reaction
        /// </summary>
        [System.NonSerialized] public S3DReactSpeechAudio enabledEnterReactSpeechAudio;

        /// <summary>
        /// There should only be at most one exit emotion playing for a reaction
        /// at any one time.
        /// </summary>
        [System.NonSerialized] public S3DReactEmotion enabledExitReactEmotion;

        /// <summary>
        /// There should only be at most one exit speechAudio playing for a reaction
        /// </summary>
        [System.NonSerialized] public S3DReactSpeechAudio enabledExitReactSpeechAudio;

        /// <summary>
        /// There should only be at most one stay emotion playing for a reaction
        /// at any one time.
        /// </summary>
        [System.NonSerialized] public S3DReactEmotion enabledStayReactEmotion;

        /// <summary>
        /// There should only be at most one stat speechAudio playing for a reaction
        /// </summary>
        [System.NonSerialized] public S3DReactSpeechAudio enabledStayReactSpeechAudio;

        /// <summary>
        /// Is an enter reaction currently enabled (in use)?
        /// </summary>
        [System.NonSerialized] public bool isEnterReactEnabled;

        /// <summary>
        /// Is an exit reaction currently enabled (in use)?
        /// </summary>
        [System.NonSerialized] public bool isExitReactEnabled;

        /// <summary>
        /// Is a stay reaction currently enabled (in use)?
        /// </summary>
        [System.NonSerialized] public bool isStayReactEnabled;

        /// <summary>
        /// Is the reaction currently enabled (in use)?
        /// See also isEnterReactEnabled and isExitReactEnabled
        /// </summary>
        [System.NonSerialized] public bool isReactEnabled;

        /// <summary>
        /// Runtime caching of number of enter S3DReactEmotions
        /// </summary>
        [System.NonSerialized] public int numEnterEmotions;

        /// <summary>
        /// Runtime caching of number of enter S3DReactSpeechAudios
        /// </summary>
        [System.NonSerialized] public int numEnterSpeechAudios;

        /// <summary>
        /// Runtime caching of number of exit S3DReactEmotions
        /// </summary>
        [System.NonSerialized] public int numExitEmotions;

        /// <summary>
        /// Runtime caching of number of exit S3DReactSpeechAudios
        /// </summary>
        [System.NonSerialized] public int numExitSpeechAudios;

        /// <summary>
        /// Which other characters should this character respond to?
        /// </summary>
        [System.NonSerialized] public int reactToInt;

        /// <summary>
        /// The character performing the reaction (typically the character
        /// attached to stickyShapeModule).
        /// </summary>
        [System.NonSerialized] public int stickyID;

        /// <summary>
        /// The other character that is currently being reacted to
        /// </summary>
        [System.NonSerialized] public int reactedToStickyID;

        /// <summary>
        /// The model Id of the other character that is currently being reacted to 
        /// </summary>
        [System.NonSerialized] public int reactedToModelId;

        /// <summary>
        /// If the other character that is currently being reacted to is friend, foe or neutral
        /// </summary>
        [System.NonSerialized] public int reactedToFriendOrFoe;

        #endregion

        #region Public Events

        /// <summary>
        /// These are triggered immediately before reacting to when another character come nearby.
        /// </summary>
        public S3DReactEvt1 onPreEnter;

        /// <summary>
        /// These are triggered immediately after reacting to when another character come nearby.
        /// </summary>
        public S3DReactEvt1 onPostEnter;

        /// <summary>
        /// These are triggered immediately before reacting to when other characters stop being nearby.
        /// </summary>
        public S3DReactEvt1 onPreExit;

        /// <summary>
        /// These are triggered immediately after reacting to when other characters stop being nearby.
        /// </summary>
        public S3DReactEvt1 onPostExit;

        #endregion

        #region Public Variables - Editor

        /// <summary>
        /// Remember which tabs etc were shown in the editor
        /// </summary>
        [HideInInspector] public int selectedTabInt;

        /// <summary>
        /// Are the emotion settings expanded?
        /// </summary>
        [HideInInspector] public bool isReactEnterExpanded;

        /// <summary>
        /// Are the event settings expanded?
        /// </summary>
        [HideInInspector] public bool isReactStayExpanded;

        /// <summary>
        /// Are the speech audio settings expanded?
        /// </summary>
        [HideInInspector] public bool isReactExitExpanded;

        /// <summary>
        /// [EDITOR ONLY] Is the enter react emotion list expanded in the Inspector?
        /// </summary>
        [HideInInspector] public bool isEnterEmotionListExpanded;

        /// <summary>
        /// [EDITOR ONLY] Is the enter react events expanded in the Inspector?
        /// </summary>
        [HideInInspector] public bool isEnterEventsExpanded;

        /// <summary>
        /// [EDITOR ONLY] Is the enter react speech audio list expanded in the Inspector?
        /// </summary>
        [HideInInspector] public bool isEnterSpeechAudioListExpanded;

        /// <summary>
        /// [EDITOR ONLY] Is the stay react emotion list expanded in the Inspector?
        /// </summary>
        [HideInInspector] public bool isStayEmotionListExpanded;

        /// <summary>
        /// [EDITOR ONLY] Is the stay react events expanded in the Inspector?
        /// </summary>
        [HideInInspector] public bool isStayEventsExpanded;

        /// <summary>
        /// [EDITOR ONLY] Is the stay react speech audio list expanded in the Inspector?
        /// </summary>
        [HideInInspector] public bool isStaySpeechAudioListExpanded;

        /// <summary>
        /// [EDITOR ONLY] Is the exit react emotion list expanded in the Inspector?
        /// </summary>
        [HideInInspector] public bool isExitEmotionListExpanded;

        /// <summary>
        /// [EDITOR ONLY] Is the exit react events expanded in the Inspector?
        /// </summary>
        [HideInInspector] public bool isExitEventsExpanded;

        /// <summary>
        /// [EDITOR ONLY] Is the exit react speech audio list expanded in the Inspector?
        /// </summary>
        [HideInInspector] public bool isExitSpeechAudioListExpanded;

        #endregion

        #region Public Properties

        #endregion

        #region Constructors

        public S3DReact()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="s3dReact"></param>
        public S3DReact(S3DReact s3dReact)
        {
            if (s3dReact == null) { SetClassDefaults(); }
            else
            {
                guidHash = s3dReact.guidHash;
                reactName = s3dReact.reactName;
                showInEditor = s3dReact.showInEditor;
                isEnterEmotionListExpanded = s3dReact.isEnterEmotionListExpanded;
                isExitEmotionListExpanded = s3dReact.isExitEmotionListExpanded;
                isStayEmotionListExpanded = s3dReact.isStayEmotionListExpanded;
                isAutoEnterEmotions = s3dReact.isAutoEnterEmotions;
                isAutoExitEmotions = s3dReact.isAutoExitEmotions;
                reactTo = s3dReact.reactTo;

                // Deep copy
                if (s3dReact.enterReactEmotionList == null) { this.enterReactEmotionList = new List<S3DReactEmotion>(2); }
                else { this.enterReactEmotionList = s3dReact.enterReactEmotionList.ConvertAll(re => new S3DReactEmotion(re)); }

                if (s3dReact.enterReactSpeechAudioList == null) { this.enterReactSpeechAudioList = new List<S3DReactSpeechAudio>(2); }
                else { this.enterReactSpeechAudioList = s3dReact.enterReactSpeechAudioList.ConvertAll(rsa => new S3DReactSpeechAudio(rsa)); }

                if (s3dReact.exitReactEmotionList == null) { this.exitReactEmotionList = new List<S3DReactEmotion>(2); }
                else { this.exitReactEmotionList = s3dReact.exitReactEmotionList.ConvertAll(re => new S3DReactEmotion(re)); }

                if (s3dReact.exitReactSpeechAudioList == null) { this.exitReactSpeechAudioList = new List<S3DReactSpeechAudio>(2); }
                else { this.exitReactSpeechAudioList = s3dReact.exitReactSpeechAudioList.ConvertAll(rsa => new S3DReactSpeechAudio(rsa)); }

                if (s3dReact.modelsToInclude == null) { this.modelsToInclude = null; }
                else { this.modelsToInclude = System.Array.ConvertAll(s3dReact.modelsToInclude, m => m); }

                // This doesn't do a deep copy and UnityEvents are problematic to copy
                //onPreEnter = s3dReact.onPreEnter;
                //onPostEnter = s3dReact.onPostEnter;
                //onPreExit = s3dReact.onPreExit;
                //onPostExit = s3dReact.onPostExit;

                // Currently can't copy them...
                onPreEnter = null;
                onPostEnter = null;
                onPreExit = null;
                onPostExit = null;

                // Off by default
                isEnterPreviewMode = false;
                isExitPreviewMode = false;
                isStayPreviewMode = false;
                isReactEnterExpanded = false;
                isReactStayExpanded = false;
                isReactExitExpanded = false;

                reactedToStickyID = 0;
                reactedToModelId = 0;
                reactedToFriendOrFoe = 0;

                // Disable after a copy
                isReactEnabled = false;
                isEnterReactEnabled = false;
                isExitReactEnabled = false;
                isStayReactEnabled = false;

                CacheData();
            }
        }


        #endregion

        #region Public member Methods

        /// <summary>
        /// Update any cached data required at runtime
        /// </summary>
        public virtual void CacheData()
        {
            numEnterEmotions = enterReactEmotionList.Count;
            numEnterSpeechAudios = enterReactSpeechAudioList.Count;
            numExitEmotions = exitReactEmotionList.Count;
            numExitSpeechAudios = exitReactSpeechAudioList.Count;

            numModelsToInclude = modelsToInclude == null ? 0 : modelsToInclude.Length;

            reactToInt = (int)reactTo;
        }

        public virtual void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            isEnterPreviewMode = false;
            isExitPreviewMode = false;
            isStayPreviewMode = false;

            enterReactEmotionList = new List<S3DReactEmotion>(2);
            enterReactSpeechAudioList = new List<S3DReactSpeechAudio>(2);
            exitReactEmotionList = new List<S3DReactEmotion>(2);
            exitReactSpeechAudioList = new List<S3DReactSpeechAudio>(2);
            reactName = "[reaction name]";

            showInEditor = true;
            selectedTabInt = 0;
            isAutoEnterEmotions = false;
            isAutoExitEmotions = false;
            isEnterEmotionListExpanded = false;
            isExitEmotionListExpanded = false;
            isStayEmotionListExpanded = false;

            reactTo = ReactTo.Any;

            modelsToInclude = null;
            numModelsToInclude = 0;
            onPreEnter = null;
            onPostEnter = null;
            onPreExit = null;
            onPostExit = null;

            isReactEnabled = false;
            isEnterReactEnabled = false;
            isExitReactEnabled = false;
            isStayReactEnabled = false;
            isReactEnterExpanded = false;
            isReactStayExpanded = false;

            reactedToStickyID = 0;
            reactedToModelId = 0;
            reactedToFriendOrFoe = 0;
        }

        #endregion
    }
}