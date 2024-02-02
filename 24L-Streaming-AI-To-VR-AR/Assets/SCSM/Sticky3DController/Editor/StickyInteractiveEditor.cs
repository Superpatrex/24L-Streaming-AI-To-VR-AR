using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyInteractive))]
    [CanEditMultipleObjects]
    public class StickyInteractiveEditor : Editor
    {
        #region Custom Editor protected variables
        // These are visible to inherited classes
        protected StickyInteractive stickyInteractive;
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
        protected Color separatorColor = new Color();
        protected float defaultEditorLabelWidth = 0f;
        protected float defaultEditorFieldWidth = 0f;
        protected bool isDebuggingEnabled = false;

        protected string[] interactiveTagNames = null;
        #endregion

        #region SceneView Variables

        protected bool isSceneDirtyRequired = false;
        protected bool isHandHold1Selected = false;
        protected bool isHandHold2Selected = false;
        protected bool isEquipPointSelected = false;
        protected bool isSocketPointSelected = false;

        protected Quaternion sceneViewInteractiveRot = Quaternion.identity;
        protected Vector3 componentHandlePosition = Vector3.zero;
        protected Quaternion componentHandleRotation = Quaternion.identity;
        protected float relativeHandleSize = 1f;

        // Pale Green
        protected Color handHoldGizmoColour = new Color(152f / 255f, 251f / 255f, 152f / 255f, 1f);

        protected Color fadedGizmoColour;

        #endregion

        #region Static Strings

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("This enables you to make an object in the scene interactive");
        private readonly static GUIContent[] tabTexts = { new GUIContent("Interactive"), new GUIContent("Events") };

        #endregion

        #region GUIContent - General
        protected readonly static GUIContent initialiseOnStartContent = new GUIContent(" Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the Sticky Interactive component is enabled through code.");
        protected readonly static GUIContent isActivableContent = new GUIContent(" Is Activable", "Can this item be activated? Typically used with grabbable objects.");
        protected readonly static GUIContent isAutoDeactivateContent = new GUIContent("  Auto Deactivate", "When the item is activated, it is automatically deactivated making it a single on and off action.  Only the OnActivated events are triggered.");
        protected readonly static GUIContent isDeactivateOnDropContent = new GUIContent("  Deactivate on Drop", "When a grabbable and activable object is dropped while activated, it will be deactivated.");
        protected readonly static GUIContent isActivablePriorityOverGrabContent = new GUIContent("  Priority over Grab", "When activable and Is grabbable is true, activable will be considered before grab when EngageLookAtInteractive() is called.");
        protected readonly static GUIContent isEquippableContent = new GUIContent(" Is Equippable", "Can the item be attached to the character body (typically a bone). When equipped, interactive-enabled objects become dormant or inactive. The objects remain visible. See also Is Grabbable and Is Stashable.");
        protected readonly static GUIContent isReadableContent = new GUIContent(" Is Readable", "Can values be read from this object? This is typically used for reading the position of a virtual lever or joystick.");
        protected readonly static GUIContent isGrabbableContent = new GUIContent(" Is Grabbable", "Can this item be grabbed or held?");
        protected readonly static GUIContent isSelectableContent = new GUIContent(" Is Selectable", "Can this item be selected in the scene?");
        protected readonly static GUIContent isSittableContent = new GUIContent(" Is Sittable", "Is this item suitable for sitting on?");
        protected readonly static GUIContent isSocketableContent = new GUIContent(" Is Socketable", "Is this item be attached to a StickySocket?");
        protected readonly static GUIContent isStashableContent = new GUIContent(" Is Stashable", "Can this item be stashed in the inventory of a character?");
        protected readonly static GUIContent isAutoUnselectContent = new GUIContent("  Auto Unselect", "When the item is selected, it is automatically unselected making it like a button click event. Only the OnSelected events are triggered.");
        protected readonly static GUIContent isTouchableContent = new GUIContent(" Is Touchable", "Can this item be touched by a character? Typically used with Hand IK.");
        protected readonly static GUIContent isCarryInHandContent = new GUIContent("  Carry in Hand", "When grabbed, the interactive-enabled object will be held in the palm of the hand. Turn this off for objects like levers with a non-kinematic (dynamic) rigidbody which you do not want the character to carry. Currently, should always be on unless using VR and StickyXRInteractors.");
        protected readonly static GUIContent isParentOnGrabContent = new GUIContent("  Parented on Grab", "When grabbed, the interactive-enabled object will be parented to the hand of S3D character");
        protected readonly static GUIContent isReparentOnDropContent = new GUIContent("  Re-parented on Drop", "Attempt to reparent the interactive-enabled object to the original parent gameobject when it was equipped, grabbed, or stashed.");
        protected readonly static GUIContent isDisableRegularColOnGrabContent = new GUIContent("  Disable Regular Colliders", "When grabbed, non-trigger colliders on this interactive-enabled object will be disabled. Unless this is a Weapon or Magazine, when dropped, they will NOT be enabled. To enable them add EnableNonTriggerColliders() to OnDropped.");
        protected readonly static GUIContent isDisableTriggerColOnGrabContent = new GUIContent("  Disable Trigger Colliders", "When grabbed, trigger colliders on this interactive-enabled object will be disabled. Unless this is a Weapon or Magazine, when dropped, they will NOT be enabled. To enable them add EnableTriggerColliders() to OnDropped.");
        protected readonly static GUIContent isRemoveRigidbodyOnGrabContent = new GUIContent("  Remove Rigidbody on Grab", "When grabbed, if there is a rigidbody attached, remove it.");
        #endregion

        #region GUIContent - Gravity
        protected readonly static GUIContent gravitySettingsContent = new GUIContent(" Gravity Settings");
        protected readonly static GUIContent isUseGravityContent = new GUIContent(" Use Gravity", "The object is affected by gravity");
        protected readonly static GUIContent gravityModeContent = new GUIContent(" Gravity Mode", "The method used to determine in which direction gravity is acting.");
        protected readonly static GUIContent gravitationalAccelerationContent = new GUIContent(" Gravity", "The gravitational acceleration, in metres per second per second, that acts downward for the object");
        protected readonly static GUIContent gravityDirectionContent = new GUIContent(" Gravity Direction", "The world space direction that gravity acts upon the weapon when Gravity Mode is Direction.");
        protected readonly static GUIContent dragContent = new GUIContent(" Drag", "The amount of drag the object has. A solid block of metal would be 0.001, while a feather would be 10.");
        protected readonly static GUIContent angularDragContent = new GUIContent(" Angular Drag", "The amount of angular drag the object has");
        protected readonly static GUIContent interpolationContent = new GUIContent(" Interpolation", "The rigidbody interpolation");
        protected readonly static GUIContent collisionDetectionContent = new GUIContent(" Collision Detection", "The rigidbody collision detection mode");
        protected readonly static GUIContent initialReferenceFrameContent = new GUIContent(" Initial Reference Frame", "Initial or default reference frame transform the object will stick to when Use Gravity is enabled and Gravity Mode is ReferenceFrame.");
        protected readonly static GUIContent isInheritGravityContent = new GUIContent(" Inherit Gravity", "If gravity is enabled and Gravity Mode is Reference Frame, when the object is dropped, it will inherit the reference frame from the character.");

        #endregion

        #region GUIContent - Equippable
        protected readonly static GUIContent equipPointContent = new GUIContent("  Equippable Point Relative Position");
        protected readonly static GUIContent equipOffsetContent = new GUIContent("  Equip Offset", "The local space equip position offset from a character equip point");
        protected readonly static GUIContent equipRotationContent = new GUIContent("  Equip Rotation", "The local space rotation, stored in degrees, around a character equip point");
        protected readonly static GUIContent equipFromHeldDelayContent = new GUIContent("  Equip from Held Delay", "The amount of time, in seconds, that a held object will delay being parented to the equip point. This could be used to allow time for say a weapon holster animation to run before the equip operation is completed.");

        #endregion

        #region GUIContent - Hand Hold
        protected readonly static GUIContent handHold1Content = new GUIContent(" Hand Hold Primary Relative Position", "The first or primary relative local space hand hold position");
        protected readonly static GUIContent handHold1OffsetContent = new GUIContent("  Hold 1 Offset", "The first or primary local space hand hold offset");
        protected readonly static GUIContent handHold1RotationContent = new GUIContent("  Hold 1 Rotation", "The first or primary local space hand hold rotation stored in degrees. Arrow gizmo points in direction hand palm is facing. Sphere points towards thumb direction.");
        protected readonly static GUIContent handHold1FlipForLeftHandContent = new GUIContent("  Hold 1 Flip for LH", "For the first or primary hand hold, flip the rotation for left hand when using Sticky XR Interactor. This can be useful when the hand hold position is on a symmetrical handle like a bat, racket, or spear.");
        protected readonly static GUIContent handHold2Content = new GUIContent(" Hand Hold Secondary Relative Position", "The second or secondary relative local space hand hold position");
        protected readonly static GUIContent handHold2OffsetContent = new GUIContent("  Hold 2 Offset", "The second or secondary local space hand hold offset");
        protected readonly static GUIContent handHold2RotationContent = new GUIContent("  Hold 2 Rotation", "The second or secondary local space hand hold rotation stored in degrees. Arrow gizmo points in direction hand palm is facing. Sphere points towards thumb direction.");
        protected readonly static GUIContent handHold2FlipForLeftHandContent = new GUIContent("  Hold 2 Flip for LH", "For the second or secondary hand hold, flip the rotation for left hand when using Sticky XR Interactor. This can be useful when the hand hold position is on a symmetrical handle like a bat, racket, or spear.");
        #endregion

        #region GUIContent - Readable
        //protected readonly static GUIContent readableJointContent = new GUIContent("  Readable Joint", "The joint to read positional data from when Is Readable is true.");
        protected readonly static GUIContent readablePivotContent = new GUIContent("  Readable Pivot", "The point where the readable lever pivots around.");
        protected readonly static GUIContent readableDeadZoneContent = new GUIContent("  Dead Zone", "The normalised amount the pivot can move before values are updated.");
        protected readonly static GUIContent readableInvertXContent = new GUIContent("  Invert X", "Invert the x-axis value (left and right) from the readable pivot.");
        protected readonly static GUIContent readableInvertZContent = new GUIContent("  Invert Z", "Invert the z-axis value (forward and backward) from the readable pivot.");
        protected readonly static GUIContent readableMinXContent = new GUIContent("  Min X (left)", "The maximum angle, in degrees, the pivot can rotate left.");
        protected readonly static GUIContent readableMinZContent = new GUIContent("  Min Z (back)", "The maximum angle, in degrees, the pivot can rotate backward.");
        protected readonly static GUIContent readableMaxXContent = new GUIContent("  Max X (right)", "The maximum angle, in degrees, the pivot can rotate right.");
        protected readonly static GUIContent readableMaxZContent = new GUIContent("  Max Z (forward)", "The maximum angle, in degrees, the pivot can rotate forward.");
        protected readonly static GUIContent readableAutoRecentreContent = new GUIContent("  Auto Recentre", "When the readable object is released (dropped) it will automatically return to the centre using the spring mechanism.");
        protected readonly static GUIContent readableSensitivityXContent = new GUIContent("  Sensitivity X", "Speed to move towards target left-right value. Lower values make it less sensitive");
        protected readonly static GUIContent readableSensitivityZContent = new GUIContent("  Sensitivity Z", "Speed to move towards target forward-back value. Lower values make it less sensitive");
        protected readonly static GUIContent readableSpringStrengthContent = new GUIContent("  Spring Strength", "The the strength of the spring used to return the readable pivot back to centre");
        #endregion

        #region GUIContent - Sittable

        protected readonly static GUIContent sitTargetOffsetContent = new GUIContent("  Sit Target Offset", "The local space relative offset that the character should aim for when getting ready to sit down.");
        protected readonly static GUIContent isSeatAllocatedContent = new GUIContent("  Is Seat Allocated", "If the object is sittable, is the seat allocated? Typically, used when reserving a seat for a character to sit on.");

        #endregion

        #region GUIContent - Equippable
        protected readonly static GUIContent socketPointContent = new GUIContent("  Socket Point Relative Position");
        protected readonly static GUIContent socketOffsetContent = new GUIContent("  Socket Offset", "The local space position offset which will snap to the StickySocket");
        protected readonly static GUIContent socketRotationContent = new GUIContent("  Socket Rotation", "The local space rotation, stored in degrees, around a StickySocket point");
        #endregion

        #region GUIContent - Stashable

        #endregion

        #region GUIContent - Other
        protected readonly static GUIContent massContent = new GUIContent(" Mass (KG)", "Mass of the object in kilograms. Can be used when dropping the object.");
        #endregion

        #region GUIContent - Popup
        protected readonly static GUIContent defaultPopupOffsetContent = new GUIContent(" Popup Offset", "The default local space offset a StickyPopupModule appears relative to the interactive-enabled object");
        protected readonly static GUIContent isPopupRelativeUpContent = new GUIContent(" Popup Relative Up", "Use the relative up direction to apply default Popup Offset. This can be useful if you want a popup to appear above the interactive object. Only applies when Use Gravity is enabled.");

        #endregion

        #region GUIContent - Tags
        protected readonly static GUIContent interactiveTagsContent = new GUIContent(" Interactive Tags", "This Scriptable Object containing a list of 32 tags. Typically, you will only need one per project. To create custom tags, in the Project pane, click Create->Sticky3D->Interactive Tags.");
        protected readonly static GUIContent interactiveTagContent = new GUIContent(" Tag", "The interactive tag used to determine compatibility with things like StickySockets or character Equip Points.");
        #endregion

        #region GUIContent - Events
        protected readonly static GUIContent onActivatedContent = new GUIContent(" On Activated", "These are triggered by a S3D character when they activate this interactive object.");
        protected readonly static GUIContent onDeactivatedContent = new GUIContent(" On Deactivated", "These are triggered by a S3D character when they deactivate this interactive object.");
        protected readonly static GUIContent onGrabbedContent = new GUIContent(" On Grabbed", "These are triggered by a S3D character when they grab this interactive object.");
        protected readonly static GUIContent onDroppedContent = new GUIContent(" On Dropped", "These are triggered by a S3D character when they drop this interactive object.");
        protected readonly static GUIContent onHoverEnterContent = new GUIContent(" On Hover Enter", "These are triggered by a S3D character when they start looking at this interactive object.");
        protected readonly static GUIContent onHoverExitContent = new GUIContent(" On Hover Exit", "These are triggered by a S3D character when they stop looking at this interactive object.");
        protected readonly static GUIContent onPostEquippedContent = new GUIContent(" On Post Equipped", "These are triggered by a S3D character immediately after they equip this interactive object.");
        protected readonly static GUIContent onPostGrabbedContent = new GUIContent(" On Post Grabbed", "These are triggered by a S3D character immediately after they grab this interactive object.");
        protected readonly static GUIContent onPostInitialisedEvtDelayContent = new GUIContent("   On Post Init Event Delay", "The number of seconds to delay firing the onPostInitialised event methods after the interactive object has been initialised.");

        protected readonly static GUIContent onPostInitialisedContent = new GUIContent(" On Post Initialised", "These are triggered immediately after the interactive-enabled object is initialised.");
        protected readonly static GUIContent onPostStashedContent = new GUIContent(" On Post Stashed", "These are triggered by a S3D character immediately after they stash (inventory) this interactive object.");
        protected readonly static GUIContent onSelectedContent = new GUIContent(" On Selected", "These are triggered when this interactive object is selected via the API.");
        protected readonly static GUIContent onUnselectedContent = new GUIContent(" On Unselected", "These are triggered when this interactive object is unselected via the API.");
        protected readonly static GUIContent onTouchedContent = new GUIContent(" On Touched", "These are triggered by a S3D character when the interactive object is first touched via the API.");
        protected readonly static GUIContent onStoppedTouchingContent = new GUIContent(" On Stopped Touching", "These are triggered by a S3D character when the interactive object is no longer being touched via the API.");
        protected readonly static GUIContent onReadableValueChangedContent = new GUIContent(" On Readable Value Changed", "These are triggered when the value of the joint position changes if the interactive object is readable.");
        #endregion

        #region GUIContent - Debug
        protected readonly static GUIContent debugIsHeldContent = new GUIContent(" Is Held?");
        protected readonly static GUIContent debugHeldByContent = new GUIContent(" Held By");
        protected readonly static GUIContent debugIsSocketedContent = new GUIContent(" Is Socketed?");
        protected readonly static GUIContent debugSocketedOnContent = new GUIContent(" Socketed On");
        protected readonly static GUIContent debugIsStashedContent = new GUIContent(" Is Stashed?");
        protected readonly static GUIContent debugStashedByContent = new GUIContent(" Stashed By");
        protected readonly static GUIContent debugCurrentReferenceFrameContent = new GUIContent(" Current Reference Frame");
        protected readonly static GUIContent debugIsReadableContent = new GUIContent(" IsReadable");
        protected readonly static GUIContent debugReadableXContent = new GUIContent(" Left-Right");
        protected readonly static GUIContent debugReadableZContent = new GUIContent(" Forward-Back");

        #endregion

        #region Serialized Properties - General
        protected SerializedProperty selectedTabIntProp;
        protected SerializedProperty initialiseOnStartProp;
        protected SerializedProperty isActivableProp;
        protected SerializedProperty isEquippableProp;
        protected SerializedProperty isReadableProp;
        protected SerializedProperty isGrabbableProp;
        protected SerializedProperty isSelectableProp;
        protected SerializedProperty isSittableProp;
        protected SerializedProperty isSocketableProp;
        protected SerializedProperty isStashableProp;
        protected SerializedProperty isTouchableProp;
        protected SerializedProperty isAutoDeactivateProp;
        protected SerializedProperty isDeactivateOnDropProp;
        protected SerializedProperty isActivablePriorityOverGrabProp;
        protected SerializedProperty isAutoUnselectProp;
        protected SerializedProperty isCarryInHandProp;
        protected SerializedProperty isParentOnGrabProp;
        protected SerializedProperty isReparentOnDropProp;
        protected SerializedProperty isDisableRegularColOnGrabProp;
        protected SerializedProperty isDisableTriggerColOnGrabProp;
        protected SerializedProperty isRemoveRigidbodyOnGrabProp;
        protected SerializedProperty massProp;
        #endregion

        #region Serialized Properties - Equippable
        protected SerializedProperty equipOffsetProp;
        protected SerializedProperty equipRotationProp;
        protected SerializedProperty equipFromHeldDelayProp;
        protected SerializedProperty showEQPGizmosInSceneViewProp;
        #endregion

        #region Serialized Properties - Gravity
        private SerializedProperty showGravitySettingsInEditorProp;
        protected SerializedProperty isUseGravityProp;
        protected SerializedProperty gravityModeProp;
        protected SerializedProperty gravitationalAccelerationProp;
        protected SerializedProperty gravityDirectionProp;
        protected SerializedProperty dragProp;
        protected SerializedProperty angularDragProp;
        protected SerializedProperty interpolationProp;
        protected SerializedProperty collisionDetectionProp;
        protected SerializedProperty initialReferenceFrameProp;
        protected SerializedProperty isInheritGravityProp;
        #endregion

        #region Serialized Properties - Readable
        //private SerializedProperty readableJointProp;
        private SerializedProperty readablePivotProp;
        private SerializedProperty readableDeadZoneProp;
        private SerializedProperty readableInvertXProp;
        private SerializedProperty readableInvertZProp;
        private SerializedProperty readableMinXProp;
        private SerializedProperty readableMinZProp;
        private SerializedProperty readableMaxXProp;
        private SerializedProperty readableMaxZProp;
        private SerializedProperty readableSensitivityXProp;
        private SerializedProperty readableSensitivityZProp;
        private SerializedProperty readableSpringStrengthProp;
        private SerializedProperty readableAutoRecentreProp;
        #endregion

        #region Serialized Properties - Hand Hold
        private SerializedProperty handHold1OffsetProp;
        private SerializedProperty handHold2OffsetProp;
        private SerializedProperty handHold1RotationProp;
        private SerializedProperty handHold2RotationProp;
        private SerializedProperty handHold1FlipForLeftHandProp;
        private SerializedProperty handHold2FlipForLeftHandProp;
        private SerializedProperty showHH1GizmosInSceneViewProp;
        private SerializedProperty showHH1LHGizmosInSceneViewProp;
        private SerializedProperty showHH2GizmosInSceneViewProp;
        private SerializedProperty showHH2LHGizmosInSceneViewProp;
        #endregion

        #region Serialized Properties - Popup
        private SerializedProperty defaultPopupOffsetProp;
        private SerializedProperty isPopupRelativeUpProp;
        #endregion

        #region Serialized Properties - Tags
        private SerializedProperty interactiveTagsProp;
        private SerializedProperty interactiveTagProp;
        #endregion

        #region Serialized Properties - Sittable
        private SerializedProperty sitTargetOffsetProp;
        private SerializedProperty isSeatAllocatedProp;
        #endregion

        #region Serialized Properties - Socketable
        protected SerializedProperty socketOffsetProp;
        protected SerializedProperty socketRotationProp;
        protected SerializedProperty showSOCPGizmosInSceneViewProp;
        #endregion

        #region Serialized Properties - Stashable

        #endregion

        #region Serializable Properties - Events
        private SerializedProperty onActivatedProp;
        private SerializedProperty onDeactivatedProp;
        private SerializedProperty onGrabbedProp;
        private SerializedProperty onDroppedProp;
        private SerializedProperty onHoverEnterProp;
        private SerializedProperty onHoverExitProp;
        private SerializedProperty onPostEquippedProp;
        private SerializedProperty onPostGrabbedProp;
        private SerializedProperty onPostInitialisedEvtDelayProp;
        private SerializedProperty onPostInitialisedProp;
        private SerializedProperty onPostStashedProp;
        private SerializedProperty onSelectedProp;
        private SerializedProperty onUnselectedProp;
        private SerializedProperty onTouchedProp;
        private SerializedProperty onStoppedTouchingProp;
        private SerializedProperty onReadableValueChangedProp;
        #endregion

        #region Events

        protected virtual void OnEnable()
        {
            stickyInteractive = (StickyInteractive)target;

            SceneView.duringSceneGui -= SceneGUI;
            SceneView.duringSceneGui += SceneGUI;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            // Reset GUIStyles
            isStylesInitialised = false;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;

            interactiveTagNames = null;

            #region Find Properties - General
            selectedTabIntProp = serializedObject.FindProperty("selectedTabInt");
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            isActivableProp = serializedObject.FindProperty("isActivable");
            isAutoDeactivateProp = serializedObject.FindProperty("isAutoDeactivate");
            isDeactivateOnDropProp = serializedObject.FindProperty("isDeactivateOnDrop");
            isActivablePriorityOverGrabProp = serializedObject.FindProperty("isActivablePriorityOverGrab");
            isEquippableProp = serializedObject.FindProperty("isEquippable");
            isReadableProp = serializedObject.FindProperty("isReadable");
            isGrabbableProp = serializedObject.FindProperty("isGrabbable");
            isSelectableProp = serializedObject.FindProperty("isSelectable");
            isSittableProp = serializedObject.FindProperty("isSittable");
            isSocketableProp = serializedObject.FindProperty("isSocketable");
            isStashableProp = serializedObject.FindProperty("isStashable");
            isTouchableProp = serializedObject.FindProperty("isTouchable");
            isAutoUnselectProp = serializedObject.FindProperty("isAutoUnselect");
            isCarryInHandProp = serializedObject.FindProperty("isCarryInHand");
            isParentOnGrabProp = serializedObject.FindProperty("isParentOnGrab");
            isReparentOnDropProp = serializedObject.FindProperty("isReparentOnDrop");
            isDisableRegularColOnGrabProp = serializedObject.FindProperty("isDisableRegularColOnGrab");
            isDisableTriggerColOnGrabProp = serializedObject.FindProperty("isDisableTriggerColOnGrab");
            isRemoveRigidbodyOnGrabProp = serializedObject.FindProperty("isRemoveRigidbodyOnGrab");
            massProp = serializedObject.FindProperty("mass");

            #endregion

            #region Find Properties - Equippable
            equipOffsetProp = serializedObject.FindProperty("equipOffset");
            equipRotationProp = serializedObject.FindProperty("equipRotation");
            equipFromHeldDelayProp = serializedObject.FindProperty("equipFromHeldDelay");
            showEQPGizmosInSceneViewProp = serializedObject.FindProperty("showEQPGizmosInSceneView");
            #endregion

            #region Find Properties - Gravity
            showGravitySettingsInEditorProp = serializedObject.FindProperty("showGravitySettingsInEditor");
            isUseGravityProp = serializedObject.FindProperty("isUseGravity");
            gravityModeProp = serializedObject.FindProperty("gravityMode");
            gravitationalAccelerationProp = serializedObject.FindProperty("gravitationalAcceleration");
            gravityDirectionProp = serializedObject.FindProperty("gravityDirection");
            dragProp = serializedObject.FindProperty("drag");
            angularDragProp = serializedObject.FindProperty("angularDrag");
            interpolationProp = serializedObject.FindProperty("interpolation");
            collisionDetectionProp = serializedObject.FindProperty("collisionDetection");
            initialReferenceFrameProp = serializedObject.FindProperty("initialReferenceFrame");
            isInheritGravityProp = serializedObject.FindProperty("isInheritGravity");
            #endregion

            #region Find Properties - Readable
            //readableJointProp = serializedObject.FindProperty("readableJoint");
            readablePivotProp = serializedObject.FindProperty("readablePivot");
            readableDeadZoneProp = serializedObject.FindProperty("readableDeadZone");
            readableInvertXProp = serializedObject.FindProperty("readableInvertX");
            readableInvertZProp = serializedObject.FindProperty("readableInvertZ");
            readableMinXProp = serializedObject.FindProperty("readableMinX");
            readableMinZProp = serializedObject.FindProperty("readableMinZ");
            readableMaxXProp = serializedObject.FindProperty("readableMaxX");
            readableMaxZProp = serializedObject.FindProperty("readableMaxZ");
            readableAutoRecentreProp = serializedObject.FindProperty("readableAutoRecentre");
            readableSensitivityXProp = serializedObject.FindProperty("readableSensitivityX");
            readableSensitivityZProp = serializedObject.FindProperty("readableSensitivityZ");
            readableSpringStrengthProp = serializedObject.FindProperty("readableSpringStrength");
            #endregion

            #region Find Properties - Handhold
            handHold1OffsetProp = serializedObject.FindProperty("handHold1Offset");
            handHold2OffsetProp = serializedObject.FindProperty("handHold2Offset");
            handHold1RotationProp = serializedObject.FindProperty("handHold1Rotation");
            handHold2RotationProp = serializedObject.FindProperty("handHold2Rotation");
            handHold1FlipForLeftHandProp = serializedObject.FindProperty("handHold1FlipForLeftHand");
            handHold2FlipForLeftHandProp = serializedObject.FindProperty("handHold2FlipForLeftHand");
            showHH1GizmosInSceneViewProp = serializedObject.FindProperty("showHH1GizmosInSceneView");
            showHH1LHGizmosInSceneViewProp = serializedObject.FindProperty("showHH1LHGizmosInSceneView");
            showHH2GizmosInSceneViewProp = serializedObject.FindProperty("showHH2GizmosInSceneView");
            showHH2LHGizmosInSceneViewProp = serializedObject.FindProperty("showHH2LHGizmosInSceneView");
            #endregion

            #region Find Properties - Popup
            defaultPopupOffsetProp = serializedObject.FindProperty("defaultPopupOffset");
            isPopupRelativeUpProp = serializedObject.FindProperty("isPopupRelativeUp");
            #endregion

            #region Find Properties - Sittable
            sitTargetOffsetProp = serializedObject.FindProperty("sitTargetOffset");
            isSeatAllocatedProp = serializedObject.FindProperty("isSeatAllocated");
            #endregion

            #region Find Properties - Equippable
            socketOffsetProp = serializedObject.FindProperty("socketOffset");
            socketRotationProp = serializedObject.FindProperty("socketRotation");
            showSOCPGizmosInSceneViewProp = serializedObject.FindProperty("showSOCPGizmosInSceneView");
            #endregion

            #region Find Properties - Stashable

            #endregion

            #region Find Properties - Tags
            interactiveTagsProp = serializedObject.FindProperty("interactiveTags");
            interactiveTagProp = serializedObject.FindProperty("interactiveTag");
            #endregion

            #region Find Properties - Events
            onActivatedProp = serializedObject.FindProperty("onActivated");
            onDeactivatedProp = serializedObject.FindProperty("onDeactivated");
            onGrabbedProp = serializedObject.FindProperty("onGrabbed");
            onDroppedProp = serializedObject.FindProperty("onDropped");
            onHoverEnterProp = serializedObject.FindProperty("onHoverEnter");
            onHoverExitProp = serializedObject.FindProperty("onHoverExit");
            onPostEquippedProp = serializedObject.FindProperty("onPostEquipped");
            onPostGrabbedProp = serializedObject.FindProperty("onPostGrabbed");
            onPostInitialisedProp = serializedObject.FindProperty("onPostInitialised");
            onPostInitialisedEvtDelayProp = serializedObject.FindProperty("onPostInitialisedEvtDelay");
            onPostStashedProp = serializedObject.FindProperty("onPostStashed");
            onSelectedProp = serializedObject.FindProperty("onSelected");
            onUnselectedProp = serializedObject.FindProperty("onUnselected");
            onTouchedProp = serializedObject.FindProperty("onTouched");
            onStoppedTouchingProp = serializedObject.FindProperty("onStoppedTouching");
            onReadableValueChangedProp = serializedObject.FindProperty("onReadableValueChanged");
            #endregion
        }

        /// <summary>
        /// Called when the gameobject loses focus or Unity Editor enters/exits
        /// play mode
        /// </summary>
        protected virtual void OnDestroy()
        {
            SceneView.duringSceneGui -= SceneGUI;

            // Always unhide Unity tools when losing focus on this gameObject
            Tools.hidden = false;
        }

        /// <summary>
        /// Gets called automatically 10 times per second
        /// Comment out if not required
        /// </summary>
        //private void OnInspectorUpdate()
        //{
        //    // OnInspectorGUI() only registers events when the mouse is positioned over the custom editor window
        //    // This code forces OnInspectorGUI() to run every frame, so it registers events even when the mouse
        //    // is positioned over the scene view
        //    if (stickyInteractive.allowRepaint) { Repaint(); }
        //}

        #endregion

        #region Private and Protected Methods

        /// <summary>
        /// Check if we should turn off Tools (only do this if there are no Gizmos shown)
        /// </summary>
        protected void CheckHandHoldTools()
        {
            if (!showHH1GizmosInSceneViewProp.boolValue && !showHH2GizmosInSceneViewProp.boolValue &&
                !showEQPGizmosInSceneViewProp.boolValue && !showSOCPGizmosInSceneViewProp.boolValue &&
                Tools.hidden) { Tools.hidden = false; }
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

                buttonCompact = new GUIStyle("Button");
                buttonCompact.fontSize = 10;

                // Create a new button or else will effect the Button style for other buttons too
                toggleCompactButtonStyleNormal = new GUIStyle("Button");
                toggleCompactButtonStyleToggled = new GUIStyle(toggleCompactButtonStyleNormal);
                toggleCompactButtonStyleNormal.fontStyle = FontStyle.Normal;
                toggleCompactButtonStyleToggled.fontStyle = FontStyle.Bold;
                toggleCompactButtonStyleToggled.normal.background = toggleCompactButtonStyleToggled.active.background;

                isStylesInitialised = true;
            }
        }

        /// <summary>
        /// Draw the base events available in StickyInteractive
        /// </summary>
        protected void DrawBaseEvents()
        {
            if (isActivableProp.boolValue)
            {
                EditorGUILayout.PropertyField(onActivatedProp, onActivatedContent);
                EditorGUILayout.PropertyField(onDeactivatedProp, onDeactivatedContent);
            }

            if (isEquippableProp.boolValue)
            {
                EditorGUILayout.PropertyField(onPostEquippedProp, onPostEquippedContent);
            }

            if (isGrabbableProp.boolValue)
            {
                EditorGUILayout.PropertyField(onGrabbedProp, onGrabbedContent);
                EditorGUILayout.PropertyField(onPostGrabbedProp, onPostGrabbedContent);
            }

            if (isGrabbableProp.boolValue || isStashableProp.boolValue)
            {
                EditorGUILayout.HelpBox("See StickyInteractive - Dropping Objects in the (Help) manual", MessageType.Info);
                EditorGUILayout.PropertyField(onDroppedProp, onDroppedContent);
            }

            EditorGUILayout.PropertyField(onHoverEnterProp, onHoverEnterContent);
            EditorGUILayout.PropertyField(onHoverExitProp, onHoverExitContent);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(onPostInitialisedEvtDelayProp, onPostInitialisedEvtDelayContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyInteractive.SetOnPostInitialisedEvtDelay(onPostInitialisedEvtDelayProp.floatValue);
            }

            EditorGUILayout.PropertyField(onPostInitialisedProp, onPostInitialisedContent);

            if (isReadableProp.boolValue)
            {
                EditorGUILayout.PropertyField(onReadableValueChangedProp, onReadableValueChangedContent);
            }

            if (isSelectableProp.boolValue)
            {
                EditorGUILayout.PropertyField(onSelectedProp, onSelectedContent);
                if (!isAutoUnselectProp.boolValue)
                {
                    EditorGUILayout.PropertyField(onUnselectedProp, onUnselectedContent);
                }
            }

            if (isStashableProp.boolValue)
            {
                EditorGUILayout.PropertyField(onPostStashedProp, onPostStashedContent);
            }

            if (isTouchableProp.boolValue)
            {
                EditorGUILayout.PropertyField(onTouchedProp, onTouchedContent);
                EditorGUILayout.PropertyField(onStoppedTouchingProp, onStoppedTouchingContent);
            }
        }

        /// <summary>
        /// Draw if this object is initialised in the inspector.
        /// </summary>
        protected void DrawDebugIsInitialised()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(StickyEditorHelper.debugIsInitialisedIndent1Content, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
            EditorGUILayout.LabelField(stickyInteractive.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw enable or disable debugging in the inspector
        /// </summary>
        protected void DrawDebugToggle()
        {
            isDebuggingEnabled = EditorGUILayout.Toggle(StickyEditorHelper.debugModeIndent1Content, isDebuggingEnabled);
        }

        /// <summary>
        /// Draw who is holding this object in the inspector
        /// </summary>
        protected void DrawDebugHeldBy(float indent, float rightLabelWidth)
        {
            bool isHeld = stickyInteractive.IsHeld;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(debugIsHeldContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
            EditorGUILayout.LabelField(isHeld ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            StickyEditorHelper.DrawLabelIndent(indent, debugHeldByContent, defaultEditorLabelWidth - indent - 3f);
            EditorGUILayout.LabelField(isHeld && stickyInteractive.Sticky3DCharacter != null ? stickyInteractive.Sticky3DCharacter.name : "--", GUILayout.MaxWidth(rightLabelWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw current readable values in the inspector
        /// </summary>
        /// <param name="rightLabelWidth"></param>
        protected void DrawDebugReadable(float indent, float rightLabelWidth)
        {
            bool isReadable = stickyInteractive.IsReadable;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(debugIsReadableContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
            EditorGUILayout.LabelField(isReadable ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
            EditorGUILayout.EndHorizontal();

            Vector3 currentReadableValue = stickyInteractive.CurrentReadableValue;

            EditorGUILayout.BeginHorizontal();
            StickyEditorHelper.DrawLabelIndent(indent, debugReadableXContent, defaultEditorLabelWidth - indent - 3f);
            EditorGUILayout.LabelField(isReadable ? StickyEditorHelper.GetFloatText(currentReadableValue.x,3) : "--", GUILayout.MaxWidth(rightLabelWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            StickyEditorHelper.DrawLabelIndent(indent, debugReadableZContent, defaultEditorLabelWidth - indent - 3f);
            EditorGUILayout.LabelField(isReadable ? StickyEditorHelper.GetFloatText(currentReadableValue.z, 3) : "--", GUILayout.MaxWidth(rightLabelWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw current reference frame information in the inspector
        /// </summary>
        /// <param name="rightLabelWidth"></param>
        protected void DrawDebugReferenceFrame(float rightLabelWidth)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(debugCurrentReferenceFrameContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
            EditorGUILayout.LabelField(stickyInteractive.CurrentReferenceFrame == null ? "--" : stickyInteractive.CurrentReferenceFrame.name, GUILayout.MaxWidth(rightLabelWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw which socket, if any, is this object attached to in the inspector
        /// </summary>
        /// <param name="indent"></param>
        /// <param name="rightLabelWidth"></param>
        protected void DrawDebugSocketedOn(float indent, float rightLabelWidth)
        {
            bool isSocketed = stickyInteractive.IsSocketed;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(debugIsSocketedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
            EditorGUILayout.LabelField(isSocketed ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            StickyEditorHelper.DrawLabelIndent(indent, debugSocketedOnContent, defaultEditorLabelWidth - indent - 3f);
            EditorGUILayout.LabelField(isSocketed && stickyInteractive.StickySocketedOn != null ? stickyInteractive.StickySocketedOn.name : "--", GUILayout.MaxWidth(rightLabelWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw who is stashing this object in the inspector
        /// </summary>
        protected void DrawDebugStashedBy(float indent, float rightLabelWidth)
        {
            bool isStashed = stickyInteractive.IsStashed;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(debugIsStashedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
            EditorGUILayout.LabelField(isStashed ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            StickyEditorHelper.DrawLabelIndent(indent, debugStashedByContent, defaultEditorLabelWidth - indent - 3f);
            EditorGUILayout.LabelField(isStashed && stickyInteractive.Sticky3DCharacter != null ? stickyInteractive.Sticky3DCharacter.name : "--", GUILayout.MaxWidth(rightLabelWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw primary and secondary hand hold settings in the inspector
        /// </summary>
        protected void DrawHandHoldSettings()
        {
            DrawHandhold1();
            DrawHandHold2();
            CheckHandHoldTools();
        }

        /// <summary>
        /// Draw primary hand hold settings in the inspector
        /// </summary>
        protected void DrawHandhold1()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(handHold1Content);
            StickyEditorHelper.DrawLHGizmosButton(showHH1LHGizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);
            // Find (select) in the scene
            SelectItemInSceneViewButton(ref isHandHold1Selected, showHH1GizmosInSceneViewProp);
            // Toggle selection in scene view on/off
            StickyEditorHelper.DrawGizmosButton(showHH1GizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(handHold1OffsetProp, handHold1OffsetContent);
            EditorGUILayout.PropertyField(handHold1RotationProp, handHold1RotationContent);
            EditorGUILayout.PropertyField(handHold1FlipForLeftHandProp, handHold1FlipForLeftHandContent);
        }

        /// <summary>
        /// Draw the secondary hand hold settings in the inspector
        /// </summary>
        protected void DrawHandHold2()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(handHold2Content);
            StickyEditorHelper.DrawLHGizmosButton(showHH2LHGizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);
            // Find (select) in the scene
            SelectItemInSceneViewButton(ref isHandHold2Selected, showHH2GizmosInSceneViewProp);
            // Toggle selection in scene view on/off
            StickyEditorHelper.DrawGizmosButton(showHH2GizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(handHold2OffsetProp, handHold2OffsetContent);
            EditorGUILayout.PropertyField(handHold2RotationProp, handHold2RotationContent);
            EditorGUILayout.PropertyField(handHold2FlipForLeftHandProp, handHold2FlipForLeftHandContent);
        }

        /// <summary>
        /// Draw the default popup settings in the inspector
        /// </summary>
        protected void DrawPopupSettings()
        {
            EditorGUILayout.PropertyField(defaultPopupOffsetProp, defaultPopupOffsetContent);
            EditorGUILayout.PropertyField(isPopupRelativeUpProp, isPopupRelativeUpContent);
        }

        /// <summary>
        /// Draw the default Interactive Tag settings in the inspector
        /// </summary>
        protected void DrawTagSettings()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(interactiveTagsProp, interactiveTagsContent);
            if (EditorGUI.EndChangeCheck())
            {
                // Force the names to be repopulated
                serializedObject.ApplyModifiedProperties();
                interactiveTagNames = null;
                serializedObject.Update();
            }

            if (interactiveTagNames == null)
            {
                if (interactiveTagsProp.objectReferenceValue != null)
                {
                    interactiveTagNames = ((S3DInteractiveTags)interactiveTagsProp.objectReferenceValue).GetTagNames(true);
                }
                else
                {
                    interactiveTagNames = new string[] { "Default" };
                }
            }

            // Convert the bitmask into a 0-based position in the list
            int tagIndex = S3DInteractiveTags.GetFirstPosition(interactiveTagProp.intValue);

            // If the tag name has been removed it will just display as blank (which is fine)
            tagIndex = EditorGUILayout.Popup(interactiveTagContent, tagIndex, interactiveTagNames);

            // Save the position back as single-selected bitmask
            interactiveTagProp.intValue = 1 << tagIndex;
        }

        /// <summary>
        /// Allow the user to modify the relative position and rotation of a hand hold position in the scene view using gizmos.
        /// NOTE: This also exists in StickyWeaponEditor. If a better methods is developed, consider creating a shared method
        /// for both in StickyEditorHelper.cs.
        /// </summary>
        /// <param name="isSecondaryHandHold"></param>
        /// <param name="handlePos"></param>
        /// <param name="handleRot"></param>
        /// <param name="gizmoColour"></param>
        /// <param name="isHandHoldSelected"></param>
        protected void ModifyHandHold(bool isSecondaryHandHold, Vector3 handlePos, Quaternion handleRot, Color gizmoColour, ref bool isHandHoldSelected)
        {
            using (new Handles.DrawingScope(handHoldGizmoColour))
            {
                if (isHandHoldSelected)
                {
                    // Choose which handle to draw based on which Unity tool is selected
                    if (Tools.current == Tool.Rotate)
                    {
                        EditorGUI.BeginChangeCheck();

                        // Draw a rotation handle
                        handleRot = Handles.RotationHandle(handleRot, handlePos);

                        // Use the rotation handle to edit the hand hold normal
                        if (EditorGUI.EndChangeCheck())
                        {
                            isSceneDirtyRequired = true;
                            Undo.RecordObject(stickyInteractive, "Rotate Hand Hold Point");
                            stickyInteractive.SetHandHoldRotation((Quaternion.Inverse(stickyInteractive.transform.rotation) * handleRot).eulerAngles, isSecondaryHandHold);
                        }
                    }

                    if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                    {
                        EditorGUI.BeginChangeCheck();

                        // Draw a movement handle
                        handlePos = Handles.PositionHandle(handlePos, handleRot);

                        // Use the position handle to edit the position of the local damage region
                        if (EditorGUI.EndChangeCheck())
                        {
                            isSceneDirtyRequired = true;
                            Undo.RecordObject(stickyInteractive, "Move Hand Hold Position");
                            stickyInteractive.SetHandHoldOffset(S3DUtils.GetLocalSpacePosition(stickyInteractive.transform, handlePos), isSecondaryHandHold);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw a (F)ind button which will select the item in the scene view
        /// </summary>
        /// <param name="isSelectedInSceneView"></param>
        /// <param name="showGizmoInSceneViewProp"></param>
        private void SelectItemInSceneViewButton(ref bool isSelectedInSceneView, SerializedProperty showGizmoInSceneViewProp)
        {
            // Add a minimum height so it doesn't appear small in U2019.x+
            if (GUILayout.Button(StickyEditorHelper.gizmoFindBtnContent, buttonCompact, GUILayout.MaxWidth(22f), GUILayout.MinHeight(18f)))
            {
                serializedObject.ApplyModifiedProperties();
                DeselectAllComponents();
                serializedObject.Update();
                isSelectedInSceneView = true;
                showGizmoInSceneViewProp.boolValue = true;
                // Hide Unity tools
                Tools.hidden = true;
            }
        }

        /// <summary>
        /// Draw a selectable button in the scene view
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isSelected"></param>
        private void SceneViewSelectButton(Vector3 pos, float buttonRadius, ref bool isSelected)
        {
            // Allow the user to select/deselect the hand location in the scene view
            if (Handles.Button(pos, Quaternion.identity, buttonRadius, buttonRadius * 0.5f, Handles.SphereHandleCap))
            {
                if (isSelected)
                {
                    DeselectAllComponents();
                }
                else
                {
                    DeselectAllComponents();
                    isSelected = true;
                    // Hide Unity tools
                    Tools.hidden = true;
                }
            }
        }

        /// <summary>
        /// Show (or hide) hand hold 1 gizmos in the scene view
        /// </summary>
        protected void ShowHandHold1Gizmos(bool isShown)
        {
            showHH1GizmosInSceneViewProp.boolValue = isShown;
        }

        /// <summary>
        /// Show (or hide) hand hold 2 gizmos in the scene view
        /// </summary>
        protected void ShowHandHold2Gizmos(bool isShown)
        {
            showHH2GizmosInSceneViewProp.boolValue = isShown;
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Deselect all components in the scene view edit mode, and unhides the Unity tools
        /// </summary>
        protected virtual void DeselectAllComponents()
        {
            isHandHold1Selected = false;
            isHandHold2Selected = false;
            isEquipPointSelected = false;
            isSocketPointSelected = false;

            // Unhide Unity tools
            Tools.hidden = false;
        }

        protected virtual void DrawActivable()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isActivableProp, isActivableContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyInteractive.SetIsActivable(isActivableProp.boolValue);

                if (!isActivableProp.boolValue)
                {
                    isAutoDeactivateProp.boolValue = false;
                    isDeactivateOnDropProp.boolValue = false;
                }
            }

            if (isActivableProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isAutoDeactivateProp, isAutoDeactivateContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyInteractive.SetIsAutoDeactivate(isAutoDeactivateProp.boolValue);
                }

                // Deactivate on drop and Priority over Grab only apply if the object is both Activable and Grabbable
                if (isGrabbableProp.boolValue)
                {
                    EditorGUILayout.PropertyField(isDeactivateOnDropProp, isDeactivateOnDropContent);
                    EditorGUILayout.PropertyField(isActivablePriorityOverGrabProp, isActivablePriorityOverGrabContent);
                }
            }
        }

        /// <summary>
        /// This function overides what is normally seen in the inspector window
        /// This allows stuff like buttons to be drawn there
        /// </summary>
        protected virtual void DrawBaseInspector()
        {
            #region Initialise
            stickyInteractive.allowRepaint = false;
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
            DrawToolBar(tabTexts);
            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.BeginVertical("HelpBox");

            #region Interactive Settings
            if (selectedTabIntProp.intValue == 0)
            {
                EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);

                DrawActivable();
                DrawEquippable();
                DrawGrabbable();
                DrawReadable();
                DrawSelectable();
                DrawSittable();
                DrawSocketable();
                DrawStashable();
                DrawTouchable();
                //DrawMass();

                StickyEditorHelper.DrawUILine(separatorColor, 2);
                DrawHandHoldSettings();

                StickyEditorHelper.DrawUILine(separatorColor, 2);
                DrawPopupSettings();

                StickyEditorHelper.DrawUILine(separatorColor, 2);
                DrawTagSettings();

                DrawGravitySettings();
            }
            #endregion

            #region Event Settings
            else
            {
                // Add small horizontal gap
                StickyEditorHelper.DrawHorizontalGap(2f);

                DrawBaseEvents();
            }
            #endregion

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            stickyInteractive.allowRepaint = true;

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawDebugToggle();
            if (isDebuggingEnabled && stickyInteractive != null)
            {
                Repaint();
                float rightLabelWidth = 175f;

                StickyEditorHelper.PerformanceImpact();

                DrawDebugIsInitialised();

                DrawDebugHeldBy(10f, rightLabelWidth);
                DrawDebugSocketedOn(10f, rightLabelWidth);
                DrawDebugStashedBy(10f, rightLabelWidth);

                DrawDebugReadable(10f, rightLabelWidth);

                DrawDebugReferenceFrame(rightLabelWidth);
            }
            EditorGUILayout.EndVertical();
            #endregion
        }

        /// <summary>
        /// Draw the Equippable settings in the inspector
        /// </summary>
        protected virtual void DrawEquippable()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isEquippableProp, isEquippableContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyInteractive.SetIsEquippable(isEquippableProp.boolValue);
            }

            if (isEquippableProp.boolValue)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(equipPointContent);
                // Find (select) in the scene
                SelectItemInSceneViewButton(ref isEquipPointSelected, showEQPGizmosInSceneViewProp);
                // Toggle selection in scene view on/off
                StickyEditorHelper.DrawGizmosButton(showEQPGizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);
                GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(equipOffsetProp, equipOffsetContent);
                EditorGUILayout.PropertyField(equipRotationProp, equipRotationContent);
                EditorGUILayout.PropertyField(equipFromHeldDelayProp, equipFromHeldDelayContent);
                EditorGUILayout.PropertyField(isReparentOnDropProp, isReparentOnDropContent);
            }
        }

        /// <summary>
        /// Draw the grabbable settings in the inspector
        /// </summary>
        protected virtual void DrawGrabbable()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isGrabbableProp, isGrabbableContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyInteractive.SetIsGrabbable(isGrabbableProp.boolValue);
            }

            if (isGrabbableProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isCarryInHandProp, isCarryInHandContent);
                if (EditorGUI.EndChangeCheck())
                {
                    // If not carried, then it also cannot be parented
                    if (!isCarryInHandProp.boolValue) { isParentOnGrabProp.boolValue = false; }
                }
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isParentOnGrabProp, isParentOnGrabContent);
                if (EditorGUI.EndChangeCheck())
                {
                    // Parenting infers the object will be carried in the palm of the hand.
                    if (isParentOnGrabProp.boolValue) { isCarryInHandProp.boolValue = true; }
                }
                EditorGUILayout.PropertyField(isReparentOnDropProp, isReparentOnDropContent);
                EditorGUILayout.PropertyField(isDisableRegularColOnGrabProp, isDisableRegularColOnGrabContent);
                EditorGUILayout.PropertyField(isDisableTriggerColOnGrabProp, isDisableTriggerColOnGrabContent);
                EditorGUILayout.PropertyField(isRemoveRigidbodyOnGrabProp, isRemoveRigidbodyOnGrabContent);
            }
        }

        /// <summary>
        /// Draw the gravity settings in the inspector
        /// </summary>
        protected virtual void DrawGravitySettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            StickyEditorHelper.DrawS3DFoldout(showGravitySettingsInEditorProp, gravitySettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showGravitySettingsInEditorProp.boolValue)
            {
                EditorGUILayout.PropertyField(isUseGravityProp, isUseGravityContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(gravityModeProp, gravityModeContent);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyInteractive.GravityMode = (StickyManager.GravityMode)gravityModeProp.intValue;
                    serializedObject.Update();
                }

                if (gravityModeProp.intValue != StickyManager.GravityModeUnityInt)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(gravitationalAccelerationProp, gravitationalAccelerationContent);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        stickyInteractive.GravitationalAcceleration = gravitationalAccelerationProp.floatValue;
                        serializedObject.Update();
                    }

                    if (gravityModeProp.intValue == StickyManager.GravityModeDirectionInt)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(gravityDirectionContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
                        if (GUILayout.Button(StickyEditorHelper.btnResetContent, buttonCompact, GUILayout.Width(20f)))
                        {
                            gravityDirectionProp.vector3Value = Vector3.down;
                        }
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(gravityDirectionProp, GUIContent.none);
                        if (EditorGUI.EndChangeCheck())
                        {
                            serializedObject.ApplyModifiedProperties();
                            stickyInteractive.GravityDirection = gravityDirectionProp.vector3Value;
                            serializedObject.Update();
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.PropertyField(dragProp, dragContent);
                EditorGUILayout.PropertyField(angularDragProp, angularDragContent);
                EditorGUILayout.PropertyField(interpolationProp, interpolationContent);
                EditorGUILayout.PropertyField(collisionDetectionProp, collisionDetectionContent);
                DrawMass();

                if (gravityModeProp.intValue == StickyManager.GravityModeRefFrameInt)
                {
                    EditorGUILayout.PropertyField(initialReferenceFrameProp, initialReferenceFrameContent);
                    EditorGUILayout.PropertyField(isInheritGravityProp, isInheritGravityContent);
                }
            }
        }

        protected virtual void DrawMass()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(massProp, massContent);
            if (EditorGUI.EndChangeCheck())
            {
                if (EditorApplication.isPlaying)
                {
                    stickyInteractive.SetMass(massProp.floatValue);
                }
                else if (massProp.floatValue <= 0f) { massProp.floatValue = 1f; }
            }
        }

        /// <summary>
        /// Draw the readable properties in the inspector
        /// </summary>
        protected virtual void DrawReadable()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isReadableProp, isReadableContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyInteractive.SetIsReadable(isReadableProp.boolValue);
            }

            if (isReadableProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(readablePivotProp, readablePivotContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyInteractive.SetReadablePivot((Transform)readablePivotProp.objectReferenceValue);
                }

                EditorGUILayout.PropertyField(readableDeadZoneProp, readableDeadZoneContent);
                EditorGUILayout.PropertyField(readableInvertXProp, readableInvertXContent);
                EditorGUILayout.PropertyField(readableInvertZProp, readableInvertZContent);

                EditorGUILayout.PropertyField(readableMinXProp, readableMinXContent);
                EditorGUILayout.PropertyField(readableMaxXProp, readableMaxXContent);
                EditorGUILayout.PropertyField(readableMinZProp, readableMinZContent);
                EditorGUILayout.PropertyField(readableMaxZProp, readableMaxZContent);
                EditorGUILayout.PropertyField(readableSensitivityXProp, readableSensitivityXContent);
                EditorGUILayout.PropertyField(readableSensitivityZProp, readableSensitivityZContent);
                EditorGUILayout.PropertyField(readableAutoRecentreProp, readableAutoRecentreContent);
                EditorGUILayout.PropertyField(readableSpringStrengthProp, readableSpringStrengthContent);
            }
        }

        /// <summary>
        /// Draw the selectable properties in the inspector
        /// </summary>
        protected virtual void DrawSelectable()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isSelectableProp, isSelectableContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyInteractive.SetIsSelectable(isSelectableProp.boolValue);

                if (!isSelectableProp.boolValue) { isAutoUnselectProp.boolValue = false; }
            }

            if (isSelectableProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isAutoUnselectProp, isAutoUnselectContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyInteractive.SetIsAutoUnselect(isAutoUnselectProp.boolValue);
                }
            }
        }

        /// <summary>
        /// Draw sittable settings in the inspector
        /// </summary>
        protected virtual void DrawSittable()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isSittableProp, isSittableContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyInteractive.SetIsSittable(isSittableProp.boolValue);
            }

            if (isSittableProp.boolValue)
            {
                EditorGUILayout.PropertyField(sitTargetOffsetProp, sitTargetOffsetContent);
                EditorGUILayout.PropertyField(isSeatAllocatedProp, isSeatAllocatedContent);
            }
        }

        /// <summary>
        /// Draw socketable settings in the inspector
        /// </summary>
        protected virtual void DrawSocketable()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isSocketableProp, isSocketableContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyInteractive.SetIsSocketable(isSocketableProp.boolValue);
            }

            if (isSocketableProp.boolValue)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(socketPointContent);
                // Find (select) in the scene
                SelectItemInSceneViewButton(ref isSocketPointSelected, showSOCPGizmosInSceneViewProp);
                // Toggle selection in scene view on/off
                StickyEditorHelper.DrawGizmosButton(showSOCPGizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);
                GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(socketOffsetProp, socketOffsetContent);
                EditorGUILayout.PropertyField(socketRotationProp, socketRotationContent);
            }
        }

        /// <summary>
        /// Draw stashable settings in the inspector
        /// </summary>
        protected virtual void DrawStashable()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isStashableProp, isStashableContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyInteractive.SetIsStashable(isStashableProp.boolValue);
            }

            if (isStashableProp.boolValue)
            {
                EditorGUILayout.PropertyField(isReparentOnDropProp, isReparentOnDropContent);
            }
        }

        /// <summary>
        /// Draw the toolbar using the supplied array of tab text.
        /// </summary>
        /// <param name="tabGUIContent"></param>
        protected virtual void DrawToolBar(GUIContent[] tabGUIContent)
        {
            int prevTab = selectedTabIntProp.intValue;

            // Show a toolbar to allow the user to switch between viewing different areas
            selectedTabIntProp.intValue = GUILayout.Toolbar(selectedTabIntProp.intValue, tabGUIContent);

            // When switching tabs, disable focus on previous control
            if (prevTab != selectedTabIntProp.intValue) { GUI.FocusControl(null); }
        }

        protected virtual void DrawTouchable()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isTouchableProp, isTouchableContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyInteractive.SetIsTouchable(isTouchableProp.boolValue);
            }
        }

        /// <summary>
        /// Draw gizmos and handles for the Equip point on the interactive object in the scene view
        /// </summary>
        protected virtual void DrawSceneViewEquipPoint()
        {
            if (stickyInteractive.ShowEQPGizmosInSceneView && stickyInteractive.IsEquippable)
            {
                componentHandlePosition = stickyInteractive.GetEquipPosition();
                componentHandleRotation = stickyInteractive.GetEquipRotation();

                // Use a fixed size rather than one that changes with scene view camera distance
                relativeHandleSize = 0.1f;

                fadedGizmoColour = stickyInteractive.equipPointGizmoColour;

                // If this equip position is not selected, show it a little more transparent
                if (!isEquipPointSelected)
                {
                    fadedGizmoColour.a *= 0.65f;
                    if (fadedGizmoColour.a < 0.1f) { fadedGizmoColour.a = stickyInteractive.equipPointGizmoColour.a; }
                }

                // Draw point in the scene that is non-interactable
                if (Event.current.type == EventType.Repaint)
                {
                    using (new Handles.DrawingScope(fadedGizmoColour))
                    {
                        // Forwards direction of the equip point
                        Handles.ArrowHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition, componentHandleRotation, 0.1f, EventType.Repaint);

                        // Draw the up direction with a sphere on top of a line
                        Quaternion upRotation = componentHandleRotation * Quaternion.Euler(270f, 0f, 0f);
                        Handles.DrawLine(componentHandlePosition, componentHandlePosition + upRotation * (Vector3.forward * 0.08f));
                        Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition + (upRotation * (Vector3.forward * 0.08f)), upRotation, 0.03f, EventType.Repaint);
                    }
                }

                // Allow the user to move and/or rotate the equip point
                using (new Handles.DrawingScope(stickyInteractive.equipPointGizmoColour))
                {
                    if (isEquipPointSelected)
                    {
                        // Choose which handle to draw based on which Unity tool is selected
                        if (Tools.current == Tool.Rotate)
                        {
                            EditorGUI.BeginChangeCheck();

                            // Draw a rotation handle
                            componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                            // Use the rotation handle to edit the equip point normal
                            if (EditorGUI.EndChangeCheck())
                            {
                                isSceneDirtyRequired = true;
                                Undo.RecordObject(stickyInteractive, "Rotate Equip Point");

                                stickyInteractive.equipRotation = (Quaternion.Inverse(stickyInteractive.transform.rotation) * componentHandleRotation).eulerAngles;
                            }
                        }

                        if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                        {
                            EditorGUI.BeginChangeCheck();

                            // Draw a movement handle
                            componentHandlePosition = Handles.PositionHandle(componentHandlePosition, componentHandleRotation);

                            // Use the position handle to edit the position of the local equip point
                            if (EditorGUI.EndChangeCheck())
                            {
                                isSceneDirtyRequired = true;
                                Undo.RecordObject(stickyInteractive, "Move Equip Position");
                                stickyInteractive.equipOffset = S3DUtils.GetLocalSpacePosition(stickyInteractive.transform, componentHandlePosition);
                            }
                        }
                    }
                }

                using (new Handles.DrawingScope(fadedGizmoColour))
                {
                    SceneViewSelectButton(componentHandlePosition, 0.5f * relativeHandleSize, ref isEquipPointSelected);

                    if (isEquipPointSelected)
                    {
                        isHandHold1Selected = false;
                        isHandHold2Selected = false;
                        isSocketPointSelected = false;
                    }
                }

            }
        }

        /// <summary>
        /// Draw gizmos and handles for the Socket point on the interactive object in the scene view
        /// </summary>
        protected virtual void DrawSceneViewSocketPoint()
        {
            if (stickyInteractive.ShowSOCPGizmosInSceneView && stickyInteractive.IsSocketable)
            {
                componentHandlePosition = stickyInteractive.GetSocketPosition();
                componentHandleRotation = stickyInteractive.GetSocketRotation();

                // Use a fixed size rather than one that changes with scene view camera distance
                relativeHandleSize = 0.1f;

                fadedGizmoColour = stickyInteractive.socketPointGizmoColour;

                // If this socket position is not selected, show it a little more transparent
                if (!isSocketPointSelected)
                {
                    fadedGizmoColour.a *= 0.65f;
                    if (fadedGizmoColour.a < 0.1f) { fadedGizmoColour.a = stickyInteractive.socketPointGizmoColour.a; }
                }

                // Draw point in the scene that is non-interactable
                if (Event.current.type == EventType.Repaint)
                {
                    using (new Handles.DrawingScope(fadedGizmoColour))
                    {
                        // Forwards direction of the equip point
                        Handles.ArrowHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition, componentHandleRotation, 0.1f, EventType.Repaint);

                        // Draw the up direction with a sphere on top of a line
                        Quaternion upRotation = componentHandleRotation * Quaternion.Euler(270f, 0f, 0f);
                        Handles.DrawLine(componentHandlePosition, componentHandlePosition + upRotation * (Vector3.forward * 0.08f));
                        Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition + (upRotation * (Vector3.forward * 0.08f)), upRotation, 0.03f, EventType.Repaint);
                    }
                }

                // Allow the user to move and/or rotate the socket point
                using (new Handles.DrawingScope(stickyInteractive.socketPointGizmoColour))
                {
                    if (isSocketPointSelected)
                    {
                        // Choose which handle to draw based on which Unity tool is selected
                        if (Tools.current == Tool.Rotate)
                        {
                            EditorGUI.BeginChangeCheck();

                            // Draw a rotation handle
                            componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                            // Use the rotation handle to edit the spocket point normal
                            if (EditorGUI.EndChangeCheck())
                            {
                                isSceneDirtyRequired = true;
                                Undo.RecordObject(stickyInteractive, "Rotate Socket Point");

                                stickyInteractive.socketRotation = (Quaternion.Inverse(stickyInteractive.transform.rotation) * componentHandleRotation).eulerAngles;
                            }
                        }

                        if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                        {
                            EditorGUI.BeginChangeCheck();

                            // Draw a movement handle
                            componentHandlePosition = Handles.PositionHandle(componentHandlePosition, componentHandleRotation);

                            // Use the position handle to edit the position of the local socket point
                            if (EditorGUI.EndChangeCheck())
                            {
                                isSceneDirtyRequired = true;
                                Undo.RecordObject(stickyInteractive, "Move Socket Position");
                                stickyInteractive.socketOffset = S3DUtils.GetLocalSpacePosition(stickyInteractive.transform, componentHandlePosition);
                            }
                        }
                    }
                }

                using (new Handles.DrawingScope(fadedGizmoColour))
                {
                    SceneViewSelectButton(componentHandlePosition, 0.5f * relativeHandleSize, ref isSocketPointSelected);

                    if (isSocketPointSelected)
                    {
                        isHandHold1Selected = false;
                        isHandHold2Selected = false;
                        isEquipPointSelected = false;
                    }
                }
            }
        }

        /// <summary>
        /// This lets us modify and display things in the scene view
        /// </summary>
        /// <param name="sv"></param>
        protected virtual void SceneGUI (SceneView sv)
        {
            if (stickyInteractive != null && stickyInteractive.gameObject.activeInHierarchy)
            {
                isSceneDirtyRequired = false;

                // IMPORTANT: Do not use transform.TransformPoint or InverseTransformPoint because they won't work correctly
                // when the parent gameobject has scale not equal to 1,1,1.

                // Get the rotation of the interactive object in the scene
                sceneViewInteractiveRot = Quaternion.LookRotation(stickyInteractive.transform.forward, stickyInteractive.transform.up);

                DrawSceneViewEquipPoint();

                DrawSceneViewSocketPoint();

                #region Hand Hold 1

                if (stickyInteractive.ShowHH1GizmosInSceneView)
                {
                    //Vector3 localScale = stickyInteractive.transform.localScale;

                    componentHandlePosition = stickyInteractive.GetHandHoldPosition(false);

                    // Get component handle rotation
                    //componentHandleRotation = stickyInteractive.transform.rotation * Quaternion.Euler(stickyInteractive.handHold1Rotation);
                    componentHandleRotation = stickyInteractive.GetHandHoldRotation(false);

                    // Use a fixed size rather than one that changes with scene view camera distance
                    relativeHandleSize = 0.1f;
                    //relativeHandleSize = HandleUtility.GetHandleSize(componentHandlePosition);

                    fadedGizmoColour = handHoldGizmoColour;

                    // If this hand hold position is not selected, show it a little more transparent
                    if (!isHandHold1Selected)
                    {
                        fadedGizmoColour.a *= 0.65f;
                        if (fadedGizmoColour.a < 0.1f) { fadedGizmoColour.a = handHoldGizmoColour.a; }
                    }

                    // Draw point in the scene that is non-interactable
                    if (Event.current.type == EventType.Repaint)
                    {
                        StickyEditorHelper.DrawHandNonInteractableGizmos(componentHandlePosition, componentHandleRotation, fadedGizmoColour, relativeHandleSize, stickyInteractive.ShowHH1LHGizmosInSceneView);
                    }

                    ModifyHandHold(false, componentHandlePosition, componentHandleRotation, handHoldGizmoColour, ref isHandHold1Selected);

                    using (new Handles.DrawingScope(fadedGizmoColour))
                    {
                        SceneViewSelectButton(componentHandlePosition, 0.5f * relativeHandleSize, ref isHandHold1Selected);

                        if (isHandHold1Selected)
                        {
                            isHandHold2Selected = false;
                            isEquipPointSelected = false;
                            isSocketPointSelected = false;
                        }
                    }
                }

                #endregion

                #region Hand Hold 2

                if (stickyInteractive.ShowHH2GizmosInSceneView)
                {
                    //Vector3 localScale = stickyInteractive.transform.localScale;

                    componentHandlePosition = stickyInteractive.GetHandHoldPosition(true);

                    // Get component handle rotation
                    componentHandleRotation = stickyInteractive.GetHandHoldRotation(true);

                    // Use a fixed size rather than one that changes with scene view camera distance
                    relativeHandleSize = 0.1f;
                    //relativeHandleSize = HandleUtility.GetHandleSize(componentHandlePosition);

                    fadedGizmoColour = handHoldGizmoColour;

                    // If this hand hold position is not selected, show it a little more transparent
                    if (!isHandHold2Selected)
                    {
                        fadedGizmoColour.a *= 0.65f;
                        if (fadedGizmoColour.a < 0.1f) { fadedGizmoColour.a = handHoldGizmoColour.a; }
                    }

                    // Draw point in the scene that is non-interactable
                    if (Event.current.type == EventType.Repaint)
                    {
                        StickyEditorHelper.DrawHandNonInteractableGizmos(componentHandlePosition, componentHandleRotation, fadedGizmoColour, relativeHandleSize, stickyInteractive.ShowHH2LHGizmosInSceneView);
                    }

                    ModifyHandHold(true, componentHandlePosition, componentHandleRotation, handHoldGizmoColour, ref isHandHold2Selected);

                    using (new Handles.DrawingScope(fadedGizmoColour))
                    {
                        SceneViewSelectButton(componentHandlePosition, 0.5f * relativeHandleSize, ref isHandHold2Selected);

                        if (isHandHold2Selected)
                        {
                            isHandHold1Selected = false;
                            isEquipPointSelected = false;
                            isSocketPointSelected = false;
                        }
                    }
                }

                #endregion

                if (isSceneDirtyRequired && !Application.isPlaying)
                {
                    isSceneDirtyRequired = false;
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
            else
            {
                // Always unhide Unity tools and deselect all components when the object is disabled
                Tools.hidden = false;
                DeselectAllComponents();
            }
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