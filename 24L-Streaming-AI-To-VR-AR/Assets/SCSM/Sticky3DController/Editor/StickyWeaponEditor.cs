using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyWeapon))]
    [CanEditMultipleObjects]
    public class StickyWeaponEditor : StickyInteractiveEditor
    {
        #region Custom Editor private variables
        private StickyWeapon stickyWeapon = null;
        private bool isDebugAimPosition = false;
        private GameObject debugAimTargetGO = null;
        private string[] ammoTypeNames = null;
        private string[] magTypeNames = null;

        // Anim Actions move/insert/delete
        private int s3dAnimActionMoveDownPos = -1;
        private int s3dAnimActionInsertPos = -1;
        private int s3dAnimActionDeletePos = -1;

        // Anim Conditions move/insert/delete
        private int s3dAnimConditionMoveDownPos = -1;
        private int s3dAnimConditionInsertPos = -1;
        private int s3dAnimConditionDeletePos = -1;

        // Weapon Anim Sets move/insert/delete
        private int s3dWeaponAnimSetMoveDownPos = -1;
        private int s3dWeaponAnimSetDeletePos = -1;

        // Parameters for default weapon animator
        private List<S3DAnimParm> animParamsBoolList;
        private List<S3DAnimParm> animParamsTriggerList;
        private List<S3DAnimParm> animParamsFloatList;
        private List<S3DAnimParm> animParamsIntegerList;

        private string[] animParamBoolNames;
        private string[] animParamTriggerNames;
        private string[] animParamFloatNames;
        private string[] animParamIntegerNames;

        private List<S3DAnimLayer> animLayerList;
        private string[] animLayerNames;

        #endregion

        #region SceneView variables

        // Red
        private Color firePointGizmoColour = new Color(200f / 255f, 0f / 0f, 0f / 255f, 1f);

        // Yellow
        private Color spentEjectGizmoColour = new Color(1f, 0.92f, 0.016f, 0.7f);

        protected bool isFirePositionSelected = false;
        protected bool isSpentCartridgePositionSelected = false;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("A weapon is an interactive-enabled object that can be grabbed, dropped, equipped, socketed, stashed, fired, and reloaded.");
        private readonly static GUIContent[] tabTexts = { new GUIContent("Interactive"), new GUIContent("Weapon"), new GUIContent("Events") };
        #endregion

        #region GUIContent - StickyInteractive (modified)
        private readonly static GUIContent initialiseOnStartWPContent = new GUIContent(" Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the Sticky Weapon component is enabled through code.");
        #endregion

        #region GUIContent - Weapon Common
        private readonly static GUIContent weaponTypeContent = new GUIContent(" Weapon Type", "The type or style of weapon e.g., Projectile Raycast/Standard, Beam etc. We do not support changing the weaponType after it has been initialised.");

        private readonly static GUIContent newContent = new GUIContent("New");

        private readonly static GUIContent generalSettingsContent = new GUIContent(" General Settings");
        private readonly static GUIContent fireDirectionContent = new GUIContent(" Fire Direction", "The local space direction in which the weapon fires [Default: Forward]");
        private readonly static GUIContent relativeFirePositionContent = new GUIContent(" Relative Fire Position", "The local space fire position aligned to the end of the barrel.");
        private readonly static GUIContent firePositionOffsetsContent = new GUIContent(" Fire Position Offsets", "An array of local space fire position offsets from the relative fire position. Typically only used for a weapon with multiple barrels.");
        private readonly static GUIContent fireInterval1Content = new GUIContent(" Fire Interval 1", "The minimum time (in seconds) between consecutive firings of the primary fire button.");
        private readonly static GUIContent fireInterval2Content = new GUIContent(" Fire Interval 2", "The minimum time (in seconds) between consecutive firings of the seondary fire button.");
        private readonly static GUIContent isOnlyFireWhenAiming1Content = new GUIContent(" Only Fire When Aiming 1", "Fire button 1 can only fire when the weapon is being aimed");
        private readonly static GUIContent isOnlyFireWhenAiming2Content = new GUIContent(" Only Fire When Aiming 2", "Fire button 2 can only fire when the weapon is being aimed");

        private readonly static GUIContent maxRangeContent = new GUIContent(" Max Range", "The maximum range the weapon can shoot");
        private readonly static GUIContent firingButton1Content = new GUIContent(" Firing Button1", "The main trigger for the weapon to fire e.g. None, Manual, AutoFire. Set to None by default so that weapon doesn't automatically fire when a weapon is added at runtime.");
        private readonly static GUIContent firingButton2Content = new GUIContent(" Firing Button2", "The second trigger for the weapon to fire e.g. None, Manual, AutoFire. Set to None by default so that weapon doesn't automatically fire when a weapon is added at runtime. Most weapons will only use the first firing button or trigger.");
        private readonly static GUIContent firingType1Content = new GUIContent(" Firing Type1", "The main firing type of this weapon. Semi-Auto fires single shots but automatically loads the next projectile if there is available ammo. Full-Auto can continuously fire while there is available ammo.");
        private readonly static GUIContent firingType2Content = new GUIContent(" Firing Type2", "The secondary firing type of this weapon. Semi-Auto fires single shots but automatically loads the next projectile if there is available ammo. Full-Auto can continuously fire while there is available ammo.");
        private readonly static GUIContent magazineContent = new GUIContent(" Magazine", "The magazine or cartridge attachment that holds the ammunition for the weapon");
        private readonly static GUIContent hitLayerMaskContent = new GUIContent(" Hit Layer Mask", "When fired, the weapon can hit any objects in these Unity Layers");
        private readonly static GUIContent isOnlyUseWhenHeldContent = new GUIContent(" Only Use When Held", "The weapon can only fire, reload, or be animated, when it is held by a character");
        private readonly static GUIContent checkLineOfSightContent = new GUIContent(" Check Line Of Sight", "Whether the auto-firing weapon checks line of sight before firing (in order to prevent friendly fire) each frame. Since this uses raycasts it can lead to reduced performance.");

        private readonly static GUIContent muzzleFXSettingsContent = new GUIContent(" Muzzle FX Settings");
        private readonly static GUIContent muzzleEffects1Content = new GUIContent(" Effects Objects", "An array of randomly selected Sticky Effects Modules spawned when the weapon fires. Beams should use looping FX.");
        private readonly static GUIContent muzzleEffectsOffsetContent = new GUIContent(" Muzzle FX Offset", "The distance in local space that the muzzle Effects Object should be instantiated from the weapon firing point. Typically only the z-axis will be used.");

        private readonly static GUIContent smokeSettingsContent = new GUIContent(" Smoke Settings");
        private readonly static GUIContent maxActiveSmokeEffects1Content = new GUIContent(" Max Active Smoke FX", "The maximum number of smoke effects that can be active at any time on this weapon");
        private readonly static GUIContent smokeEffectsSet1Content = new GUIContent(" Effects Set Firing Button 1", "The set containing one or more StickyEffectsModules to be randomly selected when primary mechanism is fired.");
        private readonly static GUIContent smokeEffectsSet2Content = new GUIContent(" Effects Set Firing Button 2", "The set containing one or more StickyEffectsModules to be randomly selected when secondary mechanism is fired.");

        private readonly static GUIContent spentCartridgeSettingsContent = new GUIContent(" Spent Cartridge Settings");
        private readonly static GUIContent spentCartridgePrefabContent = new GUIContent(" Spent Cartridge Prefab" , "The dynamic object prefab for the spent or empty cartridge that is ejected from the weapon after it fires. It must have a StickyDynamicModule component on the parent gameobject.");
        private readonly static GUIContent spentEjectPositionContent = new GUIContent(" Spent Eject Position" , "The local space position on the weapon from which the spent cartridge is ejected.");
        private readonly static GUIContent spentEjectForceContent = new GUIContent(" Spent Eject Force" , "The force used to eject the spent cartridge from the weapon in the Spent Eject Direction.");
        private readonly static GUIContent spentEjectDirectionContent = new GUIContent(" Spent Eject Direction" , "The local space direction the spent cartridge is ejected");
        private readonly static GUIContent spentEjectRotationContent = new GUIContent(" Spent Eject Rotation" , "The local space rotation, in Euler angles, of the spent cartridge when ejected. This is not shown by the gizmos in the scene view.");

        private readonly static GUIContent gotoDynamicObjectsFolderBtnContent = new GUIContent("F", "Find and highlight the sample Dynamic Objects folder");
        private readonly static GUIContent gotoEffectFolderBtnContent = new GUIContent("F", "Find and highlight the sample Effects folder");


        #endregion

        #region GUIContent - Weapon Aiming and Reticle
        private readonly static GUIContent aimingAndReticleSettingsContent = new GUIContent(" Aiming and Reticle Settings");
        private readonly static GUIContent aimingAndReticleInfoContent = new GUIContent("Reticles are typically shown on a StickyDisplayModule (HUD) in the scene. See manual for more details.");
        private readonly static GUIContent aimingFirstPersonFoVContent = new GUIContent(" Aiming First Person FoV", "When aiming the held weapon, this is the character first person camera field of view.");
        private readonly static GUIContent aimingThirdPersonFoVContent = new GUIContent(" Aiming Third Person FoV", "When aiming the held weapon, this is the character third person camera field of view.");
        private readonly static GUIContent aimingSpeedContent = new GUIContent(" Aiming Speed", "The rate at which the transition between aiming and not aiming will progress.");

        private readonly static GUIContent defaultReticleSpriteContent = new GUIContent(" Default Reticle", "The reticle (sprite) used when the weapon is held by a S3D player");
        private readonly static GUIContent aimingReticleSpriteContent = new GUIContent(" Aiming Reticle", "The reticle (sprite) used when the weapon is held by a S3D player and aiming. If none, it will fall back to Default Reticle unless No Reticle when Aiming the Weapon [FPS,TPS] is true.");
        private readonly static GUIContent noReticleContent = new GUIContent(" No Reticle When:");
        private readonly static GUIContent isNoReticleOnAimingFPSContent = new GUIContent("Aiming the Weapon FPS", "Do not display a reticle on the StickyDisplayModule (HUD) when the weapon is aiming with first-person camera. Only applies if weapon is held by a player (non-NPC) character and there is an active StickyDisplayModule in the scene.");
        private readonly static GUIContent isNoReticleOnAimingTPSContent = new GUIContent("Aiming the Weapon TPS", "Do not display a reticle on the StickyDisplayModule (HUD) when the weapon is aiming with third-person camera. Only applies if weapon is held by a player (non-NPC) character and there is an active StickyDisplayModule in the scene.");
        private readonly static GUIContent isNoReticleIfHeldContent = new GUIContent("Held by a Player", "Do not display a reticle on the StickyDisplayModule (HUD) when the weapon is held. Only applies if weapon is held by a player (non-NPC) character and there is an active StickyDisplayModule in the scene.");
        private readonly static GUIContent isNoReticleIfHeldNoFreeLookContent = new GUIContent("Held with Free Look OFF", "Do not display a reticle on the StickyDisplayModule (HUD) when the weapon is held and Free Look on the character is NOT enabled. Only applies if weapon is held by a player (non-NPC) character and there is an active StickyDisplayModule in the scene.");

        private readonly static GUIContent gotoDisplayReticleFolderBtnContent = new GUIContent("F", "Find and highlight the sample Display Reticle folder");

        #endregion

        #region GUIContent - Weapon Ammo

        private readonly static GUIContent ammoSettingsContent = new GUIContent(" Ammo Settings");
        private readonly static GUIContent ammoBeamWeaponContent = new GUIContent(" Beam weapons do not use ammo.");
        private readonly static GUIContent ammoTypesContent = new GUIContent(" Available Ammo Types", "This is Scriptable Object containing a list of 26 Ammo types. To create custom types, in the Project pane, click Create->Sticky3D->Ammo Types.");
        private readonly static GUIContent magTypesContent = new GUIContent(" Available Mag Types", "This is Scriptable Object containing a list of 26 Magazine types. To create custom types, in the Project pane, click Create->Sticky3D->Magazine Types.");
        private readonly static GUIContent compatibleAmmoFireButton1Content = new GUIContent(" Firing Button 1", "Ammo types that are compatible with the main firing mechanism");
        private readonly static GUIContent compatibleAmmoFireButton2Content = new GUIContent(" Firing Button 2", "Ammo types that are compatible with the secondary firing mechanism");
        private readonly static GUIContent compatibleMag1Content = new GUIContent("Magazine Type", "The Magazine Type from the scriptable object for the main firing mechanism");
        private readonly static GUIContent compatibleMag2Content = new GUIContent("Magazine Type", "The Magazine Type from the scriptable object for the second firing mechanism");

        private readonly static GUIContent unlimitedAmmoContent = new GUIContent("Unlimited Ammo", "Can this projectile weapon keep firing and never run out of ammunition?");
        private readonly static GUIContent ammunition1Content = new GUIContent("Ammunition", "The quantity of projectiles or ammunition available for this weapon. This is read-only and automatically updated when Is Sticky Mag Required is true.");
        private readonly static GUIContent ammunition2Content = new GUIContent("Ammunition", "The quantity of projectiles or ammunition available for this weapon. This is read-only and automatically updated when Is Sticky Mag Required is true.");
        private readonly static GUIContent reloadDelay1Content = new GUIContent("Reload Delay", "The short delay, in seconds, between when the primary button mechanism fired, and it attempts to reload");
        private readonly static GUIContent reloadDelay2Content = new GUIContent("Reload Delay", "The short delay, in seconds, between when the secondary button mechanism fired, and it attempts to reload");
        private readonly static GUIContent reloadEquipDelay1Content = new GUIContent("Reload Equip Delay", "The delay during reloading, in seconds, between when the primary button mechanism unequips the old mag and starts to equip a new mag.");
        private readonly static GUIContent reloadEquipDelay2Content = new GUIContent("Reload Equip Delay", "The delay during reloading, in seconds, between when the secondary button mechanism unequips the old mag and starts to equip a new mag.");
        private readonly static GUIContent reloadDuration1Content = new GUIContent("Reload Duration", "The time, in seconds, it takes the primary button mechanism to reload");
        private readonly static GUIContent reloadDuration2Content = new GUIContent("Reload Duration", "The time, in seconds, it takes the secondary button mechanism to reload");
        private readonly static GUIContent reloadSoundFX1Content = new GUIContent("Reload Sound FX", "The Sound FX for the primary button mechanism used when the weapon begins reloading. The Effects Type must be SoundFX.");
        private readonly static GUIContent reloadSoundFX2Content = new GUIContent("Reload Sound FX", "The Sound FX for the secondary button mechanism used when the weapon begins reloading. The Effects Type must be SoundFX.");
        private readonly static GUIContent reloadEquipSoundFX1Content = new GUIContent("Reload Equip Sound FX", "The Sound FX for the primary button mechanism used when the weapon equips a new mag during reloading. The Effects Type must be SoundFX.");
        private readonly static GUIContent reloadEquipSoundFX2Content = new GUIContent("Reload Equip Sound FX", "The Sound FX for the secondary button mechanism used when the weapon equips a new mag during reloading. The Effects Type must be SoundFX.");
        private readonly static GUIContent reloadType1Content = new GUIContent("Reload Type", "The method for reloading the primary button mechanism.");
        private readonly static GUIContent reloadType2Content = new GUIContent("Reload Type", "The method for reloading the secondary button mechanism.");
        private readonly static GUIContent isMagRequired1Content = new GUIContent("Is Sticky Mag Required", "Does the primary firing mechanism require a StickyMagazine to be attached before it can fire?");
        private readonly static GUIContent isMagRequired2Content = new GUIContent("Is Sticky Mag Required", "Does the secondary firing mechanism require a StickyMagazine to be attached before it can fire?");
        private readonly static GUIContent magCapacity1Content = new GUIContent("Mag Capacity", "The amount of ammo the primary firing mechanism magazine (clip) can hold. Becomes read-only when Is Mag Required is true. It is automatically updated when a mag is retrieved from Stash.");
        private readonly static GUIContent magCapacity2Content = new GUIContent("Mag Capacity", "The amount of ammo the secondary firing mechanism magazine (clip) can hold. Becomes read-only when Is Mag Required is true. It is automatically updated when a mag is retrieved from Stash.");

        private readonly static GUIContent unlimitedMagsContent = new GUIContent("Unlimited Mags", "Is there an unlimited number of magazines with which to keep reloading?");
        private readonly static GUIContent magsInReserve1Content = new GUIContent("Mags in Reserve", "The number of additional magazines available for the primary firing mechanism. This is read-only and automatically updated when Reload Type is Auto Stash.");
        private readonly static GUIContent magsInReserve2Content = new GUIContent("Mags in Reserve", "The number of additional magazines available for the secondary firing mechanism. This is read-only and automatically updated when Reload Type is Auto Stash.");
        private readonly static GUIContent fireEmptyEffectsSet1Content = new GUIContent("Fire Empty Effects Set", "The set containing one or more StickyEffectsModules to be randomly selected when attempting to fire with no available ammo on fire button 1. Ensure Is Reparented is NOT selected on each prefab.");
        private readonly static GUIContent fireEmptyEffectsSet2Content = new GUIContent("Fire Empty Effects Set", "The set containing one or more StickyEffectsModules to be randomly selected when attempting to fire with no available ammo on fire button 2. Ensure Is Reparented is NOT selected on each prefab.");

        #endregion

        #region GUIContent - Weapon Animate
        private readonly static GUIContent animateSettingsContent = new GUIContent(" Animate Settings");
        private readonly static GUIContent defaultAnimatorContent = new GUIContent(" Weapon Animator", "The animator that will control animations for this weapon. This should be attached to, or a child of, the weapon gameobject.");
        private readonly static GUIContent aaRefreshParamsContent = new GUIContent(" Refresh", "Refresh the Parameter from the Animator Controller");
        #endregion

        #region GUIContent - Weapon Anim Sets
        private readonly static GUIContent weaponAnimSetsContent = new GUIContent(" Weapon Anim Sets (for characters)", "The list of weapon anim sets for character model IDs that apply to this weapon");
        private readonly static GUIContent weaponAnimSetsInfoContent = new GUIContent("These map animation clips and parameters to Sticky3D characters. This enables the weapon to be used by characters with different rigs and animation controllers.");
        private readonly static GUIContent weaponAnimSetContent = new GUIContent(" Weapon Anim Set", "Contains a list of animation clip pairs used with one or more Sticky3D character Model IDs");

        #endregion

        #region GUIContent - Animate Actions
        private readonly static GUIContent animateActionsContent = new GUIContent(" Weapon Actions", "The animate actions that interact with your weapon animation controller");
        private readonly static GUIContent aaWeaponActionContent = new GUIContent(" Weapon Action", "This is the action that happens that causes the animation to take place. It helps you remember why you set it up.");
        private readonly static GUIContent aaParmTypeContent = new GUIContent(" Parameter Type", "The type of animation parameter, if any, used with this action");
        private readonly static GUIContent aaParmNamesContent = new GUIContent(" Parameter Name", "The parameter name from the animation controller that applies to this action");
        private readonly static GUIContent aaBoolValueContent = new GUIContent(" Bool Value", "The realtime value from the weapon that will be sent to the model's animation controller");
        private readonly static GUIContent aaFloatValueContent = new GUIContent(" Float Value", "The realtime value from the weapon that will be sent to the model's animation controller");
        private readonly static GUIContent aaTriggerValueContent = new GUIContent(" Trigger Value", "The realtime value from the weapon that will be sent to the model's animation controller");
        private readonly static GUIContent aaIntegerValueContent = new GUIContent(" Integer Value", "The realtime value from the weapon that will be sent to the model's animation controller");
        private readonly static GUIContent aaFloatMultiplierContent = new GUIContent(" Float Multiplier", "A value that is used to multiple or change the value of the float value being passed to the animation controller. Can speed up or slow down an animation.");
        private readonly static GUIContent aaFloatFixedValueContent = new GUIContent(" Fixed Value", "The fixed float value being passed to the animation controller.");
        private readonly static GUIContent aaBoolFixedValueContent = new GUIContent(" Fixed Value", "True or False being passed to the animation controller.");
        private readonly static GUIContent aaDampingContent = new GUIContent(" Damping", "The damping applied to help smooth transitions, especially with Blend Trees. Currently only used for floats. For quick transitions to the new float value use a low damping value, for the slower transitions use more damping.");
        private readonly static GUIContent aaCustomValueContent = new GUIContent("User game code");
        private readonly static GUIContent aaIsResetCustomAfterUseContent = new GUIContent(" Reset After Use", "Works with bool custom anim actions to reset to false after it has been sent to the animator controller. Has no effect if Toggle is true. [Default: True]");
        private readonly static GUIContent aaIsInvertContent = new GUIContent(" Invert", "When the value is true, use false instead. When the value is false, use true instead. Not compatible with Toggle");
        private readonly static GUIContent aaIsToggleContent = new GUIContent(" Toggle", "Works with bool custom anim actions to toggle the existing parameter value in the animator controller. Not compatible with Invert or Reset After Use.");
        private readonly static GUIContent aaTransNamesContent = new GUIContent(" Transition Name", "The transition name from the animation controller that applies to this action");
        private readonly static GUIContent aaNoneContent = new GUIContent(" None Available", "No data available");
        private readonly static GUIContent conditionsInfoContent = new GUIContent("Data is only sent to your animator when the conditions are true");
        #endregion

        #region GUIContent - Weapon Attachments
        private readonly static GUIContent attachmentSettingsContent = new GUIContent(" Attachment Settings");
        private readonly static GUIContent laserSightAimFromContent = new GUIContent(" Laser Sight", "The child transform that determines where the laser sight aims from");
        private readonly static GUIContent isLaserSightOnContent = new GUIContent(" Laser Sight On", "Should the laser sight beam be showing?");
        private readonly static GUIContent isLaserSightAutoOnContent = new GUIContent(" Laser Sight Auto On", "Should the laser sight beam automatically turn on when it is equipped?");
        private readonly static GUIContent laserSightColourContent = new GUIContent(" Laser Sight Colour", "The colour of the laser sight beam");
        private readonly static GUIContent magAttachPoint1Content = new GUIContent(" Mag Attach Point 1", "The child transform where the primary magazine (clip) attaches to the weapon");
        private readonly static GUIContent magAttachPoint2Content = new GUIContent(" Mag Attach Point 2", "The child transform where the secondary magazine (clip) attaches to the weapon");
        private readonly static GUIContent isScopeOnContent = new GUIContent(" Scope On", "Should the scope currently being used for more precise visual aiming?");
        private readonly static GUIContent scopeCameraContent = new GUIContent(" Scope Camera", "The camera used to render the scope display");
        private readonly static GUIContent scopeCameraRendererContent = new GUIContent(" Scope Renderer", "The (mesh) renderer to project the Scope Camera onto.");
        private readonly static GUIContent isScopeAllowNPCContent = new GUIContent(" Scope Allow NPC", "Allow NPCs to use the Scope. Typically, you don’t want to enable this as each Scope used in the scene requires the whole scene to be rendered again which has a significant performance overhead.");
        private readonly static GUIContent isScopeAutoOnContent = new GUIContent(" Scope Auto On", "Should the Scope be automatically turned on when grabbed by a character? Essentially, it will stay on all the time the weapon is held.");

        private readonly static GUIContent gotoWeaponsFolderBtnContent = new GUIContent("F", "Find and highlight the sample Weapon prefab folder");

        #endregion

        #region GUIContent - Weapon Beam
        private readonly static GUIContent beamPrefabContent = new GUIContent(" Beam Prefab", "Prefab template of the " +
            "beam fired by this weapon. Beam prefabs need to have a Sticky Beam Module script attached to them.");
        private readonly static GUIContent chargeAmountContent = new GUIContent(" Charge Amount", "The amount of charge or power the beam weapon has.");
        private readonly static GUIContent rechargeTimeContent = new GUIContent(" Recharge Time", "The time (in seconds) it takes the fully discharged beam weapon to reach maximum charge");

        private readonly static GUIContent gotoBeamsFolderBtnContent = new GUIContent("F", "Find and highlight the sample Beams folder");

        #endregion

        #region GUIContent - Weapon Projectile
        private readonly static GUIContent projectilePrefabContent = new GUIContent(" Projectile Prefab", "Prefab template of the " +
            "projectiles fired by this weapon. Projectile prefabs need to have a Sticky Projectile Module script attached to them.");

        private readonly static GUIContent gotoProjectilesFolderBtnContent = new GUIContent("F", "Find and highlight the sample Projectiles prefab folder");
        #endregion

        #region GUIContent - Weapon Health
        private readonly static GUIContent healthSettingsContent = new GUIContent(" Health and Heat Settings");
        private readonly static GUIContent healthContent = new GUIContent(" Health", "The overall health of the weapon. Must be in the range 0 to 100.");
        private readonly static GUIContent heatLevelContent = new GUIContent(" Heat Level", "The heat of the weapon - range 0.0 (starting temp) to 100.0 (max temp).");
        private readonly static GUIContent heatUpRateContent = new GUIContent(" Heat Up Rate", "The rate heat is added per second for beam weapons or the amount added each time a projectile weapon fires. If rate is 0, heat level never changes.");
        private readonly static GUIContent heatDownRateContent = new GUIContent(" Cool Down Rate", "The rate heat is removed per second. This is the rate the weapon cools when not in use.");
        private readonly static GUIContent overHeatThresholdContent = new GUIContent(" Overheat Threshold", "The heat level that the weapon will begin to overheat and start being less efficient.");
        private readonly static GUIContent isBurnoutOnMaxHeatContent = new GUIContent(" Burnout on Max Heat", "When the weapon reaches max heat level of 100, will the weapon be inoperable until it is repaired?");
        private readonly static GUIContent isFiringPausedContent = new GUIContent(" Firing Paused", "Is firing paused? This can be useful when only wanting to prevent the weapon from firing like when displaying a Sticky popup.");

        #endregion

        #region GUIContent - Weapon Recoil

        private readonly static GUIContent recoilSettingsContent = new GUIContent(" Recoil Settings");
        private readonly static GUIContent recoilFiringButton1Content = new GUIContent(" Firing Button 1");
        private readonly static GUIContent recoilFiringButton2Content = new GUIContent(" Firing Button 2");
        private readonly static GUIContent recoilSpeedContent = new GUIContent(" Recoil Speed", "The rate at which the weapon recoils when fired. NOTE: Currently only applies to FPS players when aiming.");
        private readonly static GUIContent recoilReturnRateContent = new GUIContent(" Return Rate", "The rate at which the weapon returns to its stable position.");
        private readonly static GUIContent recoilX1Content = new GUIContent(" Recoil Angle X", "The angle the weapon pitches up when the primary mechanism fires");
        private readonly static GUIContent recoilX2Content = new GUIContent(" Recoil Angle X", "The angle the weapon pitches up when the secondary mechanism fires");
        private readonly static GUIContent recoilY1Content = new GUIContent(" Recoil Angle Y", "The maximum angle the weapon rotates around the local y-axis when the primary mechanism fires");
        private readonly static GUIContent recoilY2Content = new GUIContent(" Recoil Angle Y", "The maximum angle the weapon rotates around the local y-axis when the secondary mechanism fires");
        private readonly static GUIContent recoilZ1Content = new GUIContent(" Recoil Angle Z", "The maximum angle the weapon rotates around the local z-axis when the primary mechanism fires");
        private readonly static GUIContent recoilZ2Content = new GUIContent(" Recoil Angle Z", "The maximum angle the weapon rotates around the local z-axis when the secondary mechanism fires");
        private readonly static GUIContent recoilMaxKickZ1Content = new GUIContent(" Recoil Kick Back", "The maximum distance, in metres, the weapon will kick back on the local z-axis when the primary mechanism fires");
        private readonly static GUIContent recoilMaxKickZ2Content = new GUIContent(" Recoil Kick Back", "The maximum distance, in metres, the weapon will kick back on the local z-axis when the secondary mechanism fires");


        #endregion

        #region GUIContent - Debug       
        private readonly static GUIContent debugIsFire1InputContent = new GUIContent(" Fire Button 1");
        private readonly static GUIContent debugIsFire2InputContent = new GUIContent(" Fire Button 2");
        private readonly static GUIContent debugIsAimPositionContent = new GUIContent(" Show Aim Position", "Show a small sphere at the Aim Position in the scene");
        private readonly static GUIContent debugAimPositionContent = new GUIContent("  Position", "World space position where the weapon is aiming");
        private readonly static GUIContent debugNumWeaponAimSetsContent = new GUIContent(" Weapon Anim Sets", "The number Weapon Anim Sets (ones that are null are removed at runtime");
        #endregion

        #region Serialized Properties - Weapon Common
        private SerializedProperty showGeneralSettingsInEditorProp;
        private SerializedProperty showAimingAndReticleSettingsInEditorProp;
        private SerializedProperty showAmmoSettingsInEditorProp;
        private SerializedProperty showAnimateSettingsInEditorProp;
        private SerializedProperty showMuzzleFXSettingsInEditorProp;
        private SerializedProperty showRecoilSettingsInEditorProp;
        private SerializedProperty showSmokeSettingsInEditorProp;
        private SerializedProperty showAttachmentSettingsInEditorProp;
        private SerializedProperty showHealthSettingsInEditorProp;
        private SerializedProperty showSpentCartridgeSettingsInEditorProp;
        private SerializedProperty weaponTypeProp;
        private SerializedProperty fireInterval1Prop;
        private SerializedProperty fireInterval2Prop;
        private SerializedProperty isOnlyFireWhenAiming1Prop;
        private SerializedProperty isOnlyFireWhenAiming2Prop;
        private SerializedProperty fireDirectionProp;
        private SerializedProperty relativeFirePositionProp;
        private SerializedProperty firePositionOffsetsProp;
        private SerializedProperty maxRangeProp;
        private SerializedProperty firingButton1Prop;
        private SerializedProperty firingButton2Prop;
        private SerializedProperty firingType1Prop;
        private SerializedProperty firingType2Prop;
        private SerializedProperty hitLayerMaskProp;
        private SerializedProperty checkLineOfSight1Prop;
        private SerializedProperty checkLineOfSight2Prop;
        private SerializedProperty muzzleEffects1Prop;
        private SerializedProperty muzzleEffects1OffsetProp;
        private SerializedProperty isOnlyUseWhenHeldProp;
        private SerializedProperty spentCartridgePrefabProp;
        private SerializedProperty spentEjectPositionProp;
        private SerializedProperty spentEjectDirectionProp;
        private SerializedProperty spentEjectForceProp;
        private SerializedProperty spentEjectRotationProp;
        private SerializedProperty showWFPGizmosInSceneViewProp;
        private SerializedProperty showWSCEPGizmosInSceneViewProp;
        #endregion

        #region Serialized Properties - Weapon Aiming
        private SerializedProperty aimingFirstPersonFOVProp;
        private SerializedProperty aimingThirdPersonFOVProp;
        private SerializedProperty aimingSpeedProp;
        private SerializedProperty defaultReticleSpriteProp;
        private SerializedProperty aimingReticleSpriteProp;
        private SerializedProperty isNoReticleOnAimingFPSProp;
        private SerializedProperty isNoReticleOnAimingTPSProp;
        private SerializedProperty isNoReticleIfHeldProp;
        private SerializedProperty isNoReticleIfHeldNoFreeLookProp;

        #endregion

        #region Serialized Properties - Weapon Ammo

        private SerializedProperty ammoTypesProp;
        private SerializedProperty magTypesProp;
        private SerializedProperty compatibleAmmo1Prop;
        private SerializedProperty compatibleAmmo2Prop;
        private SerializedProperty compatibleMag1Prop;
        private SerializedProperty compatibleMag2Prop;
        private SerializedProperty compatibleAmmoTypeProp;
        private SerializedProperty ammunition1Prop;
        private SerializedProperty ammunition2Prop;
        private SerializedProperty reloadDelay1Prop;
        private SerializedProperty reloadDelay2Prop;
        private SerializedProperty reloadEquipDelay1Prop;
        private SerializedProperty reloadEquipDelay2Prop;
        private SerializedProperty reloadDuration1Prop;
        private SerializedProperty reloadDuration2Prop;
        private SerializedProperty reloadSoundFX1Prop;
        private SerializedProperty reloadSoundFX2Prop;
        private SerializedProperty reloadEquipSoundFX1Prop;
        private SerializedProperty reloadEquipSoundFX2Prop;
        private SerializedProperty reloadType1Prop;
        private SerializedProperty reloadType2Prop;
        private SerializedProperty isMagRequired1Prop;
        private SerializedProperty isMagRequired2Prop;
        private SerializedProperty magCapacity1Prop;
        private SerializedProperty magCapacity2Prop;
        private SerializedProperty magsInReserve1Prop;
        private SerializedProperty magsInReserve2Prop;
        private SerializedProperty fireEmptyEffectsSet1Prop;
        private SerializedProperty fireEmptyEffectsSet2Prop;

        #endregion

        #region Serialized Properties - Weapon Animate

        private SerializedProperty defaultAnimatorProp;

        // Animation Actions
        private SerializedProperty s3dAAListProp;
        private SerializedProperty isAnimActionsExpandedProp;
        private SerializedProperty isS3DAnimActionListExpandedProp;
        private SerializedProperty s3dAnimActionProp;
        private SerializedProperty s3dAAShowInEditorProp;
        private SerializedProperty s3dAAWeaponActionProp;
        private SerializedProperty s3dAAParamTypeProp;
        private SerializedProperty s3dAAParamHashCodeProp;
        private SerializedProperty s3dAAValueProp;
        private SerializedProperty s3dAAIsInvertProp;
        private SerializedProperty s3dAAIsToggleProp;
        private SerializedProperty s3dACListProp;
        private SerializedProperty s3dAnimConditionProp;
        private SerializedProperty s3dACShowInEditorProp;
        private SerializedProperty s3dACConditionTypeProp;
        private SerializedProperty s3dACActionConditionProp;

        // Weapon Anim Sets
        private SerializedProperty isWeaponAnimSetsExpandedProp;
        private SerializedProperty s3dWpnASetListProp;
        private SerializedProperty s3dWeaponAnimSetProp;
        private SerializedProperty isModelsExpandedInEditorProp;

        #endregion

        #region Serialized Properties - Weapon Attachments
        private SerializedProperty laserSightAimFromProp;
        private SerializedProperty isLaserSightOnProp;
        private SerializedProperty isLaserSightAutoOnProp;
        private SerializedProperty laserSightColourProp;
        private SerializedProperty magAttachPointProp1;
        private SerializedProperty magAttachPointProp2;
        private SerializedProperty isScopeOnProp;
        private SerializedProperty scopeCameraProp;
        private SerializedProperty scopeCameraRendererProp;
        private SerializedProperty isScopeAllowNPCProp;
        private SerializedProperty isScopeAutoOnProp;
        #endregion

        #region Serialized Properties - Weapon Beam
        private SerializedProperty beamPrefabProp;
        private SerializedProperty chargeAmountProp;
        private SerializedProperty rechargeTimeProp;
        #endregion

        #region Serialized Properties - Weapon Health
        private SerializedProperty healthProp;
        private SerializedProperty heatLevelProp;
        private SerializedProperty heatUpRateProp;
        private SerializedProperty heatDownRateProp;
        private SerializedProperty overHeatThresholdProp;
        private SerializedProperty isBurnoutOnMaxHeatProp;
        private SerializedProperty isFiringPausedProp;

        #endregion

        #region Serialized Properties - Weapon Projectile
        private SerializedProperty projectilePrefabProp;

        #endregion

        #region Serialized Properties - Weapon Recoil
        private SerializedProperty recoilSpeedProp;
        private SerializedProperty recoilReturnRateProp;
        private SerializedProperty recoilX1Prop;
        private SerializedProperty recoilX2Prop;
        private SerializedProperty recoilY1Prop;
        private SerializedProperty recoilY2Prop;
        private SerializedProperty recoilZ1Prop;
        private SerializedProperty recoilZ2Prop;
        private SerializedProperty recoilMaxKickZ1Prop;
        private SerializedProperty recoilMaxKickZ2Prop;
        #endregion

        #region Serialized Properties - Weapon Smoke

        private SerializedProperty maxActiveSmokeEffects1Prop;
        private SerializedProperty smokeEffectsSet1Prop;
        private SerializedProperty smokeEffectsSet2Prop;

        #endregion

        #region Events

        protected void OnDisable()
        {
            if (isDebugAimPosition && debugAimTargetGO != null)
            {
                DestroyImmediate(debugAimTargetGO);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            isDebugAimPosition = false;

            stickyWeapon = (StickyWeapon)target;
            if (stickyWeapon != null) { stickyInteractive = stickyWeapon; }

            stickyWeapon.ValidateAmmo();

            #region Find Properties - Weapon Common
            showGeneralSettingsInEditorProp = serializedObject.FindProperty("showGeneralSettingsInEditor");
            showAimingAndReticleSettingsInEditorProp = serializedObject.FindProperty("showAimingAndReticleSettingsInEditor");
            showAmmoSettingsInEditorProp = serializedObject.FindProperty("showAmmoSettingsInEditor");
            showAnimateSettingsInEditorProp = serializedObject.FindProperty("showAnimateSettingsInEditor");
            showAttachmentSettingsInEditorProp = serializedObject.FindProperty("showAttachmentSettingsInEditor");
            showHealthSettingsInEditorProp = serializedObject.FindProperty("showHealthSettingsInEditor");
            showMuzzleFXSettingsInEditorProp = serializedObject.FindProperty("showMuzzleFXSettingsInEditor");
            showSmokeSettingsInEditorProp = serializedObject.FindProperty("showSmokeSettingsInEditor");
            showRecoilSettingsInEditorProp = serializedObject.FindProperty("showRecoilSettingsInEditor");
            showSpentCartridgeSettingsInEditorProp = serializedObject.FindProperty("showSpentCartridgeSettingsInEditor");
            weaponTypeProp = serializedObject.FindProperty("weaponType");
            fireDirectionProp = serializedObject.FindProperty("fireDirection");
            relativeFirePositionProp = serializedObject.FindProperty("relativeFirePosition");
            firePositionOffsetsProp = serializedObject.FindProperty("firePositionOffsets");
            fireInterval1Prop = serializedObject.FindProperty("fireInterval1");
            fireInterval2Prop = serializedObject.FindProperty("fireInterval2");
            isOnlyFireWhenAiming1Prop = serializedObject.FindProperty("isOnlyFireWhenAiming1");
            isOnlyFireWhenAiming2Prop = serializedObject.FindProperty("isOnlyFireWhenAiming2");
            maxRangeProp = serializedObject.FindProperty("maxRange");
            firingButton1Prop = serializedObject.FindProperty("firingButton1");
            firingButton2Prop = serializedObject.FindProperty("firingButton2");
            firingType1Prop = serializedObject.FindProperty("firingType1");
            firingType2Prop = serializedObject.FindProperty("firingType2");
            hitLayerMaskProp = serializedObject.FindProperty("hitLayerMask");
            isOnlyUseWhenHeldProp = serializedObject.FindProperty("isOnlyUseWhenHeld");           
            checkLineOfSight1Prop = serializedObject.FindProperty("checkLineOfSight1");
            checkLineOfSight2Prop = serializedObject.FindProperty("checkLineOfSight2");
            muzzleEffects1Prop = serializedObject.FindProperty("muzzleEffects1");
            muzzleEffects1OffsetProp = serializedObject.FindProperty("muzzleEffects1Offset");
            spentCartridgePrefabProp = serializedObject.FindProperty("spentCartridgePrefab");
            spentEjectPositionProp = serializedObject.FindProperty("spentEjectPosition");
            spentEjectDirectionProp = serializedObject.FindProperty("spentEjectDirection");
            spentEjectForceProp = serializedObject.FindProperty("spentEjectForce");
            spentEjectRotationProp = serializedObject.FindProperty("spentEjectRotation");
            showWFPGizmosInSceneViewProp = serializedObject.FindProperty("showWFPGizmosInSceneView");
            showWSCEPGizmosInSceneViewProp = serializedObject.FindProperty("showWSCEPGizmosInSceneView");

            #endregion

            #region Find Properties - Weapon Aiming
            aimingFirstPersonFOVProp = serializedObject.FindProperty("aimingFirstPersonFOV");
            aimingThirdPersonFOVProp = serializedObject.FindProperty("aimingThirdPersonFOV");
            aimingSpeedProp = serializedObject.FindProperty("aimingSpeed");
            defaultReticleSpriteProp = serializedObject.FindProperty("defaultReticleSprite");
            aimingReticleSpriteProp = serializedObject.FindProperty("aimingReticleSprite");
            isNoReticleOnAimingFPSProp = serializedObject.FindProperty("isNoReticleOnAimingFPS");
            isNoReticleOnAimingTPSProp = serializedObject.FindProperty("isNoReticleOnAimingTPS");
            isNoReticleIfHeldProp = serializedObject.FindProperty("isNoReticleIfHeld");
            isNoReticleIfHeldNoFreeLookProp = serializedObject.FindProperty("isNoReticleIfHeldNoFreeLook");
            #endregion

            #region Find Properties - Weapon Ammo
            ammoTypesProp = serializedObject.FindProperty("ammoTypes");
            magTypesProp = serializedObject.FindProperty("magTypes");
            compatibleAmmo1Prop = serializedObject.FindProperty("compatibleAmmo1");
            compatibleAmmo2Prop = serializedObject.FindProperty("compatibleAmmo2");
            compatibleMag1Prop = serializedObject.FindProperty("compatibleMag1");
            compatibleMag2Prop = serializedObject.FindProperty("compatibleMag2");
            ammunition1Prop = serializedObject.FindProperty("ammunition1");
            ammunition2Prop = serializedObject.FindProperty("ammunition2");
            reloadDelay1Prop = serializedObject.FindProperty("reloadDelay1");
            reloadDelay2Prop = serializedObject.FindProperty("reloadDelay2");
            reloadEquipDelay1Prop = serializedObject.FindProperty("reloadEquipDelay1");
            reloadEquipDelay2Prop = serializedObject.FindProperty("reloadEquipDelay2");
            reloadDuration1Prop = serializedObject.FindProperty("reloadDuration1");
            reloadDuration2Prop = serializedObject.FindProperty("reloadDuration2");
            reloadSoundFX1Prop = serializedObject.FindProperty("reloadSoundFX1");
            reloadSoundFX2Prop = serializedObject.FindProperty("reloadSoundFX2");
            reloadEquipSoundFX1Prop = serializedObject.FindProperty("reloadEquipSoundFX1");
            reloadEquipSoundFX2Prop = serializedObject.FindProperty("reloadEquipSoundFX2");
            reloadType1Prop = serializedObject.FindProperty("reloadType1");
            reloadType2Prop = serializedObject.FindProperty("reloadType2");
            isMagRequired1Prop = serializedObject.FindProperty("isMagRequired1");
            isMagRequired2Prop = serializedObject.FindProperty("isMagRequired2");
            magCapacity1Prop = serializedObject.FindProperty("magCapacity1");
            magCapacity2Prop = serializedObject.FindProperty("magCapacity2");
            magsInReserve1Prop = serializedObject.FindProperty("magsInReserve1");
            magsInReserve2Prop = serializedObject.FindProperty("magsInReserve2");
            fireEmptyEffectsSet1Prop = serializedObject.FindProperty("fireEmptyEffectsSet1");
            fireEmptyEffectsSet2Prop = serializedObject.FindProperty("fireEmptyEffectsSet2");

            #endregion

            #region Find Properties - Animate
            defaultAnimatorProp = serializedObject.FindProperty("defaultAnimator");

            // Animation Actions
            s3dAAListProp = serializedObject.FindProperty("s3dAnimActionList");
            isAnimActionsExpandedProp = serializedObject.FindProperty("isAnimActionsExpanded");
            isS3DAnimActionListExpandedProp = serializedObject.FindProperty("isS3DAnimActionListExpanded");

            // Weapon Anim Sets
            isWeaponAnimSetsExpandedProp = serializedObject.FindProperty("isWeaponAnimSetsExpanded");
            s3dWpnASetListProp = serializedObject.FindProperty("s3dWeaponAnimSetList");

            #endregion

            #region Find Properties - Weapon Attahments
            laserSightAimFromProp = serializedObject.FindProperty("laserSightAimFrom");
            isLaserSightOnProp = serializedObject.FindProperty("isLaserSightOn");
            isLaserSightAutoOnProp = serializedObject.FindProperty("isLaserSightAutoOn");
            laserSightColourProp = serializedObject.FindProperty("laserSightColour");
            magAttachPointProp1 = serializedObject.FindProperty("magAttachPoint1");
            magAttachPointProp2 = serializedObject.FindProperty("magAttachPoint2");
            isScopeOnProp = serializedObject.FindProperty("isScopeOn");
            scopeCameraProp = serializedObject.FindProperty("scopeCamera");
            scopeCameraRendererProp = serializedObject.FindProperty("scopeCameraRenderer");
            isScopeAllowNPCProp = serializedObject.FindProperty("isScopeAllowNPC");
            isScopeAutoOnProp = serializedObject.FindProperty("isScopeAutoOn");

            #endregion

            #region Find Properties - Weapon Beam
            beamPrefabProp = serializedObject.FindProperty("beamPrefab");
            
            chargeAmountProp = serializedObject.FindProperty("chargeAmount");
            rechargeTimeProp = serializedObject.FindProperty("rechargeTime");
            #endregion

            #region Find Properties - Weapon Health
            healthProp = serializedObject.FindProperty("health");
            heatLevelProp = serializedObject.FindProperty("heatLevel");
            heatUpRateProp = serializedObject.FindProperty("heatUpRate");
            heatDownRateProp = serializedObject.FindProperty("heatDownRate");
            overHeatThresholdProp = serializedObject.FindProperty("overHeatThreshold");
            isBurnoutOnMaxHeatProp = serializedObject.FindProperty("isBurnoutOnMaxHeat");
            isFiringPausedProp = serializedObject.FindProperty("isFiringPaused");
            #endregion

            #region Find Properties - Weapon Recoil
            recoilSpeedProp = serializedObject.FindProperty("recoilSpeed");
            recoilReturnRateProp = serializedObject.FindProperty("recoilReturnRate");
            recoilX1Prop = serializedObject.FindProperty("recoilX1");
            recoilX2Prop = serializedObject.FindProperty("recoilX2");
            recoilY1Prop = serializedObject.FindProperty("recoilY1");
            recoilY2Prop = serializedObject.FindProperty("recoilY2");
            recoilZ1Prop = serializedObject.FindProperty("recoilZ1");
            recoilZ2Prop = serializedObject.FindProperty("recoilZ2");
            recoilMaxKickZ1Prop = serializedObject.FindProperty("recoilMaxKickZ1");
            recoilMaxKickZ2Prop = serializedObject.FindProperty("recoilMaxKickZ2");

            #endregion

            #region Find Properties - Weapon Smoke
            maxActiveSmokeEffects1Prop = serializedObject.FindProperty("maxActiveSmokeEffects1");
            smokeEffectsSet1Prop = serializedObject.FindProperty("smokeEffectsSet1");
            smokeEffectsSet2Prop = serializedObject.FindProperty("smokeEffectsSet2");
            #endregion

            #region Find Properties - Weapon Projectile
            projectilePrefabProp = serializedObject.FindProperty("projectilePrefab");
            #endregion

            RefreshAnimatorParameters();
        }

        #endregion

        #region Private and Protected Methods

        /// <summary>
        /// Deselect all components in the scene view edit mode, and unhides the Unity tools
        /// </summary>
        protected override void DeselectAllComponents()
        {
            isHandHold1Selected = false;
            isHandHold2Selected = false;
            isEquipPointSelected = false;
            isSocketPointSelected = false;
            isFirePositionSelected = false;
            isSpentCartridgePositionSelected = false;

            // Unhide Unity tools
            Tools.hidden = false;
        }

        /// <summary>
        /// Draw Aiming Rectile Sprite in the inspector
        /// </summary>
        protected void DrawAimingReticleSprite()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(aimingReticleSpriteContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
            if (GUILayout.Button(gotoDisplayReticleFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { StickyEditorHelper.HighlightFolderInProjectWindow(StickySetup.texturesFolder + "/Display", false, true); }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(aimingReticleSpriteProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyWeapon.AimingReticleSprite = (Sprite)aimingReticleSpriteProp.objectReferenceValue;
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the aiming settings in the inspector
        /// </summary>
        protected void DrawAimingSettings()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(aimingFirstPersonFOVProp, aimingFirstPersonFoVContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                serializedObject.ApplyModifiedProperties();
                stickyWeapon.SetAimingFirstPersonFOV(aimingFirstPersonFOVProp.floatValue);
                serializedObject.Update();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(aimingThirdPersonFOVProp, aimingThirdPersonFoVContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                serializedObject.ApplyModifiedProperties();
                stickyWeapon.SetAimingThirdPersonFOV(aimingThirdPersonFOVProp.floatValue);
                serializedObject.Update();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(aimingSpeedProp, aimingSpeedContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                serializedObject.ApplyModifiedProperties();
                stickyWeapon.AimingSpeed = aimingSpeedProp.floatValue;
                serializedObject.Update();
            }
        }

        /// <summary>
        /// Show the aiming and reticle settings in the inspector
        /// </summary>
        protected void DrawAimingAndReticleSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            StickyEditorHelper.DrawS3DFoldout(showAimingAndReticleSettingsInEditorProp, aimingAndReticleSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showAimingAndReticleSettingsInEditorProp.boolValue)
            {
                EditorGUILayout.LabelField(aimingAndReticleInfoContent, helpBoxRichText);
                DrawAimingSettings();

                DrawDefaultReticleSprite();
                DrawAimingReticleSprite();
                DrawNoReticleOptions();
            }
        }

        /// <summary>
        /// Draw the ammo settings in the inspector
        /// </summary>
        protected void DrawAmmoSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            bool isBeamWeapon = IsBeamWeapon();

            // Currently beam weapons don't use ammo or magazines
            if (!isBeamWeapon)
            {
                if (stickyWeapon.AmmoTypes == null) { StickyEditorHelper.NoAmmoTypesAssigned(); }
                if (stickyWeapon.MagTypes == null) { StickyEditorHelper.NoMagTypesAssigned(); }
            }

            StickyEditorHelper.DrawS3DFoldout(showAmmoSettingsInEditorProp, ammoSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showAmmoSettingsInEditorProp.boolValue)
            {
                if (isBeamWeapon)
                {
                    EditorGUILayout.LabelField(ammoBeamWeaponContent);
                }
                else
                {
                    DrawAmmoTypes();
                    DrawMagTypes();

                    DrawAmmoCompatibility(compatibleAmmo1Prop, compatibleAmmoFireButton1Content, 10f);
                    DrawMagCompatibility(compatibleMag1Prop, compatibleMag1Content, 1, 10f);
                    DrawIsMagRequired(isMagRequired1Prop, isMagRequired1Content, 1, 10f);
                    DrawAvailableAmmunition(ammunition1Prop, ammunition1Content, isMagRequired1Prop, 1, 10f);
                    DrawReloadType(reloadType1Prop, reloadType1Content, 1, 10f);
                    StickyEditorHelper.DrawPropertyIndent(10f, reloadDelay1Prop, reloadDelay1Content, defaultEditorLabelWidth);

                    StickyEditorHelper.DrawPropertyIndent(10f, reloadEquipDelay1Prop, reloadEquipDelay1Content, defaultEditorLabelWidth);
                    if (reloadEquipDelay1Prop.floatValue > reloadDuration1Prop.floatValue) { reloadDuration1Prop.floatValue = reloadEquipDelay1Prop.floatValue; }

                    StickyEditorHelper.DrawPropertyIndent(10f, reloadDuration1Prop, reloadDuration1Content, defaultEditorLabelWidth);
                    DrawReloadSoundFX(reloadSoundFX1Prop, reloadSoundFX1Content, 1, 10f);
                    DrawReloadEquipSoundFX(reloadEquipSoundFX1Prop, reloadEquipDelay1Prop, reloadEquipSoundFX1Content, 1, 10f);
                    DrawMagCapacity(magCapacity1Prop, magCapacity1Content, isMagRequired1Prop, ammunition1Prop, 1, 10f);
                    DrawAvailableMagazines(magsInReserve1Prop, magsInReserve1Content, reloadType1Prop, 1, 10f);
                    DrawFireEmptyEffectsSet(fireEmptyEffectsSet1Prop, fireEmptyEffectsSet1Content, 1, 10f);

                    StickyEditorHelper.DrawHorizontalGap(6f);
                    DrawAmmoCompatibility(compatibleAmmo2Prop, compatibleAmmoFireButton2Content, 10f);
                    DrawMagCompatibility(compatibleMag2Prop, compatibleMag2Content, 2, 10f);
                    DrawIsMagRequired(isMagRequired2Prop, isMagRequired2Content, 2, 10f);
                    DrawAvailableAmmunition(ammunition2Prop, ammunition2Content, isMagRequired2Prop, 2, 10f);
                    DrawReloadType(reloadType2Prop, reloadType2Content, 2, 10f);
                    StickyEditorHelper.DrawPropertyIndent(10f, reloadDelay2Prop, reloadDelay2Content, defaultEditorLabelWidth);

                    StickyEditorHelper.DrawPropertyIndent(10f, reloadEquipDelay2Prop, reloadEquipDelay2Content, defaultEditorLabelWidth);
                    if (reloadEquipDelay2Prop.floatValue > reloadDuration2Prop.floatValue) { reloadDuration2Prop.floatValue = reloadEquipDelay2Prop.floatValue; }

                    StickyEditorHelper.DrawPropertyIndent(10f, reloadDuration2Prop, reloadDuration2Content, defaultEditorLabelWidth);
                    DrawReloadSoundFX(reloadSoundFX2Prop, reloadSoundFX2Content, 2, 10f);
                    DrawReloadEquipSoundFX(reloadEquipSoundFX2Prop, reloadEquipDelay2Prop, reloadEquipSoundFX2Content, 1, 10f);
                    DrawMagCapacity(magCapacity2Prop, magCapacity2Content, isMagRequired2Prop, ammunition2Prop, 1, 10f);
                    DrawAvailableMagazines(magsInReserve2Prop, magsInReserve2Content, reloadType2Prop, 2, 10f);
                    DrawFireEmptyEffectsSet(fireEmptyEffectsSet2Prop, fireEmptyEffectsSet2Content, 2, 10f);
                }
            }
        }

        /// <summary>
        /// Draw the array of compatible ammo types for a firing button
        /// </summary>
        /// <param name="compatibleAmmoProp"></param>
        /// <param name="compatibleAmmoContent"></param>
        /// <param name="indent"></param>
        private void DrawAmmoCompatibility(SerializedProperty compatibleAmmoProp, GUIContent compatibleAmmoContent, float indent)
        {
            StickyEditorHelper.DrawHorizontalGap(2f);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(compatibleAmmoContent);

            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(60f)))
            {
                ammoTypeNames = null;
                magTypeNames = null;
            }

            if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
            {
                // Limit to up to 9 different ammo types for the same weapon firing mechanism (should only ever need 3 or 4)
                if (compatibleAmmoProp.arraySize < 9)
                {
                    compatibleAmmoProp.arraySize++;
                }
            }

            if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
            {
                if (compatibleAmmoProp.arraySize > 1) { compatibleAmmoProp.arraySize--; }
            }
            GUILayout.EndHorizontal();

            StickyEditorHelper.DrawHorizontalGap(2f);

            if (ammoTypeNames == null)
            {
                ammoTypeNames = S3DAmmoTypes.GetAmmoTypeNames(stickyWeapon.AmmoTypes, true);
            }

            for (int arrayIdx = 0; arrayIdx < compatibleAmmoProp.arraySize; arrayIdx++)
            {
                compatibleAmmoTypeProp = compatibleAmmoProp.GetArrayElementAtIndex(arrayIdx);

                GUILayout.BeginHorizontal();
                StickyEditorHelper.DrawLabelIndent(indent);
                EditorGUILayout.LabelField("Compatible Ammo Type", GUILayout.Width(defaultEditorLabelWidth - indent - 3f));
                compatibleAmmoTypeProp.intValue = EditorGUILayout.Popup(compatibleAmmoTypeProp.intValue, ammoTypeNames);
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draw the ammo types scriptable object in the inspector
        /// </summary>
        protected void DrawAmmoTypes()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(ammoTypesProp, ammoTypesContent);
            if (EditorGUI.EndChangeCheck())
            {
                // Force the names to be repopulated
                serializedObject.ApplyModifiedProperties();
                ammoTypeNames = null;
                serializedObject.Update();
            }
        }

        /// <summary>
        /// Draw the list of animation actions for the weapon in the inspector.
        /// These apply to the weapon defaultAnimator (not the character holding the weapon)
        /// </summary>
        protected void DrawAnimationActions()
        {
            StickyEditorHelper.DrawS3DFoldout(isAnimActionsExpandedProp, animateActionsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (isAnimActionsExpandedProp.boolValue)
            {
                #region Check if s3dAnimActionList is null
                // Checking the property for being NULL doesn't check if the list is actually null.
                if (stickyWeapon.AnimActionList == null)
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    stickyWeapon.AnimActionList = new List<S3DAnimAction>(4);
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();
                }
                #endregion

                #region Add-Remove AnimActions

                int numAnimActions = s3dAAListProp.arraySize;

                // Reset button variables
                s3dAnimActionMoveDownPos = -1;
                s3dAnimActionInsertPos = -1;
                s3dAnimActionDeletePos = -1;

                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                EditorGUI.BeginChangeCheck();
                isS3DAnimActionListExpandedProp.boolValue = EditorGUILayout.Foldout(isS3DAnimActionListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(stickyWeapon.AnimActionList, isS3DAnimActionListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }
                EditorGUI.indentLevel -= 1;

                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("Animation Actions: " + numAnimActions.ToString("00"), labelFieldRichText);

                if (GUILayout.Button(aaRefreshParamsContent, GUILayout.MaxWidth(80f)))
                {
                    stickyWeapon.gameObject.SetActive(false);
                    stickyWeapon.gameObject.SetActive(true);
                    RefreshAnimatorParameters();
                }

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(stickyWeapon, "Add Animate Action");
                    stickyWeapon.AnimActionList.Add(new S3DAnimAction());
                    ExpandList(stickyWeapon.AnimActionList, false);
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();

                    numAnimActions = s3dAAListProp.arraySize;
                    if (numAnimActions > 0)
                    {
                        // Force new AnimAction to be serialized in scene
                        s3dAnimActionProp = s3dAAListProp.GetArrayElementAtIndex(numAnimActions - 1);
                        s3dAAShowInEditorProp = s3dAnimActionProp.FindPropertyRelative("showInEditor");
                        s3dAAShowInEditorProp.boolValue = !s3dAAShowInEditorProp.boolValue;
                        // Show the new AnimAction
                        s3dAAShowInEditorProp.boolValue = true;
                    }
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numAnimActions > 0) { s3dAnimActionDeletePos = s3dAAListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();

                #endregion

                #region Anim Action List
                numAnimActions = s3dAAListProp.arraySize;

                for (int aaIdx = 0; aaIdx < numAnimActions; aaIdx++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    s3dAnimActionProp = s3dAAListProp.GetArrayElementAtIndex(aaIdx);

                    #region Get Properties for the Animate Action
                    s3dAAShowInEditorProp = s3dAnimActionProp.FindPropertyRelative("showInEditor");
                    s3dAAWeaponActionProp = s3dAnimActionProp.FindPropertyRelative("weaponAction");

                    #endregion

                    #region AnimAction Move/Insert/Delete buttons
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    s3dAAShowInEditorProp.boolValue = EditorGUILayout.Foldout(s3dAAShowInEditorProp.boolValue, "Animate Action " + (aaIdx + 1).ToString("00") + (s3dAAShowInEditorProp.boolValue ? "" : " - (" + (S3DAnimAction.WeaponAction)s3dAAWeaponActionProp.intValue + ")"));
                    EditorGUI.indentLevel -= 1;

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numAnimActions > 1) { s3dAnimActionMoveDownPos = aaIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { s3dAnimActionInsertPos = aaIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dAnimActionDeletePos = aaIdx; }
                    GUILayout.EndHorizontal();
                    #endregion

                    if (s3dAAShowInEditorProp.boolValue)
                    {
                        s3dAAParamTypeProp = s3dAnimActionProp.FindPropertyRelative("parameterType");
                        s3dAAParamHashCodeProp = s3dAnimActionProp.FindPropertyRelative("paramHashCode");

                        EditorGUILayout.PropertyField(s3dAAWeaponActionProp, aaWeaponActionContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(s3dAAParamTypeProp, aaParmTypeContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            s3dAAParamHashCodeProp.intValue = 0;
                        }

                        #region Parameters

                        #region Bool
                        if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeBoolInt)
                        {
                            DrawParameterSelection(animParamsBoolList, animParamBoolNames, s3dAAParamHashCodeProp, aaParmNamesContent);
                            s3dAAIsInvertProp = s3dAnimActionProp.FindPropertyRelative("isInvert");
                            s3dAAIsToggleProp = s3dAnimActionProp.FindPropertyRelative("isToggle");
                            if (IsCustomAction(s3dAAWeaponActionProp))
                            {
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(s3dAAIsInvertProp, aaIsInvertContent);
                                if (EditorGUI.EndChangeCheck() && s3dAAIsInvertProp.boolValue && s3dAAIsToggleProp.boolValue)
                                {
                                    s3dAAIsToggleProp.boolValue = false;
                                }

                                if (!s3dAAIsInvertProp.boolValue)
                                {
                                    EditorGUILayout.PropertyField(s3dAAIsToggleProp, aaIsToggleContent);
                                }

                                if (!s3dAAIsToggleProp.boolValue)
                                {
                                    EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("isResetCustomAfterUse"), aaIsResetCustomAfterUseContent);
                                }
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(aaBoolValueContent, GUILayout.Width(defaultEditorLabelWidth));
                                EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                                GUILayout.EndHorizontal();
                            }
                            else
                            {
                                s3dAAValueProp = s3dAnimActionProp.FindPropertyRelative("actionWeaponBoolValue");
                                if (s3dAAValueProp.intValue != S3DAnimAction.ActionBoolValueFixedInt)
                                {
                                    EditorGUILayout.PropertyField(s3dAAIsInvertProp, aaIsInvertContent);
                                }
                                EditorGUILayout.PropertyField(s3dAAValueProp, aaBoolValueContent);

                                // Is this a fixed bool value the user is sending to the animation controller?
                                if (s3dAAValueProp.intValue == S3DAnimAction.ActionBoolValueFixedInt)
                                {
                                    EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("fixedBoolValue"), aaBoolFixedValueContent);
                                }
                            }
                        }
                        #endregion

                        #region Trigger
                        else if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeTriggerInt)
                        {
                            DrawParameterSelection(animParamsTriggerList, animParamTriggerNames, s3dAAParamHashCodeProp, aaParmNamesContent);
                            if (IsCustomAction(s3dAAWeaponActionProp))
                            {
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(aaTriggerValueContent, GUILayout.Width(defaultEditorLabelWidth));
                                EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                                GUILayout.EndHorizontal();
                            }
                            else
                            {
                                s3dAAValueProp = s3dAnimActionProp.FindPropertyRelative("actionWeaponTriggerValue");
                                EditorGUILayout.PropertyField(s3dAAValueProp, aaTriggerValueContent);
                            }
                        }
                        #endregion

                        #region Float
                        else if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeFloatInt)
                        {
                            DrawParameterSelection(animParamsFloatList, animParamFloatNames, s3dAAParamHashCodeProp, aaParmNamesContent);
                            if (IsCustomAction(s3dAAWeaponActionProp))
                            {
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(aaFloatValueContent, GUILayout.Width(defaultEditorLabelWidth));
                                EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                                GUILayout.EndHorizontal();
                                EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("floatMultiplier"), aaFloatMultiplierContent);
                                //EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("damping"), aaBlendRateContent);
                            }
                            else
                            {
                                s3dAAValueProp = s3dAnimActionProp.FindPropertyRelative("actionWeaponFloatValue");
                                EditorGUILayout.PropertyField(s3dAAValueProp, aaFloatValueContent);

                                // Is this a fixed float value the user is sending to the animation controller?
                                if (s3dAAValueProp.intValue == S3DAnimAction.ActionFloatValueFixedInt)
                                {
                                    EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("fixedFloatValue"), aaFloatFixedValueContent);
                                }
                                else
                                {
                                    EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("floatMultiplier"), aaFloatMultiplierContent);
                                }
                                EditorGUILayout.PropertyField(s3dAnimActionProp.FindPropertyRelative("damping"), aaDampingContent);
                            }
                        }
                        #endregion

                        #region Integer
                        else if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeIntegerInt)
                        {
                            DrawParameterSelection(animParamsIntegerList, animParamIntegerNames, s3dAAParamHashCodeProp, aaParmNamesContent);
                            if (IsCustomAction(s3dAAWeaponActionProp))
                            {
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(aaIntegerValueContent, GUILayout.Width(defaultEditorLabelWidth));
                                EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                                GUILayout.EndHorizontal();
                            }
                            else
                            {
                                // Currently we don't have any integer realtime values in weapons for S3D
                            }
                        }
                        #endregion

                        #endregion

                        #region Transistions

                        //DrawTransitionSelection();

                        #endregion

                        #region Conditions

                        s3dACListProp = s3dAnimActionProp.FindPropertyRelative("s3dAnimConditionList");

                        #region Check if s3dAnimConditionList is null
                        // Checking the property for being NULL doesn't check if the list is actually null.
                        if (stickyWeapon.AnimActionList[aaIdx].s3dAnimConditionList == null)
                        {
                            // Apply property changes
                            serializedObject.ApplyModifiedProperties();
                            stickyWeapon.AnimActionList[aaIdx].s3dAnimConditionList = new List<S3DAnimCondition>(2);
                            isSceneModified = true;
                            // Read in the properties
                            serializedObject.Update();
                        }
                        #endregion

                        #region Add-Remove Animation Conditions

                        int numAnimConditions = s3dACListProp.arraySize;

                        // Reset button variables
                        s3dAnimConditionMoveDownPos = -1;
                        s3dAnimConditionInsertPos = -1;
                        s3dAnimConditionDeletePos = -1;

                        GUILayout.BeginHorizontal();

                        EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                        EditorGUILayout.LabelField(" Conditions: " + numAnimConditions.ToString("00"), labelFieldRichText);

                        if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                        {
                            // Apply property changes
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(stickyWeapon, "Add Animate Condition");
                            stickyWeapon.AnimActionList[aaIdx].s3dAnimConditionList.Add(new S3DAnimCondition());
                            isSceneModified = true;
                            // Read in the properties
                            serializedObject.Update();

                            numAnimConditions = s3dACListProp.arraySize;
                            if (numAnimConditions > 0)
                            {
                                // Force new AnimCondition to be serialized in scene
                                s3dAnimConditionProp = s3dACListProp.GetArrayElementAtIndex(numAnimConditions - 1);
                                s3dACShowInEditorProp = s3dAnimConditionProp.FindPropertyRelative("showInEditor");
                                s3dACShowInEditorProp.boolValue = !s3dACShowInEditorProp.boolValue;
                                // Show the new AnimCondition
                                s3dACShowInEditorProp.boolValue = true;
                            }
                        }
                        if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                        {
                            if (numAnimConditions > 0) { s3dAnimConditionDeletePos = s3dACListProp.arraySize - 1; }
                        }

                        GUILayout.EndHorizontal();

                        #endregion

                        #region Anim Condition List
                        numAnimConditions = s3dACListProp.arraySize;

                        if (numAnimConditions > 0)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(" ", GUILayout.Width(1f));
                            EditorGUILayout.LabelField(conditionsInfoContent, EditorStyles.wordWrappedLabel);
                            GUILayout.EndHorizontal();
                        }

                        for (int acIdx = 0; acIdx < numAnimConditions; acIdx++)
                        {
                            s3dAnimConditionProp = s3dACListProp.GetArrayElementAtIndex(acIdx);

                            #region Get Properties for the Animate Condition
                            s3dACShowInEditorProp = s3dAnimConditionProp.FindPropertyRelative("showInEditor");
                            s3dACConditionTypeProp = s3dAnimConditionProp.FindPropertyRelative("conditionType");
                            s3dACActionConditionProp = s3dAnimConditionProp.FindPropertyRelative("actionWeaponCondition");
                            #endregion

                            #region Condition and  Move/Insert/Delete buttons
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField((acIdx + 1).ToString(" 00"), GUILayout.Width(25f));
                            EditorGUILayout.PropertyField(s3dACConditionTypeProp, GUIContent.none, GUILayout.MaxWidth(60f));
                            EditorGUILayout.PropertyField(s3dACActionConditionProp, GUIContent.none);

                            // Move down button
                            if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numAnimConditions > 1) { s3dAnimConditionMoveDownPos = acIdx; }
                            // Create duplicate button
                            if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { s3dAnimConditionInsertPos = acIdx; }
                            // Delete button
                            if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dAnimConditionDeletePos = acIdx; }
                            GUILayout.EndHorizontal();
                            #endregion
                        }

                        #endregion

                        #region Move/Insert/Delete Anim Conditions

                        if (s3dAnimConditionDeletePos >= 0 || s3dAnimConditionInsertPos >= 0 || s3dAnimConditionMoveDownPos >= 0)
                        {
                            GUI.FocusControl(null);
                            // Don't permit multiple operations in the same pass
                            if (s3dAnimConditionMoveDownPos >= 0)
                            {
                                // Move down one position, or wrap round to start of list
                                if (s3dAnimConditionMoveDownPos < s3dACListProp.arraySize - 1)
                                {
                                    s3dACListProp.MoveArrayElement(s3dAnimConditionMoveDownPos, s3dAnimConditionMoveDownPos + 1);
                                }
                                else { s3dACListProp.MoveArrayElement(s3dAnimConditionMoveDownPos, 0); }

                                s3dAnimConditionMoveDownPos = -1;
                            }
                            else if (s3dAnimConditionInsertPos >= 0)
                            {
                                // NOTE: Undo doesn't work with Insert.

                                // Apply property changes before potential list changes
                                serializedObject.ApplyModifiedProperties();

                                stickyWeapon.AnimActionList[aaIdx].s3dAnimConditionList.Insert(s3dAnimConditionInsertPos, new S3DAnimCondition(stickyWeapon.AnimActionList[aaIdx].s3dAnimConditionList[s3dAnimConditionInsertPos]));

                                // Read all properties from the weapon
                                serializedObject.Update();

                                // Hide original Animate Condition
                                s3dACShowInEditorProp = s3dACListProp.GetArrayElementAtIndex(s3dAnimConditionInsertPos).FindPropertyRelative("showInEditor");

                                // Force new condition to be serialized in scene
                                s3dACShowInEditorProp.boolValue = !s3dACShowInEditorProp.boolValue;

                                // Show inserted duplicate dockingPoint
                                s3dACShowInEditorProp.boolValue = true;

                                s3dAnimConditionInsertPos = -1;

                                isSceneModified = true;
                            }
                            else if (s3dAnimConditionDeletePos >= 0)
                            {
                                // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                                int _deleteIndex = s3dAnimConditionDeletePos;

                                if (EditorUtility.DisplayDialog("Delete Animate Condition " + (s3dAnimConditionDeletePos + 1) + "?", "Animate Condition " + (s3dAnimConditionDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the Animate Condition from the list and cannot be undone.", "Delete Now", "Cancel"))
                                {
                                    s3dACListProp.DeleteArrayElementAtIndex(_deleteIndex);
                                    s3dAnimConditionDeletePos = -1;
                                }
                            }

                            serializedObject.ApplyModifiedProperties();
                            // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                            if (!Application.isPlaying)
                            {
                                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                            }
                            GUIUtility.ExitGUI();
                        }

                        #endregion

                        #endregion
                    }

                    GUILayout.EndVertical();
                }

                #endregion

                #region Move/Insert/Delete Anim Actions

                if (s3dAnimActionDeletePos >= 0 || s3dAnimActionInsertPos >= 0 || s3dAnimActionMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);
                    // Don't permit multiple operations in the same pass
                    if (s3dAnimActionMoveDownPos >= 0)
                    {
                        // Move down one position, or wrap round to start of list
                        if (s3dAnimActionMoveDownPos < s3dAAListProp.arraySize - 1)
                        {
                            s3dAAListProp.MoveArrayElement(s3dAnimActionMoveDownPos, s3dAnimActionMoveDownPos + 1);
                        }
                        else { s3dAAListProp.MoveArrayElement(s3dAnimActionMoveDownPos, 0); }

                        s3dAnimActionMoveDownPos = -1;
                    }
                    else if (s3dAnimActionInsertPos >= 0)
                    {
                        // NOTE: Undo doesn't work with Insert.

                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        S3DAnimAction insertedAnimAction = new S3DAnimAction(stickyWeapon.AnimActionList[s3dAnimActionInsertPos]);
                        insertedAnimAction.showInEditor = true;
                        // Generate a new hashcode for the duplicated AnimAction
                        insertedAnimAction.guidHash = S3DMath.GetHashCodeFromGuid();

                        stickyWeapon.AnimActionList.Insert(s3dAnimActionInsertPos, insertedAnimAction);

                        // Read all properties from the Sticky Controller
                        serializedObject.Update();

                        // Hide original Animate Action
                        s3dAAListProp.GetArrayElementAtIndex(s3dAnimActionInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                        s3dAAShowInEditorProp = s3dAAListProp.GetArrayElementAtIndex(s3dAnimActionInsertPos).FindPropertyRelative("showInEditor");

                        // Force new action to be serialized in scene
                        s3dAAShowInEditorProp.boolValue = !s3dAAShowInEditorProp.boolValue;

                        // Show inserted duplicate AnimAction
                        s3dAAShowInEditorProp.boolValue = true;

                        s3dAnimActionInsertPos = -1;

                        isSceneModified = true;
                    }
                    else if (s3dAnimActionDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                        int _deleteIndex = s3dAnimActionDeletePos;

                        if (EditorUtility.DisplayDialog("Delete Animate Action " + (s3dAnimActionDeletePos + 1) + "?", "Animate Action " + (s3dAnimActionDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the Animate Action from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            s3dAAListProp.DeleteArrayElementAtIndex(_deleteIndex);
                            s3dAnimActionDeletePos = -1;
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                    // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    GUIUtility.ExitGUI();
                }

                #endregion

            }

            StickyEditorHelper.DrawHorizontalGap(2f);
        }


        /// <summary>
        /// Draw the available ammunition for a firing button in the inspector
        /// </summary>
        /// <param name="ammunitionProp"></param>
        /// <param name="ammunitionContent"></param>
        /// <param name="indent"></param>
        protected void DrawAvailableAmmunition(SerializedProperty ammunitionProp, GUIContent ammunitionContent, SerializedProperty isMagRequiredProp, int weaponButtonNumber, float indent)
        {
            bool unlimitedAmmo = ammunitionProp.intValue < 0 && !isMagRequiredProp.boolValue;

            if (!isMagRequiredProp.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                StickyEditorHelper.DrawLabelIndent(indent, unlimitedAmmoContent, defaultEditorLabelWidth - indent - 3f);
                unlimitedAmmo = EditorGUILayout.Toggle(unlimitedAmmo);
                EditorGUILayout.EndHorizontal();
            }

            if (unlimitedAmmo && ammunitionProp.intValue != -1) { ammunitionProp.intValue = -1; }
            else if (!unlimitedAmmo)
            {
                if (ammunitionProp.intValue < 0)
                {
                    // Attempt to set the ammo amount to 10 (unless a StickyMagazine is required).
                    int newValue = isMagRequiredProp.boolValue ? 0 : 10;
                    ammunitionProp.intValue = newValue;

                    // Verify it AFTER first setting it else it could remain at -1.
                    serializedObject.ApplyModifiedProperties();                    
                    stickyWeapon.SetAmmunition(weaponButtonNumber, newValue);
                    serializedObject.Update();
                }

                // When is a stickymagazine is required, only get ammuntion number at runtime.
                if (isMagRequiredProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawLabelIndent(indent, ammunitionContent, defaultEditorLabelWidth - indent - 3f);
                    if (EditorApplication.isPlaying && stickyWeapon.IsInitialised)
                    {
                        EditorGUILayout.LabelField(ammunitionProp.intValue.ToString());
                    }
                    else
                    {
                        EditorGUILayout.LabelField("--");
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    StickyEditorHelper.DrawPropertyIndent(indent, ammunitionProp, ammunitionContent, defaultEditorLabelWidth);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        stickyWeapon.SetAmmunition(weaponButtonNumber, ammunitionProp.intValue);
                        serializedObject.Update();
                    }
                }
            }
        }

        /// <summary>
        /// Draw the available ammunition for a firing button in the inspector
        /// </summary>
        /// <param name="magsInReserveProp"></param>
        /// <param name="magsInReserveContent"></param>
        /// <param name="reloadTypeProp"></param>
        /// <param name="weaponButtonNumber">1 (primary) or 2 (secondary)</param>
        /// <param name="indent"></param>
        protected void DrawAvailableMagazines(SerializedProperty magsInReserveProp, GUIContent magsInReserveContent, SerializedProperty reloadTypeProp, int weaponButtonNumber, float indent)
        {
            bool unlimitedMags = magsInReserveProp.intValue < 0;

            // Don't display unlimited mags option when reload type is Auto Stash
            if (reloadTypeProp.intValue != StickyWeapon.ReloadTypeAutoStashInt || reloadTypeProp.intValue == StickyWeapon.ReloadTypeAutoStashWithDropInt)
            {
                EditorGUILayout.BeginHorizontal();
                StickyEditorHelper.DrawLabelIndent(indent, unlimitedMagsContent, defaultEditorLabelWidth - indent - 3f);
                unlimitedMags = EditorGUILayout.Toggle(unlimitedMags);
                EditorGUILayout.EndHorizontal();
            }

            if (unlimitedMags && magsInReserveProp.intValue != -1) { magsInReserveProp.intValue = -1; }
            else if (!unlimitedMags)
            {
                if (magsInReserveProp.intValue < 0)
                {
                    int defaultMags = weaponButtonNumber == 1 ? 5 : 1;

                    // Attempt to set the number of magazines to defaultMags
                    magsInReserveProp.intValue = defaultMags;

                    // Verify it AFTER first setting it else it could remain at -1.
                    serializedObject.ApplyModifiedProperties();                    
                    stickyWeapon.SetMagsInReserve(weaponButtonNumber, defaultMags);
                    serializedObject.Update();
                }

                // When reloading from Stash, we can only get the number of reserved magazines at runtime.
                if (reloadTypeProp.intValue == StickyWeapon.ReloadTypeAutoStashInt || reloadTypeProp.intValue == StickyWeapon.ReloadTypeAutoStashWithDropInt)
                {
                    EditorGUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawLabelIndent(indent, magsInReserveContent, defaultEditorLabelWidth - indent - 3f);
                    if (EditorApplication.isPlaying && stickyWeapon.IsInitialised)
                    {
                        EditorGUILayout.LabelField(magsInReserveProp.intValue.ToString());
                    }
                    else
                    {
                        EditorGUILayout.LabelField("--");
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    StickyEditorHelper.DrawPropertyIndent(indent, magsInReserveProp, magsInReserveContent, defaultEditorLabelWidth);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        stickyWeapon.SetMagsInReserve(weaponButtonNumber, magsInReserveProp.intValue);
                        serializedObject.Update();
                    }
                }
            }
        }

        /// <summary>
        /// Draw the animate settings in the inspector
        /// </summary>
        protected void DrawAnimateSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            StickyEditorHelper.DrawS3DFoldout(showAnimateSettingsInEditorProp, animateSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showAnimateSettingsInEditorProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(defaultAnimatorProp, defaultAnimatorContent);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyWeapon.SetDefaultAnimator(stickyWeapon.DefaultAnimator);
                    serializedObject.Update();
                    RefreshAnimatorParameters();
                }

                DrawAnimationActions();
                DrawWeaponAnimSets();

                //EditorGUILayout.PropertyField(muzzleEffectsOffsetProp, muzzleEffectsOffsetContent);
            }
        }

        /// <summary>
        /// Draw the scope settings in the inspector
        /// </summary>
        protected void DrawAttachmentSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            StickyEditorHelper.DrawS3DFoldout(showAttachmentSettingsInEditorProp, attachmentSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            bool isBeamWeapon = IsBeamWeapon();

            if (!isBeamWeapon)
            {
                if (isMagRequired1Prop.boolValue && magAttachPointProp1.objectReferenceValue == null)
                {
                    StickyEditorHelper.NoMagAttachPointAssigned(1);
                }

                if (isMagRequired2Prop.boolValue && magAttachPointProp2.objectReferenceValue == null)
                {
                    StickyEditorHelper.NoMagAttachPointAssigned(2);
                }
            }

            if (showAttachmentSettingsInEditorProp.boolValue)
            {
                #region Laser Sight
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(laserSightAimFromProp, laserSightAimFromContent);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyWeapon.LaserSightAimFrom = laserSightAimFromProp.objectReferenceValue as Transform;
                    serializedObject.Update();

                    // Ensure it gets remove when in the editor while NOT in play mode
                    if (!EditorApplication.isPlaying)
                    {
                        if (laserSightAimFromProp.objectReferenceValue == null)
                        {
                            stickyWeapon.UnEquipLaserSight();
                        }
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isLaserSightOnProp, isLaserSightOnContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    if (isLaserSightOnProp.boolValue) { stickyWeapon.TurnOnLaserSight(); }
                    else { stickyWeapon.TurnOffLaserSight(); }
                }

                EditorGUILayout.PropertyField(isLaserSightAutoOnProp, isLaserSightAutoOnContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(laserSightColourProp, laserSightColourContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    // We could just use the property.colorValue but this will more accurate and use the Color32 value.
                    serializedObject.ApplyModifiedProperties();
                    stickyWeapon.SetLaserSightColour(stickyWeapon.LaserSightColour);
                    serializedObject.Update();
                }

                #endregion

                #region Magazines

                if (!isBeamWeapon)
                {
                    DrawMagAttachPoint(magAttachPointProp1, magAttachPoint1Content, 1);
                    DrawMagAttachPoint(magAttachPointProp2, magAttachPoint2Content, 2);
                }

                #endregion

                #region Scope
                DrawScope();
                #endregion
            }
        }

        /// <summary>
        /// Draw Beam Module prefab in the inspector
        /// </summary>
        protected void DrawBeamPrefab()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(beamPrefabContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
            if (GUILayout.Button(gotoBeamsFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { StickyEditorHelper.HighlightFolderInProjectWindow(StickySetup.demosBeamsFolder, false, true); }
            EditorGUILayout.PropertyField(beamPrefabProp, GUIContent.none);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw Default Rectile Sprite in the inspector
        /// </summary>
        protected void DrawDefaultReticleSprite()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(defaultReticleSpriteContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
            if (GUILayout.Button(gotoDisplayReticleFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { StickyEditorHelper.HighlightFolderInProjectWindow(StickySetup.texturesFolder + "/Display", false, true); }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(defaultReticleSpriteProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyWeapon.DefaultReticleSprite = (Sprite)defaultReticleSpriteProp.objectReferenceValue;
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw fireButton1 in the inspector
        /// </summary>
        protected void DrawFireButton1()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(firingButton1Prop, firingButton1Content);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyWeapon.FiringButton1 = (StickyWeapon.FiringButton)firingButton1Prop.intValue;
            }

            if (firingButton1Prop.intValue == StickyWeapon.FiringButtonAutoInt)
            {
                EditorGUILayout.PropertyField(checkLineOfSight1Prop, checkLineOfSightContent);
            }
        }

        /// <summary>
        /// Draw fireButton2 in the inspector
        /// </summary>
        protected void DrawFireButton2()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(firingButton2Prop, firingButton2Content);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyWeapon.FiringButton2 = (StickyWeapon.FiringButton)firingButton2Prop.intValue;
            }

            if (firingButton2Prop.intValue == StickyWeapon.FiringButtonAutoInt)
            {
                EditorGUILayout.PropertyField(checkLineOfSight2Prop, checkLineOfSightContent);
            }
        }

        /// <summary>
        /// Draw an empty Effects Set in the inspector
        /// </summary>
        /// <param name="fireEffectsSetProp"></param>
        /// <param name="fireEffectsSetContent"></param>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="indent"></param>
        protected void DrawFireEmptyEffectsSet (SerializedProperty fireEffectsSetProp, GUIContent fireEffectsSetContent, int weaponButtonNumber, float indent)
        {
            EditorGUI.BeginChangeCheck();
            StickyEditorHelper.DrawPropertyIndent(indent, fireEffectsSetProp, fireEffectsSetContent, defaultEditorLabelWidth);
            //EditorGUILayout.PropertyField(effectsSetProp, effectsSetContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                serializedObject.ApplyModifiedProperties();
                stickyWeapon.ReinitialiseEmptyFire(weaponButtonNumber);
                serializedObject.Update();
            }
        }

        /// <summary>
        /// Draw fire interval1 in the inspector
        /// </summary>
        protected void DrawFireInterval1()
        {
            EditorGUILayout.PropertyField(fireInterval1Prop, fireInterval1Content);
        }

        /// <summary>
        /// Draw fire interval2 in the inspector
        /// </summary>
        protected void DrawFireInterval2()
        {
            EditorGUILayout.PropertyField(fireInterval2Prop, fireInterval2Content);
        }

        /// <summary>
        /// Draw fireType1 in the inspector
        /// </summary>
        protected void DrawFireType1()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(firingType1Prop, firingType1Content);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyWeapon.FiringType1 = (StickyWeapon.FiringType)firingType1Prop.intValue;
            }
        }

        /// <summary>
        /// Draw fireType2 in the inspector
        /// </summary>
        protected void DrawFireType2()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(firingType2Prop, firingType2Content);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyWeapon.FiringType2 = (StickyWeapon.FiringType)firingType2Prop.intValue;
            }
        }

        /// <summary>
        /// Draw the health and heat settings in the inspector
        /// </summary>
        protected void DrawHealthSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            StickyEditorHelper.DrawS3DFoldout(showHealthSettingsInEditorProp, healthSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showHealthSettingsInEditorProp.boolValue)
            {
                healthProp.floatValue = EditorGUILayout.Slider(healthContent, healthProp.floatValue * 100f, 0f, 100f) / 100f;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(heatLevelProp, heatLevelContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyWeapon.SetHeatLevel(heatLevelProp.floatValue);
                }
                EditorGUILayout.PropertyField(heatUpRateProp, heatUpRateContent);
                EditorGUILayout.PropertyField(heatDownRateProp, heatDownRateContent);
                EditorGUILayout.PropertyField(overHeatThresholdProp, overHeatThresholdContent);
                EditorGUILayout.PropertyField(isBurnoutOnMaxHeatProp, isBurnoutOnMaxHeatContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isFiringPausedProp, isFiringPausedContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyWeapon.SetIsFiringPaused(isFiringPausedProp.boolValue);
                }
            }
        }

        /// <summary>
        /// Draw the hit layer mask in the inspector with a reset button
        /// </summary>
        protected void DrawHitLayerMask()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(hitLayerMaskContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
            if (GUILayout.Button(StickyEditorHelper.btnResetContent, buttonCompact, GUILayout.Width(20f)))
            {
                hitLayerMaskProp.intValue = Physics.DefaultRaycastLayers;
            }
            EditorGUILayout.PropertyField(hitLayerMaskProp, GUIContent.none);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw if the firing button or mechanism can only be fired if the weapon is currently being aimed.
        /// </summary>
        /// <param name="fireButtonNumber"></param>
        protected void DrawIsOnlyFireWhenAiming(int fireButtonNumber)
        {
            if (fireButtonNumber == 1)
            {
                EditorGUILayout.PropertyField(isOnlyFireWhenAiming1Prop, isOnlyFireWhenAiming1Content);
            }
            else if (fireButtonNumber == 2)
            {
                EditorGUILayout.PropertyField(isOnlyFireWhenAiming2Prop, isOnlyFireWhenAiming2Content);
            }
        }

        /// <summary>
        /// Draw if a StickyMagazine is required in the inspector
        /// </summary>
        /// <param name="isMagRequriedProp"></param>
        /// <param name="isMagRequiredContent"></param>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="indent"></param>
        protected void DrawIsMagRequired (SerializedProperty isMagRequriedProp, GUIContent isMagRequiredContent, int weaponButtonNumber, float indent)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            StickyEditorHelper.DrawLabelIndent(indent, isMagRequiredContent, defaultEditorLabelWidth - indent - 3f);
            EditorGUILayout.PropertyField(isMagRequriedProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                stickyWeapon.SetIsMagRequired(weaponButtonNumber, isMagRequriedProp.boolValue);
                serializedObject.Update();
            }
        }

        /// <summary>
        /// Draw a magazine attachment point in the inspector
        /// </summary>
        /// <param name="magAttachPointProp"></param>
        /// <param name="magAttachPointContent"></param>
        /// <param name="weaponButtonNumber"></param>
        protected void DrawMagAttachPoint(SerializedProperty magAttachPointProp, GUIContent magAttachPointContent, int weaponButtonNumber)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(magAttachPointContent, GUILayout.Width(defaultEditorLabelWidth - 53f));
            if (GUILayout.Button(newContent, buttonCompact, GUILayout.MaxWidth(50f)) && magAttachPointProp.objectReferenceValue == null)
            {
                serializedObject.ApplyModifiedProperties();

                string goName = "Mag Attach Point " + (weaponButtonNumber == 1 ? "(Primary)" : "(Secondary)");

                Undo.SetCurrentGroupName("New " + goName);
                int undoGroup = UnityEditor.Undo.GetCurrentGroup();
                Undo.RecordObject(stickyWeapon, string.Empty);

                GameObject mapGameObject = new GameObject(goName);
                Undo.RegisterCreatedObjectUndo(mapGameObject, string.Empty);
                mapGameObject.transform.SetParent(stickyWeapon.transform, false);
                stickyWeapon.SetMagazineAttachPoint(mapGameObject.transform, weaponButtonNumber);
                Undo.CollapseUndoOperations(undoGroup);

                // Should be non-scene objects but is required to force being set as dirty
                EditorUtility.SetDirty(stickyWeapon);

                GUIUtility.ExitGUI();
            }
            EditorGUILayout.PropertyField(magAttachPointProp, GUIContent.none);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                stickyWeapon.SetMagazineAttachPoint(magAttachPointProp.objectReferenceValue as Transform, weaponButtonNumber);
                serializedObject.Update();

                // Ensure it gets remove when in the editor while NOT in play mode
                if (!EditorApplication.isPlaying)
                {
                    if (magAttachPointProp.objectReferenceValue == null)
                    {
                        stickyWeapon.UnEquipMagazine(weaponButtonNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Draw the capacity of the primary or secondary magazine in the inspector
        /// </summary>
        /// <param name="magCapacityPro"></param>
        /// <param name="magCapacityContent"></param>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="indent"></param>
        protected void DrawMagCapacity
        (
            SerializedProperty magCapacityProp, GUIContent magCapacityContent,
            SerializedProperty isMagRequiredProp, SerializedProperty ammunitionProp,
            int weaponButtonNumber, float indent
        )
        {
            if (isMagRequiredProp.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                StickyEditorHelper.DrawLabelIndent(indent, magCapacityContent, defaultEditorLabelWidth - indent - 3f);
                if (EditorApplication.isPlaying && stickyWeapon.IsInitialised)
                {
                    EditorGUILayout.LabelField(magCapacityProp.intValue.ToString());
                }
                else
                {
                    EditorGUILayout.LabelField("--");
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                StickyEditorHelper.DrawLabelIndent(indent, magCapacityContent, defaultEditorLabelWidth - indent - 3f);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(magCapacityProp, GUIContent.none);
                // Ensure the available ammunition is always at or above the magazine capacity
                if (EditorGUI.EndChangeCheck() && magCapacityProp.intValue < ammunitionProp.intValue)
                {
                    ammunitionProp.intValue = magCapacityProp.intValue;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draw a dropdown for the compatible magazine type
        /// </summary>
        /// <param name="compatibleMagProp"></param>
        /// <param name="compatibleMagContent"></param>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="indent"></param>
        protected void DrawMagCompatibility(SerializedProperty compatibleMagProp, GUIContent compatibleMagContent, int weaponButtonNumber, float indent)
        {
            if (magTypeNames == null)
            {
                magTypeNames = S3DMagTypes.GetMagTypeNames(stickyWeapon.MagTypes, true);
            }

            GUILayout.BeginHorizontal();
            StickyEditorHelper.DrawLabelIndent(indent);
            EditorGUILayout.LabelField(compatibleMagContent, GUILayout.Width(defaultEditorLabelWidth - indent - 3f));
            EditorGUI.BeginChangeCheck();
            compatibleMagProp.intValue = EditorGUILayout.Popup(compatibleMagProp.intValue, magTypeNames);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                serializedObject.ApplyModifiedProperties();
                stickyWeapon.SetCompatibleMagType(weaponButtonNumber, compatibleMagProp.intValue);
                serializedObject.Update();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the mag types scriptable object in the inspector
        /// </summary>
        protected void DrawMagTypes()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(magTypesProp, magTypesContent);
            if (EditorGUI.EndChangeCheck())
            {
                // Force the names to be repopulated
                serializedObject.ApplyModifiedProperties();
                magTypeNames = null;

                // If there is no magTypes scriptable object, reset the compatible mag types to default
                if (stickyWeapon.MagTypes == null)
                {
                    stickyWeapon.SetCompatibleMagType(1, 0);
                    stickyWeapon.SetCompatibleMagType(2, 0);
                }
                serializedObject.Update();
            }
        }

        /// <summary>
        /// Draw the settings and the array of Muzzle Effects objects for this weapon
        /// </summary>
        protected void DrawMuzzleFXSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            StickyEditorHelper.DrawS3DFoldout(showMuzzleFXSettingsInEditorProp, muzzleFXSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showMuzzleFXSettingsInEditorProp.boolValue)
            {
                EditorGUILayout.PropertyField(muzzleEffects1OffsetProp, muzzleEffectsOffsetContent);

                StickyEditorHelper.DrawArray(muzzleEffects1Prop, muzzleEffects1Content, defaultEditorLabelWidth, "FX");
            }
        }

        /// <summary>
        /// Draw the no reticle options in the inspector
        /// </summary>
        protected void DrawNoReticleOptions()
        {
            EditorGUILayout.LabelField(noReticleContent);

            StickyEditorHelper.DrawPropertyIndent(10f, isNoReticleOnAimingFPSProp, isNoReticleOnAimingFPSContent, defaultEditorLabelWidth + 25f);
            StickyEditorHelper.DrawPropertyIndent(10f, isNoReticleOnAimingTPSProp, isNoReticleOnAimingTPSContent, defaultEditorLabelWidth + 25f);
            StickyEditorHelper.DrawPropertyIndent(10f, isNoReticleIfHeldProp, isNoReticleIfHeldContent, defaultEditorLabelWidth + 25f);
            StickyEditorHelper.DrawPropertyIndent(10f, isNoReticleIfHeldNoFreeLookProp, isNoReticleIfHeldNoFreeLookContent, defaultEditorLabelWidth + 25f);
        }

        /// <summary>
        ///  Draw the recoil settings in the inspector
        /// </summary>
        protected void DrawRecoilSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            StickyEditorHelper.DrawS3DFoldout(showRecoilSettingsInEditorProp, recoilSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showRecoilSettingsInEditorProp.boolValue)
            {
                EditorGUILayout.PropertyField(recoilSpeedProp, recoilSpeedContent);
                EditorGUILayout.PropertyField(recoilReturnRateProp, recoilReturnRateContent);

                EditorGUILayout.LabelField(recoilFiringButton1Content);
                StickyEditorHelper.DrawPropertyIndent(10f, recoilX1Prop, recoilX1Content, defaultEditorLabelWidth);
                StickyEditorHelper.DrawPropertyIndent(10f, recoilY1Prop, recoilY1Content, defaultEditorLabelWidth);
                StickyEditorHelper.DrawPropertyIndent(10f, recoilZ1Prop, recoilZ1Content, defaultEditorLabelWidth);
                StickyEditorHelper.DrawPropertyIndent(10f, recoilMaxKickZ1Prop, recoilMaxKickZ1Content, defaultEditorLabelWidth);

                EditorGUILayout.LabelField(recoilFiringButton2Content);
                StickyEditorHelper.DrawPropertyIndent(10f, recoilX2Prop, recoilX2Content, defaultEditorLabelWidth);
                StickyEditorHelper.DrawPropertyIndent(10f, recoilY2Prop, recoilY2Content, defaultEditorLabelWidth);
                StickyEditorHelper.DrawPropertyIndent(10f, recoilZ2Prop, recoilZ2Content, defaultEditorLabelWidth);
                StickyEditorHelper.DrawPropertyIndent(10f, recoilMaxKickZ2Prop, recoilMaxKickZ2Content, defaultEditorLabelWidth);
            }
        }

        /// <summary>
        /// Draw the smoke settings in the inspector
        /// </summary>
        protected void DrawSmokeSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            StickyEditorHelper.DrawS3DFoldout(showSmokeSettingsInEditorProp, smokeSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showSmokeSettingsInEditorProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(maxActiveSmokeEffects1Prop, maxActiveSmokeEffects1Content);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    /// TODO - fix up things after changing maxActiveSmokeEffects1 at runtime...
                    
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(smokeEffectsSet1Prop, smokeEffectsSet1Content);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyWeapon.ReinitialiseSmoke(1);
                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(smokeEffectsSet2Prop, smokeEffectsSet2Content);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyWeapon.ReinitialiseSmoke(2);
                    serializedObject.Update();
                }
            }
        }

        /// <summary>
        /// Draw Projectile Module prefab in the inspector
        /// </summary>
        protected void DrawProjectilePrefab()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(projectilePrefabContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
            if (GUILayout.Button(gotoProjectilesFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { StickyEditorHelper.HighlightFolderInProjectWindow(StickySetup.demosProjectilesFolder, false, true); }
            EditorGUILayout.PropertyField(projectilePrefabProp, GUIContent.none);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a drop down list of animator parameters
        /// </summary>
        /// <param name="paramList"></param>
        /// <param name="paramNames"></param>
        /// <param name="hashCodeProperty"></param>
        /// <param name="labelContent"></param>
        private void DrawParameterSelection(List<S3DAnimParm> paramList, string[] paramNames, SerializedProperty hashCodeProperty, GUIContent labelContent)
        {
            if (paramNames == null) { RefreshAnimatorParameters(); }

            if (paramNames == null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelContent, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(aaNoneContent, GUILayout.Width(defaultEditorLabelWidth - 8f));
                GUILayout.EndHorizontal();
            }
            else
            {
                int paramIdx = paramList.FindIndex(p => p.hashCode == hashCodeProperty.intValue);

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelContent, GUILayout.Width(defaultEditorLabelWidth - 1f));
                EditorGUI.BeginChangeCheck();
                paramIdx = EditorGUILayout.Popup(paramIdx, paramNames);
                if (EditorGUI.EndChangeCheck())
                {
                    // The parameter list and the name array should be in synch. See RefreshAnimatorParameters()
                    if (paramIdx < paramList.Count)
                    {
                        hashCodeProperty.intValue = paramList[paramIdx].hashCode;
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draw a reloading Equip Sound FX in the inspector
        /// </summary>
        /// <param name="reloadEquipSoundFXProp"></param>
        /// <param name="reloadEquipDelayProp"></param>
        /// <param name="reloadEquipSoundFXContent"></param>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="indent"></param>
        protected void DrawReloadEquipSoundFX (SerializedProperty reloadEquipSoundFXProp, SerializedProperty reloadEquipDelayProp, GUIContent reloadEquipSoundFXContent, int weaponButtonNumber, float indent)
        {
            if (reloadEquipDelayProp.floatValue > 0f)
            {
                GUILayout.BeginHorizontal();
                StickyEditorHelper.DrawLabelIndent(indent, reloadEquipSoundFXContent, defaultEditorLabelWidth - 24f - indent);
                if (GUILayout.Button(gotoEffectFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { StickyEditorHelper.HighlightFolderInProjectWindow(StickySetup.demosEffectsFolder, false, true); }
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(reloadEquipSoundFXProp, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyWeapon.SetReloadEquipSoundFX(weaponButtonNumber, stickyWeapon.GetReloadEquipSoundFX(weaponButtonNumber));
                    serializedObject.Update();
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draw a reloading Sound FX in the inspector
        /// </summary>
        /// <param name="reloadSoundFXProp"></param>
        /// <param name="reloadSoundFXContent"></param>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="indent"></param>
        protected void DrawReloadSoundFX (SerializedProperty reloadSoundFXProp, GUIContent reloadSoundFXContent, int weaponButtonNumber, float indent)
        {
            GUILayout.BeginHorizontal();
            StickyEditorHelper.DrawLabelIndent(indent, reloadSoundFXContent, defaultEditorLabelWidth - 24f - indent);
            if (GUILayout.Button(gotoEffectFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { StickyEditorHelper.HighlightFolderInProjectWindow(StickySetup.demosEffectsFolder, false, true); }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(reloadSoundFXProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                stickyWeapon.SetReloadSoundFX(weaponButtonNumber, stickyWeapon.GetReloadSoundFX(weaponButtonNumber));
                serializedObject.Update();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the reloadtype in the inspector
        /// </summary>
        /// <param name="reloadTypeProp"></param>
        /// <param name="reloadTypeContent"></param>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="indent"></param>
        protected void DrawReloadType (SerializedProperty reloadTypeProp, GUIContent reloadTypeContent, int weaponButtonNumber, float indent)
        {
            EditorGUILayout.BeginHorizontal();
            StickyEditorHelper.DrawLabelIndent(indent, reloadTypeContent, defaultEditorLabelWidth - indent - 3f);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(reloadTypeProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                stickyWeapon.SetReloadType(weaponButtonNumber, (StickyWeapon.ReloadType)reloadTypeProp.intValue);
                serializedObject.Update();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the scope settings (used for precise visual aiming) for this weapon in the inspector.
        /// </summary>
        protected void DrawScope()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isScopeOnProp, isScopeOnContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                if (isScopeOnProp.boolValue) { stickyWeapon.TurnOnScope(); }
                else { stickyWeapon.TurnOffScope(); }
            }

            #region Scope Camera
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(scopeCameraContent, GUILayout.Width(defaultEditorLabelWidth - 53f));
            if (GUILayout.Button(newContent, buttonCompact, GUILayout.MaxWidth(50f)) && scopeCameraProp.objectReferenceValue == null)
            {
                // Add a new camera to the weapon to be used for the Scope
                serializedObject.ApplyModifiedProperties();

                string goName = "Scope Camera";

                Undo.SetCurrentGroupName("New " + goName);
                int undoGroup = UnityEditor.Undo.GetCurrentGroup();
                Undo.RecordObject(stickyWeapon, string.Empty);

                GameObject camGameObject = new GameObject(goName);
                Undo.RegisterCreatedObjectUndo(camGameObject, string.Empty);
                Camera _newCamera = Undo.AddComponent(camGameObject, typeof(Camera)) as Camera;

                // By default, give it a narrow field of view to make it "zoomed in".
                _newCamera.fieldOfView = 12.5f;
                _newCamera.nearClipPlane = 0.05f;
                _newCamera.farClipPlane = stickyWeapon.MaxRange * 1.1f;

                stickyWeapon.SetScopeCamera(_newCamera);

                camGameObject.transform.SetParent(stickyWeapon.transform, false);
                camGameObject.transform.localPosition = stickyWeapon.RelativeFirePosition;

                Undo.CollapseUndoOperations(undoGroup);

                // Should be non-scene objects but is required to force being set as dirty
                EditorUtility.SetDirty(stickyWeapon);

                GUIUtility.ExitGUI();
            }
            EditorGUILayout.PropertyField(scopeCameraProp, GUIContent.none);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                stickyWeapon.SetScopeCamera(stickyWeapon.ScopeCamera);
                serializedObject.Update();
            }
            #endregion

            #region Scope Renderer
            Renderer prevScopeRender = stickyWeapon.ScopeCameraRenderer;

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(scopeCameraRendererContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
            if (GUILayout.Button(gotoWeaponsFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { StickyEditorHelper.HighlightFolderInProjectWindow(StickySetup.demosWeaponsFolder, false, true); }
            EditorGUILayout.PropertyField(scopeCameraRendererProp, GUIContent.none);
            GUILayout.EndHorizontal();           
            if (EditorGUI.EndChangeCheck())
            {
                Renderer newRenderer = (Renderer)scopeCameraRendererProp.objectReferenceValue;

                if (prevScopeRender != null)
                {
                    // Potentially need to destroy old render texture.
                    stickyWeapon.SetScopeCameraRenderer(null);
                }

                serializedObject.ApplyModifiedProperties();
                stickyWeapon.SetScopeCameraRenderer(newRenderer);
                serializedObject.Update();
            }
            #endregion

            EditorGUILayout.PropertyField(isScopeAllowNPCProp, isScopeAllowNPCContent);
            EditorGUILayout.PropertyField(isScopeAutoOnProp, isScopeAutoOnContent);
        }

        /// <summary>
        /// Draw the spent cartridge settings for this weapon.
        /// These get ejected from the weapon when it fires.
        /// </summary>
        protected void DrawSpentCartridgeSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            StickyEditorHelper.DrawS3DFoldout(showSpentCartridgeSettingsInEditorProp, spentCartridgeSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showSpentCartridgeSettingsInEditorProp.boolValue)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(spentCartridgePrefabContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
                if (GUILayout.Button(gotoDynamicObjectsFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { StickyEditorHelper.HighlightFolderInProjectWindow(StickySetup.demosDynamicObjectFolder, false, true); }
                EditorGUILayout.PropertyField(spentCartridgePrefabProp, GUIContent.none);
                GUILayout.EndHorizontal();

                StickyEditorHelper.DrawHorizontalGap(2f);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(" "));
                // Find (select) in the scene
                SelectItemInSceneViewButton(ref isSpentCartridgePositionSelected, showWSCEPGizmosInSceneViewProp);
                // Toggle selection in scene view on/off
                StickyEditorHelper.DrawGizmosButton(showWSCEPGizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);
                GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(spentEjectPositionProp, spentEjectPositionContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(spentEjectDirectionProp, spentEjectDirectionContent);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyWeapon.SpentEjectDirection = spentEjectDirectionProp.vector3Value;
                    serializedObject.Update();
                }

                EditorGUILayout.PropertyField(spentEjectRotationProp, spentEjectRotationContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(spentEjectForceProp, spentEjectForceContent);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyWeapon.SpentEjectForce = spentEjectForceProp.floatValue;
                    serializedObject.Update();
                }
            }
        }

        /// <summary>
        /// Draw the list of Weapon Anim Sets in the inspector
        /// </summary>
        protected void DrawWeaponAnimSets()
        {
            StickyEditorHelper.DrawS3DFoldout(isWeaponAnimSetsExpandedProp, weaponAnimSetsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (isWeaponAnimSetsExpandedProp.boolValue)
            {
                #region Check if s3dAnimActionList is null
                // Checking the property for being NULL doesn't check if the list is actually null.
                if (stickyWeapon.WeaponAnimSetList == null)
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    stickyWeapon.WeaponAnimSetList = new List<S3DWeaponAnimSet>(4);
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();
                }
                #endregion

                StickyEditorHelper.DrawInformationLabel(weaponAnimSetsInfoContent);

                #region Add-Remove Weapon Anim Sets

                int numWpnAnimSets = s3dWpnASetListProp.arraySize;

                // Reset button variables
                s3dWeaponAnimSetMoveDownPos = -1;
                s3dWeaponAnimSetDeletePos = -1;

                GUILayout.BeginHorizontal();

                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("Weapon Anim Sets: " + numWpnAnimSets.ToString("00"), labelFieldRichText);

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(stickyWeapon, "Add Weapon Anim Set");
                    // Add an empty slot
                    stickyWeapon.WeaponAnimSetList.Add(null);
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();

                    numWpnAnimSets = s3dWpnASetListProp.arraySize;
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numWpnAnimSets > 0) { s3dWeaponAnimSetDeletePos = s3dWpnASetListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();

                #endregion

                #region Weapon Anim Sets List
                numWpnAnimSets = s3dWpnASetListProp.arraySize;

                for (int wasIdx = 0; wasIdx < numWpnAnimSets; wasIdx++)
                {
                    s3dWeaponAnimSetProp = s3dWpnASetListProp.GetArrayElementAtIndex(wasIdx);

                    EditorGUILayout.BeginHorizontal();

                    GUIContent tempContent = new GUIContent(weaponAnimSetContent);
                    tempContent.text += (wasIdx + 1).ToString(" 00");
                    EditorGUILayout.LabelField(tempContent, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.PropertyField(s3dWeaponAnimSetProp, GUIContent.none);

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numWpnAnimSets > 1) { s3dWeaponAnimSetMoveDownPos = wasIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dWeaponAnimSetDeletePos = wasIdx; }

                    EditorGUILayout.EndHorizontal();
                }

                #endregion

                #region Move/Delete Weapon Anim Sets

                if (s3dWeaponAnimSetDeletePos >= 0 || s3dWeaponAnimSetMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);
                    // Don't permit multiple operations in the same pass
                    if (s3dWeaponAnimSetMoveDownPos >= 0)
                    {
                        // Move down one position, or wrap round to start of list
                        if (s3dWeaponAnimSetMoveDownPos < s3dWpnASetListProp.arraySize - 1)
                        {
                            s3dWpnASetListProp.MoveArrayElement(s3dWeaponAnimSetMoveDownPos, s3dWeaponAnimSetMoveDownPos + 1);
                        }
                        else { s3dWpnASetListProp.MoveArrayElement(s3dWeaponAnimSetMoveDownPos, 0); }

                        s3dWeaponAnimSetMoveDownPos = -1;
                    }
                    else if (s3dWeaponAnimSetDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                        int _deleteIndex = s3dWeaponAnimSetDeletePos;

                        if (EditorUtility.DisplayDialog("Delete Weapon Anim Set " + (s3dWeaponAnimSetDeletePos + 1) + "?", "Weapon Anim Set " + (s3dWeaponAnimSetDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the Weapon Anim Set from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            s3dWpnASetListProp.DeleteArrayElementAtIndex(_deleteIndex);
                            s3dWeaponAnimSetDeletePos = -1;
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                    // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    GUIUtility.ExitGUI();
                }

                #endregion

            }
        }

        /// <summary>
        /// Expand (show) or collapse (hide) all items in a list in the editor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="componentList"></param>
        /// <param name="isExpanded"></param>
        protected void ExpandList<T>(List<T> componentList, bool isExpanded)
        {
            int numComponents = componentList == null ? 0 : componentList.Count;

            if (numComponents > 0)
            {
                System.Type compType = typeof(T);

                if (compType == typeof(S3DAnimAction))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as S3DAnimAction).showInEditor = isExpanded;
                    }
                }
            }
        }

        /// <summary>
        /// Is this a beam weapon?
        /// </summary>
        /// <returns></returns>
        protected bool IsBeamWeapon()
        {
            return weaponTypeProp.intValue == StickyWeapon.BeamStandardInt;
        }

        /// <summary>
        /// Is this a Custom "weapon" action? If so, it will get it's value from code.
        /// </summary>
        /// <param name="weaponActionProp"></param>
        /// <returns></returns>
        protected bool IsCustomAction(SerializedProperty weaponActionProp)
        {
            return weaponActionProp.intValue == (int)S3DAnimAction.WeaponAction.Custom;
        }

        /// <summary>
        /// This lets us modify and display things in the scene view
        /// </summary>
        /// <param name="sv"></param>
        protected override void SceneGUI (SceneView sv)
        {
            if (stickyWeapon != null && stickyWeapon.gameObject.activeInHierarchy)
            {
                isSceneDirtyRequired = false;

                // IMPORTANT: Do not use transform.TransformPoint or InverseTransformPoint because they won't work correctly
                // when the parent gameobject has scale not equal to 1,1,1.

                // Get the rotation of the interactive object in the scene
                sceneViewInteractiveRot = Quaternion.LookRotation(stickyInteractive.transform.forward, stickyInteractive.transform.up);

                DrawSceneViewEquipPoint();

                if (isEquipPointSelected)
                {
                    isHandHold1Selected = false;
                    isHandHold2Selected = false;
                    isSocketPointSelected = false;
                    isSpentCartridgePositionSelected = false;
                    isFirePositionSelected = false;
                }

                DrawSceneViewSocketPoint();

                if (isSocketPointSelected)
                {
                    isHandHold1Selected = false;
                    isHandHold2Selected = false;
                    isEquipPointSelected = false;
                    isSpentCartridgePositionSelected = false;
                    isFirePositionSelected = false;
                }

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

                        // If selected, unselect the other gizmos for this component
                        if (isHandHold1Selected)
                        {
                            isHandHold2Selected = false;
                            isFirePositionSelected = false;
                            isSpentCartridgePositionSelected = false;
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

                        // If selected, unselect the other gizmos for this component
                        if (isHandHold2Selected)
                        {
                            isHandHold1Selected = false;
                            isFirePositionSelected = false;
                            isSpentCartridgePositionSelected = false;
                            isEquipPointSelected = false;
                            isSocketPointSelected = false;
                        }
                    }
                }

                #endregion

                #region Fire Points

                if (stickyWeapon.ShowWFPGizmosInSceneView)
                {
                    componentHandlePosition = stickyWeapon.GetWorldFireBasePosition();

                    // Get component handle rotation
                    componentHandleRotation = Quaternion.LookRotation(stickyWeapon.GetWorldFireDirection(), stickyWeapon.transform.up);

                    // Use a fixed size rather than one that changes with scene view camera distance
                    relativeHandleSize = 0.08f;

                    fadedGizmoColour = firePointGizmoColour;

                    // If the fire position is not selected, show it a little more transparent
                    if (!isFirePositionSelected)
                    {
                        fadedGizmoColour.a *= 0.65f;
                        if (fadedGizmoColour.a < 0.1f) { fadedGizmoColour.a = firePointGizmoColour.a; }
                    }

                    // Draw points in the scene that is non-interactable
                    if (Event.current.type == EventType.Repaint)
                    {
                        using (new Handles.DrawingScope(fadedGizmoColour))
                        {
                            // Draw the direction of fire at the fire base position
                            Handles.ArrowHandleCap(0, componentHandlePosition, componentHandleRotation, relativeHandleSize, EventType.Repaint);

                            int numFirePositionOffsets = stickyWeapon.NumberFirePositionOffsets;

                            // Draw non-interactable fire positions
                            if (numFirePositionOffsets == 0)
                            {
                                // No offsets, so draw one at the end of the barrel
                                Handles.SphereHandleCap(0, componentHandlePosition, componentHandleRotation, relativeHandleSize * 0.1f, EventType.Repaint);
                            }

                            for (int fpoIdx = 0; fpoIdx < numFirePositionOffsets; fpoIdx++)
                            {
                                Handles.SphereHandleCap(0, stickyWeapon.GetWorldFirePosition(componentHandlePosition, fpoIdx), componentHandleRotation, relativeHandleSize * 0.1f, EventType.Repaint);
                            }
                        }
                    }

                    ModifyRelativePosition(componentHandlePosition, componentHandleRotation, firePointGizmoColour, ref isFirePositionSelected);

                    // Draw the selectable point in the scene
                    using (new Handles.DrawingScope(fadedGizmoColour))
                    {
                        SceneViewSelectButton(componentHandlePosition, 0.5f * relativeHandleSize, ref isFirePositionSelected);

                        // If selected, unselect the other gizmos for this component
                        if (isFirePositionSelected)
                        {
                            isHandHold1Selected = false;
                            isHandHold2Selected = false;
                            isSpentCartridgePositionSelected = false;
                            isEquipPointSelected = false;
                            isSocketPointSelected = false;
                        }
                    }
                }

                #endregion

                #region Spent Cartridge Ejection Point
                if (stickyWeapon.ShowWSCEPGizmosInSceneView)
                {
                    componentHandlePosition = stickyWeapon.GetWorldSpentEjectPosition();

                    // Get component handle rotation
                    componentHandleRotation = Quaternion.LookRotation(stickyWeapon.GetWorldSpentEjectDirection(), stickyWeapon.transform.up);

                    // Use a fixed size rather than one that changes with scene view camera distance
                    relativeHandleSize = 0.08f;

                    fadedGizmoColour = spentEjectGizmoColour;

                    // If the spent eject position is not selected, show it a little more transparent
                    if (!isSpentCartridgePositionSelected)
                    {
                        fadedGizmoColour.a *= 0.65f;
                        if (fadedGizmoColour.a < 0.1f) { fadedGizmoColour.a = spentEjectGizmoColour.a; }
                    }

                    // Draw point in the scene that is non-interactable
                    if (Event.current.type == EventType.Repaint)
                    {
                        using (new Handles.DrawingScope(fadedGizmoColour))
                        {
                            // Draw the eject direction at the eject position
                            Handles.ArrowHandleCap(0, componentHandlePosition, componentHandleRotation, relativeHandleSize, EventType.Repaint);

                            // Draw the eject position
                            Handles.SphereHandleCap(0, componentHandlePosition, componentHandleRotation, relativeHandleSize * 0.1f, EventType.Repaint);
                        }
                    }

                    ModifySpentEjectPosition(componentHandlePosition, componentHandleRotation, spentEjectGizmoColour, ref isSpentCartridgePositionSelected);

                    // Draw the selectable point in the scene
                    using (new Handles.DrawingScope(fadedGizmoColour))
                    {
                        SceneViewSelectButton(componentHandlePosition, 0.5f * relativeHandleSize, ref isSpentCartridgePositionSelected);

                        // If selected, unselect the other gizmos for this component
                        if (isSpentCartridgePositionSelected)
                        {
                            isHandHold1Selected = false;
                            isHandHold2Selected = false;
                            isFirePositionSelected = false;
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

        /// <summary>
        /// Allow the user to modify the relative position and rotation in the scene view using gizmos.
        /// </summary>
        /// <param name="handlePos"></param>
        /// <param name="handleRot"></param>
        /// <param name="gizmoColour"></param>
        /// <param name="isPositionSelected"></param>
        private void ModifyRelativePosition(Vector3 handlePos, Quaternion handleRot, Color gizmoColour, ref bool isPositionSelected)
        {
            using (new Handles.DrawingScope(gizmoColour))
            {
                if (isPositionSelected)
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
                            Undo.RecordObject(stickyWeapon, "Rotate Point");

                            stickyWeapon.SetFireDirection(Quaternion.Inverse(stickyInteractive.transform.rotation) * handleRot * Vector3.forward); 
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
                            Undo.RecordObject(stickyWeapon, "Move Position");
                            stickyWeapon.RelativeFirePosition = S3DUtils.GetLocalSpacePosition(stickyInteractive.transform, handlePos);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allow the user to modify the spent cartridge eject position and rotation in the scene view using gizmos.
        /// </summary>
        /// <param name="handlePos"></param>
        /// <param name="handleRot"></param>
        /// <param name="gizmoColour"></param>
        /// <param name="isPositionSelected"></param>
        private void ModifySpentEjectPosition(Vector3 handlePos, Quaternion handleRot, Color gizmoColour, ref bool isPositionSelected)
        {
            using (new Handles.DrawingScope(gizmoColour))
            {
                if (isPositionSelected)
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
                            Undo.RecordObject(stickyWeapon, "Rotate Point");

                            stickyWeapon.SetSpentEjectDirection(Quaternion.Inverse(stickyInteractive.transform.rotation) * handleRot * Vector3.forward); 
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
                            Undo.RecordObject(stickyWeapon, "Move Position");
                            stickyWeapon.SetSpentEjectPosition(S3DUtils.GetLocalSpacePosition(stickyInteractive.transform, handlePos));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fetch the current paramaters from the weapon animator
        /// </summary>
        protected void RefreshAnimatorParameters()
        {
            if (animParamsBoolList == null) { animParamsBoolList = new List<S3DAnimParm>(10); }
            else { animParamsBoolList.Clear(); }
            if (animParamsTriggerList == null) { animParamsTriggerList = new List<S3DAnimParm>(10); }
            else { animParamsTriggerList.Clear(); }
            if (animParamsFloatList == null) { animParamsFloatList = new List<S3DAnimParm>(10); }
            else { animParamsFloatList.Clear(); }
            if (animParamsIntegerList == null) { animParamsIntegerList = new List<S3DAnimParm>(10); }
            else { animParamsIntegerList.Clear(); }

            if (stickyWeapon.DefaultAnimator != null)
            {
                // Synch the list of parameters from the animator controller with the array of names used in the editor

                // Populate the lists from the animator
                S3DAnimParm.GetParameterList(stickyWeapon.DefaultAnimator, animParamsBoolList, S3DAnimAction.ParameterType.Bool);
                S3DAnimParm.GetParameterList(stickyWeapon.DefaultAnimator, animParamsTriggerList, S3DAnimAction.ParameterType.Trigger);
                S3DAnimParm.GetParameterList(stickyWeapon.DefaultAnimator, animParamsFloatList, S3DAnimAction.ParameterType.Float);
                S3DAnimParm.GetParameterList(stickyWeapon.DefaultAnimator, animParamsIntegerList, S3DAnimAction.ParameterType.Integer);

                // Get arrays of names for use in the popup controls
                animParamBoolNames = S3DAnimParm.GetParameterNames(animParamsBoolList);
                animParamTriggerNames = S3DAnimParm.GetParameterNames(animParamsTriggerList);
                animParamFloatNames = S3DAnimParm.GetParameterNames(animParamsFloatList);
                animParamIntegerNames = S3DAnimParm.GetParameterNames(animParamsIntegerList);
            }
            else
            {
                animParamBoolNames = null;
                animParamTriggerNames = null;
                animParamFloatNames = null;
                animParamIntegerNames = null;
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

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Toggle all gizmos used by this weapon on/off.
        /// Includes interactive gizmos.
        /// </summary>
        protected virtual void ToggleWeaponGizmos()
        {
            // Get the gizmo status and flip it.
            bool isShown = !showWFPGizmosInSceneViewProp.boolValue;

            // Set to the same
            ShowHandHold1Gizmos(isShown);
            ShowHandHold2Gizmos(isShown);

            showWFPGizmosInSceneViewProp.boolValue = isShown;
            showWSCEPGizmosInSceneViewProp.boolValue = isShown;
            showEQPGizmosInSceneViewProp.boolValue = isShown;
            showSOCPGizmosInSceneViewProp.boolValue = isShown;
        }

        #endregion

        #region OnInspectorGUI

        protected override void DrawBaseInspector()
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
         
            // Draw the toolbar using the tabTexts from this editor class (rather than the base class).
            DrawToolBar(tabTexts);        

            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.BeginVertical("HelpBox");

            #region Interactive Settings

            if (selectedTabIntProp.intValue == 0)
            {
                EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartWPContent);

                DrawActivable();
                DrawEquippable();
                DrawGrabbable();
                DrawSocketable();
                DrawStashable();

                StickyEditorHelper.DrawUILine(separatorColor, 2);
                DrawHandHoldSettings();

                StickyEditorHelper.DrawUILine(separatorColor, 2);
                DrawPopupSettings();

                StickyEditorHelper.DrawUILine(separatorColor, 2);
                DrawTagSettings();

                DrawGravitySettings();
            }
            #endregion

            #region Weapon Settings
            else if (selectedTabIntProp.intValue == 1)
            {
                #region Toggle Gizmos and change Weapon Type
                EditorGUILayout.BeginHorizontal();

                //buttonCompact.contentOffset = new Vector2(buttonCompact.contentOffset.x, buttonCompact.contentOffset.y + 1);
                if (GUILayout.Button(StickyEditorHelper.gizmoBtnContent, buttonCompact, GUILayout.Width(20f), GUILayout.Height(19f)))
                {
                    ToggleWeaponGizmos();
                }
                //buttonCompact.contentOffset = new Vector2(buttonCompact.contentOffset.x, buttonCompact.contentOffset.y - 1);

                // This shouldn't be changed at runtime after initialised
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField(weaponTypeContent, GUILayout.Width(defaultEditorLabelWidth -25f));
                EditorGUILayout.PropertyField(weaponTypeProp, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    // If the weapon has changed from Projectile to Beam or vis versa, clear the old prefab and set defaults.
                    if (weaponTypeProp.intValue == StickyWeapon.BeamStandardInt)
                    {
                        projectilePrefabProp.objectReferenceValue = null;

                        // Currently beam weapons don't use magazines
                        isMagRequired1Prop.boolValue = false;
                        isMagRequired2Prop.boolValue = false;
                    }
                    else if (weaponTypeProp.intValue == StickyWeapon.ProjectileStandardInt)
                    {
                        beamPrefabProp.objectReferenceValue = null;
                    }
                    else
                    {
                        // Projectile Raycast doesn't require beam prefab
                        // It uses the projectile prefab to get some info like hit FX etc.
                        beamPrefabProp.objectReferenceValue = null;
                    }
                }
                EditorGUILayout.EndHorizontal();
                #endregion

                #region General Settings
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
                StickyEditorHelper.DrawS3DFoldout(showGeneralSettingsInEditorProp,generalSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (showGeneralSettingsInEditorProp.boolValue)
                {
                    //StickyEditorHelper.DrawHorizontalGap(2f);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(" "));
                    // Find (select) in the scene
                    SelectItemInSceneViewButton(ref isFirePositionSelected, showWFPGizmosInSceneViewProp);
                    // Toggle selection in scene view on/off
                    StickyEditorHelper.DrawGizmosButton(showWFPGizmosInSceneViewProp, toggleCompactButtonStyleNormal, toggleCompactButtonStyleToggled);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(fireDirectionContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
                    if (GUILayout.Button(StickyEditorHelper.btnResetContent, buttonCompact, GUILayout.Width(20f)))
                    {
                        fireDirectionProp.vector3Value = Vector3.forward;
                    }
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(fireDirectionProp, GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        stickyWeapon.FireDirection = fireDirectionProp.vector3Value;
                        serializedObject.Update();
                    }
                    GUILayout.EndHorizontal();

                    StickyEditorHelper.DrawHorizontalGap(2f);
                    EditorGUILayout.PropertyField(relativeFirePositionProp, relativeFirePositionContent);
                    StickyEditorHelper.DrawArray(firePositionOffsetsProp, firePositionOffsetsContent, defaultEditorLabelWidth, "Pos");

                    StickyEditorHelper.DrawHorizontalGap(2f);

                    #region Beam Standard
                    if (weaponTypeProp.intValue == StickyWeapon.BeamStandardInt)
                    {
                        DrawBeamPrefab();

                        EditorGUILayout.PropertyField(maxRangeProp, maxRangeContent);

                        // It only makes sense for the fire type to be always FullAuto for beam weapons.
                        DrawFireButton1();
                        DrawFireInterval1();
                        DrawIsOnlyFireWhenAiming(1);


                        DrawFireButton2();
                        DrawFireInterval2();
                        DrawIsOnlyFireWhenAiming(2);

                        // Currently beams auto-recharge while projectile weapons use ammunition
                        EditorGUILayout.PropertyField(chargeAmountProp, chargeAmountContent);
                        EditorGUILayout.PropertyField(rechargeTimeProp, rechargeTimeContent);
                    }
                    #endregion

                    #region Projectile Standard
                    else if (weaponTypeProp.intValue == StickyWeapon.ProjectileStandardInt)
                    {
                        DrawProjectilePrefab();

                        // Max range is used while aiming
                        EditorGUILayout.PropertyField(maxRangeProp, maxRangeContent);

                        DrawFireButton1();
                        DrawFireType1();
                        DrawFireInterval1();
                        DrawIsOnlyFireWhenAiming(1);

                        DrawFireButton2();
                        DrawFireType2();
                        DrawFireInterval2();
                        DrawIsOnlyFireWhenAiming(2);
                    }
                    #endregion

                    #region Projectile Raycast
                    else if (weaponTypeProp.intValue == StickyWeapon.ProjectileRayCastInt)
                    {
                        DrawProjectilePrefab();

                        EditorGUILayout.PropertyField(maxRangeProp, maxRangeContent);

                        DrawFireButton1();
                        DrawFireType1();
                        DrawFireInterval1();
                        DrawIsOnlyFireWhenAiming(1);

                        DrawFireButton2();
                        DrawFireType2();
                        DrawFireInterval2();
                        DrawIsOnlyFireWhenAiming(2);
                    }
                    #endregion

                    DrawHitLayerMask();

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(isOnlyUseWhenHeldProp, isOnlyUseWhenHeldContent);
                    if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                    {
                        serializedObject.ApplyModifiedProperties();
                        stickyWeapon.SetOnlyUseWhenHeld(isOnlyUseWhenHeldProp.boolValue);
                        serializedObject.Update();
                    }
                }
                #endregion

                DrawAimingAndReticleSettings();
                DrawAmmoSettings();
                DrawAnimateSettings();
                DrawAttachmentSettings();
                DrawHealthSettings();
                DrawMuzzleFXSettings();
                DrawRecoilSettings();
                DrawSmokeSettings();
                DrawSpentCartridgeSettings();
            }
            #endregion

            #region Event Settings
            else
            {
                StickyEditorHelper.DrawHorizontalGap(2f);
                DrawBaseEvents();
            }
            #endregion

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            stickyWeapon.allowRepaint = true;

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawDebugToggle();
            if (isDebuggingEnabled && stickyWeapon != null)
            {
                Repaint();
                float rightLabelWidth = 175f;

                StickyEditorHelper.PerformanceImpact();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(StickyEditorHelper.debugIsInitialisedIndent1Content, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyWeapon.IsWeaponInitialised ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsFire1InputContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyWeapon.IsFire1Input ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsFire2InputContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyWeapon.IsFire2Input ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                DrawDebugHeldBy(10f, rightLabelWidth);
                DrawDebugSocketedOn(10f, rightLabelWidth);
                DrawDebugStashedBy(10f, rightLabelWidth);

                EditorGUI.BeginChangeCheck();
                isDebugAimPosition = EditorGUILayout.Toggle(debugIsAimPositionContent, isDebugAimPosition);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    if (isDebugAimPosition)
                    {
                        debugAimTargetGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        DestroyImmediate(debugAimTargetGO.GetComponent<Collider>());
                        debugAimTargetGO.transform.localScale *= 0.1f;
                    }
                    else
                    {
                        if (debugAimTargetGO != null) { DestroyImmediate(debugAimTargetGO); }
                    }
                }

                if (isDebugAimPosition && debugAimTargetGO != null)
                {
                    debugAimTargetGO.transform.position = stickyWeapon.AimAtPosition;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugAimPositionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(stickyWeapon.AimAtPosition, 1), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugNumWeaponAimSetsContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyWeapon.NumberOfWeaponAnimSets.ToString(), GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                DrawDebugReferenceFrame(rightLabelWidth);
            }
            EditorGUILayout.EndVertical();
            #endregion
        }

        #endregion
    }
}