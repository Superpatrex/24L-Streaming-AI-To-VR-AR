using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// A sticky weapon is an interactive-enabled object that can be grabbed, dropped, stashed and fired.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Objects/Sticky Weapon")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyWeapon : StickyInteractive, IStickyGravity
    {
        #region Enumerations

        /// <summary>
        /// An enumeration of weapon types. 0-99 are reserved for Sticky3D.
        /// </summary>
        public enum WeaponType
        {
            BeamStandard = 0,
            ProjectileRaycast = 20,
            ProjectileStandard = 21
        }

        public enum FiringButton
        {
            None = 0,
            Manual = 1,
            AutoFire = 2
        }

        /// <summary>
        /// Semi-Auto fires single shots but automatically loads the next projectile if there is available ammo.
        /// Full-Auto can continuously fire while there is available ammo.
        /// </summary>
        public enum FiringType
        {
            SemiAuto = 0,
            FullAuto = 1
        }

        /// <summary>
        /// The method used for reloading the weapon firing mechanism.
        /// ManualOnly - call Reload(..) only.
        /// AutoReserve - Get mags from Reserve (weapon doesn't need to be held by a character)
        /// AutoStash - Stash used mag when reloading. Get mags from character Stash (assume using StickyMagazines). Checks first if holding a mag in a hand.
        /// AutoStashWithDrop - Drop used mag when reloading. Get mags from character Stash (assume using StickyMagazines). Checks first if holding a mag in a hand.
        /// </summary>
        public enum ReloadType
        {
            ManualOnly = 0,
            AutoReserve = 10,
            AutoStash = 20,
            AutoStashWithDrop = 25
        }

        #endregion

        #region Public Variables

        /// <summary>
        /// The prefab for the beam to be fired by this weapon.
        /// If you modify this, call ReinitialiseWeapon()
        /// </summary>
        public StickyBeamModule beamPrefab;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// The ID number for this weapon's beam prefab (as assigned by the StickyManager in the scene).
        /// </summary>
        public int beamPrefabID;

        /// <summary>
        /// The prefab for the projectile fired by this weapon.
        /// If you modify this, call ReinitialiseWeapon()
        /// </summary>
        public StickyProjectileModule projectilePrefab;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// The ID number for this weapon's projectile prefab template (as assigned by the StickyManager in the scene).
        /// </summary>
        public int projectilePrefabID;

        /// <summary>
        /// The dynamic object prefab for the spent or empty cartridge that is ejected from the weapon after it fires.
        /// If you modify this, call ReinitialiseWeapon()
        /// </summary>
        public StickyDynamicModule spentCartridgePrefab;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// The ID number for this weapon's cartridge dyanmic object prefab (as assigned by the StickyManager in the scene).
        /// </summary>
        public int spentCartridgePrefabID;

        #endregion

        #region Public Properties - General

        /// <summary>
        /// Get where the weapon is currently aiming
        /// </summary>
        public Vector3 AimAtPosition { get { return aimAtPosition; } }

        /// <summary>
        /// Get or Set the weapon ChargeAmount
        /// </summary>
        public float ChargeAmount { get { return chargeAmount; } set { SetChargeAmount(value); } }

        /// <summary>
        /// Get or Set if the line of sight is checked before firing primary mechanism. Only relevant if it is AutoFire.
        /// </summary>
        public bool CheckLineOfSight1 { get { return checkLineOfSight1; } set { SetCheckLineOfSight1(value); } }

        /// <summary>
        /// Get or Set if the line of sight is checked before firing primary mechanism. Only relevant if it is AutoFire.
        /// </summary>
        public bool CheckLineOfSight2 { get { return checkLineOfSight2; } set { SetCheckLineOfSight2(value); } }

        /// <summary>
        /// The estimated range (in metres) of the weapon
        /// </summary>
        public float EstimatedRange { get; internal set; }

        /// <summary>
        /// Get or set the local space normalised fire direction of the weapon
        /// </summary>
        public Vector3 FireDirection { get { return fireDirection; } set { SetFireDirection(value); } }

        /// <summary>
        /// Get or set the main trigger for the weapon to fire e.g. None, Manual, AutoFire.
        /// </summary>
        public FiringButton FiringButton1 { get { return firingButton1; } set { SetFiringButton1(value); } }

        /// <summary>
        /// Get or set the second trigger for the weapon to fire e.g. None, Manual, AutoFire.
        /// </summary>
        public FiringButton FiringButton2 { get { return firingButton2; } set { SetFiringButton2(value); } }

        /// <summary>
        /// Get or set the weapon power up time.
        /// </summary>
        public float FireInterval { get { return fireInterval1; } set { SetFireInterval(value); } }

        /// <summary>
        /// Get or set the main firing type for the weapon. e.g. SemiAuto or FullAuto.
        /// </summary>
        public FiringType FiringType1 { get { return firingType1; } set { SetFiringType1(value); } }

        /// <summary>
        /// Get or set the secondary firing type for the weapon. e.g. SemiAuto or FullAuto.
        /// </summary>
        public FiringType FiringType2 { get { return firingType2; } set { SetFiringType2(value); } }

        /// <summary>
        /// Has the weapon primary mechanism attempted to fire with no ammo since the last fixed update?
        /// </summary>
        public bool HasEmptyFired1 { get { return hasEmptyFired1; } }

        /// <summary>
        /// Has the weapon secondary mechanism attempted to fire with no ammo since the last fixed update?
        /// </summary>
        public bool HasEmptyFired2 { get { return hasEmptyFired2; } }

        /// <summary>
        /// Has the weapon fired the primary mechanism since the last fixed update?
        /// </summary>
        public bool HasFired1 { get { return hasFired1; } }

        /// <summary>
        /// Has the weapon fired the secondary mechanism since the last fixed update?
        /// </summary>
        public bool HasFired2 { get { return hasFired2; } }

        /// <summary>
        /// Has the weapon started reloading the primary mechanism since the last fixed update?
        /// </summary>
        public bool HasReloadStarted1 { get { return hasReloadStarted1; } }

        /// <summary>
        /// Has the weapon started reloading the secondary mechanism since the last fixed update?
        /// </summary>
        public bool HasReloadStarted2 { get { return hasReloadStarted2; } }

        /// <summary>
        /// Has the weapon finished reloading the primary mechanism since the last fixed update?
        /// </summary>
        public bool HasReloadFinished1 { get { return hasReloadFinished1; } }

        /// <summary>
        /// Has the weapon finished reloading the finished mechanism since the last fixed update?
        /// </summary>
        public bool HasReloadFinished2 { get { return hasReloadFinished2; } }

        /// <summary>
        /// Returns whether the weapon primary mechanism has line-of-sight to the target. NOTE: This does not calculate line-of-sight, it
        /// merely returns the last calculated value. To calculate line-of-sight for a weapon, call WeaponHasLineOfSight(1).
        /// </summary>
        public bool HasLineOfSight1 { get; private set; }

        /// <summary>
        /// Returns whether the weapon primary mechanism has line-of-sight to the target. NOTE: This does not calculate line-of-sight, it
        /// merely returns the last calculated value. To calculate line-of-sight for a weapon, call WeaponHasLineOfSight(2).
        /// </summary>
        public bool HasLineOfSight2 { get; private set; }

        /// <summary>
        /// Get or set the hit layer mask. When fired, the weapon can hit any objects in these Unity Layers.
        /// </summary>
        public LayerMask HitLayerMask { get { return hitLayerMask; } set { hitLayerMask = value; } }

        /// <summary>
        /// Get or set the weapon maximum range
        /// </summary>
        public float MaxRange { get { return maxRange; } set { SetMaxRange(value); } }

        /// <summary>
        /// [READ ONLY] Is the weapon one that can fire beams, rays or lasers?
        /// </summary>
        public bool IsBeamWeapon { get { return isWeaponInitialised ? weaponTypeInt == BeamStandardInt : weaponType == WeaponType.BeamStandard; } }

        /// <summary>
        /// [READONLY] Is the weapon being instructed to fire button 1?
        /// </summary>
        public bool IsFire1Input { get { return isFire1Input; } }

        /// <summary>
        /// [READONLY] Is the fire1 button being held down?
        /// </summary>
        public bool IsFire1InputHeld { get { return isFire1InputHeld; } }

        /// <summary>
        /// [READONLY] Is the weapon being instructed to fire button 2?
        /// </summary>
        public bool IsFire2Input { get { return isFire2Input; } }

        /// <summary>
        /// [READONLY] Is the fire2 button being held down?
        /// </summary>
        public bool IsFire2InputHeld { get { return isFire2InputHeld; } }

        /// <summary>
        /// Get or set if weapon can only fire, reload, or be animated, when it is held by a character
        /// </summary>
        public bool IsOnlyUseWhenHeld { get { return isOnlyUseWhenHeld; } set { SetOnlyUseWhenHeld(value); } }

        /// <summary>
        /// [READ ONLY] Is the weapon one that can fire a Raycast projectile?
        /// </summary>
        public bool IsProjectileRaycastWeapon { get { return isWeaponInitialised ? weaponTypeInt == ProjectileRayCastInt : weaponType == WeaponType.ProjectileRaycast; } }

        /// <summary>
        /// [READ ONLY] Is the weapon one that can fire a Standard projectile?
        /// </summary>
        public bool IsProjectileStandardWeapon { get { return isWeaponInitialised ? weaponTypeInt == ProjectileStandardInt : weaponType == WeaponType.ProjectileStandard; } }

        /// <summary>
        /// [READ ONLY] Is the weapon one that can fire a (Standard or Raycast) projectile?
        /// </summary>
        public bool IsProjectileWeapon { get { return isWeaponInitialised ? weaponTypeInt == ProjectileStandardInt || weaponTypeInt == ProjectileRayCastInt : weaponType == WeaponType.ProjectileStandard || weaponType == WeaponType.ProjectileRaycast; } }

        /// <summary>
        /// Is this an interactive-enabled StickyWeapon?
        /// </summary>
        public override bool IsStickyWeapon { get { return true; } }

        /// <summary>
        /// Get or set if the weapon is enabled
        /// </summary>
        public bool IsWeaponEnabled { get { return isWeaponEnabled; } set { EnableOrDisableWeapon(value); } }

        /// <summary>
        /// Has the weapon been initialised?
        /// </summary>
        public bool IsWeaponInitialised { get { return isWeaponInitialised; } }

        /// <summary>
        /// Get or Set if the weapon is paused.
        /// </summary>
        public bool IsWeaponPaused { get { return isWeaponPaused; } set { PauseOrUnPauseWeapon(value); } }

        /// <summary>
        /// Get or set the array of StickyEffectsModules used for Muzzle FX.
        /// </summary>
        public StickyEffectsModule[] MuzzleEffects1 { get { return muzzleEffects1; } set { SetMuzzleEffects1(value); } }

        /// <summary>
        /// Get the number of local space fire position offsets from the relative fire position.
        /// There may be 0 or more. See also RelativeFirePosition.
        /// </summary>
        public int NumberFirePositionOffsets { get { return isWeaponInitialised ? numFirePositionOffsets : firePositionOffsets == null ? 0 : firePositionOffsets.Length; } }

        /// <summary>
        /// Get the number of (unvalidated) StickyEffectsModules for the muzzle FX
        /// </summary>
        public int NumberMuzzleEffects1 { get { return isWeaponInitialised ? numMuzzleEffects1 : muzzleEffects1 == null ? 0 : muzzleEffects1.Length; } }

        /// <summary>
        /// Get the number of StickyEffectsModules available for the muzzle FX
        /// </summary>
        public int NumberMuzzleEffects1Valid { get { return numMuzzleEffects1Valid; } internal set { numMuzzleEffects1Valid = value; } }

        /// <summary>
        /// Get or set the weapon recharge time.
        /// </summary>
        public float RechargeTime { get { return rechargeTime; } set { SetRechargeTime(value); } }

        /// <summary>
        /// Get or set the local space relative fire position which should be aligned to the end of the barrel. 
        /// </summary>
        public Vector3 RelativeFirePosition { get { return relativeFirePosition; } set { relativeFirePosition = value; } }

        /// <summary>
        /// Get or set the local space direction the spent cartridge is ejected
        /// </summary>
        public Vector3 SpentEjectDirection { get { return spentEjectDirection; } set { SetSpentEjectDirection(value); } }

        /// <summary>
        /// Get or set the force used to eject the spent cartridge from the weapon in the Spent Eject Direction.
        /// </summary>
        public float SpentEjectForce { get { return spentEjectForce; } set { SetSpentEjectForce(value); } }

        /// <summary>
        /// Get or set the local space position on the weapon from which the spent cartridge is ejected.
        /// </summary>
        public Vector3 SpentEjectPosition { get { return spentEjectPosition; } set { SetSpentEjectPosition(value); } }

        /// <summary>
        /// Get or set the local space rotation, in Euler angles, of the spent cartridge when ejected.
        /// </summary>
        public Vector3 SpentEjectRotation { get { return spentEjectRotation; } set { SetSpentEjectRotation(value); } }

        #endregion

        #region Public Properties - Aiming

        /// <summary>
        /// Get or set the character first person camera field of view when aiming a held weapon
        /// </summary>
        public float AimingFirstPersonFOV { get { return aimingFirstPersonFOV; } set { SetAimingFirstPersonFOV(value); } }

        /// <summary>
        /// Get or set the rate at which the transition between aiming and not aiming will progress
        /// </summary>
        public float AimingSpeed { get { return aimingSpeed; } set { aimingSpeed = value; } }

        /// <summary>
        /// Get or set the reticle (sprite) used when the weapon is held by a S3D player and aiming
        /// </summary>
        public Sprite AimingReticleSprite { get { return aimingReticleSprite; } set { SetAimingReticleSprite(value); } }

        /// <summary>
        /// Get or set the character third person camera field of view when aiming a held weapon
        /// </summary>
        public float AimingThirdPersonFOV { get { return aimingThirdPersonFOV; } set { SetAimingThirdPersonFOV(value); } }

        /// <summary>
        /// Get or set the reticle (sprite) used when the weapon is held by a S3D player
        /// </summary>
        public Sprite DefaultReticleSprite { get { return defaultReticleSprite; } set { SetDefaultReticleSprite(value); } }

        /// <summary>
        /// Get or set if the weapon is attempting to aim or face a target.
        /// </summary>
        public bool IsAiming { get { return isAiming; } set { EnableOrDisableAiming(value, true); } }

        /// <summary>
        /// Get or set if not to display a reticle on the active StickyDisplayModule when the weapon is held by a player
        /// </summary>
        public bool IsNoReticleIfHeld { get { return isNoReticleIfHeld; } set { isNoReticleIfHeld = value; } }

        /// <summary>
        /// Get or set if not to display a reticle on the active StickyDisplayModule when the weapon is held by a player while aiming
        /// with first-person camera
        /// </summary>
        public bool IsNoReticleOnAimingFPS { get { return isNoReticleOnAimingFPS; } set { isNoReticleOnAimingFPS = value; } }

        /// <summary>
        /// Get or set if not to display a reticle on the active StickyDisplayModule when the weapon is held by a player while aiming
        /// with third-person camera
        /// </summary>
        public bool IsNoReticleOnAimingTPS { get { return isNoReticleOnAimingTPS; } set { isNoReticleOnAimingTPS = value; } }

        /// <summary>
        /// Get or set if Fire button 1 can only fire when the weapon is being aimed
        /// </summary>
        public bool IsOnlyFireWhenAiming1 { get { return isOnlyFireWhenAiming1; } set { isOnlyFireWhenAiming1 = value; } }

        /// <summary>
        ///  Get or set if Fire button 2 can only fire when the weapon is being aimed
        /// </summary>
        public bool IsOnlyFireWhenAiming2 { get { return isOnlyFireWhenAiming2; } set { isOnlyFireWhenAiming2 = value; } }

        #endregion

        #region Public Properties - Ammo

        /// <summary>
        /// [READONLY]
        /// Get the (common) array of ammo types.
        /// </summary>
        public S3DAmmoTypes AmmoTypes { get { return ammoTypes; } }

        /// <summary>
        /// Get or set the ammunition currently loaded into the weapon for firing button 1
        /// -1 = Unlimited.
        /// </summary>
        public int Ammunition1 { get { return ammunition1; } set { SetAmmunition(1, value); } }

        /// <summary>
        /// Get or set the ammunition currently loaded into the weapon for firing button 2
        /// -1 = Unlimited.
        /// </summary>
        public int Ammunition2 { get { return ammunition2; } set { SetAmmunition(2, value); } }

        /// <summary>
        /// Get or set the zero-based index from the magTypes scriptable object for the main firing mechanism
        /// </summary>
        public int CompatibleMag1 { get { return compatibleMag1; } set { SetCompatibleMagType(1, value); } }

        /// <summary>
        /// Get or set the zero-based index from the magTypes scriptable object for the secondary firing mechanism
        /// </summary>
        public int CompatibleMag2 { get { return compatibleMag2; } set { SetCompatibleMagType(2, value); } }

        /// <summary>
        /// Is the primary button mechanism empty (is out of ammo)?
        /// </summary>
        public bool IsFireButton1Empty { get { return ammunition1 == 0; } }

        /// <summary>
        /// Is the secondary button mechanism empty (is out of ammo)?
        /// </summary>
        public bool IsFireButton2Empty { get { return ammunition2 == 0; } }

        /// <summary>
        /// Get or set if the primary firing mechanism requires a StickyMagazine to be attached before it can fire?
        /// </summary>
        public bool IsMagRequired1 { get { return isMagRequired1; } set { SetIsMagRequired(1, value); } }

        /// <summary>
        /// Get or set if the secondary firing mechanism requires a StickyMagazine to be attached before it can fire?
        /// </summary>
        public bool IsMagRequired2 { get { return isMagRequired2; } set { SetIsMagRequired(2, value); } }

        /// <summary>
        /// Is the weapon reloadable? Always returns false for Beam weapons or when not initialised.
        /// </summary>
        public bool IsReloadable { get { return isReloadable; } }

        /// <summary>
        /// Get or set the amount of ammo the the primary firing mechanism magazine (clip) can hold. Becomes read-only when IsMagRequired1 is true.
        /// It is automatically updated when a mag is retrieved from Stash.
        /// </summary>
        public int MagCapacity1 { get { return magCapacity1; } set { if (!isMagRequired1) { SetMagCapacity(value, 1); } } }

        /// <summary>
        /// Get or set the amount of ammo the the secondary firing mechanism magazine (clip) can hold. Becomes read-only when IsMagRequired2 is true.
        /// It is automatically updated when a mag is retrieved from Stash.
        /// </summary>
        public int MagCapacity2 { get { return magCapacity2; } set { if (!isMagRequired2) { SetMagCapacity(value, 2); } } }

        /// <summary>
        /// Get or set the number of additional magazines available for the primary firing mechanism.
        /// -1 = Unlimited.
        /// </summary>
        public int MagsInReserve1 { get { return magsInReserve1; } set { SetMagsInReserve(1, value); } }

        /// <summary>
        /// Get or set the number of additional magazines available for the secondary firing mechanism.
        /// -1 = Unlimited.
        /// </summary>
        public int MagsInReserve2 { get { return magsInReserve2; } set { SetMagsInReserve(2, value); } }

        /// <summary>
        /// [READONLY]
        /// Get the (common) array of mag types
        /// </summary>
        public S3DMagTypes MagTypes { get { return magTypes; } }

        /// <summary>
        /// Get or set the short delay between when the primary button mechanism fired, and it attempts to reload
        /// </summary>
        public float ReloadDelay1 { get { return reloadDelay1; } set { reloadDelay1 = value; } }

        /// <summary>
        /// Get or set the short delay between when the secondary button mechanism fired, and it attempts to reload
        /// </summary>
        public float ReloadDelay2 { get { return reloadDelay2; } set { reloadDelay2 = value; } }

        /// <summary>
        /// Get or set the delay during reloading, in seconds, between when the primary button mechanism unequips the old mag
        /// and starts to equip a new mag.
        /// </summary>
        public float ReloadEquipDelay1 { get { return reloadEquipDelay1; } set { reloadEquipDelay1 = value; } }

        /// <summary>
        /// Get or set the delay during reloading, in seconds, between when the secondary button mechanism unequips the old mag
        /// and starts to equip a new mag.
        /// </summary>
        public float ReloadEquipDelay2 { get { return reloadEquipDelay2; } set { reloadEquipDelay2 = value; } }

        /// <summary>
        /// Get or set the time it takes the primary button mechanism to reload.
        /// </summary>
        public float ReloadDuration1 { get { return reloadDuration1; } set { reloadDuration1 = value; } }

        /// <summary>
        /// Get or set the time it takes the secondary button mechanism to reload.
        /// </summary>
        public float ReloadDuration2 { get { return reloadDuration2; } set { reloadDuration2 = value; } }

        /// <summary>
        /// Get or set the method for reloading the primary button mechanism.
        /// </summary>
        public ReloadType ReloadType1 { get { return reloadType1; } set { SetReloadType(1, value); } }

        /// <summary>
        /// Get or set the method for reloading the secondary button mechanism.
        /// </summary>
        public ReloadType ReloadType2 { get { return reloadType2; } set { SetReloadType(2, value); } }

        /// <summary>
        /// Get or set the Sound FX for the primary button mechanism used when the weapon begins reloading.
        /// </summary>
        public StickyEffectsModule ReloadSoundFX1 { get { return reloadSoundFX1; } set { SetReloadSoundFX(1, value); } }

        /// <summary>
        /// Get or set the Sound FX for the secondary button mechanism used when the weapon begins reloading.
        /// </summary>
        public StickyEffectsModule ReloadSoundFX2 { get { return reloadSoundFX2; } set { SetReloadSoundFX(2, value); } }

        /// <summary>
        /// Get or set the Sound FX for the primary button mechanism used when the weapon equips a new mag during reloading.
        /// </summary>
        public StickyEffectsModule ReloadEquipSoundFX1 { get { return reloadEquipSoundFX1; } set { SetReloadEquipSoundFX(1, value); } }

        /// <summary>
        /// Get or set the Sound FX for the secondary button mechanism used when the weapon equips a new mag during reloading.
        /// </summary>
        public StickyEffectsModule ReloadEquipSoundFX2 { get { return reloadEquipSoundFX2; } set { SetReloadEquipSoundFX(2, value); } }

        #endregion

        #region Public Properties - Animate

        /// <summary>
        /// Get or set the list of animation actions for the weapon animator.
        /// </summary>
        public List<S3DAnimAction> AnimActionList { get { return s3dAnimActionList; } set { SetAnimActionsList(value); } }

        /// <summary>
        /// Get or set the animator for the weapon. This should be attached to, or a child of, the weapon gameobject
        /// </summary>
        public Animator DefaultAnimator { get { return defaultAnimator; } set { SetDefaultAnimator(value); } }

        /// <summary>
        /// [READONLY] Is animate enabled and ready to animate the weapon?
        /// </summary>
        public bool IsAnimateEnabled { get { return isAnimateEnabled; } }

        /// <summary>
        /// [READONLY] The number of Anim Actions configured on the Animate tab
        /// </summary>
        public int NumberOfAnimActions { get { return isWeaponInitialised ? numAnimateActions : s3dAnimActionList == null ? 0 : s3dAnimActionList.Count; } }

        /// <summary>
        /// [READONLY] The number of Weapon Anim Sets configured on the Animate tab
        /// </summary>
        public int NumberOfWeaponAnimSets { get { return isWeaponInitialised ? numWeaponAnimSets : s3dWeaponAnimSetList == null ? 0 : s3dWeaponAnimSetList.Count; } }

        /// <summary>
        /// Get or set the list of weapon anim sets for character model IDs that apply to this weapon
        /// </summary>
        public List<S3DWeaponAnimSet> WeaponAnimSetList { get { return s3dWeaponAnimSetList; } set { SetWeaponAnimSetList(value); } }

        #endregion

        #region Public Properties - Attachments

        /// <summary>
        /// [READONLY] Is the laser sight equipped and currently turned on?
        /// </summary>
        public bool IsLaserSightOn { get { return isLaserSightReady && isLaserSightOn; } }

        /// <summary>
        /// [READONLY] Is there a StickyMagazine currently equipped for the primary fire button?
        /// </summary>
        public bool IsMagEquipped1 { get { return isMagEquipped1 && equippedMag1 != null; } }

        /// <summary>
        /// [READONLY] Is there a StickyMagazine currently equipped for the secondary fire button?
        /// </summary>
        public bool IsMagEquipped2 { get { return isMagEquipped2 && equippedMag2 != null; } }

        /// <summary>
        /// [READONLY] Is the Scope currently turned on?
        /// </summary>
        public bool IsScopeOn { get { return isScopeOn; } }

        /// <summary>
        /// Get or set the child transform that determines where the laser sight aims from
        /// </summary>
        public Transform LaserSightAimFrom { get { return laserSightAimFrom; } set { SetLaserSightAimFrom(value); } }

        /// <summary>
        /// Get or set the laser sight beam colour.
        /// </summary>
        public Color32 LaserSightColour { get { return laserSightColour; } set { SetLaserSightColour(value); } }

        /// <summary>
        /// Get or set the child transform where the magazine attaches to the weapon for the primary fire button
        /// </summary>
        public Transform MagazineAttachPoint1 { get { return magAttachPoint1; } set { SetMagazineAttachPoint(value, 1); } }

        /// <summary>
        /// Get or set the child transform where the magazine attaches to the weapon for the secondary fire button
        /// </summary>
        public Transform MagazineAttachPoint2 { get { return magAttachPoint2; } set { SetMagazineAttachPoint(value, 2); } }

        /// <summary>
        /// Get or set the camera used to render the Scope display
        /// </summary>
        public Camera ScopeCamera { get { return scopeCamera; } set { SetScopeCamera(value); } }

        /// <summary>
        /// Get or set the (mesh) renderer to project the Scope Camera onto.
        /// </summary>
        public Renderer ScopeCameraRenderer { get { return scopeCameraRenderer; } set { SetScopeCameraRenderer(value); } }

        #endregion

        #region Public Properties - Health

        /// <summary>
        /// The current performance level of this weapon (determined by the Health and Heat). The performance level affects how
        /// efficient it is. At a performance level of one it operates normally. At a performance level of 0 it can do nothing.
        /// </summary>
        public float CurrentPerformance { get { return currentPerformance; } }

        /// <summary>
        /// The overall health of the weapon. Must be in the range 0 to 100.
        /// </summary>
        public float Health
        {
            get { return health * 100f; }
            set
            {
                if (value < 0f) { health = 0f; }
                else if (value > 100f) { health = 1f; }
                else { health = value / 100f; }
            }
        }

        /// <summary>
        /// Get or set the heat level of the weapon. 0.0 is not heat. 100.0 is overheated.
        /// </summary>
        public float HeatLevel { get { return heatLevel; } set { SetHeatLevel(value); } }

        /// <summary>
        /// Get or set if primary and secondary mechanisms are paused and cannot fire.
        /// </summary>
        public bool IsFiringPaused { get { return isFiringPaused; } set { SetIsFiringPaused(value); } }

        #endregion

        #region Public and Internal Only Properties
        #if UNITY_EDITOR
        public bool ShowWFPGizmosInSceneView { get { return showWFPGizmosInSceneView; } }
        public bool ShowWSCEPGizmosInSceneView { get { return showWSCEPGizmosInSceneView; } }
        #endif

        #endregion

        #region Public Static Variables
        public static readonly int BeamStandardInt = (int)WeaponType.BeamStandard;
        public static readonly int ProjectileStandardInt = (int)WeaponType.ProjectileStandard;
        public static readonly int ProjectileRayCastInt = (int)WeaponType.ProjectileRaycast;

        public static readonly int FiringButtonNoneInt = (int)FiringButton.None;
        public static readonly int FiringButtonManualInt = (int)FiringButton.Manual;
        public static readonly int FiringButtonAutoInt = (int)FiringButton.AutoFire;

        public static readonly int FiringTypeSemiAutoInt = (int)FiringType.SemiAuto;
        public static readonly int FiringTypeFullAutoInt = (int)FiringType.FullAuto;

        public static readonly int ReloadTypeManualOnlyInt = (int)ReloadType.ManualOnly;
        public static readonly int ReloadTypeAutoReserveInt = (int)ReloadType.AutoReserve;
        public static readonly int ReloadTypeAutoStashInt = (int)ReloadType.AutoStash;
        public static readonly int ReloadTypeAutoStashWithDropInt = (int)ReloadType.AutoStashWithDrop;
        #endregion

        #region Protected Variables - Serialized General

        /// <summary>
        /// The reticle (sprite) used when the weapon is held by a S3D player and aiming.
        /// If none, it will fall back to defaultReticleSprite isNoReticleOnAiming is true.
        /// </summary>
        [SerializeField] protected Sprite aimingReticleSprite = null;

        /// <summary>
        /// Reference to a common set of ammo types. All weapons and magazines should use the same scriptable object.
        /// </summary>
        [SerializeField] protected S3DAmmoTypes ammoTypes = null;

        /// <summary>
        /// The ammunition currently loaded into the weapon for firing button 1
        /// -1 = Unlimited.
        /// </summary>
        [SerializeField] protected int ammunition1 = -1;

        /// <summary>
        /// The ammunition currently loaded into the weapon for firing button 2
        /// -1 = Unlimited.
        /// </summary>
        [SerializeField] protected int ammunition2 = -1;

        /// <summary>
        /// The amount of charge or power the beam weapon has.
        /// </summary>
        [SerializeField, Range(0f, 1f)] protected float chargeAmount = 1f;

        /// <summary>
        /// Whether the weapon checks line of sight before firing primary (in order to prevent friendly fire) each frame.
        /// Only relevant if the weapon is AutoFire. Since this uses raycasts it can lead to reduced performance.
        /// </summary>
        [SerializeField] protected bool checkLineOfSight1;

        /// <summary>
        /// Whether the weapon checks line of sight before firing secondary (in order to prevent friendly fire) each frame.
        /// Only relevant if the weapon is AutoFire. Since this uses raycasts it can lead to reduced performance.
        /// </summary>
        [SerializeField] protected bool checkLineOfSight2;

        /// <summary>
        /// An array of ammo type zero-based indexes that the main firing mechanism can consume.
        /// </summary>
        [SerializeField] protected int[] compatibleAmmo1 = null;

        /// <summary>
        /// An array of ammo type zero-based indexes that the second firing mechanism can consume
        /// </summary>
        [SerializeField] protected int[] compatibleAmmo2 = null;

        /// <summary>
        /// The zero-based index from the magTypes scriptable object for the main firing mechanism
        /// </summary>
        [SerializeField] protected int compatibleMag1 = 0;

        /// <summary>
        /// The zero-based index from the magTypes scriptable object for the second firing mechanism
        /// </summary>
        [SerializeField] protected int compatibleMag2 = 0;

        /// <summary>
        /// The animator for this weapon. This should be attached to, or a child of, the weapon gameobject
        /// </summary>
        [SerializeField] protected Animator defaultAnimator;

        /// <summary>
        /// The reticle (sprite) used when the weapon is held by a S3D player
        /// </summary>
        [SerializeField] protected Sprite defaultReticleSprite = null;

        /// <summary>
        /// The main trigger for the weapon to fire e.g. None, Manual, AutoFire.
        /// Set to None by default so that weapon doesn't automatically fire when a
        /// weapon is added at runtime.
        /// </summary>
        [SerializeField] protected FiringButton firingButton1 = FiringButton.None;

        /// <summary>
        /// The second trigger for the weapon to fire e.g. None, Manual, AutoFire.
        /// Set to None by default so that weapon doesn't automatically fire when a
        /// weapon is added at runtime.
        /// Most weapons will only use the first firing button or trigger.
        /// </summary>
        [SerializeField] protected FiringButton firingButton2 = FiringButton.None;

        /// <summary>
        /// The local space direction in which the weapon fires [Default: Forward]
        /// </summary>
        [SerializeField] protected Vector3 fireDirection = Vector3.forward;

        /// <summary>
        /// The set containing one or more StickyEffectsModules to be randomly selected when attempting to fire with no available ammo on fire button 1.
        /// </summary>
        [SerializeField] protected S3DEffectsSet fireEmptyEffectsSet1;

        /// <summary>
        /// The set containing one or more StickyEffectsModules to be randomly selected when attempting to fire with no available ammo on fire button 2.
        /// </summary>
        [SerializeField] protected S3DEffectsSet fireEmptyEffectsSet2;

        /// <summary>
        /// The main firing type of this weapon. Semi-Auto fires single shots but automatically loads the next projectile if there is available ammo.
        /// Full-Auto can continuously fire while there is available ammo.
        /// </summary>
        [SerializeField] protected FiringType firingType1 = FiringType.SemiAuto;

        /// <summary>
        /// The secondary firing type of this weapon. Semi-Auto fires single shots but automatically loads the next projectile if there is available ammo.
        /// Full-Auto can continuously fire while there is available ammo.
        /// </summary>
        [SerializeField] protected FiringType firingType2 = FiringType.SemiAuto;

        /// <summary>
        /// An array of local space fire position offsets from the relative fire position.
        /// Typically only used for a weapon with multiple barrels.
        /// </summary>
        [SerializeField] protected Vector3[] firePositionOffsets;

        /// <summary>
        /// The minimum time (in seconds) between consecutive firings of the primary fire button.
        /// </summary>
        [SerializeField, Range(0.05f, 10f)] protected float fireInterval1 = 0.5f;

        /// <summary>
        /// The minimum time (in seconds) between consecutive firings of the secondary fire button.
        /// </summary>
        [SerializeField, Range(0.05f, 10f)] protected float fireInterval2 = 5f;

        /// <summary>
        /// When fired, the weapon can hit any objects in these Unity Layers
        /// </summary>
        [SerializeField] protected LayerMask hitLayerMask = Physics.DefaultRaycastLayers;

        /// <summary>
        /// Does the primary firing mechanism require a StickyMagazine to be attached before it can fire?
        /// </summary>
        [SerializeField] protected bool isMagRequired1 = false;

        /// <summary>
        /// Does the secondary firing mechanism require a StickyMagazine to be attached before it can fire?
        /// </summary>
        [SerializeField] protected bool isMagRequired2 = false;

        /// <summary>
        /// Do not display a reticle on the StickyDisplayModule (HUD) when the weapon is aiming with first-person camera.
        /// Only applies if weapon is held by a player (non-NPC) character and there is an
        /// active StickyDisplayModule in the scene.
        /// </summary>
        [SerializeField] protected bool isNoReticleOnAimingFPS = false;

        /// <summary>
        /// Do not display a reticle on the StickyDisplayModule (HUD) when the weapon is aiming with third-person camera.
        /// Only applies if weapon is held by a player (non-NPC) character and there is an
        /// active StickyDisplayModule in the scene.
        /// </summary>
        [SerializeField] protected bool isNoReticleOnAimingTPS = false;

        /// <summary>
        /// Do not display a reticle on the StickyDisplayModule (HUD) when weapon is held.
        /// Only applies if weapon is held by a player (non-NPC) character and there is an
        /// active StickyDisplayModule in the scene.
        /// </summary>
        [SerializeField] protected bool isNoReticleIfHeld = false;

        /// <summary>
        /// Do not display a reticle on the StickyDisplayModule (HUD) when weapon is held,
        /// and Free Look on the character is NOT enabled.
        /// Only applies if weapon is held by a player (non-NPC) character and there is an
        /// active StickyDisplayModule in the scene.
        /// </summary>
        [SerializeField] protected bool isNoReticleIfHeldNoFreeLook = false;

        /// <summary>
        /// Fire button 1 can only fire when the weapon is being aimed
        /// </summary>
        [SerializeField] protected bool isOnlyFireWhenAiming1 = false;

        /// <summary>
        /// Fire button 2 can only fire when the weapon is being aimed
        /// </summary>
        [SerializeField] protected bool isOnlyFireWhenAiming2 = false;

        /// <summary>
        /// The weapon can only fire, reload, or be animated, when it is held by a character.
        /// </summary>
        [SerializeField] protected bool isOnlyUseWhenHeld = true;

        /// <summary>
        /// The amount of ammo the magazine can hold for the primary firing mechanism
        /// </summary>
        [SerializeField, Range(1, 50)] protected int magCapacity1 = 10;

        /// <summary>
        /// The amount of ammo the magazine can hold for the secondary firing mechanism
        /// </summary>
        [SerializeField, Range(1, 50)] protected int magCapacity2 = 1;

        /// <summary>
        /// The number of additional magazines available for the primary firing mechanism.
        /// -1 = unlimited. This is automatically updated when Reload Type is Auto Stash.
        /// </summary>
        [SerializeField] protected int magsInReserve1 = -1;

        /// <summary>
        /// The number of additional magazines available for the secondary firing mechanism.
        /// -1 = unlimited. This is automatically updated when Reload Type is Auto Stash.
        /// </summary>
        [SerializeField] protected int magsInReserve2 = -1;

        /// <summary>
        /// Reference to a common set of magazine types. All weapons and magazines should use the same scriptable object.
        /// </summary>
        [SerializeField] protected S3DMagTypes magTypes = null;

        /// <summary>
        /// The maximum range the weapon can shoot
        /// </summary>
        [SerializeField, Range(1f, 5000f)] protected float maxRange = 100f;

        /// <summary>
        /// The maximum number of smoke effects that can be active at any time on this weapon
        /// </summary>
        [SerializeField, Range(1, 20f)] protected int maxActiveSmokeEffects1 = 2;

        /// <summary>
        /// An array of randomly selected Sticky Effects Modules spawned when the weapon fires. Beams should use looping FX.
        /// They can include both audio and particle systems.
        /// </summary>
        [SerializeField] protected StickyEffectsModule[] muzzleEffects1 = null;

        /// <summary>
        /// The distance in local space that the muzzle Effects Object should be instantiated
        /// from the weapon firing point. Typically only the z-axis would be used when the projectile
        /// is instantiated in front or forwards from the actual weapon.
        /// </summary>
        [SerializeField] protected Vector3 muzzleEffects1Offset = Vector3.zero;

        /// <summary>
        /// The time (in seconds) it takes the fully discharged beam weapon to reach maximum charge
        /// </summary>
        [SerializeField, Range(0f, 30f)] protected float rechargeTime = 0.1f;

        /// <summary>
        /// The local space fire position aligned to the end of the barrel.
        /// </summary>
        [SerializeField] protected Vector3 relativeFirePosition = new Vector3(0f, 0f, 0.5f);

        /// <summary>
        /// The short delay between when the primary button mechanism fired, and it attempts to reload
        /// </summary>
        [SerializeField, Range(0f, 5f)] protected float reloadDelay1 = 0.2f;

        /// <summary>
        /// The short delay between when the secondary button mechanism fired, and it attempts to reload
        /// </summary>
        [SerializeField, Range(0f, 5f)] protected float reloadDelay2 = 0.2f;

        /// <summary>
        /// The time it takes the primary button mechanism to reload.
        /// </summary>
        [SerializeField, Range(0f, 30f)] protected float reloadDuration1 = 5f;

        /// <summary>
        /// The time it takes the secondary button mechanism to reload.
        /// </summary>
        [SerializeField, Range(0f, 30f)] protected float reloadDuration2 = 5f;

        /// <summary>
        /// The delay during reloading, in seconds, between when the primary button mechanism unequips the old mag
        /// and starts to equip a new mag.
        /// </summary>
        [SerializeField, Range(0f, 15f)] protected float reloadEquipDelay1 = 0.1f;

        /// <summary>
        /// The delay during reloading, in seconds, between when the secondary button mechanism unequips the mag
        /// and starts to equip a new mag.
        /// </summary>
        [SerializeField, Range(0f, 15f)] protected float reloadEquipDelay2 = 0.1f;

        /// <summary>
        /// The method used for reloading the primary firing mechanism
        /// </summary>
        [SerializeField] protected ReloadType reloadType1 = ReloadType.ManualOnly;

        /// <summary>
        /// The method used for reloading the secondary firing mechanism
        /// </summary>
        [SerializeField] protected ReloadType reloadType2 = ReloadType.ManualOnly;

        /// <summary>
        /// The Sound FX for the primary button mechanism used when the weapon begins reloading.
        /// The Effects Type must be SoundFX.
        /// </summary>
        [SerializeField] protected StickyEffectsModule reloadSoundFX1 = null;

        /// <summary>
        /// The Sound FX for the secondary button mechanism used when the weapon begins reloading.
        /// The Effects Type must be SoundFX.
        /// </summary>
        [SerializeField] protected StickyEffectsModule reloadSoundFX2 = null;

        /// <summary>
        /// The Sound FX for the primary button mechanism used when the weapon equips a new mag during reloading.
        /// The Effects Type must be SoundFX.
        /// </summary>
        [SerializeField] protected StickyEffectsModule reloadEquipSoundFX1 = null;

        /// <summary>
        /// The Sound FX for the secondary button mechanism used when the weapon equips a new mag during reloading.
        /// The Effects Type must be SoundFX.
        /// </summary>
        [SerializeField] protected StickyEffectsModule reloadEquipSoundFX2 = null;

        /// <summary>
        /// The set containing one or more StickyEffectsModules to be randomly selected when primary mechanism is fired.
        /// </summary>
        [SerializeField] protected S3DEffectsSet smokeEffectsSet1;

        /// <summary>
        /// The set containing one or more StickyEffectsModules to be randomly selected when secondary mechanism is fired.
        /// </summary>
        [SerializeField] protected S3DEffectsSet smokeEffectsSet2;

        /// <summary>
        /// The local space direction the spent cartridge is ejected
        /// </summary>
        [SerializeField] protected Vector3 spentEjectDirection = new Vector3(1f, 0f, 0f);

        /// <summary>
        /// The force used to eject the spent cartridge from the weapon in the Spent Eject Direction.
        /// </summary>
        [SerializeField] protected float spentEjectForce = 5f;

        /// <summary>
        /// The local space position on the weapon from which the spent cartridge is ejected.
        /// </summary>
        [SerializeField] protected Vector3 spentEjectPosition = new Vector3(-0.01f, 0.05f, 0f);

        /// <summary>
        /// The local space rotation, in Euler angles, of the spent cartridge when ejected.
        /// </summary>
        [SerializeField] protected Vector3 spentEjectRotation = Vector3.zero;

        /// <summary>
        /// The list of animation actions for this weapon
        /// </summary>
        [SerializeField] protected List<S3DAnimAction> s3dAnimActionList;

        /// <summary>
        /// The list of weapon anim sets for character model IDs that apply to this weapon
        /// </summary>
        [SerializeField] protected List<S3DWeaponAnimSet> s3dWeaponAnimSetList;

        /// <summary>
        /// The type or style of weapon e.g., Projectile Raycast/Standard, Beam etc.
        /// We do not support changing the weaponType after it has been initialised.
        /// </summary>
        [SerializeField] protected WeaponType weaponType = WeaponType.BeamStandard;

        /// <summary>
        /// Show the relative fire position gizmos in the scene view
        /// </summary>
        [SerializeField] private bool showWFPGizmosInSceneView = false;

        /// <summary>
        /// Show the spent cartrige eject position gizmos in the scene view
        /// </summary>
        [SerializeField] private bool showWSCEPGizmosInSceneView = false;

        // Editor only
        [SerializeField] private bool isS3DAnimActionListExpanded = true;
        [SerializeField] private bool isAnimActionsExpanded = true;
        [SerializeField] private bool isWeaponAnimSetsExpanded = true;

        #endregion

        #region Protected Variables - Serialized Aiming and Reticle

        /// <summary>
        /// When aiming the held weapon, this is the character first person camera field of view.
        /// </summary>
        [SerializeField, Range(5f, 80f)] protected float aimingFirstPersonFOV = 30f;

        /// <summary>
        /// The rate at which the transition betwen aiming and not aiming will progress
        /// </summary>
        [SerializeField, Range(0.1f, 100f)] protected float aimingSpeed = 10f;

        /// <summary>
        /// When aiming the held weapon, this is the character third person camera field of view.
        /// </summary>
        [SerializeField, Range(5f, 80f)] protected float aimingThirdPersonFOV = 30f;

        #endregion

        #region Protectd Variables - Serialized Attachments

        /// <summary>
        /// Should the laser sight beam automatically turn on when it is equipped?
        /// </summary>
        [SerializeField] protected bool isLaserSightAutoOn = false;

        /// <summary>
        /// Should the laser sight beam be showing?
        /// </summary>
        [SerializeField] protected bool isLaserSightOn = false;

        /// <summary>
        /// Allow NPCs to use the Scope. Typically, you don’t want to enable this as each Scope used in the
        /// scene requires the whole scene to be rendered again which has a significant performance overhead.
        /// </summary>
        [SerializeField] protected bool isScopeAllowNPC = false;

        /// <summary>
        /// Should the Scope be automatically turned on when grabbed by a character?
        /// Essentially, it will stay on all the time the weapon is held.
        /// </summary>
        [SerializeField] protected bool isScopeAutoOn = false;

        /// <summary>
        /// Should the scope currently be used for more precise visual aiming?
        /// </summary>
        [SerializeField] protected bool isScopeOn = false;

        /// <summary>
        /// The child transform that determines where the laser sight aims from
        /// </summary>
        [SerializeField] protected Transform laserSightAimFrom;

        /// <summary>
        /// The colour of the laser sight beam
        /// </summary>
        [SerializeField] protected Color32 laserSightColour = new Color(1f, 0f, 0f, 0.3f);

        /// <summary>
        /// The child transform where the primary magazine attaches to the weapon
        /// </summary>
        [SerializeField] protected Transform magAttachPoint1;

        /// <summary>
        /// The child transform where the secondary magazine attaches to the weapon
        /// </summary>
        [SerializeField] protected Transform magAttachPoint2;

        /// <summary>
        /// The camera used to render the scope display
        /// </summary>
        [SerializeField] protected Camera scopeCamera;

        /// <summary>
        /// The (mesh) renderer to project the Scope Camera onto.
        /// </summary>
        [SerializeField] protected Renderer scopeCameraRenderer;

        #endregion

        #region Protected Variables - Serialized Health

        /// <summary>
        /// The overall health of the weapon.
        /// </summary>
        [SerializeField, Range(0f, 1f)] protected float health = 1f;

        /// <summary>
        /// The heat of the weapon - range 0.0 (starting temp) to 100.0 (max temp).
        /// At runtime call either weapon.SetHeatLevel(..) or shipInstance.SetHeatLevel(..)
        /// </summary>
        [SerializeField, Range(0f, 100f)] protected float heatLevel = 0;

        /// <summary>
        /// The rate heat is added per second for beam weapons or the amount added each time
        /// a projectile weapon fires. If rate is 0, heat level never changes.
        /// </summary>
        [SerializeField, Range(0f, 50f)] protected float heatUpRate = 0;

        /// <summary>
        /// The rate heat is removed per second. This is the rate the weapon cools when not in use.
        /// </summary>
        [SerializeField, Range(0f, 20f)] protected float heatDownRate = 2f;

        /// <summary>
        /// The heat level that the weapon will begin to overheat and start being less efficient.
        /// </summary>
        [SerializeField, Range(50f, 100f)] protected float overHeatThreshold = 80f;

        /// <summary>
        /// When the weapon reaches max heat level of 100, will the weapon be inoperable
        /// until it is repaired?
        /// </summary>
        [SerializeField] protected bool isBurnoutOnMaxHeat = false;

        /// <summary>
        /// Is firing paused? This can be useful when only wanting to prevent the weapon from
        /// firing like when displaying a Sticky popup.
        /// </summary>
        [SerializeField] protected bool isFiringPaused = false;

        #endregion

        #region Protected Variables - Serialized Recoil

        /// <summary>
        /// The maximum distance, in metres, the weapon will kick back on the local z-axis when the primary mechanism fires
        /// </summary>
        [SerializeField, Range(0f, 0.2f)] protected float recoilMaxKickZ1 = 0.02f;

        /// <summary>
        /// The maximum distance, in metres, the weapon will kick back on the local z-axis when the secondary mechanism fires
        /// </summary>
        [SerializeField, Range(0f, 0.2f)] protected float recoilMaxKickZ2 = 0.02f;

        /// <summary>
        /// The rate at which the weapon returns to its stable position
        /// </summary>
        [SerializeField, Range(0.1f, 25f)] protected float recoilReturnRate = 7f;

        /// <summary>
        /// The rate at which the weapon recoils when fired.
        /// NOTE: Currently only applies to FPS players when aiming
        /// </summary>
        [SerializeField, Range(0f, 25f)] protected float recoilSpeed = 5f;

        /// <summary>
        /// The angle the weapon pitches up when the primary mechanism fires
        /// </summary>
        [SerializeField, Range(0f, 5f)] protected float recoilX1 = 0.5f;

        /// <summary>
        /// The angle the weapon pitches up when the secondary mechanism fires
        /// </summary>
        [SerializeField, Range(0f, 5f)] protected float recoilX2 = 0.5f;

        /// <summary>
        /// The maximum angle the weapon rotates around the local y-axis the primary mechanism fires
        /// </summary>
        [SerializeField, Range(0f, 5f)] protected float recoilY1 = 1f;

        /// <summary>
        /// The maximum angle the weapon rotates around the local y-axis the secondary mechanism fires
        /// </summary>
        [SerializeField, Range(0f, 5f)] protected float recoilY2 = 1f;

        /// <summary>
        /// The maximum angle the weapon rotates around the local z-axis the primary mechanism fires
        /// </summary>
        [SerializeField, Range(0f, 5f)] protected float recoilZ1 = 0.5f;

        /// <summary>
        /// The maximum angle the weapon rotates around the local z-axis the secondary mechanism fires
        /// </summary>
        [SerializeField, Range(0f, 5f)] protected float recoilZ2 = 0.5f;

        #endregion

        #region Protected Variables - General

        /// <summary>
        /// World space position the weapon is aiming at
        /// </summary>
        protected Vector3 aimAtPosition = Vector3.zero;

        /// <summary>
        /// Temp variable used with SmoothDamp
        /// </summary>
        protected Vector3 aimAtPositionVelo = Vector3.zero;

        /// <summary>
        /// Local space position where the weapon was aiming
        /// </summary>
        protected Vector3 prevAimAtPositionLS = Vector3.zero;

        /// <summary>
        /// World space position where the weapon is aiming from
        /// </summary>
        protected Vector3 aimFromPosition = Vector3.zero;

        /// <summary>
        /// If held, the world space position the character is looking at
        /// </summary>
        protected Vector3 characterLookAtPosition = Vector3.zero;

        /// <summary>
        /// If held, the world space position the character is looking from
        /// </summary>
        protected Vector3 characterLookFromPosition = Vector3.zero;

        /// <summary>
        /// If there is a HUD save current value of Locked Reticle To Cursor.
        /// Currently only used when held or enter/exit Aiming
        /// </summary>
        protected bool savedLockedReticleToCursor = false;

        /// <summary>
        /// Can the weapon currently be fired from the hip?
        /// [IN FUTURE SHOULD BE EXPOSED IN THE EDITOR]
        /// </summary>
        protected bool isHipFire = false;

        /// <summary>
        /// Is this weapon reloadable?
        /// </summary>
        protected bool isReloadable = false;

        /// <summary>
        /// A reference to the StickyManager in the scene.
        /// </summary>
        [System.NonSerialized] protected StickyManager stickyManager = null;

        /// <summary>
        /// Has the weapon primary mechanism attempted to fire with no ammo since the last fixed update?
        /// </summary>
        protected bool hasEmptyFired1 = false;

        /// <summary>
        /// Has the weapon secondary mechanism attempted to fire with no ammo since the last fixed update?
        /// </summary>
        protected bool hasEmptyFired2 = false;

        /// <summary>
        /// Has the weapon fired the primary mechanism since the last fixed update?
        /// </summary>
        protected bool hasFired1 = false;

        /// <summary>
        /// Has the weapon fired the secondary mechanism since the last fixed update?
        /// </summary>
        protected bool hasFired2 = false;

        /// <summary>
        /// Has the weapon started reloading the primary mechanism since the last fixed update?
        /// </summary>
        protected bool hasReloadStarted1 = false;

        /// <summary>
        /// Has the weapon started reloading the secondary mechanism since the last fixed update?
        /// </summary>
        protected bool hasReloadStarted2 = false;

        /// <summary>
        /// Has the weapon finished reloading the primary mechanism since the last fixed update?
        /// </summary>
        protected bool hasReloadFinished1 = false;

        /// <summary>
        /// Has the weapon finished reloading the secondary mechanism since the last fixed update?
        /// </summary>
        protected bool hasReloadFinished2 = false;

        protected bool isFire1Input = false;
        protected bool isFire2Input = false;
        protected bool isPrevFire1Input = false;
        protected bool isPrevFire2Input = false;
        /// <summary>
        /// Is the fire1 button being held down?
        /// </summary>
        protected bool isFire1InputHeld = false;
        /// <summary>
        /// Is the fire2 button being held down?
        /// </summary>
        protected bool isFire2InputHeld = false;

        protected bool isReloading1 = false;
        protected bool isReloading2 = false;
        protected bool isReloadUnequipping1 = false;
        protected bool isReloadUnequipping2 = false;
        protected bool isReloadEquipping1 = false;
        protected bool isReloadEquipping2 = false;

        protected bool isWeaponInitialised = false;

        protected bool isWeaponEnabled = true;

        /// <summary>
        /// Is the weapon currently paused?
        /// </summary>
        protected bool isWeaponPaused = false;

        protected S3DRandom randomSelectFX = null;
        protected int numFirePositionOffsets = 0;

        /// <summary>
        /// The number of (unvalidated) EffectsModules in the editor (some may be null or invalid)
        /// </summary>
        protected int numMuzzleEffects1 = 0;

        /// <summary>
        /// The number of validated or available EffectsModules for muzzle FX.
        /// </summary>
        protected int numMuzzleEffects1Valid = 0;

        protected int weaponTypeInt = -1;
        protected int weaponFiringButton1Int = 0;
        protected int weaponFiringButton2Int = 0;
        protected int weaponFiringType1Int = -1;
        protected int weaponFiringType2Int = -1;

        /// <summary>
        /// A reusable array of RaycastHit structs.
        /// </summary>
        [NonSerialized] protected RaycastHit[] raycastHitInfoArray;
        [NonSerialized] protected RaycastHit raycastHit;
        [NonSerialized] protected Ray raycastRay;

        /// <summary>
        /// The time (in seconds) until this weapon primary button can fire again.
        /// </summary>
        protected float fireIntervalTimer1 = 0f;

        /// <summary>
        /// The time (in seconds) until this weapon secondary button can fire again.
        /// </summary>
        protected float fireIntervalTimer2 = 0f;

        /// <summary>
        /// Is the weapon primary locked onto the target. If it is fired, is it
        /// likely to hit the target? i.e. is it facing the correct direction?
        /// </summary>
        protected bool isLockedOnTarget1 = false;

        /// <summary>
        /// Is the weapon secondary locked onto the target. If it is fired, is it
        /// likely to hit the target? i.e. is it facing the correct direction?
        /// </summary>
        protected bool isLockedOnTarget2 = false;

        /// <summary>
        /// The value of isOnlyFireWhenAiming1 when the weapon is picked up by a character
        /// </summary>
        protected bool savedIsOnlyFireWhenAiming1;

        /// <summary>
        /// The value of isOnlyFireWhenAiming2 when the weapon is picked up by a character
        /// </summary>
        protected bool savedIsOnlyFireWhenAiming2;

        //protected Vector3 weaponRelativeFireDirectionLOS = Vector3.zero;

        #endregion

        #region Protected Variables - Aiming

        /// <summary>
        /// For saving the original field of view before aiming is enabled
        /// </summary>
        protected float aimOriginalFieldOfView = 0f;

        /// <summary>
        /// For saving the original camera near clipping plane before aiming is enabled
        /// </summary>
        protected float aimOriginalNearClipping = 0;

        /// <summary>
        /// If a character is holding the weapon, are they attempting to aim it?
        /// Typically when the character is looking down the sights or using the Scope.
        /// </summary>
        protected bool isAiming = false;

        /// <summary>
        /// Is the weapon in the process of transitioning to Aiming?
        /// </summary>
        protected bool isAimingSmoothEnable = false;

        /// <summary>
        /// Is the weapon in the process of transitioning from Aiming?
        /// </summary>
        protected bool isAimingSmoothDisable = false;

        /// <summary>
        /// When smoothly going in/out of Aiming, this tracks the character FOV
        /// </summary>
        protected float prevAimingFieldOfView = 0f;

        /// <summary>
        /// When smoothly going in/out of Aiming, this track the character camera near clipping plane
        /// </summary>
        protected float prevAimingNearClipping = 0f;

        /// <summary>
        /// The display reticle on the active HUD before the weapon was held.
        /// </summary>
        protected int savedActiveDisplayReticleHash = 0;

        /// <summary>
        /// Is the HUD display reticle being shown when the weapon is picked
        /// up by a character
        /// </summary>
        protected bool savedIsDisplayReticleShown = false;

        /// <summary>
        /// When smoothly transitioning to or from Aiming, the desired character FOV
        /// </summary>
        protected float targetAimingFieldofView = 0f;

        /// <summary>
        /// When smoothly transitioning to or from Aiming, the desired camera near clipping plane
        /// </summary>
        protected float targetAimingNearClipping = 0f;

        #endregion

        #region Protected Variables - Ammo

        protected int reloadType1Int = -1;
        protected int reloadType2Int = -1;

        /// <summary>
        /// The ammo type currently being used for fire button 1.
        /// -1 unset, 0-25 (A-z)
        /// </summary>
        protected int currentAmmoType1Int = -1;

        /// <summary>
        /// The ammo type currently being used for fire button 2
        /// -1 unset, 0-25 (A-z)
        /// </summary>
        protected int currentAmmoType2Int = -1;

        /// <summary>
        /// List of EffectsModule templates available when an empty fire occurs on primary mechanism
        /// </summary>
        [NonSerialized] protected int[] fireEmptyEffects1PrefabIDs;

        /// <summary>
        /// List of EffectsModule templates available when an empty fire occurs on secondary mechanism
        /// </summary>
        [NonSerialized] protected int[] fireEmptyEffects2PrefabIDs;

        /// <summary>
        /// The number of available different empty fire effects for the primary mechanism
        /// </summary>
        protected int numFireEmptyEffects1 = 0;

        /// <summary>
        /// The number of available different empty fire effects for the secondary mechanism
        /// </summary>
        protected int numFireEmptyEffects2 = 0;

        #endregion

        #region Protected Variables - Animate

        protected bool isAnimateEnabled = false;
        protected int numAnimateActions = 0;
        protected int numWeaponAnimSets = 0;

        /// <summary>
        /// The number of anim actions for this weapon that apply to the character holding it.
        /// </summary>
        protected int numCharacterAnimActions = 0;
        [System.NonSerialized] protected List<S3DAnimActionExt> animActionExtList = null;

        /// <summary>
        /// Have all the anim action parameter names been
        /// verified against the (current) character controller?
        /// </summary>
        protected bool isWpnAnimSetsVerified = false;

        /// <summary>
        /// Used when re-enabling or unpausing a weapon.
        /// </summary>
        protected bool savedAnimateEnabledState = false;

        protected int numAnimLayers = 0;
        [System.NonSerialized] private List<S3DAnimLayerData> animlayerDataList = null;

        #endregion

        #region Protected Variables - Attachments

        /// <summary>
        /// The magazine, if any currently equipped for the primary firing mechanism
        /// </summary>
        [NonSerialized] protected StickyMagazine equippedMag1 = null;

        /// <summary>
        /// The magazine, if any currently equipped for the secondary firing mechanism
        /// </summary>
        [NonSerialized] protected StickyMagazine equippedMag2 = null;

        /// <summary>
        /// Is the laser sight setup and ready to be used?
        /// </summary>
        protected bool isLaserSightReady = false;

        /// <summary>
        /// Is there a magazine currently equipped and ready on the weapon for fire button 1?
        /// </summary>
        protected bool isMagEquipped1 = false;

        /// <summary>
        /// Is there a magazine currently equipped and ready on the weapon for fire button 2?
        /// </summary>
        protected bool isMagEquipped2 = false;

        /// <summary>
        /// Is the scope camera ready to render images?
        /// </summary>
        protected bool isScopeCamInitialised = false;
 
        [NonSerialized] protected LineRenderer laserSightLineRenderer = null;

        [NonSerialized] protected List<StickyMagazine> tempMagList = null;

        /// <summary>
        /// The render texture that will be output for scopeCamera.
        /// </summary>
        //[System.NonSerialized] protected RenderTexture scopeRTexture = null;

        #endregion

        #region Protected Variables - Editor

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showGeneralSettingsInEditor = true;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showAimingAndReticleSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showAmmoSettingsInEditor = true;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showAnimateSettingsInEditor = true;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showAttachmentSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showHealthSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showMuzzleFXSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showRecoilSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showSmokeSettingsInEditor = false;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [SerializeField] protected bool showSpentCartridgeSettingsInEditor = false;

        #endregion

        #region Protected Variables - Health

        [Range(0f, 1f)] protected float currentPerformance = 1f;

        #endregion

        #region Protected Variables - Recoil

        protected bool isRecoiling = false;
        protected Vector3 recoilCurrentPos;
        protected Vector3 recoilCurrentRot;
        protected Vector3 recoilTargetPos;
        protected Vector3 recoilTargetRot;       

        #endregion

        #region Protected Variables - Smoke

        /// <summary>
        /// The number of available different smoke effects for the primary mechanism
        /// </summary>
        protected int numSmokeEffects1 = 0;

        /// <summary>
        /// The number of available different smoke effects for the secondary mechanism
        /// </summary>
        protected int numSmokeEffects2 = 0;

        /// <summary>
        /// List of EffectsModule templates available when smoke is emitted from the primary mechanism
        /// </summary>
        [NonSerialized] protected int[] smokeEffects1PrefabIDs;

        /// <summary>
        /// List of EffectsModule templates available when smoke is emitted from the secondary mechanism
        /// </summary>
        [NonSerialized] protected int[] smokeEffects2PrefabIDs;

        /// <summary>
        /// List that tracks smoke effects that have been activated (currently contains
        /// ones for both primary and secondary mechanisms).
        /// </summary>
        [NonSerialized] protected List<S3DEffectItemKey> smokeItemKeyList = null;

        #endregion

        #region Protected Variables - Targetting

        /// <summary>
        /// Is the weapon currently attempting to target an object?
        /// </summary>
        protected bool isTargetTransformAssigned = false;

        /// <summary>
        /// Is the weapon currently attempting to target a character?
        /// </summary>
        protected bool isTargetSticky3DAssigned = false;

        /// <summary>
        /// The transform the weapon is attempting to target. This is typically used when
        /// an autofire weapon, held by an NPC, is determining if the weapon is aiming
        /// or pointing toward an object.
        /// It is NOT used to change where the weapon is aiming.
        /// </summary>
        [NonSerialized] protected Transform targetTransform = null;

        /// <summary>
        /// The character the weapon is attempting to target. This is typically used when
        /// an autofire weapon, held by an NPC, is determining if the weapon is aiming
        /// or pointing toward an enemy character.
        /// It is NOT used to change where the weapon is aiming.
        /// </summary>
        [NonSerialized] protected StickyControlModule targetSticky3D = null;

        #endregion

        #region Internal variables

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// For beam weapons, this is used to identify the active beam for each firing point
        /// </summary>
        [System.NonSerialized] internal List<S3DBeamItemKey> beamItemKeyList = null;

        /// <summary>
        /// [INTENAL USE ONLY]
        /// An array of validated StickyEffectsModule pooled template PrefabIDs for Muzzle FX
        /// </summary>
        [System.NonSerialized] internal int[] muzzleEffectsObject1PrefabIDs = null;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// This is used to identify the muzzle FX for each firing point.
        /// Currently there can be only one muzzle flash for each fire point.
        /// </summary>
        [System.NonSerialized] internal List<S3DEffectItemKey> muzzleItemKey1List = null;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// This is used to identify the reload sound FX.
        /// Currently there can be only one reload sound fx for each firing mechanism.
        /// </summary>
        [System.NonSerialized] internal List<S3DEffectItemKey> reloadSFXItemKeyList = null;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// For reloadable weapons, this is used to identify the pooled template PrefabID for the primary reload Sound FX
        /// </summary>
        [System.NonSerialized] internal int reloadSoundFXPrefabID1 = 0;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// For reloadable weapons, this is used to identify the pooled template PrefabID for the secondary reload Sound FX
        /// </summary>
        [System.NonSerialized] internal int reloadSoundFXPrefabID2 = 0;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// For reloadable weapons, this is used to identify the pooled template PrefabID for the primary Sound FX when equipping a new mag during reloading.
        /// </summary>
        [System.NonSerialized] internal int reloadEquipSoundFXPrefabID1 = 0;

        /// <summary>
        /// [INTERNAL USE ONLY]
        /// For reloadable weapons, this is used to identify the pooled template PrefabID for the secondary Sound FX when equipping a new mag during reloading.
        /// </summary>
        [System.NonSerialized] internal int reloadEquipSoundFXPrefabID2 = 0;

        #endregion

        #region Public Delegates

        public delegate void CallbackOnWeaponNoAmmo(StickyWeapon stickyWeapon, int fireButton);

        public delegate void CallbackOnWeaponFired(StickyWeapon stickyWeapon, int fireButton);

        /// <summary>
        /// The name of the custom method that is called immediately after the weapon runs
        /// out of ammunition. Your method must take a StickyWeapon and fire button parameter.
        /// This should be a lightweight method to avoid performance issues that
        /// doesn't hold references the weapon past the end of the current frame.
        /// </summary>
        [System.NonSerialized] public CallbackOnWeaponNoAmmo callbackOnWeaponNoAmmo = null;

        /// <summary>
        /// The name of the custom method that is called immediately after the weapon has
        /// fired at least 1 fire position. Your method must take a StickyWeapon and fire
        /// button parameter.
        /// This should be a lightweight method to avoid performance issues that
        /// doesn't hold references the weapon past the end of the current frame.
        /// </summary>
        [System.NonSerialized] public CallbackOnWeaponFired callbackOnWeaponFired = null;

        #endregion

        #region Update Methods

        protected override void Update()
        {
            if (isWeaponEnabled && !isEquipped && !isStashed && !isSocketed)
            {
                base.Update();

                if (isWeaponInitialised)
                {
                    // If movement data has already been updated in base.Update(), no
                    // need to do it again.
                    if (isMovementDataStale) { UpdatePositionAndMovementData(); }
                    UpdateAimAtPosition();
                    UpdateWeapon();

                    if (isAimingSmoothDisable || isAimingSmoothEnable)
                    {
                        AimTransition();
                    }

                    if (isRecoiling) { Recoiling(); }
                }
            }
        }

        protected override void FixedUpdate()
        {
            if (isWeaponEnabled && !isEquipped && !isStashed && !isSocketed)
            {
                base.FixedUpdate();

                if (isWeaponInitialised)
                {
                    // Not sure if this is really necessary
                    // Does NOT fix LineRenderer lag in MoveBeam()
                    if (isMovementDataStale) { UpdatePositionAndMovementData(); }

                    if (isAnimateEnabled && (isHeld || !isOnlyUseWhenHeld))
                    {
                        if (numAnimateActions > 0) { AnimateWeapon(false); }
                    }

                    if (isHeld && numCharacterAnimActions > 0 && !isWeaponPaused) { AnimateCharacter(false); }

                    if (!isHeld && isDelayApplyGravity) { ApplyGravity(); }
                }

                ResetHasHappenedSinceFixedUpdate();
            }
        }

        #endregion

        #region Private and Internal Methods - General

        /// <summary>
        /// This is automatically called when the character holding this weapon changes cameras.
        /// </summary>
        /// <param name="StickyID"></param>
        /// <param name="oldCamera"></param>
        /// <param name="newCamera"></param>
        /// <param name="isThirdPerson"></param>
        internal void CharacterChangedCamera (int StickyID, Camera oldCamera, Camera newCamera, bool isThirdPerson)
        {
            if (isHeld)
            {
                CharacterChangedCamera(oldCamera, newCamera, isThirdPerson);
            }
        }

        /// <summary>
        /// Check if we need to change the heat level on this weapon. If
        /// heat up rate is 0, then do nothing.
        /// When heatInput = 0, the weapon starts to cool down.
        /// heatInput is a bit vague and default value should be 1.0.
        /// Might be able to balance with projectile weapons based on how
        /// long since last fired... maybe.
        /// </summary>
        /// <param name="dTime"></param>
        /// <param name="heatInput"></param>
        internal void ManageHeat (float dTime, float heatInput)
        {
            if (heatUpRate > 0f)
            {
                // Heat or cool weapon independently to the health level.
                if (heatInput > 0f)
                {
                    // Heating up
                    if (heatLevel < 100f)
                    {
                        SetHeatLevel(heatLevel + (heatInput * heatUpRate * dTime));
                    }
                }
                // Only cool down if not burnt out
                else if (heatDownRate > 0f && (!isBurnoutOnMaxHeat || heatLevel < 100f))
                {
                    // Cooling down
                    SetHeatLevel(heatLevel - (heatDownRate * dTime));
                }
            }
        }

        /// <summary>
        /// This gets called from FixedUpdate in StickyBeamModule. It performs the following:
        /// 1. Checks if the beam should be despawned
        /// 2. Moves the beam
        /// 3. Changes the length
        /// 4. Checks if it hits anything
        /// TODO 5. Updates damage on what beam hits
        /// 6. Instantiate effects object at hit point
        /// 7. Consumes weapon power
        /// It needs to be a member of the weapon as it requires both weapon and beam data.
        /// Assumes the beam linerenderer has useWorldspace enabled.
        /// </summary>
        /// <param name="beamModule"></param>
        internal void MoveBeam(StickyBeamModule beamModule)
        {
            // If there are no firePositionOffsets, firePositionOffsetIndex will 0.
            // If the beam is inactive, the firePositionOffsetIndex will be -1.
            if (beamModule.IsBeamEnabled && beamModule.firePositionOffsetIndex >= 0)
            {
                S3DBeamItemKey beamItemKey = beamItemKeyList[beamModule.firePositionOffsetIndex];

                // Read once from the deltaTime property getter
                float _deltaTime = Time.deltaTime;

                if (weaponTypeInt == BeamStandardInt && beamItemKey.beamSequenceNumber == beamModule.itemSequenceNumber)
                {
                    // Currently the beam always uses fireButton1

                    // For autofiring weapons, this will return false.
                    bool isReadyToFire = CanFireButton(1);

                    // If this is auto-firing, check if it is ready to fire
                    if (weaponFiringButton1Int == FiringButtonAutoInt)
                    {
                        isLockedOnTarget1 = IsFacingTarget(1, 5f);

                        // NOTE: If check LoS is enabled, it will still fire if another enemy is between the weapon and the weapon.target.
                        isReadyToFire = isLockedOnTarget1 && (!checkLineOfSight1 || WeaponHasLineOfSight(1));
                    }

                    // Should this beam be despawned or returned to the pool?
                    // a) No charge in weapon
                    // b) Has no performance
                    // c) Has no health
                    // d) Weapon Firing is paused
                    // e) user stopped firing and has fired for min time permitted
                    // f) has exceeded the maximum firing duration
                    if (chargeAmount <= 0f || currentPerformance == 0f || health == 0f || isFiringPaused || (!isReadyToFire && beamModule.burstDuration > beamModule.minBurstDuration) || beamModule.burstDuration > beamModule.maxBurstDuration)
                    {
                        // Destroy any muzzle FX
                        DestroyMuzzleFX(beamModule.firePositionOffsetIndex);

                        // Unassign the beam from this weapon's fire position
                        beamItemKeyList[beamModule.firePositionOffsetIndex] = new S3DBeamItemKey(-1, -1, 0);

                        beamModule.DestroyBeam();
                    }
                    else
                    {
                        // Move the beam start
                        // Calculate the World-space Fire Position (like when weapon is first fired)
                        // Note: That the character and weapon could have moved and rotated, so we recalc here.
                        Vector3 _weaponWSBasePos = GetWorldFireBasePosition();
                        Vector3 _weaponWSFireDir = GetWorldFireDirection();
                        Vector3 _weaponWSFirePos = GetWorldFirePosition(_weaponWSBasePos, beamModule.firePositionOffsetIndex);

                        // Calc length and end position
                        float _desiredLength = beamModule.burstDuration * beamModule.speed;
                        if (_desiredLength > maxRange) { _desiredLength = maxRange; }

                        // WARNING: This can look a little strange if hit object is only a few metres infront
                        // of the character. The beam can be at an angle to the weapon rather than pointing
                        // in the direction the weapon is pointing.
                        if (!isHipFire && isHeld)
                        {
                            bool isThirdPerson = stickyControlModule.isThirdPerson && !stickyControlModule.IsNPC();

                            // Use where character is looking when not aiming but IsFreeLook is disabled.
                            bool isUseCharacterLooking = isAiming || !stickyControlModule.IsLookFreeLookEnabled;

                            if (isThirdPerson && isUseCharacterLooking)
                            {
                                _weaponWSFirePos = characterLookFromPosition;
                                _weaponWSFireDir = S3DMath.Normalise(_weaponWSFirePos, aimAtPosition);
                            }
                        }

                        Vector3 _endPosition = _weaponWSFirePos + (_weaponWSFireDir * _desiredLength);

                        // Check if it hit anything
                        // Did the beam hit a character, another object with a StickyDamageReceiver, or a non-trigger collider?
                        if (stickyManager.CheckObjectHitByBeam
                        (
                            _weaponWSFirePos, _weaponWSFireDir, _desiredLength, hitLayerMask, out raycastHit,
                            beamModule, _deltaTime, false
                        ))
                        {
                            // Adjust the end position to the hit point
                            _endPosition = raycastHit.point;

                            //DebugExtension.DebugPoint(_endPosition, Color.yellow, 0.25f);

                            // ISSUES - the effect may not be visible while firing if:
                            // 1) if the effect despawn time is less than the beam max burst duration (We have a warning in the BeamModule editor)
                            // 2) if the effect does not have looping enabled (this could be more expensive to check)

                            // Add or Move the effects object
                            if (beamModule.effectsObjectPrefabID >= 0)
                            {
                                // If the effect has not been spawned, do now
                                if (beamModule.effectsItemKey.effectsObjectSequenceNumber == 0)
                                {
                                    S3DInstantiateEffectsObjectParameters ieParms = new S3DInstantiateEffectsObjectParameters
                                    {
                                        effectsObjectPrefabID = beamModule.effectsObjectPrefabID,
                                        position = _endPosition,
                                        rotation = beamModule.transform.rotation
                                    };

                                    // Instantiate the hit effects
                                    if (stickyManager.InstantiateEffectsObject(ref ieParms) != null)
                                    {
                                        if (ieParms.effectsObjectSequenceNumber > 0)
                                        {
                                            // Record the hit effect item key
                                            beamModule.effectsItemKey = new S3DEffectItemKey(ieParms.effectsObjectPrefabID, ieParms.effectsObjectPoolListIndex, ieParms.effectsObjectSequenceNumber);
                                        }
                                    }
                                }
                                // Move the existing effects object to the end of the beam
                                else
                                {
                                    // Currently we are not checking sequence number matching (for pooled effects) as it is faster and can
                                    // avoid doing an additional GetComponent().
                                    stickyManager.MoveEffectsObject(beamModule.effectsItemKey, _endPosition, beamModule.transform.rotation, false);
                                }
                            }
                        }
                        // The beam isn't hitting anything AND there is an active effects object
                        else if (beamModule.effectsObjectPrefabID >= 0 && beamModule.effectsItemKey.effectsObjectSequenceNumber > 0)
                        {
                            stickyManager.DestroyEffectsObject(beamModule.effectsItemKey);
                        }

                        // Move the end point of the beam, in local space.
                        // useWorldspace is disabled in beamModule.InitialiseBeam(..)
                        beamModule.lineRenderer.SetPosition(1, beamModule.GetLocalPosition(_endPosition));

                        // Consume weapon power. Consumes up to 2x power when overheating.
                        if (rechargeTime > 0f) { chargeAmount -= _deltaTime * (heatLevel > overHeatThreshold ? 2f - currentPerformance : 1f) / beamModule.dischargeDuration; }

                        ManageHeat(_deltaTime, 1f);

                        // If we run out of power, de-activate it before weapon starts recharging
                        if (chargeAmount <= 0f)
                        {
                            chargeAmount = 0f;

                            // Destroy any muzzle FX
                            DestroyMuzzleFX(beamModule.firePositionOffsetIndex);

                            // Unassign the beam from this weapon's fire position
                            beamItemKeyList[beamModule.firePositionOffsetIndex] = new S3DBeamItemKey(-1, -1, 0);

                            beamModule.DestroyBeam();
                        }
                    }
                }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR stickyWeapon.MoveBeam has been called on the wrong beam. isBeamEnabled: " + beamModule.IsBeamEnabled +
                  " weapon: " + gameObject.name + " firePositionOffsetIndex: " + beamModule.firePositionOffsetIndex); }
            #endif
        }

        /// <summary>
        /// [INCOMPLETE]
        /// </summary>
        /// <param name="target"></param>
        /// <param name="trfmPos"></param>
        /// <param name="trfmRight"></param>
        /// <param name="trfmUp"></param>
        /// <param name="trfmFwd"></param>
        /// <param name="trfmInvRot"></param>
        /// <param name="factionId"></param>
        /// <param name="directToTarget"></param>
        /// <param name="obstaclesBlockLineOfSight"></param>
        /// <param name="anyEnemy"></param>
        internal void UpdateLineOfSight(GameObject target, Vector3 trfmPos, Vector3 trfmRight, Vector3 trfmUp,
            Vector3 trfmFwd, Quaternion trfmInvRot, int factionId, bool directToTarget = false,
            bool obstaclesBlockLineOfSight = true, bool anyEnemy = true)
        {

        }

        #endregion

        #region Protected Non-Virtual Methods

        /// <summary>
        /// If required, add smoke at the fire point
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        protected void AddSmoke (int weaponButtonNumber, Vector3 firePoint, Vector3 fireDirection)
        {
            if (maxActiveSmokeEffects1 > 0)
            {
                int effectsTemplatePrefabID = StickyManager.NoPrefabID;

                // 1. Find a smoke prefab pool (S3DEffectsObjectTemplate)
                // 2. Are we less than the limit actively in use?
                // 3. Check if any "active" pooled prefabs have already despawned or respawned
                // 4. Ask the manager to activate another one from the pool

                if (weaponButtonNumber == 1)
                {
                    // Find a unqiue prefab types (S3DEffectsObjectTemplate) available for use with smoke
                    if (numSmokeEffects1 > 0)
                    {
                        effectsTemplatePrefabID = smokeEffects1PrefabIDs[numSmokeEffects1 > 1 ? randomSelectFX.Range(0, numSmokeEffects1 - 1) : 0];
                    }
                }
                else if (weaponButtonNumber == 2)
                {
                    // Find a unqiue prefab types (S3DEffectsObjectTemplate) available for use with smoke
                    if (numSmokeEffects2 > 0)
                    {
                        effectsTemplatePrefabID = smokeEffects2PrefabIDs[numSmokeEffects2 > 1 ? randomSelectFX.Range(0, numSmokeEffects2 - 1) : 0];
                    }
                }

                // Did we find a unique prefab template to use?
                if (effectsTemplatePrefabID != StickyManager.NoPrefabID)
                {
                    int smokeItemKeySlotIdx = -1;

                    // Find the first unallocated smoke "slot"
                    for (int seIdx = 0; seIdx < maxActiveSmokeEffects1; seIdx++)
                    {
                        S3DEffectItemKey effectItemKey = smokeItemKeyList[seIdx];

                        // Check if this StickyEffectsModule instance is still active
                        // AND hasn't been respawned by something else
                        if (effectItemKey.effectsObjectSequenceNumber > 0)
                        {
                            // If it is still an active smoke FX for this weapon, skip it
                            if (stickyManager.IsEffectsObjectCurrent(effectItemKey))
                            {
                                continue;
                            }
                            // If it has despawned OR respawned, unallocate it from this weapon
                            else
                            {
                                // Reset the effect item key
                                effectItemKey.Reset();
                                smokeItemKeyList[seIdx] = effectItemKey;

                                // We can reuse this slot
                                smokeItemKeySlotIdx = seIdx;
                                break;
                            }
                        }
                        else
                        {
                            // Found an available slot
                            smokeItemKeySlotIdx = seIdx;
                            break;
                        }
                    }

                    // Did we find an available slot to spawn a smoke effect from the pool?
                    if (smokeItemKeySlotIdx >= 0)
                    {
                        S3DInstantiateEffectsObjectParameters ieParms = new S3DInstantiateEffectsObjectParameters
                        {
                            effectsObjectPrefabID = effectsTemplatePrefabID,
                            position = firePoint,
                            rotation = Quaternion.LookRotation(fireDirection)
                        };

                        // Instantiate the smoke FX
                        StickyEffectsModule stickyEffectsModule = stickyManager.InstantiateEffectsObject(ref ieParms);

                        if (stickyEffectsModule != null)
                        {
                            smokeItemKeyList[smokeItemKeySlotIdx] = new S3DEffectItemKey(effectsTemplatePrefabID, ieParms.effectsObjectPoolListIndex, ieParms.effectsObjectSequenceNumber);

                            if (stickyEffectsModule.isReparented)
                            {
                                stickyEffectsModule.transform.SetParent(transform);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Transition from or to aiming. If held by a character, this updates
        /// their first or third person camera field of view and near clipping plane
        /// </summary>
        protected void AimTransition()
        {
            if (!isHeld)
            {
                isAimingSmoothDisable = false;
                isAimingSmoothEnable = false;
            }
            else if (isAimingSmoothEnable)
            {
                // Continue transitioning toward the fully aimed character FoV
                float newFoV = Mathf.Lerp(prevAimingFieldOfView, targetAimingFieldofView, Time.deltaTime * aimingSpeed);

                // Continue transitioning toward the fully aimed character camera near clipping plane
                float newNCP = Mathf.Lerp(prevAimingNearClipping, targetAimingNearClipping, Time.deltaTime * aimingSpeed);

                // Is it close?
                float deltaFoV = targetAimingFieldofView - newFoV;
                // If decreasing toward target, flip the delta
                if (deltaFoV < 0f) { deltaFoV = -deltaFoV; }

                if (deltaFoV < 0.01f)
                {
                    newFoV = targetAimingFieldofView;
                    newNCP = targetAimingNearClipping;
                    isAimingSmoothEnable = false;
                }

                stickyControlModule.lookCamera1.fieldOfView = newFoV;
                stickyControlModule.lookCamera1.nearClipPlane = newNCP;

                prevAimingFieldOfView = newFoV;
                prevAimingNearClipping = newNCP;
            }
            else if (isAimingSmoothDisable)
            {
                // Continue transitioning toward the non-aiming character FoV
                float newFoV = Mathf.Lerp(prevAimingFieldOfView, targetAimingFieldofView, Time.deltaTime * aimingSpeed);

                // Continue transitioning toward the non-aiming character camera near clipping plane
                float newNCP = Mathf.Lerp(prevAimingNearClipping, targetAimingNearClipping, Time.deltaTime * aimingSpeed);

                // Is it close?
                float deltaFoV = targetAimingFieldofView - newFoV;
                // If decreasing toward target, flip the delta
                if (deltaFoV < 0f) { deltaFoV = -deltaFoV; }

                if (deltaFoV < 0.01f)
                {
                    newFoV = targetAimingFieldofView;
                    newNCP = targetAimingNearClipping;
                    isAimingSmoothDisable = false;
                    aimOriginalFieldOfView = 0f;
                    aimOriginalNearClipping = 0f;
                    isAiming = false;
                }

                stickyControlModule.lookCamera1.fieldOfView = newFoV;
                stickyControlModule.lookCamera1.nearClipPlane = newNCP;

                prevAimingFieldOfView = newFoV;
                prevAimingNearClipping = newNCP;

                // If we stopped smooth disable of weapon aiming, advice the character.
                if (!isAimingSmoothDisable)
                {
                    stickyControlModule.WeaponAimSmoothDisabled(StickyInteractiveID);
                }
            }
        }

        /// <summary>
        /// Update the character animation controller
        /// 
        /// </summary>
        protected void AnimateCharacter(bool isIgnoreDamping)
        {
            if (stickyControlModule.IsAnimateEnabled)
            {
                float dTime = Time.deltaTime;

                // Loop through the list of animation actions that were loaded from
                // the WeaponAnimSets when the weapon was grabbed by a character.
                for (int aaIdx = 0; aaIdx < numCharacterAnimActions; aaIdx++)
                {
                    S3DAnimActionExt aa = animActionExtList[aaIdx];

                    int aaWeaponActionInt = (int)aa.weaponAction;
                    int aaParamTypeInt = (int)aa.parameterType;

                    // Ignore ExitActions and ones without a valid parameter type
                    if (!aa.isExitAction && aaParamTypeInt != S3DAnimAction.ParameterTypeNoneInt)
                    {
                        #region Custom Action

                        // Currently all custom actions get directed to an overridable method
                        if (aaWeaponActionInt == S3DAnimAction.WeaponActionCustomInt)
                        {
                            AnimateCharacterCustomAction(aa);
                        }
                        #endregion

                        #region Float Type
                        else if (aaParamTypeInt == S3DAnimAction.ParameterTypeFloatInt)
                        {
                            // Is this animate action configured to send a fixed float value to the animation controller parameter?
                            if ((int)aa.actionWeaponFloatValue == S3DAnimAction.ActionWeaponFloatValueFixedInt)
                            {
                                stickyControlModule.defaultAnimator.SetFloat(aa.paramHashCode, aa.fixedFloatValue, isIgnoreDamping ? 0f : aa.damping, dTime);
                            }
                            else
                            {
                                stickyControlModule.defaultAnimator.SetFloat(aa.paramHashCode, GetAnimateFloatValue(aa.actionWeaponFloatValue) * aa.floatMultiplier, isIgnoreDamping ? 0f : aa.damping, dTime);
                            }
                        }
                        #endregion

                        #region Bool
                        else if (aaParamTypeInt == S3DAnimAction.ParameterTypeBoolInt)
                        {
                            // Is this animate action configured to send a fixed bool value (true or false) to the animation controller parameter?
                            if ((int)aa.actionWeaponBoolValue == S3DAnimAction.ActionWeaponBoolValueFixedInt)
                            {
                                stickyControlModule.defaultAnimator.SetBool(aa.paramHashCode, aa.fixedBoolValue);
                            }
                            else
                            {
                                stickyControlModule.defaultAnimator.SetBool(aa.paramHashCode, aa.isInvert ? !GetAnimateBoolValue(aa.actionWeaponBoolValue) : GetAnimateBoolValue(aa.actionWeaponBoolValue));
                            }
                        }
                        #endregion

                        #region Trigger
                        else if (aaParamTypeInt == S3DAnimAction.ParameterTypeTriggerInt)
                        {
                            // Triggers are only set if the condition is true
                            if (GetAnimateTriggerValue(aa.actionWeaponTriggerValue))
                            {
                                stickyControlModule.defaultAnimator.SetTrigger(aa.paramHashCode);
                            }
                        }
                        #endregion

                        #region Integer
                        else if (aaParamTypeInt == S3DAnimAction.ParameterTypeIntegerInt)
                        {
                            stickyControlModule.defaultAnimator.SetInteger(aa.paramHashCode, GetAnimateIntegerValue(aa.actionWeaponIntegerValue));
                        }
                        #endregion
                    }
                }
            }
        }

        /// <summary>
        /// When the weapon is being dropped, equipped, socketed or stashed, we might wish to set a trigger
        /// variable in the Animator controller to exit the current state of an animation layer.
        /// </summary>
        /// <param name="isDropped"></param>
        /// <param name="isEquipped"></param>
        /// <param name="isSocketed"></param>
        /// <param name="isStashed"></param>
        protected void AnimateCharacterStateExit(bool isDropped, bool isEquipped, bool isSocketed, bool isStashed)
        {
            if (isHeld && stickyControlModule != null)
            {
                for (int aaIdx = 0; aaIdx < numCharacterAnimActions; aaIdx++)
                {
                    S3DAnimActionExt aa = animActionExtList[aaIdx];

                    int aaParamTypeInt = (int)aa.parameterType;
                    int aaWeaponActionInt = (int)aa.weaponAction;

                    if (
                        (isDropped && aaWeaponActionInt == S3DAnimAction.WeaponActionDroppedInt) ||
                        (isEquipped && aaWeaponActionInt == S3DAnimAction.WeaponActionEquippedInt) ||
                        (isSocketed && aaWeaponActionInt == S3DAnimAction.WeaponActionSocketedInt) ||
                        (isStashed && aaWeaponActionInt == S3DAnimAction.WeaponActionStashedInt)
                    )
                    {
                        if (aaParamTypeInt == S3DAnimAction.ParameterTypeTriggerInt)
                        {
                            stickyControlModule.defaultAnimator.SetTrigger(aa.paramHashCode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// When a weapon is no longer being held, some animation parameters for
        /// a character may need to be reset. This (SHOULD) happens before the weapon is
        /// dropped, equipped, socketed, or stashed.
        /// CURRENLTY only sets bool params to false.
        /// FUTURE: Possibly store if this an anim value should be reset
        /// </summary>
        protected void AnimateCharacterReset()
        {
            if (isHeld && stickyControlModule != null)
            {
                // Loop through any Anim Actions that were added from Weapon Anim Sets for this weapon
                for (int aaIdx = 0; aaIdx < numCharacterAnimActions; aaIdx++)
                {
                    S3DAnimActionExt aa = animActionExtList[aaIdx];

                    int aaParamTypeInt = (int)aa.parameterType;

                    if (aaParamTypeInt == S3DAnimAction.ParameterTypeBoolInt)
                    {
                        stickyControlModule.defaultAnimator.SetBool(aa.paramHashCode, false);
                    }
                }
            }
        }

        /// <summary>
        /// Set any animation parameters as configured in the editor.
        /// Use isIgnoreDamping when you want to override all S3DAnimAction.damping values with 0.0.
        /// This is useful when you want to say disable movement and immediately set those values to 0.
        /// If they remain > 0, and Animate is also disabled, the animation will continue running.
        /// </summary>
        /// <param name="isIgnoreDamping"></param>
        protected void AnimateWeapon(bool isIgnoreDamping)
        {
            float dTime = Time.deltaTime;

            #region Layer Blend Weights
            // If necessary, update animator layer blend weights
            for (int aLayerIdx = 0; aLayerIdx < numAnimLayers; aLayerIdx++)
            {
                S3DAnimLayerData s3dAnimLayerData = animlayerDataList[aLayerIdx];

                if (s3dAnimLayerData.isBlendingIn)
                {
                    s3dAnimLayerData.blendWeight += dTime / s3dAnimLayerData.blendInDuration;
                    if (s3dAnimLayerData.blendWeight >= 1f)
                    {
                        s3dAnimLayerData.blendWeight = 1f;
                        s3dAnimLayerData.isBlendingIn = false;
                        if (s3dAnimLayerData.callBackOnBlendedIn != null) { s3dAnimLayerData.callBackOnBlendedIn.Invoke(s3dAnimLayerData, aLayerIdx); }
                    }

                    defaultAnimator.SetLayerWeight(aLayerIdx, s3dAnimLayerData.blendWeight);
                }
                else if (s3dAnimLayerData.isBlendingOut)
                {
                    s3dAnimLayerData.blendWeight -= dTime / s3dAnimLayerData.blendOutDuration;
                    if (s3dAnimLayerData.blendWeight <= 0f)
                    {
                        s3dAnimLayerData.blendWeight = 0f;
                        s3dAnimLayerData.isBlendingOut = false;
                        if (s3dAnimLayerData.callBackOnBlendedOut != null) { s3dAnimLayerData.callBackOnBlendedOut.Invoke(s3dAnimLayerData, aLayerIdx); }
                    }
                    
                    defaultAnimator.SetLayerWeight(aLayerIdx, s3dAnimLayerData.blendWeight);
                }
            }
            #endregion

            // Loop through all the animate actions. If the animator is null numAnimateActions is set to 0.
            for (int aaIdx = 0; aaIdx < numAnimateActions; aaIdx++)
            {
                S3DAnimAction aa = s3dAnimActionList[aaIdx];

                if (CheckAnimateConditions(aa))
                {
                    // Currently WeaponAction is mostly cosmetic (apart from Custom) but may be used in
                    // the future when potentially users can set up their own.
                    // Might be able to optimise based on the WeaponAction by skipping actions
                    // that don't apply...
                    int aaWeaponActionInt = (int)aa.weaponAction;

                    int aaParamTypeInt = (int)aa.parameterType;

                    if (aaParamTypeInt != S3DAnimAction.ParameterTypeNoneInt)
                    {
                        #region Float
                        if (aaParamTypeInt == S3DAnimAction.ParameterTypeFloatInt)
                        {
                            if (aaWeaponActionInt == S3DAnimAction.WeaponActionCustomInt)
                            {
                                /// TODO - Test sending a blendRate value with a custom action float
                                defaultAnimator.SetFloat(aa.paramHashCode, aa.customActionFloatValue * aa.floatMultiplier);
                                // Reset custom value after use
                                aa.customActionFloatValue = 0f;
                            }
                            // Is this animate action configured to send a fixed float value to the animation controller parameter?
                            else if ((int)aa.actionWeaponFloatValue == S3DAnimAction.ActionWeaponFloatValueFixedInt)
                            {
                                defaultAnimator.SetFloat(aa.paramHashCode, aa.fixedFloatValue, isIgnoreDamping ? 0f : aa.damping, dTime);
                            }
                            else
                            {
                                defaultAnimator.SetFloat(aa.paramHashCode, GetAnimateFloatValue(aa.actionWeaponFloatValue) * aa.floatMultiplier, isIgnoreDamping ? 0f : aa.damping, dTime);
                            }
                        }
                        #endregion

                        #region Bool
                        else if (aaParamTypeInt == S3DAnimAction.ParameterTypeBoolInt)
                        {
                            if (aaWeaponActionInt == S3DAnimAction.WeaponActionCustomInt)
                            {
                                defaultAnimator.SetBool(aa.paramHashCode, aa.isInvert ? !aa.customActionBoolValue : aa.customActionBoolValue);
                                // Reset custom value after use (should be on by default)
                                if (aa.isResetCustomAfterUse && !aa.isToggle) { aa.customActionBoolValue = false; }
                            }
                            // Is this animate action configured to send a fixed bool value (true or false) to the animation controller parameter?
                            else if ((int)aa.actionWeaponBoolValue == S3DAnimAction.ActionWeaponBoolValueFixedInt)
                            {
                                defaultAnimator.SetBool(aa.paramHashCode, aa.fixedBoolValue);
                            }
                            else
                            {
                                defaultAnimator.SetBool(aa.paramHashCode, aa.isInvert ? !GetAnimateBoolValue(aa.actionWeaponBoolValue) : GetAnimateBoolValue(aa.actionWeaponBoolValue));
                            }
                        }
                        #endregion

                        #region Trigger
                        else if (aaParamTypeInt == S3DAnimAction.ParameterTypeTriggerInt)
                        {
                            // Triggers are only set if the condition is true
                            if (aaWeaponActionInt == S3DAnimAction.WeaponActionCustomInt)
                            {
                                if (aa.customActionTriggerValue)
                                {
                                    defaultAnimator.SetTrigger(aa.paramHashCode);
                                }
                                // Reset custom value after use
                                aa.customActionTriggerValue = false;
                            }
                            else if (GetAnimateTriggerValue(aa.actionWeaponTriggerValue))
                            {
                                defaultAnimator.SetTrigger(aa.paramHashCode);
                            }
                        }
                        #endregion

                        #region Integer
                        else if (aaParamTypeInt == S3DAnimAction.ParameterTypeIntegerInt)
                        {
                            if (aaWeaponActionInt == S3DAnimAction.WeaponActionCustomInt)
                            {
                                defaultAnimator.SetInteger(aa.paramHashCode, aa.customActionIntegerValue);
                                // Reset custom value after use
                                aa.customActionIntegerValue = 0;
                            }
                            else
                            {
                                defaultAnimator.SetInteger(aa.paramHashCode, GetAnimateIntegerValue(aa.actionWeaponIntegerValue));
                            }
                        }
                        #endregion
                    }
                }
            }
        }

        /// <summary>
        /// Apply (or revert) any matching weapon anim sets for the character holding the weapon.
        /// This includes animation clips and animation actions.
        /// NOTE: Currently do nothing to "revert" anim actions, except clear the local list.
        /// </summary>
        protected void ApplyOrRevertWeaponAnimSets (bool isApplied)
        {
            if (isWeaponInitialised && isWeaponEnabled && isHeld)
            {
                int characterModelID = stickyControlModule.modelId;

                // They should be always unverified when being reverted (not Applied)
                isWpnAnimSetsVerified = false;

                bool isInvalidParmNameDetected = false;

                if (isApplied)
                {
                    // Clear the list of anim actions
                    animActionExtList.Clear();

                    // Remember the current state IsAimIKWhenNotAiming for the character
                    stickyControlModule.savedIsAimIKWhenNotAiming = stickyControlModule.IsAimIKWhenNotAiming;
                    // Remember the current weapon offset from first-person camera when aiming
                    stickyControlModule.savedAimIKFPWeaponOffset = stickyControlModule.AimIKFPWeaponOffset;
                    // Remember the current FP camera near clipping plane when aiming
                    stickyControlModule.savedAimIKFPNearClippingPlane = stickyControlModule.AimIKFPNearClippingPlane;

                    // Always save potentially overridden weapon settings
                    savedIsOnlyFireWhenAiming1 = isOnlyFireWhenAiming1;
                    savedIsOnlyFireWhenAiming2 = isOnlyFireWhenAiming2;
                }

                for (int wasIdx = 0; wasIdx < numWeaponAnimSets; wasIdx++)
                {
                    // These should never be null as those are removed in RefreshAnimateSettings(..)
                    S3DWeaponAnimSet weaponAnimSet = s3dWeaponAnimSetList[wasIdx];

                    // Get the array outside the IndexOf() to avoid GC
                    int[] modelIDs = weaponAnimSet.stickyModelIDs;

                    // Does this weaponAnimSet apply to the character holding the weapon?
                    if (weaponAnimSet.isAppliedToAllCharacters || (modelIDs != null && System.Array.IndexOf(modelIDs, characterModelID) >= 0))
                    {                        
                        #region Animation Clips
                        int numAnimClipPairs = weaponAnimSet.animClipPairList == null ? 0 : weaponAnimSet.animClipPairList.Count;

                        if (isApplied)
                        {
                            // Apply settings

                            // Only override the character when this is true on the S3DWeaponAnimSet
                            if (weaponAnimSet.isAimIKWhenNotAiming) { stickyControlModule.IsAimIKWhenNotAiming = true; }

                            stickyControlModule.AimIKTurnDelay = weaponAnimSet.aimIKTurnDelay;

                            stickyControlModule.AimIKFPWeaponOffset = weaponAnimSet.aimIKFPWeaponOffset;
                            stickyControlModule.AimIKFPNearClippingPlane = weaponAnimSet.aimIKFPNearClippingPlane;
                            stickyControlModule.IsAimTPUsingFPCamera = weaponAnimSet.isAimTPUsingFPCamera;
                            stickyControlModule.HeldTransitionDuration = weaponAnimSet.heldTransitionDuration;

                            if (weaponAnimSet.isFreeLookWhenHeld) { stickyControlModule.IsFreeLookWhenWeaponHeld = true; }

                            // Loop through all the Clip Pairs in the Weapon Anim Set scriptable object
                            for (int animClipPairIdx = 0; animClipPairIdx < numAnimClipPairs; animClipPairIdx++)
                            {
                                S3DAnimClipPair clipPair = weaponAnimSet.animClipPairList[animClipPairIdx];

                                stickyControlModule.ReplaceAnimationClipNoRef(clipPair.originalClip, clipPair.replacementClip);

                                //Debug.Log("[DEBUG] Apply clip " + clipPair.originalClip.name + " with " + clipPair.replacementClip.name + " from " + weaponAnimSet.name);
                            }
                        }
                        else
                        {
                            // Revert settings

                            // Sometimes we may wish to delay reverting animations to avoid things like the weapon held animation suddenly switching
                            // to the wrong one.
                            if (numAnimClipPairs > 0 && weaponAnimSet.animClipPairRevertDelay > 0f)
                            {
                                stickyControlModule.revertAnimClipsDelayed = stickyControlModule.StartCoroutine(stickyControlModule.ReplaceAnimationClipsDelayed(weaponAnimSet.animClipPairList, weaponAnimSet.animClipPairRevertDelay));
                            }
                            else
                            {
                                // Loop through all the Clip Pairs in the Weapon Anim Set scriptable object
                                for (int animClipPairIdx = 0; animClipPairIdx < numAnimClipPairs; animClipPairIdx++)
                                {
                                    S3DAnimClipPair clipPair = weaponAnimSet.animClipPairList[animClipPairIdx];

                                    stickyControlModule.ReplaceAnimationClipNoRef(clipPair.originalClip, clipPair.originalClip);

                                    //Debug.Log("[DEBUG] Revert clip " + clipPair.originalClip.name + " from " + weaponAnimSet.name);
                                }
                            }
                        }
                        #endregion

                        #region Anim Action Ext

                        if (stickyControlModule.IsAnimateEnabled)
                        {
                            if (isApplied)
                            {
                                // Attempt to build up a list of valid Animate Actions that can be applied to the character holding
                                // this weapon when it does things like fire or reload.

                                int numAnimActions = weaponAnimSet.animActionExtList == null ? 0 : weaponAnimSet.animActionExtList.Count;

                                for (int aaIdx = 0; aaIdx < numAnimActions; aaIdx++)
                                {
                                    S3DAnimActionExt animAction = weaponAnimSet.animActionExtList[aaIdx];

                                    if (weaponAnimSet.isSkipParmVerify)
                                    {
                                        animActionExtList.Add(animAction);
                                    }
                                    // Does this animate action parameter name appear in the character's animation controller?
                                    else if (stickyControlModule.VerifyAnimParameter(animAction.parameterName, animAction.parameterType) != 0)
                                    {
                                        if (animAction.paramHashCode == 0)
                                        {
                                            isInvalidParmNameDetected = true;
                                            #if UNITY_EDITOR
                                            Debug.LogWarning("ERROR Animate Action " + (aaIdx + 1) + " on " + weaponAnimSet.name + " has an a paramHashCode of 0. PLEASE REPORT!");
                                            #endif
                                        }
                                        else
                                        {
                                            animActionExtList.Add(animAction);
                                        }
                                    }
                                    else
                                    {
                                        isInvalidParmNameDetected = true;
                                        #if UNITY_EDITOR
                                        Debug.LogWarning("ERROR Animate Action " + (aaIdx + 1).ToString("00") + " on " + weaponAnimSet.name + " cannot find paramater in character animation controller. Check the spelling, case, and the Parameter type.");
                                        #endif
                                    }
                                }
                            }

                            // When reverting, AnimateCharacterReset() is called below.
                        }
                        else
                        {
                            isInvalidParmNameDetected = true;
                            #if UNITY_EDITOR
                            Debug.LogWarning("ERROR: Could not apply WeaponAnimSet Actions for " + name + " to " + stickyControlModule.name + " because the Animate is not enabled on the character.");
                            #endif
                        }

                        #endregion

                        #region Weapon Settings

                        if (isApplied && weaponAnimSet.isWeaponSettingsOverride)
                        {
                            isOnlyFireWhenAiming1 = weaponAnimSet.isOnlyFireWhenAiming1;
                            isOnlyFireWhenAiming2 = weaponAnimSet.isOnlyFireWhenAiming2;
                        }

                        #endregion
                    }
                }

                if (isApplied && !isInvalidParmNameDetected) { isWpnAnimSetsVerified = true; }

                if (!isApplied)
                {
                    // Restore the state of IsAimIKWhenNotAiming to the character
                    stickyControlModule.IsAimIKWhenNotAiming = stickyControlModule.savedIsAimIKWhenNotAiming;

                    // Restore the previous weapon offset from the first person camera when aiming
                    stickyControlModule.AimIKFPWeaponOffset = stickyControlModule.savedAimIKFPWeaponOffset;

                    // Restore the previous FP camera near clipping plane when aiming
                    stickyControlModule.AimIKFPNearClippingPlane = stickyControlModule.savedAimIKFPNearClippingPlane;

                    // Reset to default (turn off after use)
                    stickyControlModule.IsAimTPUsingFPCamera = false;

                    // Reset to default 0.
                    stickyControlModule.AimIKTurnDelay = 0f;

                    // Reset to default
                    stickyControlModule.HeldTransitionDuration = 0.5f;

                    // Reset to default (Free Look is off when a weapon is held)
                    stickyControlModule.IsFreeLookWhenWeaponHeld = false;

                    AnimateCharacterReset();

                    // Clear the list of anim actions
                    animActionExtList.Clear();

                    // Always restore weapon settings
                    isOnlyFireWhenAiming1 = savedIsOnlyFireWhenAiming1;
                    isOnlyFireWhenAiming2 = savedIsOnlyFireWhenAiming2;
                }

                numCharacterAnimActions = animActionExtList.Count;
            }
        }

        /// <summary>
        /// Check to see if all the conditions (if any) are satisfied for an animate action.
        /// NOTE: This assumes it runs in Fixed Update because items like hasFired1&2 can be
        /// true over multiple Update frames.
        /// </summary>
        /// <param name="s3dAnimAction"></param>
        /// <returns></returns>
        protected bool CheckAnimateConditions (S3DAnimAction s3dAnimAction)
        {
            if (s3dAnimAction == null || s3dAnimAction.paramHashCode == 0) { return false; }
            else if (s3dAnimAction.numConditions < 1) { return true; }
            else
            {
                bool areAllConditionsTrue = true;

                // If any conditions are not true, exit out
                for (int acIdx = 0; areAllConditionsTrue && acIdx < s3dAnimAction.numConditions; acIdx++)
                {
                    S3DAnimCondition s3dAnimCondition = s3dAnimAction.s3dAnimConditionList[acIdx];

                    int conditionTypeInt = (int)s3dAnimCondition.conditionType;

                    switch (s3dAnimCondition.actionWeaponCondition)
                    {
                        case S3DAnimCondition.ActionWeaponCondition.HasFired1:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? hasFired1 : !hasFired1;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.HasFired2:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? hasFired2 : !hasFired2;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.HasEmptyFired1:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? hasEmptyFired1 : !hasEmptyFired1;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.HasEmptyFired2:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? hasEmptyFired2 : !hasEmptyFired2;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.HasReloadStarted1:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? hasReloadStarted1 : !hasReloadStarted1;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.HasReloadStarted2:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? hasReloadStarted2 : !hasReloadStarted2;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.HasReloadFinished1:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? hasReloadFinished1 : !hasReloadFinished1;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.HasReloadFinished2:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? hasReloadFinished2 : !hasReloadFinished2;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.IsHeld:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? isHeld : !isHeld;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.IsReloading1:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? isReloading1 : !isReloading1;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.IsReloading2:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? isReloading2 : !isReloading2;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.IsReloadUnequipping1:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? isReloadUnequipping1 : !isReloadUnequipping1;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.IsReloadUnequipping2:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? isReloadUnequipping2 : !isReloadUnequipping2;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.IsReloadEquipping1:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? isReloadEquipping1 : !isReloadEquipping1;
                            break;
                        case S3DAnimCondition.ActionWeaponCondition.IsReloadEquipping2:
                            areAllConditionsTrue = conditionTypeInt == S3DAnimCondition.ConditionTypeAndInt ? isReloadEquipping2 : !isReloadEquipping2;
                            break;
                        default: break;
                    }
                }

                return areAllConditionsTrue;
            }
        }


        protected void ConfigureBeamItemKeys(int numKeysRequired)
        {
            int numBeamItemKeys = beamItemKeyList == null ? 0 : beamItemKeyList.Count;

            // Note: This may need changing for weapon.SetFireDirection(Vector3) if we implement it
            // like Ship.SetWeaponFireDirection(int, Vector3) in SSC.
            if (beamItemKeyList == null) { beamItemKeyList = new List<S3DBeamItemKey>(numKeysRequired); }
            else
            {
                if (numBeamItemKeys != numKeysRequired) { beamItemKeyList.Clear(); }
            }

            for (int fIdx = 0; fIdx < numKeysRequired; fIdx++)
            {
                if (fIdx >= numBeamItemKeys)
                {
                    beamItemKeyList.Add(new S3DBeamItemKey(-1, -1, 0));
                    numBeamItemKeys++;
                }
                // Reset exiting beam keys. This might need changing for SetFireDirection(Vector3) if we implement it
                else
                {
                    S3DBeamItemKey beamItemKey = new S3DBeamItemKey(-1, -1, 0);
                    beamItemKeyList[fIdx] = beamItemKey;
                }
            }
        }

        /// <summary>
        /// Configure item keys for each fire point offset. There should always be one S3DEffectItemKey
        /// even if there are no custom fire point offsets.
        /// Also used for Reloading SoundFX and SmokeFX.
        /// </summary>
        /// <param name="itemKeyList">Passed in as a reference as it might be null</param>
        /// <param name="numKeysRequired"></param>
        protected void ConfigureEffectsItemKeys(ref List<S3DEffectItemKey> itemKeyList, int numKeysRequired)
        {
            int numItemKeys = itemKeyList == null ? 0 : itemKeyList.Count;

            if (itemKeyList == null) { itemKeyList = new List<S3DEffectItemKey>(numKeysRequired); }
            else
            {
                if (numItemKeys != numKeysRequired) { itemKeyList.Clear(); }
            }

            for (int fIdx = 0; fIdx < numKeysRequired; fIdx++)
            {
                if (fIdx >= numItemKeys)
                {
                    itemKeyList.Add(new S3DEffectItemKey(-1, -1, 0));
                    numItemKeys++;
                }
                else
                {
                    S3DEffectItemKey effectsItemKey = new S3DEffectItemKey(-1, -1, 0);
                    itemKeyList[fIdx] = effectsItemKey;
                }
            }
        }

        /// <summary>
        /// Attempt to enable or disable animate
        /// </summary>
        /// <param name="isEnabled"></param>
        protected void EnableOrDisableAnimate (bool isEnabled)
        {
            isAnimateEnabled = isEnabled;

            if (isEnabled)
            {
                if (defaultAnimator == null)
                {
                    isAnimateEnabled = false;
                    #if UNITY_EDITOR
                    Debug.LogWarning("StickyWeapon on " + name + " could not enable animate because there is no Animator set on the Animate tab");
                    #endif
                }
                else if (defaultAnimator.runtimeAnimatorController == null)
                {          
                    isAnimateEnabled = false;
                    //defaultAnimator.speed = 0f;
                    #if UNITY_EDITOR
                    Debug.LogWarning("StickyWeapon on " + name + " could not enable animate because there is no controller set on the Animator component");
                    #endif
                }
                else
                {
                    // Get number of Animator layers
                    numAnimLayers = defaultAnimator.layerCount;

                    if (animlayerDataList == null) { animlayerDataList = new List<S3DAnimLayerData>(numAnimLayers); }
                    else { animlayerDataList.Clear(); }
                    
                    // Populate the list with layer data from the animator
                    for (int aLayerIdx = 0; aLayerIdx < numAnimLayers; aLayerIdx++)
                    {
                        S3DAnimLayerData s3dAnimLayerData = new S3DAnimLayerData()
                        {
                            layerIndex = aLayerIdx,
                            blendWeight = defaultAnimator.GetLayerWeight(aLayerIdx)
                        };

                        animlayerDataList.Add(s3dAnimLayerData);
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to turn the laser sight on or off
        /// </summary>
        /// <param name="isEnabled"></param>
        protected void EnableOrDisableLaserSight(bool isEnabled)
        {
            if (isLaserSightReady)
            {
                laserSightLineRenderer.enabled = isEnabled;

                isLaserSightOn = isEnabled;
            }
        }

        /// <summary>
        /// Get standard in-built boolean values from weapon to pass to an animation controller.
        /// These typically indicate a state of the weapon. e.g. is reloading.
        /// NOTE: This assumes it runs in Fixed Update because items like hasFired1&2 can be
        /// true over multiple Update frames.
        /// </summary>
        /// <param name="actionWeaponBoolValue"></param>
        /// <returns></returns>
        protected bool GetAnimateBoolValue (S3DAnimAction.ActionWeaponBoolValue actionWeaponBoolValue)
        {
            bool bValue = false;

            // The cases must match those found in the S3DAnimAction.ActionWeaponBoolValue enumeration 
            switch (actionWeaponBoolValue)
            {
                case S3DAnimAction.ActionWeaponBoolValue.HasFired1:
                    bValue = hasFired1;
                    break;
                case S3DAnimAction.ActionWeaponBoolValue.HasFired2:
                    bValue = hasFired2;
                    break;
                case S3DAnimAction.ActionWeaponBoolValue.HasEmptyFired1:
                    bValue = hasEmptyFired1;
                    break;
                case S3DAnimAction.ActionWeaponBoolValue.HasEmptyFired2:
                    bValue = hasEmptyFired2;
                    break;
                case S3DAnimAction.ActionWeaponBoolValue.HasReloadStarted1:
                    bValue = hasReloadStarted1;
                    break;
                case S3DAnimAction.ActionWeaponBoolValue.HasReloadStarted2:
                    bValue = hasReloadStarted2;
                    break;
                case S3DAnimAction.ActionWeaponBoolValue.HasReloadFinished1:
                    bValue = hasReloadFinished1;
                    break;
                case S3DAnimAction.ActionWeaponBoolValue.HasReloadFinished2:
                    bValue = hasReloadFinished2;
                    break;
                case S3DAnimAction.ActionWeaponBoolValue.IsReloading1:
                    bValue = isReloading1;
                    break;
                case S3DAnimAction.ActionWeaponBoolValue.IsReloading2:
                    bValue = isReloading2;
                    break;
                case S3DAnimAction.ActionWeaponBoolValue.IsHeld:
                    bValue = isHeld;
                    break;
                case S3DAnimAction.ActionWeaponBoolValue.IsAiming:
                    bValue = isAiming;
                    break;
                default: break;
            }

            return bValue;
        }


        /// <summary>
        /// Get standard in-built float values from weapon to pass to an animation controller.
        /// e.g. the weapon reload duration
        /// </summary>
        /// <param name="actionWeaponFloatValue"></param>
        /// <returns></returns>
        protected float GetAnimateFloatValue (S3DAnimAction.ActionWeaponFloatValue actionWeaponFloatValue)
        {
            float fValue = 0f;

            // The cases must match those found in the S3DAnimAction.ActionWeaponFloatValue enumeration 

            switch (actionWeaponFloatValue)
            {
                case S3DAnimAction.ActionWeaponFloatValue.ReloadDuration1:
                    fValue = reloadDuration1;
                    break;
                case S3DAnimAction.ActionWeaponFloatValue.ReloadDuration2:
                    fValue = reloadDuration2;
                    break;
                case S3DAnimAction.ActionWeaponFloatValue.ReloadDelay1:
                    fValue = reloadDelay1;
                    break;
                case S3DAnimAction.ActionWeaponFloatValue.ReloadDelay2:
                    fValue = reloadDelay2;
                    break;
                default: break;
            }

            return fValue;
        }

        /// <summary>
        /// Get standard in-built integer values from weapon to pass to an animation controller.
        /// </summary>
        /// <param name="actionWeaponIntegerValue"></param>
        /// <returns></returns>
        protected int GetAnimateIntegerValue(S3DAnimAction.ActionWeaponIntegerValue actionWeaponIntegerValue)
        {
            int ivalue = 0;

            // The cases must match those found in the S3DAnimAction.ActionWeaponIntegerValue enumeration 

            switch (actionWeaponIntegerValue)
            {
                case S3DAnimAction.ActionWeaponIntegerValue.None:
                    ivalue = 0;
                    break;

                default: break;
            }

            return ivalue;
        }

        /// <summary>
        /// Get standard in-built trigger (boolean) values from weapon to pass to an animation controller.
        /// These typically occur once as a single event.
        /// </summary>
        /// <param name="actionWeaponTriggerValue"></param>
        /// <returns></returns>
        protected bool GetAnimateTriggerValue(S3DAnimAction.ActionWeaponTriggerValue actionWeaponTriggerValue)
        {
            bool bValue = false;

            switch (actionWeaponTriggerValue)
            {
                case S3DAnimAction.ActionWeaponTriggerValue.HasFired1:
                    bValue = hasFired1;
                    break;
                case S3DAnimAction.ActionWeaponTriggerValue.HasFired2:
                    bValue = hasFired2;
                    break;
                case S3DAnimAction.ActionWeaponTriggerValue.HasEmptyFired1:
                    bValue = hasEmptyFired1;
                    break;
                case S3DAnimAction.ActionWeaponTriggerValue.HasEmptyFired2:
                    bValue = hasEmptyFired2;
                    break;
                case S3DAnimAction.ActionWeaponTriggerValue.HasReloadStarted1:
                    bValue = hasReloadStarted1;
                    break;
                case S3DAnimAction.ActionWeaponTriggerValue.HasReloadStarted2:
                    bValue = hasReloadStarted2;
                    break;
                case S3DAnimAction.ActionWeaponTriggerValue.HasReloadFinished1:
                    bValue = hasReloadFinished1;
                    break;
                case S3DAnimAction.ActionWeaponTriggerValue.HasReloadFinished2:
                    bValue = hasReloadFinished2;
                    break;
                default: break;
            }

            return bValue;
        }

        /// <summary>
        /// If this weapon is held by a character, is the character also
        /// holding a magazine that fits this weapon?
        /// Check if ammo is compatible in magazine with firing mechanism.
        /// Returns true if held and is compatible.
        /// Checks the left hand first, then the right hand if the left
        /// is not holding a suitable magazine.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="allowEmpty">If false and the magazine is empty it will return false</param>
        /// <param name="isHeldInLeftHand">Returns true if held in left hand</param>
        /// <param name="stickyMagazine">Returns the magazine held in the hand of the character</param>
        /// <returns></returns>
        protected bool GetMagazineFromCharacterHand (int weaponButtonNumber, bool allowEmpty, ref bool isHeldInLeftHand, ref StickyMagazine stickyMagazine)
        {
            bool isMagCompatible = false;

            stickyMagazine = null;

            if (isHeld && stickyControlModule != null)
            {
                if (stickyControlModule.IsLeftHandHoldingMagazine)
                {
                    stickyMagazine = (StickyMagazine)stickyControlModule.LeftHandInteractive;

                    if (allowEmpty || stickyMagazine.AmmoCount > 0)
                    {
                        if (IsMagCompatible(weaponButtonNumber, stickyMagazine) && IsAmmoCompatible(weaponButtonNumber, stickyMagazine.CompatibleAmmoTypeInt))
                        {
                            isMagCompatible = true;
                            isHeldInLeftHand = true;
                        }
                    }

                    if (!isMagCompatible) { stickyMagazine = null; }
                }

                if (!isMagCompatible && stickyControlModule.IsRightHandHoldingMagazine)
                {
                    stickyMagazine = (StickyMagazine)stickyControlModule.RightHandInteractive;

                    if (allowEmpty || stickyMagazine.AmmoCount > 0)
                    {
                        if (IsMagCompatible(weaponButtonNumber, stickyMagazine) && IsAmmoCompatible(weaponButtonNumber, stickyMagazine.CompatibleAmmoTypeInt))
                        {
                            isMagCompatible = true;
                            isHeldInLeftHand = false;
                        }
                    }

                    if (!isMagCompatible) { stickyMagazine = null; }
                }
            }

            return isMagCompatible;
        }

        /// <summary>
        /// Is this weapon held by a character and is there a compatible non-empty magazine in the Stash (inventory)?
        /// If true, returns the storeItemID, else returns S3DStoreItem.NoStoreItem.
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        /// <param name="allowEmpty">If false and the magazine is empty it will return false</param>
        /// <param name="stickyMagazine"></param>
        /// <returns>The storeItemID or S3DStoreItem.NoStoreItem</returns>
        protected int GetMagazineFromCharacterStash (int weaponButtonNumber, bool allowEmpty, ref StickyMagazine stickyMagazine)
        {
            int storeItemID = S3DStoreItem.NoStoreItem;

            stickyMagazine = null;

            if (isHeld && stickyControlModule != null)
            {
                if (weaponButtonNumber == 1)
                {
                    storeItemID = stickyControlModule.GetLastStashMagazineItemID(compatibleMag1, compatibleAmmo1 , false);
                }
                else if (weaponButtonNumber == 2)
                {
                    storeItemID = stickyControlModule.GetLastStashMagazineItemID(compatibleMag2, compatibleAmmo2, false);
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: GetMagazineFromCharacterStash on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms."); }
                #endif

                if (storeItemID != S3DStoreItem.NoStoreItem)
                {
                    // We discovered the magazine above, so we can safely retrieve it and
                    // cast it to a StickyMagazine.
                    S3DStoreItem storeItem = stickyControlModule.GetStashItem(storeItemID);
                    stickyMagazine = (StickyMagazine)storeItem.stickyInteractive;
                }
            }

            return storeItemID;
        }

        /// <summary>
        /// Get the player camera (if any)
        /// </summary>
        /// <returns></returns>
        protected Camera GetPlayerCamera()
        {
            if (isHeld && stickyControlModule != null)
            {
                return stickyControlModule.GetCurrentPlayerCamera;
            }
            else { return null; }
        }

        /// <summary>
        /// Get the player camera transform (if any)
        /// </summary>
        /// <returns></returns>
        protected Transform GetPlayerCameraTransform()
        {
            if (isHeld && stickyControlModule != null)
            {
                Camera _cam = stickyControlModule.GetCurrentPlayerCamera;

                return _cam == null ? null : _cam.transform;
            }
            else { return null; }
        }

        /// <summary>
        /// Is the weapon facing the target (if any) of the firing mechanism?
        /// NOTE: This doesn't check for Line of Sight.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="degVariance">The number of degrees variance permitted</param>
        /// <returns></returns>
        protected bool IsFacingTarget (int weaponButtonNumber, float degVariance)
        {
            bool isFacingTarget = false;

            if (isTargetSticky3DAssigned || isTargetTransformAssigned)
            {
                // Currently primary and secondary firing button use the same fire points and direction
                Vector3 targetPosWS = isTargetSticky3DAssigned ? targetSticky3D.GetWorldEyePosition() : targetTransform.position;

                // Get the local space offset between world space firepoint and the target
                Vector3 targetOffsetLS = trfmInvRot * (targetPosWS - GetWorldFireBasePosition());

                // Calculate the angle to rotate toward aim position
                float azimuthAngle = Mathf.Atan2(targetOffsetLS.x, targetOffsetLS.z) * Mathf.Rad2Deg;

                // Calculate the angle of inclination to rotate toward aim position
                float altitudeAngle = Mathf.Atan(targetOffsetLS.y /
                    Mathf.Sqrt((targetOffsetLS.x * targetOffsetLS.x) + (targetOffsetLS.z * targetOffsetLS.z))) * Mathf.Rad2Deg;

                isFacingTarget = azimuthAngle > -degVariance && azimuthAngle < degVariance && altitudeAngle > -degVariance && altitudeAngle < degVariance;

                //Debug.Log("[DEBUG] azimuthAngle: " + azimuthAngle.ToString("0.00") + " altitudeAngle: " + altitudeAngle.ToString("0.00") + " is Facing: + " + isFacingTarget + " T:" + Time.time);
            }

            return isFacingTarget;
        }

        /// <summary>
        /// Is the weapon ready to fire?
        /// Weapons have 1 or 2 fire buttons. Most weapons only have a single (primary)
        /// firing mechanism.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <returns></returns>
        protected bool IsWeaponReady(int weaponButtonNumber)
        {
            if (isOnlyUseWhenHeld && !isHeld)
            {
                return false;
            }
            else if (weaponTypeInt == ProjectileRayCastInt || weaponTypeInt == ProjectileStandardInt)
            {
                return IsProjectileWeaponReady(weaponButtonNumber);
            }
            else if (weaponTypeInt == BeamStandardInt)
            {
                return IsBeamWeaponReady(weaponButtonNumber);
            }
            else
            {
                return IsCustomWeaponReady(weaponButtonNumber);
            }
        }

        /// <summary>
        /// Parent the weapon to the first person camera.
        /// Assumes the weapon is being held by a character.
        /// </summary>
        protected void ParentToFirstPersonCamera()
        {
            transform.SetParent(stickyControlModule.lookFirstPersonTransform1);

            // Set a local position for the weapon when parented to camera in aim mode
            // Y-position is -ve offset from centre of weapon to centre of target sights on the weapon.
            transform.localPosition = stickyControlModule.AimIKFPWeaponOffset;
            transform.localRotation = Quaternion.identity;

            isTouchable = true;
            // Is weapon being held in right hand with optional left hand as secondary support?
            if (stickyControlModule.RightHandInteractiveID == StickyInteractiveID)
            {
                stickyControlModule.SetRightHandIKTargetInteractive(this, false, false);

                // Does weapon have a secondary support position for the left hand?
                if (handHold2Offset != Vector3.zero)
                {
                    stickyControlModule.SetLeftHandIKTargetInteractive(this, true, false);
                }
            }
            // Is weapon being held in left hand with optional right hand as secondary support
            else if (stickyControlModule.LeftHandInteractiveID == StickyInteractiveID)
            {
                // Not sure if thie will work with all weapons or if they need to be
                // set up for left handed use. Need to test with Hold 1 Flip for LH and Hold 2 Flip for LH enabled.
                stickyControlModule.SetLeftHandIKTargetInteractive(this, false, false);

                // Does weapon have a secondary support position for the right hand?
                if (handHold2Offset != Vector3.zero)
                {
                    stickyControlModule.SetRightHandIKTargetInteractive(this, true, false);
                }
            }

            stickyControlModule.EnableHandIK(false);
            //stickyControlModule.defaultAnimator.SetLayerWeight(3, 0f);
        }

        /// <summary>
        /// If there are Empty Fire Effects for the firing mechanism, play it now.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        protected void PlayEmptyFire (int weaponButtonNumber)
        {
            int effectsTemplatePrefabID = StickyManager.NoPrefabID;

            if (weaponButtonNumber == 1)
            {
                // Find a unique pooled prefab type (S3DEffectsObjectTemplate)
                if (numFireEmptyEffects1 > 0)
                {
                    effectsTemplatePrefabID = fireEmptyEffects1PrefabIDs[numFireEmptyEffects1 > 1 ? randomSelectFX.Range(0, numFireEmptyEffects1 - 1) : 0];
                }
            }
            else if (weaponButtonNumber == 2)
            {
                // Find a unique pooled prefab type (S3DEffectsObjectTemplate)
                if (numFireEmptyEffects2 > 0)
                {
                    effectsTemplatePrefabID = fireEmptyEffects2PrefabIDs[numFireEmptyEffects2 > 1 ? randomSelectFX.Range(0, numFireEmptyEffects2 - 1) : 0];
                }
            }

            // Did we get a unique pooled prefab type?
            if (effectsTemplatePrefabID != StickyManager.NoPrefabID)
            {
                // Ask the manager to active one from the pool
                S3DInstantiateEffectsObjectParameters ieParms = new S3DInstantiateEffectsObjectParameters
                {
                    effectsObjectPrefabID = effectsTemplatePrefabID,
                    position = trfmPos,
                    rotation = trfmRot
                };

                // No need to get result as shouldn't have IsReparented enabled... (there is a note in manual and tooltip)
                stickyManager.InstantiateEffectsObject(ref ieParms);
            }
        }

        /// <summary>
        /// Play a sound FX at a world space position. Currently used during reloading.
        /// </summary>
        /// <param name="soundFXPrefabID"></param>
        /// <param name="soundFXPosition"></param>
        /// <param name="itemKeyIndex"></param>
        protected void PlaySoundFX (int soundFXPrefabID, Vector3 soundFXPosition, int itemKeyIndex)
        {
            if (soundFXPrefabID != StickyManager.NoPrefabID)
            {
                // Use the prefab audio volume and clip
                S3DInstantiateSoundFXParameters sfxParms = new S3DInstantiateSoundFXParameters()
                {
                    effectsObjectPrefabID = soundFXPrefabID,
                    position = soundFXPosition
                };

                // Use the audio clip already attached to the prefab
                StickyEffectsModule soundFX = stickyManager.InstantiateSoundFX(ref sfxParms, null);

                // Was an effect activated in the pool?
                if (sfxParms.effectsObjectSequenceNumber > 0 && soundFX.isReparented)
                {
                    // Record the Sound FX so that it can be safely returned to the pool if still active when the weapon is destroyed.
                    reloadSFXItemKeyList[itemKeyIndex] = new S3DEffectItemKey(soundFXPrefabID, sfxParms.effectsObjectPoolListIndex, sfxParms.effectsObjectSequenceNumber);

                    soundFX.transform.SetParent(transform);
                }
            }
        }

        /// <summary>
        /// Refresh and validate Animate settings. Call this after changing any Animate settings.
        /// See also the public API RefreshAnimateSettings().
        /// Remove any Weapon Anim Sets that are null from the list.
        /// </summary>
        /// <param name="isInitialising">Is this being called from Initialise()</param>
        protected void RefreshAnimateSettings (bool isInitialising)
        {
            if (defaultAnimator != null)
            {
                if (s3dAnimActionList == null) { s3dAnimActionList = new List<S3DAnimAction>(4); }

                numAnimateActions = s3dAnimActionList == null ? 0 : s3dAnimActionList.Count;

                // cache the number of animation conditions for each Animate Action
                for (int aaIdx = 0; aaIdx < numAnimateActions; aaIdx++)
                {
                    S3DAnimAction aa = s3dAnimActionList[aaIdx];
                    if (aa != null)
                    {
                        aa.numConditions = aa.s3dAnimConditionList == null ? 0 : aa.s3dAnimConditionList.Count;
                    }
                }
            }

            numWeaponAnimSets = s3dWeaponAnimSetList == null ? 0 : s3dWeaponAnimSetList.Count;

            // Remove any Weapon Anim Sets that are null from the list.
            for (int wasIdx = numWeaponAnimSets - 1; wasIdx >= 0; wasIdx--)
            {
                if (s3dWeaponAnimSetList[wasIdx] == null)
                {
                    s3dWeaponAnimSetList.RemoveAt(wasIdx);
                    numWeaponAnimSets--;
                }
            }

            // Verify the list is defined (or cleared)
            if (isInitialising && animActionExtList == null)
            {
                animActionExtList = new List<S3DAnimActionExt>(6);
            }
            else
            {
                animActionExtList.Clear();
            }
        }

        /// <summary>
        /// Reparent the weapon to the characters hand.
        /// Assumes it is being held by a humanoid character.
        /// </summary>
        protected void ReparentToHand()
        {
            stickyControlModule.ReparentToHand(this);

            // Stop touching the weapon with Hand IK
            if (stickyControlModule.RightHandInteractiveID == StickyInteractiveID)
            {
                stickyControlModule.SetRightHandIKTargetInteractive(null, false, false);

                // Does weapon have a secondary support position for the left hand?
                if (handHold2Offset != Vector3.zero)
                {
                    stickyControlModule.SetLeftHandIKTargetInteractive(null, true, false);
                }
            }
            else if (stickyControlModule.LeftHandInteractiveID == StickyInteractiveID)
            {
                stickyControlModule.SetLeftHandIKTargetInteractive(null, false, false);

                // Does weapon have a secondary support position for the right hand?
                if (handHold2Offset != Vector3.zero)
                {
                    stickyControlModule.SetRightHandIKTargetInteractive(null, true, false);
                }
            }

            isTouchable = false;
        }

        /// <summary>
        /// Reset all the variables that could have occurred since
        /// the last fixed update started. These variables are mainly
        /// for use with animations.
        /// </summary>
        protected void ResetHasHappenedSinceFixedUpdate()
        {
            // Reset Has Fired as the end of the fixed update
            hasFired1 = false;
            hasFired2 = false;

            hasEmptyFired1 = false;
            hasEmptyFired2 = false;

            hasReloadStarted1 = false;
            hasReloadStarted2 = false;

            hasReloadFinished1 = false;
            hasReloadFinished2 = false;
        }

        /// <summary>
        /// Update where the weapon is aiming
        /// </summary>
        protected void UpdateAimAtPosition()
        {
            Vector3 fireDirectionWS = GetWorldFireDirection();
            Vector3 fireBasePositionWS = GetWorldFireBasePosition();

            bool isThirdPerson = false;
            bool isUseCharacterLooking = false;

            if (isHeld)
            {
                // When NPCs are aiming, they aim in the direction the weapon is facing (NEED TO FACT CHECK THIS..)
                aimFromPosition = fireBasePositionWS;

                isThirdPerson = stickyControlModule.isThirdPerson && !stickyControlModule.IsNPC();

                /// TODO - should also use where character is looking when not aiming but IsFreeLook is disabled.
                isUseCharacterLooking = !stickyControlModule.IsLookFreeLookEnabled || isAiming;

                Ray ray = stickyControlModule.GetAimingRay();

                if (isThirdPerson && isUseCharacterLooking)
                {
                    // When aiming in third person, need to move characterLookFromPosition forward from camera
                    // as it could be behind the end of the weapon's barrel (fireBasePositionWS).
                    // This current formula is an approximation but seems okay for the moment.
                    characterLookFromPosition = ray.origin + (ray.direction * Vector3.Distance(ray.origin, fireBasePositionWS));

                    characterLookAtPosition = stickyManager.GetHitOrMaxPoint(characterLookFromPosition, ray.direction, maxRange, hitLayerMask, stickyControlModule.StickyID, true);

                    /// TODO - lerp towards this to avoid sudden movement going between close and distant objects.                    
                    if (prevAimAtPositionLS != Vector3.zero)
                    {
                        // DAVID: Comment in/out one of the following to test
                        // When using only characterLookAtPosition the weapon will snap or "jump" when the character suddenly
                        // looks to a far point or to a close point. Like looking at the edge of a wall then looking past the wall to a distant point.
                        //aimAtPosition = characterLookAtPosition;

                        // Values below 5*dt are way too slow
                        // MoveTowards uses spheric lerping which can be very slow when going quickly between close and distant points.
                        // I'm using prevAimAtPositionLS (local space) to try and avoid issue where character quickly turns around
                        // and faces the opposite direction (in world space these distance are a long way apart)
                        // We need a solution that works for all situations.
                        //aimAtPosition = Vector3.MoveTowards(stickyControlModule.GetWorldPosition(prevAimAtPositionLS), characterLookAtPosition, 5f * Time.deltaTime);
                        //aimAtPosition = Vector3.Lerp(stickyControlModule.GetWorldPosition(prevAimAtPositionLS), characterLookAtPosition, 5f * Time.deltaTime);

                        // With SmoothDamp, the higher the speed the slower the movement.
                        aimAtPosition = Vector3.SmoothDamp(stickyControlModule.GetWorldPosition(prevAimAtPositionLS), characterLookAtPosition, ref aimAtPositionVelo, 7f * Time.deltaTime);
                    }
                    else
                    {
                        aimAtPosition = characterLookAtPosition;
                    }
                }
                else
                {
                    aimAtPosition = stickyManager.GetMaxPoint(aimFromPosition, fireDirectionWS, maxRange);

                    characterLookFromPosition = ray.origin;
                    characterLookAtPosition = stickyManager.GetMaxPoint(characterLookFromPosition, ray.direction, maxRange);
                }

                prevAimAtPositionLS = stickyControlModule.GetLocalPosition(aimAtPosition);

                // Tell the character where to aim with IK
                stickyControlModule.aimFromPos = characterLookFromPosition;
                stickyControlModule.aimTargetPos = characterLookAtPosition;

                //DebugExtension.DebugArrow(aimFromPosition, S3DMath.Normalise(aimFromPosition, aimAtPosition) , Color.yellow);
                //DebugExtension.DebugPoint(aimAtPosition, Color.yellow, 0.25f);
                //DebugExtension.DebugWireSphere(aimAtPosition, Color.yellow, 0.75f);

                // TESTING ONLY - DELETE ME
                //#if UNITY_EDITOR
                //                if (stickyControlModule.tempAimGO != null) { stickyControlModule.tempAimGO.transform.position = aimAtPosition; }
                //#endif

                stickyControlModule.weaponAimPos = aimAtPosition;
                stickyControlModule.weaponAimDirection = fireDirectionWS;
                stickyControlModule.weaponAimUp = Vector3.Cross(fireDirectionWS, transform.right);
                stickyControlModule.weaponFirePosition = fireBasePositionWS;
            }
            else
            {
                aimFromPosition = fireBasePositionWS;

                aimAtPosition = stickyManager.GetMaxPoint(aimFromPosition, fireDirectionWS, maxRange);

                characterLookFromPosition = Vector3.zero;
                characterLookAtPosition = Vector3.zero;
            }

            // Update laser sight end point
            if (isLaserSightReady && isLaserSightOn)
            {
                // Ideally the laser sight should converge on the aim position directly forward from the weapon fire direction.
                // However, the aim position may not be in front of the weapon if character Free Look is enabled.
                // If we use forward and maxRange, the laser will shine through objects - which would be undesirable.

                // Do we want the laser sight to point where the character is looking?
                bool isUseAimPosition = isThirdPerson && isUseCharacterLooking;

                // Find the first character or object the weapon would hit if fired
                Vector3 weaponAimAtPos = isUseAimPosition ? aimAtPosition : stickyManager.GetHitOrMaxPoint(fireBasePositionWS, fireDirectionWS, maxRange, hitLayerMask, 0, true);
                Vector3 laserSightAimFromPos = laserSightAimFrom.position;
                Vector3 laserSightAimFwd = isUseAimPosition ? S3DMath.Normalise(laserSightAimFromPos, aimAtPosition) : laserSightAimFrom.forward;               

                // Aim the laser at that hit point (it may not be exactly straight from laser sight position.
                // The idea is it will converge on the hit point.
                Vector3 laserSightAimDir = weaponAimAtPos - laserSightAimFromPos;

                if (laserSightAimDir.sqrMagnitude > Mathf.Epsilon) { laserSightAimDir = laserSightAimDir.normalized; }
                else { laserSightAimDir = laserSightAimFwd; }

                // If the aim at posiition is in front of the laser sight, then aim there
                float dotProdToTarget = Vector3.Dot(laserSightAimDir, laserSightAimFwd);

                if (dotProdToTarget > 0f)
                {
                    laserSightLineRenderer.SetPosition(1, GetLocalPosition(weaponAimAtPos));
                }
                else
                {
                    // Set to the default forward direction
                    laserSightLineRenderer.SetPosition(1, GetLocalPosition(laserSightAimFromPos + (laserSightAimFwd * maxRange)));
                }
            }
        }

        /// <summary>
        /// Check if the weapon has the target (if any) in the direct line of sight.
        /// It DOES NOT check if the weapon is facing the target.
        /// See also IsFacingTarget().
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        protected void UpdateLineOfSight (int weaponButtonNumber)
        {
            bool isLoS = false;
            Vector3 lookFrom = Vector3.zero, lookTo = Vector3.zero;

            if (isTargetSticky3DAssigned)
            {
                // If there is no target, unassign it
                if (targetSticky3D == null)
                {
                    UnAssignTargetCharacter();
                }
                else
                {
                    lookFrom = GetWorldFireBasePosition();
                    lookTo = targetSticky3D.GetWorldEyePosition();
                }
            }
            else if (isTargetTransformAssigned)
            {
                // If there is no target, unassign it
                if (targetTransform == null)
                {
                    UnAssignTargetTransform();
                }
                else
                {
                    lookFrom = GetWorldFireBasePosition();
                    lookTo = targetTransform.position;
                }
            }

            // If we still have a target, see if there is a direct line of sight
            if (isTargetSticky3DAssigned || isTargetTransformAssigned)
            {
                Vector3 lookAtTargetVector = lookTo - lookFrom;

                if (lookAtTargetVector.sqrMagnitude > Mathf.Epsilon)
                {
                    Vector3 lookAtTargetDir = lookAtTargetVector.normalized;

                    // Ignore anything behind the weapon fire point
                    if (Vector3.Dot(lookAtTargetDir, GetWorldFireDirection()) >= 0)
                    {
                        // Assume there is clear line of sight
                        isLoS = true;

                        raycastHit = new RaycastHit();

                        float distance = lookAtTargetVector.magnitude;

                        // Do a quick test to see if there is anything between the two world space positions
                        if (Physics.Raycast(lookFrom, lookAtTargetDir, out raycastHit, distance, ~0, QueryTriggerInteraction.Collide))
                        {
                            Ray ray = new Ray(lookFrom, lookAtTargetDir);

                            // Get all hits between lookFrom position and the target Include triggers.
                            int numHits = Physics.RaycastNonAlloc(ray, raycastHitInfoArray, distance, ~0, QueryTriggerInteraction.Collide);

                            if (numHits > 1) { S3DUtils.SortHitsAsc(raycastHitInfoArray, numHits); }

                            StickyControlModule hitCharacter = null;

                            for (int hitIdx = 0; hitIdx < numHits; hitIdx++)
                            {
                                raycastHit = raycastHitInfoArray[hitIdx];
                                Collider collider = raycastHit.collider;
                                Rigidbody hitRBody = raycastHit.rigidbody;

                                if (isTargetTransformAssigned)
                                {
                                    if (!collider.isTrigger)
                                    {
                                        // If this is regular collider test if this is part of the target
                                        isLoS = collider.transform.IsChildOf(targetTransform);
                                        break;
                                    }
                                    // Is this a character between the weapon fire point and the target
                                    else if (collider.isTrigger && hitRBody != null && hitRBody.TryGetComponent(out hitCharacter))
                                    {
                                        isLoS = false;
                                        break;
                                    }
                                }
                                else if (isTargetSticky3DAssigned)
                                {
                                    // Is this a regular or trigger collider attached to a sticky weapon
                                    isLoS = targetSticky3D.IsColliderHittableByWeapon(collider.GetInstanceID());
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (isLoS)
            {
                if (weaponButtonNumber == 1)
                {
                    HasLineOfSight1 = true;
                }
                else if (weaponButtonNumber == 2)
                {
                    HasLineOfSight2 = true;
                }
            }
            else
            {
                ResetLineOfSight(weaponButtonNumber);
            }
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Perform some custom animation action on the character animation controller.
        /// </summary>
        /// <param name="animAction"></param>
        protected virtual void AnimateCharacterCustomAction (S3DAnimActionExt animAction)
        {
            // Do something with the stickyControlModule.defaultAnimator
            // animAction.paramHashCode is the parameter to use in the defaultAnimator.
        }

        /// <summary>
        /// Can the weapon fire based on the input from fire button 1 or 2?
        /// If firing is paused, this will always return false.
        /// Returns false if only fire when aiming is true and weapon is not being aimed.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CanFireButton(int weaponButtonNumber)
        {
            if (isFiringPaused) { return false; }
            else if (weaponButtonNumber == 1)
            {
                if (isOnlyFireWhenAiming1 && !isAiming) { return false; }
                else
                {
                    bool isAllowContinuousFire = weaponTypeInt == BeamStandardInt || weaponFiringType1Int == FiringTypeFullAutoInt;
                    return isFire1Input && weaponFiringButton1Int == FiringButtonManualInt && (isAllowContinuousFire || !isFire1InputHeld);
                }
            }
            else if (weaponButtonNumber == 2)
            {
                if (isOnlyFireWhenAiming2 && !isAiming) { return false; }
                else
                {
                    bool isAllowContinuousFire = weaponTypeInt == BeamStandardInt || weaponFiringType2Int == FiringTypeFullAutoInt;
                    return isFire2Input && weaponFiringButton2Int == FiringButtonManualInt && (isAllowContinuousFire || !isFire2InputHeld);
                }
            }
            // We only support 2 fire buttons (primary and secondary)
            else { return false; }
        }

        /// <summary>
        /// Get notified when the character holding this weapon changes camera like switches between first and third person
        /// </summary>
        /// <param name="oldCamera"></param>
        /// <param name="newCamera"></param>
        /// <param name="isThirdPerson"></param>
        protected virtual void CharacterChangedCamera (Camera oldCamera, Camera newCamera, bool isThirdPerson)
        {
            // Restore the field of view to the previous camera and disable aiming
            if (aimOriginalFieldOfView != 0)
            {
                oldCamera.fieldOfView = aimOriginalFieldOfView;

                // Only disable aiming if not in the process of switching to the first-person
                // camera in third-person aiming mode.
                if (isHeld && !stickyControlModule.IsAimTPUsingFPCameraEnabled)
                {
                    EnableOrDisableAiming(false, false);
                }
            }
        }

        /// <summary>
        /// Safely clean up the Scope components
        /// </summary>
        protected virtual void DecommisionScope()
        {
            if (scopeCamera != null) { scopeCamera.enabled = false; }
            if (scopeCameraRenderer != null) { scopeCameraRenderer.enabled = false; }

            // Avoid memory leaks by safely destroying the render texture
            //if (scopeRTexture != null) { S3DUtils.DestroyRenderTexture(ref scopeRTexture); }

            isScopeCamInitialised = false;
        }

        /// <summary>
        /// Attempt to destroy any muzzle fx currently active for this firing point.
        /// If there are no fire point offsets, the index should be set to 0.
        /// </summary>
        /// <param name="firePointOffsetIndex"></param>
        protected virtual void DestroyMuzzleFX(int firePointOffsetIndex)
        {
            if (isWeaponInitialised && firePointOffsetIndex >= 0)
            {
                S3DEffectItemKey effectItemKey = muzzleItemKey1List[firePointOffsetIndex];

                // Could this be an active StickyEffectsModule in the pool?
                if (effectItemKey.effectsObjectSequenceNumber > 0)
                {
                    stickyManager.DestroyEffectsObject(effectItemKey);

                    // Reset the item key to show there is no longer an active muzzle FX
                    muzzleItemKey1List[firePointOffsetIndex] = new S3DEffectItemKey(-1, -1, 0);
                }
            }
        }

        /// <summary>
        /// Attempt to eject the spent cartridge (if any) after firing the weapon
        /// </summary>
        protected virtual void EjectSpentCartridge()
        {
            // Is there a valid pooled dynamic object pool?
            if (isWeaponInitialised && spentCartridgePrefabID >= 0)
            {
                S3DInstantiateDynamicObjectParameters idParms = new S3DInstantiateDynamicObjectParameters()
                {
                    dynamicObjectPrefabID = spentCartridgePrefabID,
                    position = GetWorldSpentEjectPosition(),
                    rotation = GetWorldSpaceEjectRotation()
                };

                StickyDynamicModule stickyDynamicModule = stickyManager.InstantiateDynamicObject(ref idParms);

                if (stickyDynamicModule != null)
                {
                    // Apply force here
                    Rigidbody spentCartRBody = stickyDynamicModule.ObjectRigidbody;

                    if (spentCartRBody != null && !spentCartRBody.isKinematic)
                    {
                        // Check that weapon is being held
                        Vector3 s3dVelocity = isHeld && stickyControlModule != null ? stickyControlModule.GetCurrentWorldVelocity : Vector3.zero;

                        // Add gravity based on world space gravity direction, mass of the object, and the fixed time step.
                        spentCartRBody.AddForce(GetWorldSpentEjectDirection() * spentEjectForce + s3dVelocity, ForceMode.Force);
                    }
                }
            }
        }

        /// <summary>
        /// Enable or disable the character aiming down the sights or through the scope.
        /// IsSmooth currently only applies to non-NPC look camera FoV.
        /// </summary>
        /// <param name="isEnabled"></param>
        protected virtual void EnableOrDisableAiming(bool isEnabled, bool isSmooth)
        {
            bool isFirePostEvents = false;

            // Start aiming
            if (isEnabled)
            {
                // Only take action if it's not already aiming
                if (!isAiming)
                {
                    if (isHeld && stickyControlModule != null)
                    {
                        if (stickyControlModule.onPreStartAim != null) { stickyControlModule.onPreStartAim.Invoke(stickyControlModule.StickyID, StickyInteractiveID, false, Vector3.zero); }
                         
                        // If the character wasn't already aiming at the target, start doing that now
                        if (!stickyControlModule.IsAimIKWhenNotAiming) { stickyControlModule.EnableAimAtTarget(true, isSmooth); }

                        // If weapon has a Scope, enable it.
                        EnableOrDisableScope(true);

                        if (!stickyControlModule.IsNPC() && stickyControlModule.IsLookEnabled)
                        {
                            // Check to see if we need to enable the first-person camera when aiming in third-person.
                            // See also S3DWeaponAnimSet (isAimTPUsingFPCamera)
                            if (stickyControlModule.isThirdPerson && stickyControlModule.CheckEnableAimTPUsingFPCamera())
                            {
                                //Debug.Log("[DEBUG] EnableOrDisableAiming (enable) isSmooth: " + isSmooth);
                            }

                            // First person player aiming - so parent to camera
                            if (!stickyControlModule.IsAimIKWhenNotAiming)
                            {
                                if (!stickyControlModule.isThirdPerson)
                                {
                                    ParentToFirstPersonCamera();
                                }
                            }

                            // Get the first or third person camera of the character holding the weapon
                            Camera s3dCamera = stickyControlModule.lookCamera1;

                            // If already smoothly disabling, we probably already have set the original FoV,
                            // so don't overwrite it with a partially zoomed FoV and Near Clipping plane
                            if (!isAimingSmoothDisable)
                            {
                                aimOriginalFieldOfView = s3dCamera.fieldOfView;
                                aimOriginalNearClipping = s3dCamera.nearClipPlane;
                            }

                            // Get the desired character aiming FoV
                            targetAimingFieldofView = stickyControlModule.isThirdPerson ? aimingThirdPersonFOV : aimingFirstPersonFOV;
                            targetAimingNearClipping = stickyControlModule.isThirdPerson ? s3dCamera.nearClipPlane : stickyControlModule.AimIKFPNearClippingPlane;

                            if (isSmooth)
                            {
                                prevAimingFieldOfView = s3dCamera.fieldOfView;
                                prevAimingNearClipping = s3dCamera.nearClipPlane;
                            }
                            else
                            {
                                // No smoothing - so instantly set the FoV
                                s3dCamera.fieldOfView = targetAimingFieldofView;
                                s3dCamera.nearClipPlane = targetAimingNearClipping;
                            }

                            isAimingSmoothEnable = isSmooth;
                            isAimingSmoothDisable = false;
                        }

                        // Reset aimAtPosition
                        prevAimAtPositionLS = Vector3.zero;
                        aimAtPositionVelo = Vector3.zero;

                        isAiming = true;

                        // If not smooth enabling, if required, we can immediately fire the onPostStartAim event at the end of this method.
                        // Otherwise, this called from StickyControlModule when smooth enable completes
                        if (!isAimingSmoothEnable) { isFirePostEvents = true; }
                    }
                }
            }

            // Stop aiming
            else if (isAiming)
            {
                bool isCharacterHoldingWeapon = isHeld && stickyControlModule != null;

                if (isCharacterHoldingWeapon && stickyControlModule.onPreStopAim != null) { stickyControlModule.onPreStopAim.Invoke(stickyControlModule.StickyID, StickyInteractiveID, false, Vector3.zero); }

                // If weapon has a Scope, turn it off unless AutoOn is enabled.
                if (!isScopeAutoOn)
                {
                    EnableOrDisableScope(false);
                }

                // Restore the look field of view and near clipping plane
                if (aimOriginalFieldOfView != 0f && isCharacterHoldingWeapon && !stickyControlModule.IsNPC() && stickyControlModule.IsLookEnabled)
                {
                    // Get the desired character aiming FoV
                    targetAimingFieldofView = aimOriginalFieldOfView;

                    // Get the desired character camera aiming near clipping plane
                    targetAimingNearClipping = aimOriginalNearClipping;

                    // Get the first or third person camera of the character holding the weapon
                    Camera s3dCamera = stickyControlModule.lookCamera1;

                    if (isSmooth)
                    {
                        prevAimingFieldOfView = s3dCamera.fieldOfView;
                        prevAimingNearClipping = s3dCamera.nearClipPlane;
                    }
                    else
                    {
                        // Instantly restore original player camera FoV
                        aimOriginalFieldOfView = 0f;
                        aimOriginalNearClipping = 0f;
                        s3dCamera.fieldOfView = targetAimingFieldofView;
                        s3dCamera.nearClipPlane = targetAimingNearClipping;
                    }

                    isAimingSmoothDisable = isSmooth;
                }
                else
                {
                    isAimingSmoothDisable = false;
                    aimOriginalFieldOfView = 0f;
                }

                // If the character should only aim at the target when the weapon is aiming, turn off Aim IK
                if (isCharacterHoldingWeapon && !stickyControlModule.IsAimIKWhenNotAiming)
                {
                    // When turning off aiming with a FPS player, we need to reparent the weapon to the hand
                    // This will also apply when third-person is using the first-person camera for aiming.
                    if (!stickyControlModule.IsNPC() && !stickyControlModule.isThirdPerson && stickyControlModule.IsLookEnabled)
                    {
                        ReparentToHand();
                    }

                    stickyControlModule.EnableAimAtTarget(false, isSmooth);
                    //stickyControlModule.defaultAnimator.SetLayerWeight(3, 1f);

                    // Check to see if we need to switch back to third-person
                    if (stickyControlModule.CheckDisableAimTPUsingFPCamera())
                    {
                        // If we did switch back to third-person, do any additional task here.
                    }
                }

                isAimingSmoothEnable = false;

                isAiming = false;

                // If not smooth disabling, if required, we can immediately fire the onPostStopAim event at the end of this method.
                // Otherwise, this called from StickyControlModule when smooth enable completes
                if (isCharacterHoldingWeapon && !isAimingSmoothDisable) { isFirePostEvents = true; }
            }

            CheckReticle();

            // If required, call the character post aiming events last
            if (isFirePostEvents)
            {
                if (isAiming)
                {
                    if (stickyControlModule.onPostStartAim != null) { stickyControlModule.onPostStartAim.Invoke(stickyControlModule.StickyID, StickyInteractiveID, false, Vector3.zero); }
                }
                else
                {
                    if (stickyControlModule.onPostStopAim != null) { stickyControlModule.onPostStopAim.Invoke(stickyControlModule.StickyID, StickyInteractiveID, false, Vector3.zero); }
                }
            }
        }

        /// <summary>
        /// Enable or disable the Scope used for more precise aiming
        /// </summary>
        /// <param name="isEnabled"></param>
        protected virtual void EnableOrDisableScope (bool isEnabled)
        {
            // Attempt to turn on
            if (isEnabled)
            {
                // Check if held by an NPC, and NPCs are not allowed to enable the Scope
                if (isHeld && !isScopeAllowNPC && stickyControlModule != null && stickyControlModule.IsNPC())
                {
                    if (isScopeCamInitialised) { DecommisionScope(); }

                    return;
                }

                // Only take action if it's not already on
                if (!isScopeOn)
                {
                    if (!isScopeCamInitialised) { InitialiseScopeCamera(); }

                    if (isScopeCamInitialised)
                    {
                        scopeCameraRenderer.enabled = true;
                        scopeCamera.enabled = true;
                        isScopeOn = true;
                    }
                }
            }
            // Turn off (no need to do anything if it is already off)
            else if (isScopeOn)
            {
                if (isScopeCamInitialised)
                {
                    scopeCamera.enabled = false;
                    scopeCameraRenderer.enabled = false;
                }

                isScopeOn = false;
            }
        }

        /// <summary>
        /// Enable or disable the weapon
        /// </summary>
        /// <param name="isEnabled"></param>
        protected virtual void EnableOrDisableWeapon(bool isEnabled)
        {
            if (!isEnabled)
            {
                //hasFired1 = false;
                //hasFired2 = false;

                // Not sure what happens if we want to disable in the middle of reloading...
                ResetHasHappenedSinceFixedUpdate();

                if (numAnimateActions > 0)
                {
                    // Damping on animation values could leave some values in a intermediate
                    // state if more values are not sent to the animator controller.
                    // NOTE: This may affect the smoothness of other animations...
                    AnimateWeapon(true);
                }
            }

            isWeaponEnabled = isEnabled;
        }

        /// <summary>
        /// Override this when adding your own weapon types.
        /// Return true if the weapon has successfully fired.
        /// </summary>
        /// <param name="firePointOffsetIndex"></param>
        protected virtual bool FireCustomWeapon(int firePointOffsetIndex, int weaponButtonNumber)
        {
            #if UNITY_EDITOR
            Debug.Log("Custom weapon fired. Button " + weaponButtonNumber + " at T:" + Time.time);
            #endif

            return false;
        }

        /// <summary>
        /// Override this when creating your own muzzle system
        /// </summary>
        /// <param name="firePointOffsetIndex">The fire point offset index. If no offset, value will be 0</param>
        /// <param name="firedPoint">World Space point where projectile or beam was fired from</param>
        /// <param name="firedDirection">World Space direction the projectile or beam was fired from</param>
        protected virtual void FireMuzzleFX(int firePointOffsetIndex, Vector3 firedPoint, Vector3 firedDirection)
        {
            // Check if there is a muzzle FX already active for this fire position
            S3DEffectItemKey _effectItemKey = muzzleItemKey1List[firePointOffsetIndex];
            if (_effectItemKey.effectsObjectSequenceNumber > 0)
            {
                // This occurs when a projectile is fired but the weapon doesn't know the muzzle FX has despawned.
                // This is a downside of associating the muzzle FX with the weapon rather than the projectile (like in SSC).
                // However, it has other benefits. One solution might be to have an internal onDestroy callback to the weapon.
                // When a beam stop firing, they update the weapon and we shouldn't get here.

                //Debug.Log("[DEBUG] Muzzle may already exist. Attempt to destroy it first for FP " + firePointOffsetIndex + " T:" +Time.time );

                stickyManager.DestroyEffectsObject(_effectItemKey);

                _effectItemKey.Reset();
                muzzleItemKey1List[firePointOffsetIndex] = _effectItemKey;
            }

            // Get the index of a valid muzzle effects prefab IDs (else set to -1)
            // If there are more than 1 in the array, select randomly.
            int validMuzzleFXIdx = numMuzzleEffects1Valid > 1 ? randomSelectFX.Range(0, numMuzzleEffects1Valid - 1) : numMuzzleEffects1Valid > 0 ? 0 : -1;

            // Use the index to get the StickyEffectsModule template prefabID used by the pooling system
            int muzzleEffectsPrefabID = validMuzzleFXIdx >= 0 && validMuzzleFXIdx < numMuzzleEffects1Valid ? muzzleEffectsObject1PrefabIDs[validMuzzleFXIdx] : StickyManager.NoPrefabID;

            // If we got a template prefabId, attempt to instantiate it
            if (muzzleEffectsPrefabID != StickyManager.NoPrefabID)
            {
                Quaternion _objRotation = Quaternion.LookRotation(firedDirection, trfmUp);

                S3DInstantiateEffectsObjectParameters ieParms = new S3DInstantiateEffectsObjectParameters
                {
                    effectsObjectPrefabID = muzzleEffectsPrefabID,
                    position = firedPoint + (_objRotation * muzzleEffects1Offset),
                    rotation = _objRotation
                };

                // Instantiate the muzzle FX
                StickyEffectsModule stickyEffectsModule = stickyManager.InstantiateEffectsObject(ref ieParms);

                if (stickyEffectsModule != null)
                {
                    muzzleItemKey1List[firePointOffsetIndex] = new S3DEffectItemKey(muzzleEffectsPrefabID, ieParms.effectsObjectPoolListIndex, ieParms.effectsObjectSequenceNumber);

                    if (stickyEffectsModule.isReparented)
                    {
                        stickyEffectsModule.transform.SetParent(transform);
                    }
                }
            }
        }

        /// <summary>
        /// Initialise, but don't enable the Scope camera for use when it is turned on with EnableOrDisableScope(..).
        /// See also DecommisionScope().
        /// </summary>
        protected virtual void InitialiseScopeCamera()
        {
            if (scopeCamera != null && scopeCameraRenderer != null)
            {
                scopeCamera.enabled = false;

                Collider renCollider;
                if (scopeCameraRenderer.TryGetComponent(out renCollider))
                {
                    #if UNITY_EDITOR
                    DestroyImmediate(renCollider);                    
                    Debug.LogWarning("ERROR " + name + " should not have a collider attached to the Scope (" + scopeCameraRenderer.transform.name + ") - removing it. Please fix this.");
                    #else
                    Destroy(renCollider);
                    #endif
                    ReinitialiseColliders();
                }
                
                scopeCameraRenderer.enabled = false;

                isScopeCamInitialised = true;
            }
            else
            {
                DecommisionScope();
            }
        }

        /// <summary>
        /// Override this method if you have a different way of determining if there is ammunition
        /// available so that the weapon can fire.
        /// Currently, always returns true for beam weapons.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <returns></returns>
        protected virtual bool IsAmmoAvailable(int weaponButtonNumber)
        {
            if (weaponTypeInt == BeamStandardInt)
            {
                return weaponButtonNumber == 1;
            }
            else
            {
                // Check if there is available ammo
                // Check if reloading (it may take some time, so don't wait)

                bool hasAmmo = (weaponButtonNumber == 1 && ammunition1 != 0) || (weaponButtonNumber == 2 && ammunition2 != 0);
                bool isReloading = IsReloading(weaponButtonNumber);
                bool isAmmoAvailable = hasAmmo && !isReloading;

                //bool isAmmoAvailable = (weaponButtonNumber == 1 && ammunition1 != 0 && !isReloading1) || (weaponButtonNumber == 2 && ammunition2 != 0 && !isReloading2);

                // Run out of ammo and not reloading, so attempt to reload
                if (!hasAmmo && !isReloading)
                {
                    int reloadTypeInt = GetReloadTypeInt(weaponButtonNumber);

                    // Check the reload type
                    if (reloadTypeInt == ReloadTypeManualOnlyInt)
                    { 
                        // Manual reloading only, so do nothing
                    }
                    else if (reloadTypeInt == ReloadTypeAutoReserveInt)
                    {
                        // If the number of magazines in reserve is -1,
                        // it means unlimited mags.
                        if (GetMagsInReserve(weaponButtonNumber) != 0)
                        {
                            Reload(weaponButtonNumber);
                        }
                    }
                    else if (reloadTypeInt == ReloadTypeAutoStashInt || reloadTypeInt == ReloadTypeAutoStashWithDropInt)
                    {
                        // We probably have to check this every time as mags
                        // can be added and removed from hands or stash by the character
                        if (GetMagsInStash(weaponButtonNumber) > 0 || GetMagsHeld(weaponButtonNumber) > 0)
                        {
                            Reload(weaponButtonNumber);
                        }
                    }
                }

                return isAmmoAvailable;
            }
        }

        /// <summary>
        /// Override this method if you have a different way to determine if a beam
        /// weapon is ready to fire.
        /// NOTE: Currently beam weapons only work on main firing button.
        /// WARNING: This assumes the user hasn't changed the beam prefab in the editor at runtime.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <returns></returns>
        protected virtual bool IsBeamWeaponReady(int weaponButtonNumber)
        {
            // Currently beam weapons only work on main firing button
            if (weaponButtonNumber != 1) { return false; }

            // Weapons with no health or performance cannot aim, recharge, reload or fire.
            // Check that the weapon has had a beam ID assigned
            // Assumes that the IDs have been correctly assigned based on the weaponType.
            bool isReady = health > 0f && currentPerformance > 0f && weaponFiringButton1Int > 0 && beamPrefabID >= 0;

            // Does the beam weapon need charging?
            if (health > 0f && currentPerformance > 0f && rechargeTime > 0f && chargeAmount < 1f)
            {
                chargeAmount += Time.deltaTime * 1f / rechargeTime;
                if (chargeAmount > 1f) { chargeAmount = 1f; }
            }

            // Check if it has the minimum amount of charge needed to fire for the minimum burst duration
            return isReady && (beamPrefab.minBurstDuration < chargeAmount * beamPrefab.dischargeDuration);
        }

        /// <summary>
        ///  Override this method if you have a different way to determine if your custom
        ///  weapon is ready to fire.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <returns></returns>
        protected virtual bool IsCustomWeaponReady(int weaponButtonNumber)
        {
            return false;
        }

        /// <summary>
        /// Override this method if you have a different way to determine if a projectile
        /// weapon is ready to fire.
        /// NOTE: Currently projectile weapons can only fire using Button 1
        /// </summary>
        /// <param name="firingButtonInt">None, Primary, Secondary or AutoFire</param>
        /// <returns></returns>
        protected virtual bool IsProjectileWeaponReady(int weaponButtonNumber)
        {
            // Currently projectile weapons can only fire using Button 1
            if (weaponButtonNumber != 1) { return false; }

            // If the weapon is not connected to a firing mechanism we can probably ignore the reload timer
            // Weapons with no health or performance cannot aim, reload or fire
            if (health > 0f && currentPerformance > 0f && weaponFiringButton1Int > 0)
            {
                // Check that the weapon has had a projectile ID assigned
                // Assumes that the IDs have been correctly assigned based on the weaponType.
                if (weaponTypeInt == ProjectileStandardInt)
                {
                    return projectilePrefabID >= 0;
                }
                else // ProjectileRaycast
                {
                    return true;
                }
            }
            else { return false; }
        }

        /// <summary>
        /// Pause or unpause this weapon.
        /// 1. Animation will be disabled
        /// 2. The Scope will pause rendering
        /// 3. The weapon will not be able to fire
        /// Has no effect if state has not changed.
        /// </summary>
        /// <param name="isPaused"></param>
        protected virtual void PauseOrUnPauseWeapon (bool isPaused)
        {
            // Only (un)pause if changed so as not to overwrite saved settings
            if (isPaused == isWeaponPaused) { return; }

            if (!isPaused)
            {
                // unpause weapon

                // Restore previous state of isAnimateEnabled
                EnableOrDisableAnimate(savedAnimateEnabledState);
            }
            else
            {
                // pause weapon
                savedAnimateEnabledState = isAnimateEnabled;
                EnableOrDisableAnimate(false);

                if (isScopeOn && scopeCamera != null)
                {
                    // When paused, the camera will switch to manual rendering
                    scopeCamera.enabled = !isPaused;

                    // Update the scope render texture once
                    if (isPaused) { scopeCamera.Render(); }
                }
            }

            isFiringPaused = isPaused;
            isWeaponPaused = isPaused;
        }

        /// <summary>
        /// Override this method to start a custom recoil.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        protected virtual void Recoil (int weaponButtonNumber)
        {
            float recoilKickZ = 0f;

            if (weaponButtonNumber == 1)
            {
                isRecoiling = true;
                recoilTargetRot += new Vector3(-recoilX1, randomSelectFX.Range(-recoilY1, recoilY1), randomSelectFX.Range(-recoilZ1, recoilZ1));
                recoilKickZ = randomSelectFX.Range(recoilMaxKickZ1 * 0.7f, recoilMaxKickZ1);                
            }
            else if (weaponButtonNumber == 2)
            {
                isRecoiling = true;
                recoilTargetRot += new Vector3(-recoilX2, randomSelectFX.Range(-recoilY2, recoilY2), randomSelectFX.Range(-recoilZ2, recoilZ2));
                recoilKickZ = randomSelectFX.Range(recoilMaxKickZ2 * 0.7f, recoilMaxKickZ2);
            }

            if (recoilKickZ > 0f && recoilKickZ > -recoilCurrentPos.z)
            {
                recoilTargetPos.z = -recoilKickZ;
            }
        }

        /// <summary>
        /// Override this method to perform any custom recoil action.
        /// </summary>
        protected virtual void Recoiling ()
        {
            if (isRecoiling)
            {
                // Currently recoil only works for a player while aiming in first person
                if (isHeld && isAiming && !stickyControlModule.IsNPC() && !stickyControlModule.isThirdPerson && stickyControlModule.IsLookEnabled)
                {
                    if (isRecoiling)
                    {
                        // Attempt to return to the stable rotation and position
                        recoilTargetRot = Vector3.Lerp(recoilTargetRot, Vector3.zero, recoilReturnRate * Time.deltaTime);
                        recoilTargetPos = Vector3.Lerp(recoilTargetPos, Vector3.zero, recoilReturnRate * Time.deltaTime);

                        // Smoothly move toward the target rotation - could try S3DMath.NLerp(..)
                        recoilCurrentRot = Vector3.Slerp(recoilCurrentRot, recoilTargetRot, recoilSpeed * Time.fixedDeltaTime);
                        recoilCurrentPos = Vector3.Lerp(recoilCurrentPos, recoilTargetPos, recoilSpeed * Time.fixedDeltaTime);

                        // Is the target approaching zero AND the current rotation is approaching the target?
                        // If so, stop the recoil action.
                        if (S3DMath.MaxAbs(recoilTargetRot) < 0.001f && S3DMath.MaxAbs(recoilCurrentRot - recoilTargetRot) < 0.001f)
                        {
                            transform.localPosition = stickyControlModule.AimIKFPWeaponOffset;
                            transform.localRotation = Quaternion.identity;
                            isRecoiling = false;
                        }
                        else
                        {
                            transform.localPosition = stickyControlModule.AimIKFPWeaponOffset + recoilCurrentPos;
                            transform.localRotation = Quaternion.Euler(recoilCurrentRot);
                        }
                    }
                    else
                    {
                        transform.localPosition = stickyControlModule.AimIKFPWeaponOffset;
                        transform.localRotation = Quaternion.identity;
                        isRecoiling = false;
                    }
                }
                else
                {
                    isRecoiling = false;
                }
            }
        }

        /// <summary>
        /// Reduce the ammount of ammunition available to the specified
        /// fire button mechanism.
        /// Most weapons only have the (primary) fire button 1.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        protected virtual void ReduceAmmunition (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1)
            {
                if (ammunition1 > 0)
                {
                    if (isMagEquipped1 && equippedMag1 != null)
                    {
                        equippedMag1.SetAmmoCount(equippedMag1.AmmoCount - 1);
                    }

                    ammunition1--;
                }
            }
            else if (weaponButtonNumber == 2)
            {
                if (ammunition2 > 0)
                {
                    if (isMagEquipped2 && equippedMag2 != null)
                    {
                        equippedMag2.SetAmmoCount(equippedMag2.AmmoCount - 1);
                    }

                    ammunition2--;
                }
            }
        }

        /// <summary>
        /// Override this when configuring your own custom weapon types
        /// </summary>
        protected virtual bool ReinitialiseCustomWeapons()
        {
            isReloadable = false;

            if (isReloadable)
            {
                // If our custom weapon is reloadable, it might have some reload sound fx.
                reloadSoundFXPrefabID1 = reloadSoundFX1 == null ? StickyManager.NoPrefabID : stickyManager.GetOrCreateSoundFXPool(reloadSoundFX1);
                reloadSoundFXPrefabID2 = reloadSoundFX2 == null ? StickyManager.NoPrefabID : stickyManager.GetOrCreateSoundFXPool(reloadSoundFX2);

                reloadEquipSoundFXPrefabID1 = reloadEquipSoundFX1 == null ? StickyManager.NoPrefabID : stickyManager.GetOrCreateSoundFXPool(reloadEquipSoundFX1);
                reloadEquipSoundFXPrefabID2 = reloadEquipSoundFX2 == null ? StickyManager.NoPrefabID : stickyManager.GetOrCreateSoundFXPool(reloadEquipSoundFX2);
            }

            return true;
        }

        /// <summary>
        /// Attempt to reload the primary firing button mechanism
        /// </summary>
        /// <param name="ammunition"></param>
        /// <param name="ammoTypeInt"></param>
        /// <param name="storeItemID">Used if reloading from character Stash</param>
        /// <param name="isMagInHand">Is the character holding a mag to reload the weapon?</param>
        /// <returns></returns>
        protected virtual IEnumerator ReloadPrimary (int ammunition, int ammoTypeInt, int storeItemID, bool isMagInHand)
        {
            isReloading1 = true;

            // If we don't have unlimited magazines, take one from the reserve
            if (magsInReserve1 > 0) { SetMagsInReserve(1, --magsInReserve1); }

            yield return new WaitForSeconds(reloadDelay1);

            // Start reloading process

            // This value will be reset at the end of the next fixed update
            hasReloadStarted1 = true;

            S3DStoreItem storeItem = null;

            // The magazine that will be used to reload the weapon (if any)
            StickyMagazine newMag = null;

            // If reloading with a mag in character hand, check it is still available and update mag capacity
            if (isMagInHand)
            {
                // Is the weapon still being held?
                if (isHeld)
                {
                    bool isHeldInLeftHand = false;

                    // If the non-empty mag is no longer in the hand of the character, cancel reloading
                    if (!GetMagazineFromCharacterHand(1, false, ref isHeldInLeftHand, ref newMag))
                    {
                        isReloading1 = false;
                        yield break;
                    }
                    else
                    {
                        magCapacity1 = newMag.MagCapacity;
                    }
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR ReloadPrimary " + name + " is no longer held but wants to use a mag in the hand of a character. PLEASE REPORT");
                    #endif
                    isReloading1 = false;
                    yield break;
                }
            }
            // If this is reloading from a mag in character Stash, check it is still available and update mag capcity
            else if (storeItemID != S3DStoreItem.NoStoreItem)
            {
                if (isHeld)
                {
                    storeItem = stickyControlModule.GetStashItem(storeItemID);

                    // If mag no longer available, cancel reloading
                    if (storeItem == null)
                    {
                        isReloading1 = false;
                        yield break;
                    }
                    else
                    {
                        newMag = (StickyMagazine)storeItem.stickyInteractive;
                        magCapacity1 = newMag.MagCapacity;
                    }
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR ReloadPrimary " + name + " is no longer held but wants to use Stashed Mags. PLEASE REPORT");
                    #endif
                    isReloading1 = false;
                    yield break;
                }
            }

            // Unequipping phase has begun
            isReloadUnequipping1 = true;

            Vector3 reloadSoundFXPos = magAttachPoint1 == null ? transform.position : magAttachPoint1.position;

            // If there is one, play the reload sound when the weapon starts to reload
            PlaySoundFX(reloadSoundFXPrefabID1, reloadSoundFXPos, 0);

            // Stash or drop the old magazine
            if (isMagRequired1 && isMagEquipped1)
            {
                UnequipMagazine1();
            }

            if (reloadEquipDelay1 > 0f)
            {
                yield return new WaitForSeconds(reloadEquipDelay1);

                // If there is an Equip Delay, also see if there is a sound to play when the mag equip phase begins
                PlaySoundFX(reloadEquipSoundFXPrefabID1, reloadSoundFXPos, 0);
            }

            // Unequipping phase has finished and equipping phase has begun
            isReloadUnequipping1 = false;
            isReloadEquipping1 = true;

            if (isHeld)
            {
                if (isMagInHand)
                {
                    if (isMagRequired1)
                    {
                        // We need to get the weapon from the character's hand, and attach it to the weapon
                        EquipMagazine1(newMag);
                    }
                    else
                    {
                        // If we don't need a StickyMagazine to be attached to the weapon,
                        // but we're getting the spare mag from character's hand, so
                        // simply destroy it.
                        newMag.DestroyInteractive();
                    }
                }
                else if (storeItem != null)
                {
                    if (isMagRequired1)
                    {
                        // We need to retrieve the stashed mag, and attach it to the weapon.
                        EquipMagazine1(newMag);
                    }
                    else
                    {
                        // If we don't need a StickyMagazine to be attached to the weapon,
                        // but we're getting the spare mag from character's Stash, so
                        // simply destroy it.

                        stickyControlModule.DestroyStashItem(storeItem);
                    }
                }
            }

            // Wait for reload to finish
            yield return new WaitForSeconds(reloadDuration1 - reloadEquipDelay1 < 0f ? 0f : reloadDuration1 - reloadEquipDelay1);

            // Finished reloading
            ammunition1 += ammunition;
            currentAmmoType1Int = ammoTypeInt;

            isReloadEquipping1 = false;

            // This value will be reset at the end of the next fixed update
            hasReloadFinished1 = true;

            isReloading1 = false;
        }

        /// <summary>
        /// Attempt to reload the secondary firing button mechanism
        /// </summary>
        /// <param name="ammunition"></param>
        /// <param name="ammoTypeInt"></param>
        /// <param name="storeItemID">Used if reloading from character Stash</param>
        /// <param name="isMagInHand">Is the character holding a mag to reload the weapon?</param>
        /// <returns></returns>
        protected virtual IEnumerator ReloadSecondary(int ammunition, int ammoTypeInt, int storeItemID, bool isMagInHand)
        {
            isReloading2 = true;
            if (magsInReserve2 > 0) { SetMagsInReserve(2, --magsInReserve2); }

            yield return new WaitForSeconds(reloadDelay2);

            // Start reloading process

            // This value will be reset at the end of the next fixed update
            hasReloadStarted2 = true;

            S3DStoreItem storeItem = null;

            // The magazine that will be used to reload the weapon (if any)
            StickyMagazine newMag = null;

            // If reloading with a mag in character hand, check it is still available and update mag capacity
            if (isMagInHand)
            {
                // Is the weapon still being held?
                if (isHeld)
                {
                    bool isHeldInLeftHand = false;

                    // If the non-empty mag is no longer in the hand of the character, cancel reloading
                    if(!GetMagazineFromCharacterHand(2, false, ref isHeldInLeftHand, ref newMag))
                    {
                        isReloading2 = false;
                        yield break;
                    }
                    else
                    {
                        magCapacity2 = newMag.MagCapacity;
                    }
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR ReloadSecondary " + name + " is no longer held but wants to use a mag in the hand of a character. PLEASE REPORT");
                    #endif
                    isReloading2 = false;
                    yield break;
                }
            }
            // If this is reloading from a mag in character Stash, check it is still available and update mag capcity
            else if (storeItemID != S3DStoreItem.NoStoreItem)
            {
                if (isHeld)
                {
                    storeItem = stickyControlModule.GetStashItem(storeItemID);

                    // If mag no longer available, cancel reloading
                    if (storeItem == null)
                    {
                        isReloading2 = false;
                        yield break;
                    }
                    else
                    {
                        newMag = (StickyMagazine)storeItem.stickyInteractive;
                        magCapacity2 = newMag.MagCapacity;
                    }
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR ReloadSecondary " + name + " is no longer held but wants to use Stashed Mags. PLEASE REPORT");
                    #endif
                    isReloading1 = false;
                    yield break;
                }
            }

            // Unequipping phase has begun
            isReloadUnequipping2 = true;

            Vector3 reloadSoundFXPos = magAttachPoint2 == null ? transform.position : magAttachPoint2.position;

            // If there is one, play the reload sound when the weapon starts to reload
            PlaySoundFX(reloadSoundFXPrefabID2, reloadSoundFXPos, 0);

            // Stash or drop the old magazine
            if (isMagRequired2 && isMagEquipped2)
            {
                UnequipMagazine2();
            }

            if (reloadEquipDelay2 > 0f)
            {
                yield return new WaitForSeconds(reloadEquipDelay2);

                // If there is an Equip Delay, also see if there is a sound to play when the mag equip phase begins
                PlaySoundFX(reloadEquipSoundFXPrefabID2, reloadSoundFXPos, 0);
            }

            // Unequipping phase has finished and equipping phase has begun
            isReloadUnequipping2 = false;
            isReloadEquipping2 = true;

            if (isHeld)
            {
                if (isMagInHand)
                {
                    if (isMagRequired2)
                    {
                        // We need to get the weapon from the character's hand, and attach it to the weapon
                        EquipMagazine2(newMag);
                    }
                    else
                    {
                        // If we don't need a StickyMagazine to be attached to the weapon,
                        // but we're getting the spare mag from character's hand, so
                        // simply destroy it.
                        newMag.DestroyInteractive();
                    }
                }
                else if (storeItem != null)
                {
                    if (isMagRequired2)
                    {
                        // We need to retrieve the stashed mag, and attach it to the weapon.
                        EquipMagazine2(newMag);
                    }
                    else
                    {
                        // If we don't need a StickyMagazine to be attached to the weapon,
                        // but we're getting the spare mag from character's Stash, so
                        // simply destroy it.

                        stickyControlModule.DestroyStashItem(storeItem);
                    }
                }
            }

            // Wait for reload to finish
            yield return new WaitForSeconds(reloadDuration2 - reloadEquipDelay2 < 0f ? 0f : reloadDuration2 - reloadEquipDelay2);

            // Finished reloading
            ammunition2 += ammunition;
            currentAmmoType2Int = ammoTypeInt;

            isReloadEquipping2 = false;

            // This value will be reset at the end of the next fixed update
            hasReloadStarted2 = true;

            isReloading2 = false;
        }

        /// <summary>
        /// Reset the heat level to 0 and reset health
        /// </summary>
        protected virtual void Repair()
        {
            heatLevel = 0;

            // Ensure threshold is a sensible value
            if (overHeatThreshold < 50f) { overHeatThreshold = 80f; }

            // This will also update the current performance
            Health = 100f;
        }

        /// <summary>
        /// Called by default in Update(), this will fire the weapon if required.
        /// </summary>
        protected virtual void UpdateWeapon()
        {
            // Get the deltaTime once from the property getter.
            float _deltaTime = Time.deltaTime;

            bool _isBeam = weaponTypeInt == BeamStandardInt;
            bool _isProjectileStandard = weaponTypeInt == ProjectileStandardInt;
            bool _isProjectileRaycast = weaponTypeInt == ProjectileRayCastInt;

            if (isWeaponEnabled)
            {
                // Do we need to cool the weapon?
                ManageHeat(_deltaTime, 0f);
            }

            // Process the primary firing button first, then the secondary (less common) next
            // Most weapons will only have a primary firing button
            for (int fBtn = 1; fBtn < 3; fBtn++)
            {
                if (!IsWeaponReady(fBtn)) { continue; }

                // Check if this weapon is waiting to fire
                if (fBtn == 1 && fireIntervalTimer1 > 0f)
                {
                    fireIntervalTimer1 -= _deltaTime;
                }
                else if (fBtn == 2 && fireIntervalTimer2 > 0f)
                {
                    fireIntervalTimer2 -= _deltaTime;
                }
                // Not waiting to fire
                else
                {
                    // If not reloading, check if we should fire the weapon or not
                    // For autofiring weapons, these will return false.
                    bool isReadyToFire = CanFireButton(fBtn);

                    // If this is a primary auto-firing, check if it is ready to fire
                    if (fBtn == 1 && weaponFiringButton1Int == FiringButtonAutoInt)
                    {
                        isLockedOnTarget1 = IsFacingTarget(1, 10f);

                        // NOTE: If check LoS is enabled, it will still fire if another enemy is between the weapon and the weapon.target.
                        isReadyToFire = isLockedOnTarget1 && (!checkLineOfSight1 || WeaponHasLineOfSight(fBtn));
                    }

                    // Check the weapon is not trying to reload
                    isReadyToFire = isReadyToFire && !IsReloading(fBtn);

                    // Trying to fire but out of ammo
                    if (isReadyToFire && !IsAmmoAvailable(fBtn))
                    {
                        PlayEmptyFire(fBtn);

                        // This allows you to run a weapon animation when attempting to fire without ammo.
                        // Avoid empty fire to occur before weapon is ready to fire again by resetting timers.
                        if (fBtn == 1)
                        {
                            hasEmptyFired1 = true;
                            fireIntervalTimer1 = fireInterval1;
                        }
                        else if (fBtn == 2)
                        {
                            hasEmptyFired2 = true;
                            fireIntervalTimer2 = fireInterval2;
                        }
                    }
                    else if (isReadyToFire)
                    {
                        #region Prepare to fire all firing position offsets

                        // Calculate the World-space Fire Position
                        Vector3 _weaponWSBasePos = GetWorldFireBasePosition();
                        Vector3 _weaponWSFireDir = GetWorldFireDirection();

                        // Muzzle positions and directions can be different for Projectile Raycast weapons
                        // as they "fire" from the camera not from the end of the weapon.
                        Vector3 _muzzleFXWSBasePos = _weaponWSBasePos;
                        Vector3 _muzzleFXWSFireDir = _weaponWSFireDir;

                        int _stickyId = 0, _factionId = 0, _modelId = 0;

                        if (isHeld && stickyControlModule != null)
                        {
                            _stickyId = stickyControlModule.StickyID;
                            _factionId = stickyControlModule.factionId;
                            _modelId = stickyControlModule.modelId;
                        }

                        // For a player, the raycast weapon should be fired from the centre of the camera so it will
                        // always hit the correct on-screen target location.
                        if (_isProjectileRaycast)
                        {
                            // This will return null for an NPC or if the weapon is not held by a player character.
                            Transform s3dPlayerCamTrfm = GetPlayerCameraTransform();

                            if (s3dPlayerCamTrfm != null)
                            {
                                _weaponWSFireDir = s3dPlayerCamTrfm.forward;
                                _weaponWSBasePos = s3dPlayerCamTrfm.position + (s3dPlayerCamTrfm.rotation * new Vector3(0f, 0f, stickyControlModule.lookCamera1.nearClipPlane));
                            }
                        }
                        else if (!isHipFire && isHeld)
                        {
                            // Recalculate fire direction using where the player is aiming
                            _weaponWSFireDir = (aimAtPosition - _weaponWSBasePos).normalized;

                            _muzzleFXWSFireDir = _weaponWSFireDir;
                        }

                        // Assume no fire position offsets (single barrel) as the most likely. Update below if required.
                        Vector3 _weaponWSFirePos = _weaponWSBasePos;
                        Vector3 _muzzleFXWSFirePos = _muzzleFXWSBasePos;

                        #endregion

                        // Have any fire positions for this mechanism fired?
                        bool hasFireButtonFired = false;

                        // Start the firing cycle with no heat generated
                        float heatValue = 0f;

                        // Loop through all fire positions
                        // NOTE: Currently only custom weapons support fire button 2.
                        for (int fp = 0; numFirePositionOffsets == 0 || fp < numFirePositionOffsets; fp++)
                        {
                            bool hasFPFired = false;

                            if (numFirePositionOffsets > 0)
                            {
                                _weaponWSFirePos = GetWorldFirePosition(_weaponWSBasePos, fp);
                                _muzzleFXWSFirePos = _isProjectileRaycast ? GetWorldFirePosition(_muzzleFXWSBasePos, fp) : _weaponWSFirePos;
                            }

                            #region Fire Beam at fire position offset
                            if (_isBeam)
                            {
                                // Currently we only support Beam Standard on primary firing mechanism
                                if (fBtn != 1) { break; }

                                // if the sequence number is 0, it means it is not active
                                if (beamItemKeyList[fp].beamSequenceNumber == 0)
                                {
                                    // When there are no fire position offsets, firePositionOffsetIndex is set to 0.
                                    // We "could" have used -1 but that the beam is not active.
                                    S3DInstantiateBeamParameters ibParms = new S3DInstantiateBeamParameters()
                                    {
                                        beamPrefabID = beamPrefabID,
                                        position = _weaponWSFirePos,
                                        fwdDirection = _weaponWSFireDir,
                                        upDirection = trfmUp,
                                        stickyId = _stickyId,
                                        factionId = _factionId,
                                        modelId = _modelId,
                                        firePositionOffsetIndex = numFirePositionOffsets > 0 ? fp : 0,
                                        beamSequenceNumber = 0,
                                        beamPoolListIndex = -1
                                    };

                                    // Create a beam using the StickyManager
                                    // Pass InstantiateBeamParameters by reference so we can get the beam index and sequence number back
                                    StickyBeamModule beamModule = stickyManager.InstantiateBeam(ref ibParms);
                                    if (beamModule != null)
                                    {
                                        hasFPFired = true;
                                        beamModule.callbackOnMove = MoveBeam;
                                        // Retrieve the unique identifiers for the beam instance
                                        beamItemKeyList[fp] = new S3DBeamItemKey(beamPrefabID, ibParms.beamPoolListIndex, ibParms.beamSequenceNumber);

                                        // Test local space 1.1.0 Beta 21
                                        beamModule.transform.SetParent(transform);

                                        // Immediately update the beam position (required for pooled beams that have previously been used)
                                        MoveBeam(beamModule);

                                        // heat is not updated here for beam weapons as it is updated in MoveBeam(..)
                                    }
                                }
                            }
                            #endregion

                            #region Fire Projectile Standard at fire position offset
                            else if (_isProjectileStandard)
                            {
                                // Currently we only support Projectile Standard on primary firing mechanism
                                if (fBtn != 1) { break; }

                                //Debug.Log("[DEBUG] Updateweapon projectile ammoTypeInt " + currentAmmoType1Int + " T:" + Time.time);

                                S3DInstantiateProjectileParameters ipParms = new S3DInstantiateProjectileParameters()
                                {
                                    projectilePrefabID = projectilePrefabID,
                                    position = _weaponWSFirePos,
                                    fwdDirection = _weaponWSFireDir,
                                    upDirection = trfmUp,
                                    stickyId = _stickyId,
                                    factionId = _factionId,
                                    modelId = _modelId,
                                    collisionMaskLayerInt = hitLayerMask,
                                    ammoTypeInt = currentAmmoType1Int,
                                    weaponVelocity = Vector3.zero,
                                    projectileSequenceNumber = 0,
                                    projectilePoolListIndex = -1
                                };

                                // Attempt to get another projectile instance from the pool.
                                StickyProjectileModule stickyProjectileModule = stickyManager.InstantiateProjectile(ref ipParms);

                                if (stickyProjectileModule != null)
                                {
                                    hasFPFired = true;

                                    // Heat value is inversely proportional to the firing interval
                                    if (fBtn == 1 && fireInterval1 > 0f) { heatValue += 1f / fireInterval1; }
                                    else if (fBtn == 2 && fireInterval2 > 0f) { heatValue += 1f / fireInterval2; }
                                }
                            }
                            #endregion

                            #region Fire Projectile Raycast at fire position offset
                            else if (_isProjectileRaycast)
                            {
                                // Currently we only support Projectile Raycast on primary firing mechanism
                                if (fBtn != 1) { break; }

                                hasFPFired = true;

                                // Heat value is inversely proportional to the firing interval
                                if (fBtn == 1 && fireInterval1 > 0f) { heatValue += 1f / fireInterval1; }
                                else if (fBtn == 2 && fireInterval2 > 0f) { heatValue += 1f / fireInterval2; }

                                // This is the prefab, not an instance of a projectile in the scene.
                                StickyProjectileModule _stickyProjectilePrefab = stickyManager.GetProjectilePrefab(projectilePrefabID);

                                // Update the non-serializable fields for the original prefab. They get overwritten
                                // when the next raycast projectile weapon or firing point uses this same prefab.
                                _stickyProjectilePrefab.sourceStickyId = _stickyId;
                                _stickyProjectilePrefab.sourceFactionId = _factionId;
                                _stickyProjectilePrefab.sourceModelId = _modelId;
                                _stickyProjectilePrefab.collisionLayerMask = hitLayerMask;
                                _stickyProjectilePrefab.ammoTypeInt = currentAmmoType1Int;
                                _stickyProjectilePrefab.effectsObjectPrefabID = -1;

                                if (stickyManager.CheckObjectHitByProjectile
                                (
                                    _weaponWSFirePos, _weaponWSFireDir, maxRange,
                                    hitLayerMask, out raycastHit, _stickyId,
                                    _stickyProjectilePrefab, false
                                ))
                                {
                                    if (_stickyProjectilePrefab.effectsObjectPrefabID >= 0)
                                    {
                                        S3DInstantiateEffectsObjectParameters ieParms = new S3DInstantiateEffectsObjectParameters
                                        {
                                            effectsObjectPrefabID = _stickyProjectilePrefab.effectsObjectPrefabID,
                                            position = raycastHit.point,
                                            rotation = Quaternion.LookRotation(_weaponWSFireDir)
                                        };

                                        // For projectiles we don't need to get the effectsObject key from ieParms.
                                        stickyManager.InstantiateEffectsObject(ref ieParms);
                                    }

                                    // Choice a decal from a list randomly
                                    int decalPrefabID = stickyManager.GetProjectileDecalPrefabID(projectilePrefabID);

                                    if (decalPrefabID >= 0)
                                    {
                                        // Get the decal from the pool and place it slightly in front of the object
                                        // to avoid z-fighting with the object renderer the projectile hit.
                                        S3DInstantiateDecalParameters idParms = new S3DInstantiateDecalParameters
                                        {
                                            decalPrefabID = decalPrefabID,
                                            position = raycastHit.point + (raycastHit.normal * 0.0005f),
                                            rotation = Quaternion.LookRotation(-raycastHit.normal),
                                            fwdDirection = -raycastHit.normal,
                                            collisionMaskLayerInt = (int)hitLayerMask,
                                            decalPoolListIndex = -1,
                                            decalSequenceNumber = 0
                                        };

                                        StickyDecalModule decalModule = stickyManager.InstantiateDecal(ref idParms);

                                        // Should we parent it to the object that was hit??
                                        if (idParms.decalSequenceNumber > 0 && decalModule != null && decalModule.isReparented)
                                        {
                                            decalModule.transform.SetParent(raycastHit.transform);
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region Fire Custom weapon at fire position offset
                            else
                            {
                                hasFPFired = FireCustomWeapon(fp, fBtn);

                                if (hasFPFired) { heatValue += 1f; }
                            }
                            #endregion

                            #region Has Fired at offset position
                            if (hasFPFired)
                            {
                                if (fBtn == 1)
                                {
                                    // This allow you to run a weapon animation when firing.
                                    hasFired1 = true;

                                    // At least 1 fire position for the primary firing mechanism has fired
                                    hasFireButtonFired = true;

                                    if (numMuzzleEffects1Valid > 0) { FireMuzzleFX(fp, _muzzleFXWSFirePos, _muzzleFXWSFireDir); }

                                    EjectSpentCartridge();

                                    // Check if we need to add smoke
                                    if (numSmokeEffects1 > 0) { AddSmoke(1, _weaponWSFirePos, _weaponWSFireDir); }

                                    // If not unlimited ammo, decrement the quantity available
                                    if (ammunition1 > 0) { ReduceAmmunition(1); }

                                    // After the last fire point has fired, we will need to wait to fire again.
                                    fireIntervalTimer1 = fireInterval1;

                                    // Did we just run out of ammo on the primary firing buttom?
                                    if (ammunition1 == 0)
                                    {
                                        if (callbackOnWeaponNoAmmo != null)
                                        {
                                            callbackOnWeaponNoAmmo.Invoke(this, 1);
                                        }
                                    }
                                }
                                else if (fBtn == 2)
                                {
                                    // This allow you to run a weapon animation when firing.
                                    hasFired2 = true;

                                    // At least 1 fire position for the secondary firing mechanism has fired
                                    hasFireButtonFired = true;

                                    // If not unlimited ammo, decrement the quantity available
                                    if (ammunition2 > 0) { ReduceAmmunition(2); }

                                    // After the last fire point has fired, we will need to wait to fire again.
                                    fireIntervalTimer2 = fireInterval2;

                                    // Did we just run out of ammo on the secondary firing button?
                                    if (ammunition2 == 0)
                                    {
                                        if (callbackOnWeaponNoAmmo != null)
                                        {
                                            callbackOnWeaponNoAmmo.Invoke(this, 2);
                                        }
                                    }
                                }

                                // Check ammo level again after a custom callback may
                                // have done an instant reload.
                                if (!IsAmmoAvailable(fBtn)) { break; }
                            }

                            #endregion

                            // If there are no fire position offsets, get out at the end of the first loop
                            if (numFirePositionOffsets == 0) { break; }
                        }

                        // Have at least 1 firing position for this firing mechanism fired?
                        if (hasFireButtonFired)
                        {
                            if (heatValue > 0f) { ManageHeat(_deltaTime, heatValue); }

                            // Recoil is not actioned if Recoil Speed is 0.
                            if (isHeld && recoilSpeed > 0f)
                            {
                                // See if we need to start the recoil action
                                Recoil(fBtn);
                            }

                            if (callbackOnWeaponFired != null)
                            {
                                callbackOnWeaponFired.Invoke(this, fBtn);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the CurrentPerformance value of this weapon. It gets automatically
        /// called when the Health is changed and when heat levels change.
        /// </summary>
        protected void UpdateWeaponPerformance()
        {
            // Update the current performance value
            if (heatLevel < 100f)
            {  
                currentPerformance = health;
                if (heatUpRate > 0f && heatLevel >= overHeatThreshold)
                {
                    currentPerformance *= 1f - S3DMath.Normalise(heatLevel, overHeatThreshold, 100f);
                }
                //currentPerformance = currentPerformance > minPerformance ? CurrentPerformance : minPerformance;
                currentPerformance = currentPerformance < 1f ? CurrentPerformance : 1f;
            }
            else { currentPerformance = 0f; }
        }

        #endregion

        #region Event Methods

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Deactivate all beam weapons that are currently firing
        /// </summary>
        public void DeactivateBeams ()
        {
            if (isWeaponInitialised && weaponTypeInt == BeamStandardInt)
            {
                int numBeams = beamItemKeyList == null ? 0 : beamItemKeyList.Count;

                for (int bItemIdx = 0; bItemIdx < numBeams; bItemIdx++)
                {
                    if (beamItemKeyList[bItemIdx].beamSequenceNumber > 0)
                    {
                        stickyManager.DeactivateBeam(beamItemKeyList[bItemIdx]);
                    }

                    beamItemKeyList[bItemIdx] = new S3DBeamItemKey(-1, -1, 0);
                }
            }
        }

        /// <summary>
        /// Safely destroy this weapon.
        /// 1. Deactivate any beams
        /// 2. Return muzzle fx to the pool
        /// 3. Return any reload sound fx to the pool
        /// 4. Drop weapon if held by character
        /// 5. Return any popup to the pool (in DestroyInteractive)
        /// 6. Destroy the gameobject (in DestroyInteractive)
        /// </summary>
        public virtual void DestroyWeapon()
        {
            if (isWeaponInitialised)
            {
                DeactivateBeams();

                for (int fp = 0; fp < numFirePositionOffsets; fp++)
                {
                    DestroyMuzzleFX(fp);
                }

                // Reload Sound FX - return to pool.
                // There should be 4 (reload SFX and reload Equip SFX)
                for (int reIdx = 0; reIdx < reloadSFXItemKeyList.Count; reIdx++)
                {
                    // Could this be an active StickyEffectsModule in the pool?
                    // We don't need to reset the S3DEffectItemKey as we're going to destroy the weapon
                    if (reloadSFXItemKeyList[reIdx].effectsObjectSequenceNumber > 0)
                    {
                        stickyManager.DestroyEffectsObject(reloadSFXItemKeyList[reIdx]);
                    }
                }

                // Smoke Effects - return to pool
                for (int seIdx = 0; seIdx < maxActiveSmokeEffects1; seIdx++)
                {
                    // Could this be an active smoke StickyEffectsModule in the pool?
                    // We don't need to reset the S3DEffectItemKey as we're going to destroy the weapon
                    if (smokeItemKeyList[seIdx].effectsObjectSequenceNumber > 0)
                    {
                        stickyManager.DestroyEffectsObject(smokeItemKeyList[seIdx]);
                    }
                }

                DecommisionScope();

                // If held by a character
                if (isHeld && stickyControlModule != null)
                {
                    // Disable gravity to avoid having to setup a rigidbody only to destroy it again
                    isUseGravity = false;
                    DropObject(stickyControlModule.StickyID);
                }
            }

            DestroyInteractive();
        }

        /// <summary>
        /// Attempt to disable the weapon
        /// </summary>
        public void DisableWeapon()
        {
            EnableOrDisableWeapon(false);
        }

        /// <summary>
        /// Drop the weapon
        /// 1. Stop firing
        /// 2. If equipped, turn off laser sight
        /// 3. Re-enable colliders that where disabled when grabbed
        /// 4. Configure gravity if Use Gravity is enabled
        /// </summary>
        /// <param name="stickyID"></param>
        public override void DropObject (int stickyID)
        {
            ResetInput();

            EnableOrDisableLaserSight(false);
            EnableOrDisableAiming(false, false);
            // Call restore reticle AFTER EnableOrDisableAiming()
            RestoreReticle();
            EnableOrDisableScope(false);
            DecommisionScope();
            AnimateCharacterStateExit(true, false, false, false);
            RestoreLookSettings();
            ApplyOrRevertWeaponAnimSets(false);
            UnAssignTargetCharacter();
            UnAssignTargetTransform();

            // This also invokes any drop events setup by the user
            base.DropObject(stickyID);

            if (isInitialised && isGrabbable)
            {
                if (isDisableRegularColOnGrab) { EnableNonTriggerColliders(); }
                if (isDisableTriggerColOnGrab) { EnableTriggerColliders(); }
            }

            if (isWeaponInitialised)
            {
                CheckMagsInReserve();

                if (isUseGravity)
                {
                    SetUpRigidbody();
                    RestoreRigidbodySettings();
                }
            }
        }

        /// <summary>
        /// Attempt to enable the weapon
        /// </summary>
        public void EnableWeapon()
        {
            EnableOrDisableWeapon(true);
        }

        //public override void GrabObject (int stickyID, bool isSecondaryHandHold)
        //{
        //   base.GrabObject (stickyID, isSecondaryHandHold);

        //}

        /// <summary>
        /// Get the world space fire position without any offsets
        /// </summary>
        /// <returns></returns>
        public Vector3 GetWorldFireBasePosition()
        {
            if (isWeaponInitialised)
            {
                return trfmPos + (trfmRot * relativeFirePosition);
            }
            else
            {
                return transform.position + (transform.rotation * relativeFirePosition);
            }
        }

        /// <summary>
        /// Get the world space fire direction of the weapon
        /// </summary>
        /// <returns></returns>
        public Vector3 GetWorldFireDirection()
        {
            if (isWeaponInitialised)
            {
                return trfmRot * fireDirection;
            }
            else
            {
                return transform.rotation * fireDirection;
            }
        }

        /// <summary>
        /// Get the world space fire position of the fire position offset (if any) 
        /// If there are no firePostionOffsets, the firePositionOffsetIndex should be set to 0.
        /// See also UpdateWeapon() which fires the weapon.
        /// </summary>
        /// <param name="weaponWorldBasePosition"></param>
        /// <param name="firePositionOffsetIndex"></param>
        /// <returns></returns>
        public Vector3 GetWorldFirePosition (Vector3 weaponWorldBasePosition, int firePositionOffsetIndex)
        {
            // If there are no fire position offsets, i.e. only a single barrel, the fire position will be the base fire position
            if (isWeaponInitialised)
            {
                return weaponWorldBasePosition + (trfmRot * (numFirePositionOffsets == 0 ? Vector3.zero : firePositionOffsets[firePositionOffsetIndex]));
            }
            else
            {
                return weaponWorldBasePosition + (transform.rotation * (NumberFirePositionOffsets == 0 ? Vector3.zero : firePositionOffsets[firePositionOffsetIndex]));
            }
        }

        /// <summary>
        /// Get the world space direction the spent cartridge is ejected
        /// </summary>
        /// <returns></returns>
        public Vector3 GetWorldSpentEjectDirection()
        {
            if (isWeaponInitialised)
            {
                return trfmRot * spentEjectDirection;
            }
            else
            {
                return transform.rotation * spentEjectDirection;
            }
        }

        /// <summary>
        /// Get the world space spent cartridge eject position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetWorldSpentEjectPosition()
        {
            if (isWeaponInitialised)
            {
                return trfmPos + (trfmRot * spentEjectPosition);
            }
            else
            {
                return transform.position + (transform.rotation * spentEjectPosition);
            }
        }

        /// <summary>
        /// Get the world space spent cartridge eject rotation
        /// </summary>
        /// <returns></returns>
        public Quaternion GetWorldSpaceEjectRotation()
        {
            if (isWeaponInitialised)
            {
                return trfmRot * Quaternion.Euler(spentEjectRotation);
            }
            else
            {
                return transform.rotation * Quaternion.Euler(spentEjectRotation);
            }
        }

        /// <summary>
        /// Initialise the weapon
        /// </summary>
        public override void Initialise()
        {
            // We want to apply gravity AFTER base.FixedUpdate() has run.
            isDelayApplyGravity = true;

            base.Initialise();

            if (isInitialised)
            {
                stickyManager = StickyManager.GetOrCreateManager(gameObject.scene.handle);

                // Keep compiler happy
                if (showWFPGizmosInSceneView && showWSCEPGizmosInSceneView && showAmmoSettingsInEditor && showAnimateSettingsInEditor &&
                    showAimingAndReticleSettingsInEditor && showGeneralSettingsInEditor && showMuzzleFXSettingsInEditor && showSpentCartridgeSettingsInEditor &&
                    showRecoilSettingsInEditor && showSmokeSettingsInEditor && showAttachmentSettingsInEditor && showHealthSettingsInEditor &&
                    isS3DAnimActionListExpanded && isAnimActionsExpanded && isWeaponAnimSetsExpanded) { }

                ReInitialiseWeapon();
            }
        }

        /// <summary>
        /// Call this after making any changes to beams, projectiles or effects for this weapon.
        /// </summary>
        public virtual void ReInitialiseWeapon()
        {
            // If the base class hasn't already been initialised, this has no effect.
            if (isInitialised)
            {
                weaponTypeInt = (int)weaponType;
                weaponFiringType1Int = (int)firingType1;
                weaponFiringType2Int = (int)firingType2;
                weaponFiringButton1Int = (int)firingButton1;
                weaponFiringButton2Int = (int)firingButton2;

                reloadType1Int = (int)reloadType1;
                reloadType2Int = (int)reloadType2;

                raycastHit = new RaycastHit();

                // Temp reusable hit array
                if (raycastHitInfoArray == null) { raycastHitInfoArray = new RaycastHit[20]; }

                numFirePositionOffsets = firePositionOffsets == null ? 0 : firePositionOffsets.Length;

                ResetHasHappenedSinceFixedUpdate();

                ResetInput();

                isReloadable = IsProjectileWeapon;

                // The number of EffectsModules in the editor (some may be null or invalid)
                numMuzzleEffects1 = muzzleEffects1 == null ? 0 : muzzleEffects1.Length;
                numMuzzleEffects1Valid = 0;

                // There must always be 1 fire "position" even when there are no position offsets.
                // When the weapon has 1 barrel, there are typically no fire position offsets.
                int _minFirePositions = numFirePositionOffsets < 1 ? 1 : numFirePositionOffsets;

                // If beam, projectile or effects associated with this weapon have
                // changed, we need to verify they have been pooled correctly.
                stickyManager.ReinitialiseWeaponPoolItems(this);

                if (randomSelectFX == null)
                {
                    randomSelectFX = new S3DRandom();
                    randomSelectFX.SetSeed(3391);
                }

                if (weaponTypeInt == BeamStandardInt)
                {
                    // If instant recharge, make fully charged
                    if (rechargeTime == 0f) { chargeAmount = 1f; }

                    ConfigureBeamItemKeys(_minFirePositions);
                }
                else { beamItemKeyList = null; }

                // The muzzle item keys are used to associate a muzzle FX in the pooling system
                // with a weapon fire point offset. There should always be one to indicate a
                // single barrel.
                ConfigureEffectsItemKeys(ref muzzleItemKey1List, _minFirePositions);

                // Items 0 and 1 are for the reload sound fx.
                // Items 2 and 3 are for the reload Equip sound fx.
                // If changing this, also update DestroyWeapon()
                ConfigureEffectsItemKeys(ref reloadSFXItemKeyList, 4);

                // Initialise raycast ray
                raycastRay = new Ray(Vector3.zero, Vector3.up);

                // Reset
                isLockedOnTarget1 = false;
                HasLineOfSight1 = false;
                HasLineOfSight2 = false;
                Repair();

                ValidateAmmo();

                // If the ammo types aren't set in the manage, attempt to update them from this weapon.
                if (stickyManager.AmmoTypes == null && ammoTypes != null) { stickyManager.SetAmmoTypes(ammoTypes); }

                CheckMagsInReserve();

                RefreshAnimateSettings(!isWeaponInitialised);

                EnableOrDisableAnimate(defaultAnimator != null);

                ReinitialiseEmptyFire(1);
                ReinitialiseEmptyFire(2);

                recoilCurrentPos = Vector3.zero;
                recoilTargetPos = Vector3.zero;
                recoilCurrentRot = Vector3.zero;
                recoilTargetRot = Vector3.zero;

                ReinitialiseSmoke(1);
                ReinitialiseSmoke(2);

                ReinitialiseEquippedMag(1);
                ReinitialiseEquippedMag(2);

                // Check if we need to equip a Laser Sight
                if (laserSightAimFrom != null) { EquipLaserSight(); }

                InitialiseScopeCamera();

                // Finally, reinitialise any custom weapons
                isWeaponInitialised = ReinitialiseCustomWeapons();
            }
        }

        /// <summary>
        /// Pause the weapon
        /// This is useful when pausing a game or scene.
        /// See also: DisableWeapon(), UnPauseWeapon().
        /// </summary>
        public void PauseWeapon()
        {
            PauseOrUnPauseWeapon (true);
        }

        /// <summary>
        /// Reset the weapon input to default settings.
        /// </summary>
        public virtual void ResetInput()
        {
            isFire1Input = false;
            isFire2Input = false;
            isPrevFire1Input = false;
            isPrevFire2Input = false;
            isFire1InputHeld = false;
            isFire2InputHeld = false;
        }

        /// <summary>
        /// Send input data to the weapon
        /// </summary>
        /// <param name="weaponInput"></param>
        public virtual void SendInput (WeaponInput weaponInput)
        {
            isFire1Input = weaponInput.fire1;
            isFire2Input = weaponInput.fire2;

            if (isPrevFire1Input && isFire1Input) { isFire1InputHeld = true; }
            else { isFire1InputHeld = false; }

            if (isPrevFire2Input && isFire2Input) { isFire2InputHeld = true; }
            else { isFire2InputHeld = false; }

            isPrevFire1Input = isFire1Input;
            isPrevFire2Input = isFire2Input;
        }

        /// <summary>
        /// Set the chargeAmount. It is automatically clamped between 0.0 and 1.0.
        /// </summary>
        /// <param name="newChargeAmount"></param>
        public void SetChargeAmount (float newChargeAmount)
        {
            // Clamp value 0.0 to 1.0
            chargeAmount = newChargeAmount < 0f ? 0f : newChargeAmount > 1f ? 1f : newChargeAmount;
        }

        /// <summary>
        /// Set if line of sight should be checked for an AutoFire mechanism
        /// </summary>
        /// <param name="newCheckLineOfSight"></param>
        /// <param name="weaponButtonNumber">primary (1) or secondary (2)</param>
        public void SetCheckLineOfSight (int weaponButtonNumber, bool newCheckLineOfSight)
        {
            if (weaponButtonNumber == 1) { checkLineOfSight1 = newCheckLineOfSight; }
            else if (weaponButtonNumber == 2) { checkLineOfSight2 = newCheckLineOfSight; }
        }

        /// <summary>
        /// Set if line of sight should be checked for an AutoFire primary mechanism
        /// </summary>
        /// <param name="newCheckLineOfSight"></param>
        public void SetCheckLineOfSight1 (bool newCheckLineOfSight)
        {
            checkLineOfSight1 = newCheckLineOfSight;
        }

        /// <summary>
        /// Set if line of sight should be checked for an AutoFire secondary mechanism
        /// </summary>
        /// <param name="newCheckLineOfSight"></param>
        public void SetCheckLineOfSight2 (bool newCheckLineOfSight)
        {
            checkLineOfSight2 = newCheckLineOfSight;
        }

        /// <summary>
        /// Set the fireInterval. It is automatically clamped between 0.05 and 10 seconds.
        /// </summary>
        /// <param name="newFireInterval"></param>
        public void SetFireInterval (float newFireInterval)
        {
            fireInterval1 = newFireInterval < 0.05f ? 0.05f : newFireInterval > 10f ? 10f : newFireInterval;
        }

        /// <summary>
        /// Set the local space fire direction of the weapon.
        /// </summary>
        /// <param name="newFireDirection"></param>
        public void SetFireDirection (Vector3 newFireDirection)
        {
            if (newFireDirection.sqrMagnitude < Mathf.Epsilon)
            {
                fireDirection = Vector3.forward;
            }
            else { fireDirection = newFireDirection.normalized; }
        }

        /// <summary>
        /// Set the FiringButton of the first (main) firing button
        /// </summary>
        /// <param name="newFiringButton"></param>
        public void SetFiringButton1 (FiringButton newFiringButton)
        {
            firingButton1 = newFiringButton;
            weaponFiringButton1Int = (int)firingButton1;
        }

        /// <summary>
        /// Set the FiringButton of the second firing button. This is an alternative
        /// firing mechanism for custom weapons.
        /// </summary>
        /// <param name="newFiringButton"></param>
        public void SetFiringButton2 (FiringButton newFiringButton)
        {
            firingButton2 = newFiringButton;
            weaponFiringButton2Int = (int)firingButton2;
        }

        /// <summary>
        /// Set the FiringType of the first (main) firing button.
        /// </summary>
        /// <param name="newFiringType"></param>
        public void SetFiringType1 (FiringType newFiringType)
        {
            firingType1 = newFiringType;
            weaponFiringType1Int = (int)firingType1;
        }

        /// <summary>
        /// Set the FiringType of the second firing button.
        /// </summary>
        /// <param name="newFiringType"></param>
        public void SetFiringType2 (FiringType newFiringType)
        {
            firingType2 = newFiringType;
            weaponFiringType2Int = (int)firingType2;
        }

        /// <summary>
        /// Set the weapon firePositionOffsets.
        /// </summary>
        /// <param name="newFirePositions"></param>
        public void SetFirePositionOffsets (Vector3[] newFirePositions)
        {
            firePositionOffsets = newFirePositions;
            numFirePositionOffsets = firePositionOffsets == null ? 0 : firePositionOffsets.Length;
        }

        /// <summary>
        /// Set the maximum range for this weapon.
        /// The minimum range is 1 metre.
        /// </summary>
        /// <param name="newMaxRange"></param>
        public void SetMaxRange (float newMaxRange)
        {
            if (newMaxRange < 1f) { newMaxRange = 1f; }
            else { maxRange = newMaxRange; }
        }

        /// <summary>
        /// Set the array of StickyEffectsModules used for Muzzle FX.
        /// </summary>
        /// <param name="newMuzzleEffects"></param>
        public void SetMuzzleEffects1 (StickyEffectsModule[] newMuzzleEffects)
        {
            muzzleEffects1 = newMuzzleEffects;
            numMuzzleEffects1 = muzzleEffects1 == null ? 0 : muzzleEffects1.Length;
        }

        /// <summary>
        /// Set if weapon can only fire, reload, or be animated, when it is held by a character
        /// </summary>
        /// <param name="newValue"></param>
        public void SetOnlyUseWhenHeld (bool newValue)
        {
            isOnlyUseWhenHeld = newValue;
        }

        /// <summary>
        /// Set the rechargeTime. It is automatically clamped between 0 and 30 seconds.
        /// </summary>
        /// <param name="newRechargeTime"></param>
        public void SetRechargeTime (float newRechargeTime)
        {
            // Clamp value 0.0 to 30.0
            rechargeTime = newRechargeTime < 0f ? 0f : newRechargeTime > 1f ? 1f : newRechargeTime;
        }

        /// <summary>
        /// Set the local space direction the spent cartridge is ejected
        /// </summary>
        /// <param name="newDirection"></param>
        public void SetSpentEjectDirection (Vector3 newDirection)
        {
            if (newDirection.sqrMagnitude < Mathf.Epsilon)
            {
                spentEjectDirection = Vector3.forward;
            }
            else { spentEjectDirection = newDirection.normalized; }
        }

        /// <summary>
        /// Set the force used to eject the spent cartridge from the weapon in the Spent Eject Direction.
        /// </summary>
        /// <param name="newForce"></param>
        public void SetSpentEjectForce (float newForce)
        {
            // Ensure the force is not a negative value
            spentEjectForce = newForce < 0f ? -newForce : newForce;
        }

        /// <summary>
        /// Set the local space position on the weapon from which the spent cartridge is ejected.
        /// </summary>
        /// <param name="newPosition"></param>
        public void SetSpentEjectPosition (Vector3 newPosition)
        {
            spentEjectPosition = newPosition;
        }

        /// <summary>
        /// Set the local space rotation, in Euler angles, of the spent cartridge when ejected.
        /// </summary>
        /// <param name="newRotation"></param>
        public void SetSpentEjectRotation (Vector3 newRotation)
        {
            spentEjectRotation = newRotation;
        }

        /// <summary>
        /// Unpause the weapon. This is useful when unpausing a game or scene.
        /// See also: EnableWeapon(), PauseWeapon()
        /// </summary>
        public void UnpauseWeapon()
        {
            PauseOrUnPauseWeapon (false);
        }

        /// <summary>
        /// Returns whether a weapon has line of sight to a target (if there is one assigned)
        /// NOTE: Currently both primary (1) and secondary (2) firing mechanisms return the same result.
        /// As we find use-cases for the secondary firing button, this may change.
        /// </summary>
        public bool WeaponHasLineOfSight (int weaponButtonNumber)
        {
            UpdateLineOfSight(weaponButtonNumber);

            return weaponButtonNumber == 1 ? HasLineOfSight1 : weaponButtonNumber == 2 ? HasLineOfSight2 : false;
        }

        #endregion

        #region Public API Methods - Aiming and Reticle

        /// <summary>
        /// Check to see if a player (non-NPC) is holding the weapon and a different reticle is
        /// required on the active display module (if there is one).
        /// </summary>
        public void CheckReticle()
        {
            if (isHeld && !stickyControlModule.IsNPC() && (isNoReticleIfHeld || isNoReticleOnAimingFPS || isNoReticleOnAimingTPS || isNoReticleIfHeldNoFreeLook || defaultReticleSprite != null || aimingReticleSprite != null))
            {
                bool isThirdPerson = stickyControlModule.isThirdPerson;

                StickyDisplayModule stickyDisplayModule = StickyDisplayModule.GetActiveDisplayModule();

                if (stickyDisplayModule != null)
                {
                    if (isNoReticleIfHeld)
                    {
                        stickyDisplayModule.HideDisplayReticle();
                    }
                    else if (isAiming)
                    {
                        if (isNoReticleOnAimingFPS && !isThirdPerson) { stickyDisplayModule.HideDisplayReticle(); }
                        else if (isNoReticleOnAimingTPS && isThirdPerson) { stickyDisplayModule.HideDisplayReticle(); }
                        else if (aimingReticleSprite != null)
                        {
                            stickyDisplayModule.LoadDisplayReticleSprite(aimingReticleSprite);
                            stickyDisplayModule.ShowDisplayReticle();
                        }
                        else if (defaultReticleSprite != null)
                        {
                            stickyDisplayModule.LoadDisplayReticleSprite(defaultReticleSprite);
                            stickyDisplayModule.ShowDisplayReticle();
                        }

                        // Centre reticle
                        stickyDisplayModule.SetDisplayReticleOffset(0f, 0f);
                    }
                    else if (isNoReticleIfHeldNoFreeLook && !stickyControlModule.IsLookFreeLookEnabled)
                    {
                        stickyDisplayModule.HideDisplayReticle();
                    }
                    else if (defaultReticleSprite != null)
                    {
                        stickyDisplayModule.LoadDisplayReticleSprite(defaultReticleSprite);

                        // If it could have been turned off, turn it back on
                        if (isNoReticleOnAimingFPS || isNoReticleOnAimingTPS) { stickyDisplayModule.ShowDisplayReticle(); }
                    }
                    else if (aimingReticleSprite != null)
                    {
                        // Maybe it was aiming but now isn't - however there is no default reticle
                        // for this weapon.
                        if (stickyDisplayModule.IsDisplayReticleShown)
                        {
                            // Make sure the aim reticle is not being displayed
                            // NOTE: Call ShowDisplayReticle() won't work because it will think
                            // nothing has changed.
                            S3DDisplayReticle reticle = stickyDisplayModule.GetDisplayReticle(savedActiveDisplayReticleHash);

                            stickyDisplayModule.LoadDisplayReticleSprite(reticle.primarySprite);
                        }
                    }
                    // No default reticle for this weapon, is held (but not aiming), and only shows the reticle
                    // if FreeLook is enabled.
                    else if (isNoReticleIfHeldNoFreeLook && !isAiming && stickyControlModule.IsLookFreeLookEnabled)
                    {
                        stickyDisplayModule.ShowDisplayReticle();
                    }
                }
            }
        }

        /// <summary>
        /// If required, attempt to restore character look settings when a weapon, just before it is no
        /// longer held.
        /// </summary>
        public void RestoreLookSettings()
        {
            if (isHeld && stickyControlModule != null)
            {
                // If FreeLook was possible when holding a weapon, AND it was on
                // before the weapon was held, turn it back on.
                // Attempt to restore the Look Follow Head state.
                if (!stickyControlModule.IsFreeLookWhenWeaponHeld)
                {
                    stickyControlModule.CheckRestoreFreeLook();
                    stickyControlModule.CheckRestoreFollowHead();
                    stickyControlModule.CheckRestoreFollowHeadTP();
                }
            }
        }

        /// <summary>
        /// If required, attempt to reset the DisplayReticle to the original one used on the
        /// active StickyDisplayModule (HUD), before the weapon was grabbed by a non-NPC character.
        /// </summary>
        public void RestoreReticle()
        {
            if (savedActiveDisplayReticleHash != 0)
            {
                StickyDisplayModule stickyDisplayModule = StickyDisplayModule.GetActiveDisplayModule();

                if (stickyDisplayModule != null)
                {
                    stickyDisplayModule.ChangeDisplayReticle(savedActiveDisplayReticleHash);

                    if (savedIsDisplayReticleShown)
                    {
                        stickyDisplayModule.ShowDisplayReticle();
                    }
                    else
                    {
                        stickyDisplayModule.HideDisplayReticle();
                    }

                    stickyDisplayModule.lockDisplayReticleToCursor = savedLockedReticleToCursor;

                    savedActiveDisplayReticleHash = 0;
                    savedIsDisplayReticleShown = false;
                    savedLockedReticleToCursor = false;
                }
            }
        }

        /// <summary>
        /// Save the StickyDisplayModule reticle settings when the weapon is picked
        /// up by a character.
        /// NOTE: This could cause issues if there is also a weapon still held in the other hand...
        /// </summary>
        public void SaveReticleSettings()
        {
            savedIsDisplayReticleShown = false;
            savedActiveDisplayReticleHash = 0;
            savedLockedReticleToCursor = false;

            if (isHeld && !stickyControlModule.IsNPC())
            {
                StickyDisplayModule stickyDisplayModule = StickyDisplayModule.GetActiveDisplayModule();

                if (stickyDisplayModule != null)
                {
                    savedIsDisplayReticleShown = stickyDisplayModule.IsDisplayReticleShown;
                    savedActiveDisplayReticleHash = stickyDisplayModule.guidHashActiveDisplayReticle;
                    savedLockedReticleToCursor = stickyDisplayModule.lockDisplayReticleToCursor;
                }
            }
        }

        /// <summary>
        /// Set the character first person camera field of view when aiming a held weapon
        /// </summary>
        /// <param name="newFoV"></param>
        public void SetAimingFirstPersonFOV (float newFoV)
        {
            aimingFirstPersonFOV = newFoV;

            if (isWeaponInitialised && isAiming && isHeld && stickyControlModule != null && !stickyControlModule.IsNPC() && !stickyControlModule.isThirdPerson && stickyControlModule.IsLookEnabled)
            {
                targetAimingFieldofView = aimingFirstPersonFOV;
                isAimingSmoothEnable = true;
            }
        }

        /// <summary>
        /// Set the reticle (sprite) used when the weapon is held by a S3D player and aiming
        /// </summary>
        /// <param name="newSprite"></param>
        public void SetAimingReticleSprite (Sprite newSprite)
        {
            aimingReticleSprite = newSprite;
        }

        /// <summary>
        /// Set the character third person camera field of view when aiming a held weapon
        /// </summary>
        /// <param name="newFoV"></param>
        public void SetAimingThirdPersonFOV (float newFoV)
        {
            aimingThirdPersonFOV = newFoV;

            if (isWeaponInitialised && isAiming && isHeld && stickyControlModule != null && !stickyControlModule.IsNPC() && stickyControlModule.isThirdPerson && stickyControlModule.IsLookEnabled)
            {
                targetAimingFieldofView = aimingThirdPersonFOV;
                isAimingSmoothEnable = true;
            }
        }

        /// <summary>
        /// Set the reticle (sprite) used when the weapon is held by a S3D player
        /// </summary>
        /// <param name="newSprite"></param>
        public void SetDefaultReticleSprite (Sprite newSprite)
        {
            defaultReticleSprite = newSprite;
        }

        /// <summary>
        /// Attempt to start aiming the weapon if held by a character
        /// </summary>
        public void StartAiming (bool isSmoothStart)
        {
            EnableOrDisableAiming(true, isSmoothStart);
        }

        /// <summary>
        /// Stop aiming the weapon if held by a character
        /// </summary>
        public void StopAiming (bool isSmoothStop)
        {
            EnableOrDisableAiming(false, isSmoothStop);
        }

        /// <summary>
        /// Attempt to toggle aiming the weapon if held by a character.
        /// If you want an instant transition, call StartAiming(false)
        /// or StopAiming(false).
        /// </summary>
        public void ToggleAiming()
        {
            EnableOrDisableAiming(!isAiming, isHeld);
        }

        #endregion

        #region Public API Methods - Attachments

        /// <summary>
        /// Check to see if we need to automatically turn on the laser sight.
        /// This is called automatically when a character grabs the weapon.
        /// </summary>
        public void EnableLaserSightIfRequired()
        {
            if (isHeld && isLaserSightReady)
            {
                EnableOrDisableLaserSight(isLaserSightAutoOn);
            }
        }

        /// <summary>
        /// Check to see if we need to automatically turn on the Scope.
        /// This is called automatically when a character grabs the weapon.
        /// </summary>
        public void EnableScopeIfRequired()
        {
            if (isHeld)
            {
                EnableOrDisableScope(isScopeAutoOn);
            }
        }

        /// <summary>
        /// Attempt to equip the laser sight on the weapon
        /// and get it ready to turn on/off.
        /// It is turned off when it is first equipped.
        /// See also SetLaserSightAimFrom(newLaserSightTransform).
        /// </summary>
        public void EquipLaserSight()
        {
            if (!isLaserSightReady && laserSightAimFrom != null)
            {
                // Check just in case user has already set up a line renderer for the laser sight
                if (!laserSightAimFrom.TryGetComponent(out laserSightLineRenderer))
                {
                    laserSightLineRenderer = laserSightAimFrom.gameObject.AddComponent<LineRenderer>();

                    laserSightLineRenderer.receiveShadows = false;
                    laserSightLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    /// TODO - cater for HDRP and URP
                    laserSightLineRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                    laserSightLineRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

                    laserSightLineRenderer.startColor = laserSightColour;
                    laserSightLineRenderer.endColor = laserSightColour;

                    laserSightLineRenderer.startWidth = 0.002f;
                    laserSightLineRenderer.endWidth = 0.002f;

                    laserSightLineRenderer.sharedMaterial = S3DUtils.GetDefaultLineMaterial();
                }

                if (laserSightLineRenderer != null)
                {
                    // The laser sight always uses local space
                    laserSightLineRenderer.useWorldSpace = false;

                    laserSightLineRenderer.loop = false;

                    laserSightLineRenderer.alignment = LineAlignment.View;

                    // Set the end position to be a small value so that when the weapon is "found"
                    // in the scene view it doesn't place the "centre" of the weapon half the maxRange away from the weapon.
                    laserSightLineRenderer.SetPositions(new Vector3[] { Vector3.zero, new Vector3(0f, 0f, 0.01f) });

                    isLaserSightReady = true;

                    EnableOrDisableLaserSight(false);
                }
            }
        }

        /// <summary>
        /// Attempt to equip a magazine on the weapon
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        public void EquipMagazine (StickyMagazine stickyMagazine, int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1) { EquipMagazine1(stickyMagazine); }
            else if (weaponButtonNumber == 2) { EquipMagazine2(stickyMagazine); }
        }

        /// <summary>
        /// Attempt to equip a magazine onto the weapon for the primary firing mechanism.
        /// </summary>
        /// <param name="stickyMagazine"></param>
        public void EquipMagazine1 (StickyMagazine stickyMagazine)
        {
            if (stickyMagazine == null)
            {
                isMagEquipped1 = false;
            }
            else if (magAttachPoint1 == null)
            {
                isMagEquipped1 = false;
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] EquipMagazine1 - Cannot equip " + stickyMagazine.name + " onto " + name + " because magAttachPoint1 is null");
                #endif
            }
            else
            {
                if (stickyMagazine.HasRigidbody)
                {
                    stickyMagazine.RemoveRigidbody();
                }

                // Check if we need to disable regular colliders on the mag when being equipped onto a weapon
                if (stickyMagazine.IsDisableRegularColOnEquip && (!stickyMagazine.IsHeld || !stickyMagazine.isDisableRegularColOnGrab))
                {
                    stickyMagazine.DisableNonTriggerColliders();
                }
                // If the magazine wasn't held by a character, but the weapon is held,
                // and we didn't disable the non-trigger colliders, register them as being part of the character.
                else if (!stickyMagazine.IsHeld && isHeld && stickyControlModule != null)
                {
                    // Register the active colliders on the magazine
                    stickyControlModule.AttachColliders(stickyMagazine.Colliders);
                }

                // Attach the magazine to the weapon
                Transform magTfrm = stickyMagazine.transform;
                magTfrm.SetParent(magAttachPoint1);
                magTfrm.localPosition = Vector3.zero;
                magTfrm.localRotation = Quaternion.identity;

                equippedMag1 = stickyMagazine;

                // Disable the magazine script
                stickyMagazine.enabled = false;

                // If this was stashed, re-enable it when equipped on the weapon
                if (!magTfrm.gameObject.activeSelf) { magTfrm.gameObject.SetActive(true); }

                isMagEquipped1 = true;
            }
        }

        /// <summary>
        /// Attempt to equip a magazine onto the weapon for the secondary firing mechanism.
        /// </summary>
        /// <param name="stickyMagazine"></param>
        public void EquipMagazine2 (StickyMagazine stickyMagazine)
        {
            if (stickyMagazine == null)
            {
                isMagEquipped2 = false;
            }
            else if (magAttachPoint2 == null)
            {
                isMagEquipped2 = false;
                #if UNITY_EDITOR
                Debug.LogWarning("[ERROR] EquipMagazine2 - Cannot equip " + stickyMagazine.name + " onto " + name + " because magAttachPoint2 is null");
                #endif
            }
            else
            {
                if (stickyMagazine.HasRigidbody)
                {
                    stickyMagazine.RemoveRigidbody();
                }

                // Check if we need to disable regular colliders on the mag when being equipped onto a weapon
                if (stickyMagazine.IsDisableRegularColOnEquip && (!stickyMagazine.IsHeld || !stickyMagazine.isDisableRegularColOnGrab))
                {
                    stickyMagazine.DisableNonTriggerColliders();
                }
                // If the magazine wasn't held by a character, but the weapon is held,
                // and we didn't disable the non-trigger colliders, register them as being part of the character.
                else if (!stickyMagazine.IsHeld && isHeld && stickyControlModule != null)
                {
                    // Register the active colliders on the magazine
                    stickyControlModule.AttachColliders(stickyMagazine.Colliders);
                }

                // Attach the magazine to the weapon
                Transform magTfrm = stickyMagazine.transform;
                magTfrm.SetParent(magAttachPoint2);
                magTfrm.localPosition = Vector3.zero;
                magTfrm.localRotation = Quaternion.identity;

                equippedMag2 = stickyMagazine;

                // Disable the magazine script
                stickyMagazine.enabled = false;

                // If this was stashed, re-enable it when equipped on the weapon
                if (!magTfrm.gameObject.activeSelf) { magTfrm.gameObject.SetActive(true); }

                isMagEquipped2 = true;
            }
        }

        /// <summary>
        /// Set or change the child transform that determines where the laser sight aims from
        /// </summary>
        /// <param name="newLaserSightTransform"></param>
        public void SetLaserSightAimFrom(Transform newLaserSightTransform)
        {
            // If the laser sight was turned on, turn it off
            if (laserSightAimFrom != null)
            {
                if (isWeaponInitialised && isLaserSightOn)
                {
                    EnableOrDisableLaserSight(false);
                }

                UnEquipLaserSight();
            }

            // Ensure it is a child of the weapon
            if (newLaserSightTransform != null && !newLaserSightTransform.IsChildOf(transform))
            {
                laserSightAimFrom = null;

                #if UNITY_EDITOR
                Debug.LogWarning("The laser sight must be a child of the parent " + name + " gameobject or part of the prefab.");
                #endif
            }
            else
            {
                laserSightAimFrom = newLaserSightTransform;

                if (laserSightAimFrom != null) { EquipLaserSight(); }
            }
        }

        /// <summary>
        /// Set the laser sight beam colour
        /// </summary>
        /// <param name="newColour"></param>
        public void SetLaserSightColour (Color32 newColour)
        {
            laserSightColour = newColour;

            if (laserSightLineRenderer != null)
            {
                laserSightLineRenderer.startColor = laserSightColour;
                laserSightLineRenderer.endColor = laserSightColour;
            }
        }

        /// <summary>
        /// Set or change the child transform where the magazine attaches to the weapon
        /// </summary>
        /// <param name="newMagazineAttachPoint"></param>
        /// <param name="weaponButtonNumber">1 (primary) or 2 (secondary)</param>
        public void SetMagazineAttachPoint(Transform newMagazineAttachPoint, int weaponButtonNumber)
        {
            if (weaponButtonNumber < 1 || weaponButtonNumber > 2) { return; }

            // Detach the current magazine
            if ((weaponButtonNumber == 1 && magAttachPoint1 != null) || (weaponButtonNumber == 2 && magAttachPoint2 == null))
            {
                UnEquipMagazine(weaponButtonNumber);
            }

            // Ensure it is a child of the weapon
            if (newMagazineAttachPoint != null && !newMagazineAttachPoint.IsChildOf(transform))
            {
                if (weaponButtonNumber == 1) { magAttachPoint1 = null; }
                else if (weaponButtonNumber == 2) { magAttachPoint2 = null; }

                #if UNITY_EDITOR
                Debug.LogWarning("The Magazine Attach Point must be a child of the parent " + name + " gameobject or part of the prefab.");
                #endif
            }
            else if (weaponButtonNumber == 1)
            {
                magAttachPoint1 = newMagazineAttachPoint;
            }
            else if (weaponButtonNumber == 2)
            {
                magAttachPoint2 = newMagazineAttachPoint;
            }
        }

        /// <summary>
        /// Set the camera that will render the Scope display
        /// </summary>
        /// <param name="newCamera"></param>
        public void SetScopeCamera (Camera newCamera)
        { 
            if (isWeaponInitialised && isScopeOn)
            {
                if (newCamera != null && !newCamera.transform.IsChildOf(transform))
                {
                    scopeCamera = null;
                    #if UNITY_EDITOR
                    Debug.LogWarning("The Scope Camera must be a child of the parent " + name + " gameobject or part of the prefab.");
                    #endif
                }
            }
            else
            {
                scopeCamera = newCamera;
                scopeCamera.enabled = false;
            }
        }

        /// <summary>
        /// Set the (mesh) renderer to project the Scope Camera onto.
        /// </summary>
        /// <param name="newRenderer"></param>
        public void SetScopeCameraRenderer (Renderer newRenderer)
        {
            // If there was previous renderer setup, and initialised,
            // decom the scope (i.e. cleanup potentially old render texture etc)
            if (isWeaponInitialised && isScopeCamInitialised && scopeCameraRenderer != null)
            {
                DecommisionScope();
            }

            if (newRenderer != null)
            {
                if (!newRenderer.transform.IsChildOf(transform))
                {
                    scopeCameraRenderer = null;
                    #if UNITY_EDITOR
                    Debug.LogWarning("The Scope Renderer must be a child of the parent " + name + " gameobject or part of the prefab.");
                    #endif
                }
                else
                {
                    scopeCameraRenderer = newRenderer;
                }
            }
            else
            {
                scopeCameraRenderer = null;
            }
        }

        /// <summary>
        /// Attempt to toggle the laser sight on/off
        /// </summary>
        public void ToggleLaserSight()
        {
            EnableOrDisableLaserSight(!isLaserSightOn);
        }

        /// <summary>
        /// Attempt to toggle the Scope for more precise aiming on/off
        /// </summary>
        public void ToggleScope()
        {
            EnableOrDisableScope(!isScopeOn);
        }

        /// <summary>
        /// Attempt to turn on the laser sight
        /// </summary>
        public void TurnOnLaserSight()
        {
            EnableOrDisableLaserSight(true);
        }

        /// <summary>
        /// Turn off the laser sight
        /// </summary>
        public void TurnOffLaserSight()
        {
            EnableOrDisableLaserSight(false);
        }

        /// <summary>
        /// Turn off the Scope
        /// </summary>
        public void TurnOffScope()
        {
            EnableOrDisableScope(false);
        }

        /// <summary>
        /// Attempt to turn on the scope for aiming
        /// </summary>
        public void TurnOnScope()
        {
            EnableOrDisableScope(true);
        }

        /// <summary>
        /// This will unequip the laser sight from the weapon, including
        /// removing the line renderer (if any)
        /// </summary>
        public void UnEquipLaserSight()
        {
            if (isLaserSightOn && isWeaponInitialised)
            {
                EnableOrDisableLaserSight(false);
            }

            if (laserSightLineRenderer != null || TryGetComponent(out laserSightLineRenderer))
            {
                #if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(laserSightLineRenderer);
                #else
                Destroy(laserSightLineRenderer);
                #endif

                laserSightLineRenderer = null;
            }

            isLaserSightReady = false;
        }

        /// <summary>
        /// Attempt to unequip a sticky magazine from the weapon
        /// </summary>
        public void UnEquipMagazine (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1)
            {
                if (isMagEquipped1 && isMagRequired1 && magAttachPoint1 != null)
                {
                    // Should we attempt to stash this mag?
                    if (isHeld && reloadType1Int == ReloadTypeAutoStashInt && stickyControlModule != null && equippedMag1 != null)
                    {
                        int storeItemID = stickyControlModule.StashItem(equippedMag1);

                        // Was the mag stashed with the character holding the weapon?
                        if (storeItemID != S3DStoreItem.NoStoreItem)
                        {
                            // If the mag colliders where not disable when equipped onto the weapon,
                            // they would have been registered with the character holding the weapon.
                            if (!equippedMag1.IsDisableRegularColOnEquip)
                            {
                                stickyControlModule.DetachColliders(equippedMag1.Colliders);
                            }

                            equippedMag1 = null;
                            isMagEquipped1 = false;
                        }
                    }

                    // Is the magazine still equipped on the weapon?
                    if (isMagEquipped1)
                    {
                        if (reloadType1Int == ReloadTypeAutoStashWithDropInt)
                        {
                            equippedMag1.transform.SetParent(null);
                            equippedMag1.DropObject(0);
                        }

                        equippedMag1 = null;
                        isMagEquipped1 = false;
                    }
                }
            }
            else if (weaponButtonNumber == 2)
            {
                if (isMagEquipped2 && isMagRequired2 && magAttachPoint2 != null)
                {
                    // Should we attempt to stash this mag?
                    if (isHeld && reloadType2Int == ReloadTypeAutoStashInt && stickyControlModule != null && equippedMag2 != null)
                    {
                        int storeItemID = stickyControlModule.StashItem(equippedMag2);

                        // Was the mag stashed with the character holding the weapon?
                        if (storeItemID != S3DStoreItem.NoStoreItem)
                        {
                            // If the mag colliders where not disable when equipped onto the weapon,
                            // they would have been registered with the character holding the weapon.
                            if (!equippedMag2.IsDisableRegularColOnEquip)
                            {
                                stickyControlModule.DetachColliders(equippedMag2.Colliders);
                            }

                            equippedMag2 = null;
                            isMagEquipped2 = false;
                        }
                    }

                    // Is the magazine still equipped on the weapon?
                    if (isMagEquipped2)
                    {
                        if (reloadType2Int == ReloadTypeAutoStashWithDropInt)
                        {
                            equippedMag2.transform.SetParent(null);
                            equippedMag2.DropObject(0);
                        }

                        equippedMag2 = null;
                        isMagEquipped2 = false;
                    }
                }
            }
        }

        /// <summary>
        /// This will attempt to unequip a magazine from the weapon attach point 1
        /// </summary>
        public void UnequipMagazine1()
        {
            UnEquipMagazine(1);
        }

        /// <summary>
        /// This will attempt to unequip a magazine from the weapon attach point 2
        /// </summary>
        public void UnequipMagazine2()
        {
            UnEquipMagazine(2);
        }

        #endregion

        #region Public API Methods - Ammo

        /// <summary>
        /// This is automatically called when the weapon is initialised, grabbed or dropped.
        /// </summary>
        public void CheckMagsInReserve()
        {
            if (!IsBeamWeapon)
            {
                // If the reload type is Auto Stash, to begin with there aren't going to be any
                // magazines in reserve. These would need to come from a character's Stash or
                // hands, when the weapon is being held (or stashed) by a character.

                if (reloadType1Int == ReloadTypeAutoStashInt || reloadType1Int == ReloadTypeAutoStashWithDropInt)
                {
                    magsInReserve1 = GetMagsInStash(1) + GetMagsHeld(1);
                }

                if (reloadType2Int == ReloadTypeAutoStashInt || reloadType2Int == ReloadTypeAutoStashWithDropInt)
                {
                    magsInReserve2 = GetMagsInStash(2) + GetMagsHeld(2);
                }
            }
        }

        /// <summary>
        /// Get if the primary (1) or secondary (2) firing mechanism requires a StickyMagazine to be attached before it can fire?
        /// See also IsMagRequired1, IsMagRequired2, and SetIsMagRequired(..).
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        public bool GetIsMagRequired (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1) { return isMagRequired1; }
            else if (weaponButtonNumber == 2) { return isMagRequired2; }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetIsMagRequired on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms.");
                #endif
                return false;
            }
        }

        /// <summary>
        /// Get the current magazine (clip) capacity for the primary (1) or secondary (2) firing mechanism.
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        /// <returns></returns>
        public int GetMagCapacity (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1)
            {
                return magCapacity1;    
            }
            else if (weaponButtonNumber == 2)
            {
                return magCapacity2;
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetMagCapacity on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms.");
                #endif
                return 0;
            }
        }

        /// <summary>
        /// Get the current magazine (clip) zero-based index from the magTypes scriptable object
        /// for the primary (1) or secondary (2) firing mechanism.
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        /// <returns></returns>
        public int GetCompatibleMagType (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1)
            {
                return compatibleMag1;    
            }
            else if (weaponButtonNumber == 2)
            {
                return compatibleMag2;
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetCompatibleMagType on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms.");
                #endif
                return 0;
            }
        }

        /// <summary>
        /// Get the number of additional magazines available for the primary (1) or secondary (2) firing mechanism.
        /// See also MagsInReserve1, MagsInReserve2, and SetMagsInReserve(..).
        /// Returns number in reserve or -1 (unlimited)
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        public int GetMagsInReserve (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1) { return magsInReserve1; }
            else if (weaponButtonNumber == 2) { return magsInReserve2; }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetMagsInReserve on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms.");
                #endif
                return 0;
            }
        }

        /// <summary>
        /// Get the number of additional magazines available for the primary (1) or secondary (2) firing mechanisms
        /// that are held in the hands of the character that has this weapon.
        /// If the weapon is held in one hand, currently the maximum number would be 1.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <returns></returns>
        public int GetMagsHeld (int weaponButtonNumber)
        {
            if (!isHeld)
            {
                return 0;
            }
            else if (weaponButtonNumber == 1)
            {
                return stickyControlModule.GetNumberHeldMagazines(compatibleMag1, compatibleAmmo1, false);
            }
            else if (weaponButtonNumber == 2)
            {
                return stickyControlModule.GetNumberHeldMagazines(compatibleMag2, compatibleAmmo2, false);
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetMagsHeld on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms.");
                #endif
                return 0;
            }
        }

        /// <summary>
        /// Get the number of additional magazines available for the primary (1) or secondary (2) firing mechanism
        /// from the Stash (inventory) of the character that has this weapon.
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        /// <returns></returns>
        public int GetMagsInStash (int weaponButtonNumber)
        {
            if (!isHeld && !isStashed)
            {
                return 0;
            }
            else if (weaponButtonNumber == 1)
            {
                return stickyControlModule.GetNumberStashedMagazines(compatibleMag1, compatibleAmmo1, false);
            }
            else if (weaponButtonNumber == 2)
            {
                return stickyControlModule.GetNumberStashedMagazines(compatibleMag2, compatibleAmmo2, false);
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetMagsInStash on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms.");
                #endif
                return 0;
            }
        }

        /// <summary>
        /// Get the Effects Module for the primary (1) or secondary (2) Sound FX when equipping the new mag begins during reloading.
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        public StickyEffectsModule GetReloadEquipSoundFX (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1) { return reloadEquipSoundFX1; }
            else if (weaponButtonNumber == 2) { return reloadEquipSoundFX2; }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetReloadEquipSoundFX on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms.");
                #endif
                return null;
            }
        }

        /// <summary>
        /// Get the Effects Module for the primary (1) or secondary (2) reloading Sound FX
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        public StickyEffectsModule GetReloadSoundFX (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1) { return reloadSoundFX1; }
            else if (weaponButtonNumber == 2) { return reloadSoundFX2; }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SetReloadSoundFX on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms.");
                #endif
                return null;
            }
        }

        // <summary>
        /// Get The method used for reloading the primary (1) or secondary (2) firing mechanism.
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        /// <returns></returns>
        public ReloadType GetReloadType (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1)
            {
                return reloadType1;
            }
            else if (weaponButtonNumber == 2)
            {
                return reloadType2;
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetReloadType on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms.");
                #endif
                return ReloadType.ManualOnly;
            }
        }

        // <summary>
        /// Get The method used for reloading the primary (1) or secondary (2) firing mechanism as an Integer
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        /// <returns></returns>
        public int GetReloadTypeInt (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1)
            {
                return isInitialised ? reloadType1Int : (int)reloadType1;
            }
            else if (weaponButtonNumber == 2)
            {
                return isInitialised ? reloadType2Int : (int)reloadType2; ;
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: GetReloadTypeInt on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms.");
                #endif
                return (int)ReloadType.ManualOnly;
            }
        }

        /// <summary>
        /// Check to see if the ammo type is compatible with a firing mechanism
        /// </summary>
        /// <param name="weaponButtonNumber">primary (1) or secondary (2)</param>
        /// <param name="ammoType">The ammoType as an integer</param>
        /// <returns></returns>
        public bool IsAmmoCompatible (int weaponButtonNumber, int ammoType)
        {
            if (ammoType < 0)
            {
                return false;
            }
            else if (weaponButtonNumber == 1)
            {
                return System.Array.IndexOf(compatibleAmmo1, ammoType) > -1;
            }
            else if (weaponButtonNumber == 2)
            {
                return System.Array.IndexOf(compatibleAmmo2, ammoType) > -1;
            }
            else { return false; }            
        }

        /// <summary>
        /// Check to see if the ammo type is compatible with a firing mechanism
        /// </summary>
        /// <param name="weaponButtonNumber">primary (1) or secondary (2)</param>
        /// <param name="ammoType"></param>
        /// <returns></returns>
        public bool IsAmmoCompatible (int weaponButtonNumber, S3DAmmo.AmmoType ammoType)
        {
            return IsAmmoCompatible(weaponButtonNumber, (int)ammoType);
        }

        /// <summary>
        /// Check to see if a magazine is compatible with a firing mechanism
        /// </summary>
        /// <param name="weaponButtonNumber">primary (1) or secondary (2)</param>
        /// <param name="stickyMagazine"></param>
        /// <returns></returns>
        public bool IsMagCompatible (int weaponButtonNumber, StickyMagazine stickyMagazine)
        {
            if (weaponButtonNumber > 0 && weaponButtonNumber < 3 && stickyMagazine != null)
            {
                return stickyMagazine.IsMagazineCompatible(weaponButtonNumber == 1 ? compatibleMag1 : compatibleMag2);
            }
            else { return false; }            
        }

        /// <summary>
        /// Is the given firing mechanism reloading?
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        public bool IsReloading (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1) { return isReloading1; }
            else if (weaponButtonNumber == 2) { return isReloading2; }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: IsReloading on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms.");
                #endif
                return false;
            }
        }

        /// <summary>
        /// Reinitialise items related to Empty Fire effects. Call this if making any changes
        /// to fireEmptyEffectsSet1 or 2.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        public void ReinitialiseEmptyFire (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1)
            {
                numFireEmptyEffects1 = fireEmptyEffectsSet1 == null ? 0 : fireEmptyEffectsSet1.NumberOfEffects;

                if (numFireEmptyEffects1 > 0)
                {
                    fireEmptyEffects1PrefabIDs = new int[numFireEmptyEffects1];
                    stickyManager.CreateEffectsPools(fireEmptyEffectsSet1, fireEmptyEffects1PrefabIDs);
                }
                else
                {
                    fireEmptyEffects1PrefabIDs = null;
                }
            }
            else if (weaponButtonNumber == 2)
            {
                numFireEmptyEffects2 = fireEmptyEffectsSet2 == null ? 0 : fireEmptyEffectsSet2.NumberOfEffects;

                if (numFireEmptyEffects2 > 0)
                {
                    fireEmptyEffects2PrefabIDs = new int[numFireEmptyEffects1];
                    stickyManager.CreateEffectsPools(fireEmptyEffectsSet2, fireEmptyEffects2PrefabIDs);
                }
                else
                {
                    fireEmptyEffects2PrefabIDs = null;
                }
            }
        }

        /// <summary>
        /// Find and identify a compatible magazine attached to the weapon under
        /// the Mag Attach Point for the primary or secondary firing mechanism.
        /// This is automatically called when the weapon is initialised so that
        /// a magazine can be pre-attached to a weapon in the scene.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        public void ReinitialiseEquippedMag (int weaponButtonNumber)
        {
            if (tempMagList == null) { tempMagList = new List<StickyMagazine>(1); }
            else { tempMagList.Clear(); }

            int numMags = 0;

            if (weaponButtonNumber == 1)
            {
                // Reset equipped.
                isMagEquipped1 = false;
                equippedMag1 = null;

                if (magAttachPoint1 != null)
                {
                    // There should be 0 or 1.
                    magAttachPoint1.GetComponentsInChildren(true, tempMagList);

                    numMags = tempMagList.Count;

                    for (int magIdx = 0; magIdx < numMags; magIdx++)
                    {
                        StickyMagazine _mag = tempMagList[magIdx];

                        // Ignore inactive gameobjects with a StickyMagazine script attached
                        if (_mag.gameObject.activeSelf)
                        {
                            if (isMagEquipped1)
                            {
                                #if UNITY_EDITOR
                                Debug.LogWarning("ERROR: " + name + " already has " + equippedMag1.name + " equipped, so cannot equip " + _mag.name + " too. Disabling magazine.");
                                #endif
                                _mag.gameObject.SetActive(false);
                            }
                            else if (!_mag.IsMagazineCompatible(compatibleMag1))
                            {
                                #if UNITY_EDITOR
                                Debug.LogWarning("ERROR: " + _mag.name + " is not compatible with " + name + " and cannot be equipped. Disabling magazine.");
                                #endif
                                _mag.gameObject.SetActive(false);
                            }
                            else if (!IsAmmoCompatible(1, _mag.CompatibleAmmoTypeInt))
                            {
                                #if UNITY_EDITOR
                                Debug.LogWarning("ERROR: " + _mag.name + " ammo is not compatible with " + name + " and cannot be equipped. Disabling magazine.");
                                #endif
                                _mag.gameObject.SetActive(false);
                            }
                            else
                            {
                                magCapacity1 = _mag.MagCapacity;
                                ammunition1 = _mag.AmmoCount;
                                EquipMagazine1(_mag);

                                #if UNITY_EDITOR
                                if (!isMagRequired1) { Debug.LogWarning("WARNING: Is Sticky Mag is [NOT] required, but you have a StickyMagazine attached to this " + name + ". Check the configuration of the weapon."); }
                                #endif
                            }
                        }
                    }
                }
            }
            else if (weaponButtonNumber == 2)
            {
                // Reset equipped.
                isMagEquipped2 = false;
                equippedMag2 = null;

                if (magAttachPoint2 != null)
                {
                    // There should be 0 or 1.
                    magAttachPoint2.GetComponentsInChildren(true, tempMagList);

                    numMags = tempMagList.Count;

                    for (int magIdx = 0; magIdx < numMags; magIdx++)
                    {
                        StickyMagazine _mag = tempMagList[magIdx];

                        // Ignore inactive gameobjects with a StickyMagazine script attached
                        if (_mag.gameObject.activeSelf)
                        {
                            if (isMagEquipped2)
                            {
                                #if UNITY_EDITOR
                                Debug.LogWarning("ERROR: " + name + " already has " + equippedMag2.name + " equipped, so cannot equip " + _mag.name + " too. Disabling magazine.");
                                #endif
                                _mag.gameObject.SetActive(false);
                            }
                            else if (!_mag.IsMagazineCompatible(compatibleMag2))
                            {
                                #if UNITY_EDITOR
                                Debug.LogWarning("ERROR: " + _mag.name + " is not compatible with " + name + " and cannot be equipped. Disabling magazine.");
                                #endif
                                _mag.gameObject.SetActive(false);
                            }
                            else if (!IsAmmoCompatible(2, _mag.CompatibleAmmoTypeInt))
                            {
                                #if UNITY_EDITOR
                                Debug.LogWarning("ERROR: " + _mag.name + " ammo is not compatible with " + name + " and cannot be equipped. Disabling magazine.");
                                #endif
                                _mag.gameObject.SetActive(false);
                            }
                            else
                            {
                                magCapacity2 = _mag.MagCapacity;
                                ammunition2 = _mag.AmmoCount;
                                EquipMagazine2(_mag);

                                #if UNITY_EDITOR
                                if (!isMagRequired2) { Debug.LogWarning("WARNING: Is Sticky Mag is [NOT] required, but you have a StickyMagazine attached to this " + name + ". Check the configuration of the weapon."); }
                                #endif
                            }
                        }
                    }
                }
            }

            if (numMags > 0) { tempMagList.Clear(); }
        }

        /// <summary>
        /// Attempt to reload the weapon for the primary (1) or secondary (2) firing button mechanism.
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        public void Reload (int weaponButtonNumber)
        {
            if (isWeaponInitialised)
            {
                if (weaponButtonNumber > 0 && weaponButtonNumber < 3)
                {
                    if (!IsReloading(weaponButtonNumber))
                    {
                        bool isReloading = false;
                        StickyMagazine stickyMagazine = null;
                        int storeItemID = S3DStoreItem.NoStoreItem;

                        bool isMagRequired = GetIsMagRequired(weaponButtonNumber);
                        int reloadTypeInt = GetReloadTypeInt(weaponButtonNumber);

                        int ammoTypeInt = -1;
                        int availableAmmunition = 0;

                        bool isMagAvailable = false;
                        bool isMagInHand = false;

                        if (isMagRequired || reloadTypeInt == StickyWeapon.ReloadTypeAutoStashInt || reloadTypeInt == ReloadTypeAutoStashWithDropInt)
                        {
                            // Attempt to find non-empty compatible magazine first in a hand, and then in Stash.
                            bool isHeldInLeftHand = false;
                            isMagAvailable = GetMagazineFromCharacterHand(weaponButtonNumber, false, ref isHeldInLeftHand, ref stickyMagazine);

                            if (isMagAvailable)
                            {
                                isMagInHand = true;
                                availableAmmunition = stickyMagazine.AmmoCount;
                                ammoTypeInt = stickyMagazine.CompatibleAmmoTypeInt;
                            }
                            else
                            {
                                // Is there a compatible mag in the character's stash?
                                // The storeItemID is used in the delayed ReloadPrimary or ReloadSecondary method.
                                storeItemID = GetMagazineFromCharacterStash(weaponButtonNumber, false, ref stickyMagazine);

                                if (storeItemID != S3DStoreItem.NoStoreItem)
                                {
                                    isMagAvailable = true;
                                    availableAmmunition = stickyMagazine.AmmoCount;
                                    ammoTypeInt = stickyMagazine.CompatibleAmmoTypeInt;
                                }
                            }
                        }
                        else
                        {
                            int magsAvailable = GetMagsInReserve(weaponButtonNumber);
                            isMagAvailable = magsAvailable != 0;

                            // If not using a StickyMagazine, assmume we always get a full mag if there is one available
                            if (isMagAvailable)
                            {
                                availableAmmunition = GetMagCapacity(weaponButtonNumber);
                                ammoTypeInt = weaponButtonNumber == 1 ? compatibleAmmo1[0] : compatibleAmmo2[0];
                            }
                        }

                        isReloading = isMagAvailable && availableAmmunition > 0;

                        if (isReloading)
                        {
                            if (weaponButtonNumber == 1)
                            {
                                StartCoroutine(ReloadPrimary(availableAmmunition, ammoTypeInt, storeItemID, isMagInHand));
                            }
                            else if (weaponButtonNumber == 2)
                            {
                                StartCoroutine(ReloadSecondary(availableAmmunition, ammoTypeInt, storeItemID, isMagInHand));
                            }
                        }
                    }
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: The weapon (" + name + ") can only reload the primary (1) or secondary (2) firing button mechanisms."); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: The weapon (" + name + ") cannot be reloaded if it has not be initialised."); }
            #endif
        }

        /// <summary>
        /// Get or set the ammunition currently loaded into the weapon for the primary (1) or secondary (2) firing mechanism.
        /// When there isn't a magazine required, ensure the ammunition doesn't exceed the capacity of the (fake) magazine.
        /// -1 = Unlimited.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="newAmmunitionValue"></param>
        public void SetAmmunition (int weaponButtonNumber, int newAmmunitionValue)
        {
            if (weaponButtonNumber == 1)
            {
                // Check if should be unlimited
                if (newAmmunitionValue < 0) { ammunition1 = -1;  }
                else if (!isMagRequired1)
                {
                    ammunition1 = newAmmunitionValue > magCapacity1 ? magCapacity1 : ammunition1;
                }
                else { ammunition1 = newAmmunitionValue; }
            }
            else if (weaponButtonNumber == 2)
            {
                // Check if should be unlimited
                if (newAmmunitionValue < 0) { ammunition2 = -1; }
                else if (!isMagRequired2)
                {
                    ammunition2 = newAmmunitionValue > magCapacity2 ? magCapacity2 : ammunition2;
                }
                else { ammunition2 = newAmmunitionValue; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: SetAmmunition weaponButtonNumber must be 1 (primary fire button) or 2 (secondary)."); }
            #endif
        }

        /// <summary>
        /// Set the zero-based index of the magazine type from the scriptable object for the primary (1)
        /// or secondary (2) firing button.
        /// </summary>
        /// <param name="weaponButtonNumber">1 (primary fire button) or 2 (secondary)</param>
        /// <param name="newCompatibleMagType">The MagType from the MagTypes scriptable object</param>
        public void SetCompatibleMagType (int weaponButtonNumber, int newCompatibleMagType)
        {
            if (weaponButtonNumber == 1)
            {
                // If no scriptable object, default to Mag Type A
                if (magTypes != null)
                {
                    if (newCompatibleMagType >= 0 && newCompatibleMagType < 26)
                    {
                        compatibleMag1 = newCompatibleMagType;
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: SetCompatibleMagazineType newCompatibleMagType must be between 0 and 25"); }
                    #endif
                }
                else { compatibleMag1 = 0; }
            }
            else if (weaponButtonNumber == 2)
            {
                // If no scriptable object, default to Mag Type A
                if (magTypes != null)
                {
                    if (newCompatibleMagType >= 0 && newCompatibleMagType < 26)
                    {
                        compatibleMag2 = newCompatibleMagType;
                    }
                    #if UNITY_EDITOR
                    else { Debug.LogWarning("ERROR: SetCompatibleMagazineType newCompatibleMagType must be between 0 and 25"); }
                    #endif
                }
                else { compatibleMag2 = 0; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: SetCompatibleMagazineType weaponButtonNumber must be 1 (primary fire button) or 2 (secondary)."); }
            #endif
        }

        /// <summary>
        /// Set if the primary (1) or secondary (2) firing mechanism requires a StickyMagazine to be attached before it can fire?
        /// See also IsMagRequired1, IsMagRequired2, and GetIsMagRequired(..).
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        public void SetIsMagRequired (int weaponButtonNumber, bool newIsMagRequired)
        {
            if (weaponButtonNumber == 1)
            {
                isMagRequired1 = newIsMagRequired;
            }
            else if (weaponButtonNumber == 2)
            {
                isMagRequired2 = newIsMagRequired;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: SetIsMagRequired on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms."); }
            #endif
        }

        /// <summary>
        /// Set the amount of ammo the magazine (clip) can hold for the primary (1) or secondary (2) firing mechanism.
        /// This has no effect if Is Mag Required is true for the firing mechanism.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="newMagCapacity"></param>
        public void SetMagCapacity (int weaponButtonNumber, int newMagCapacity)
        {
            if (newMagCapacity < 1)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SetMagCapacity magazine capacity cannot be less than 1");
                #endif
            }
            else if (weaponButtonNumber == 1)
            {
                if (!isMagRequired1)
                {
                    magCapacity1 = newMagCapacity;
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: SetMagCapacity on (" + name + ") only applies when Is Mag Required is false on the primary firing button."); }
                #endif
            }
            else if (weaponButtonNumber == 2)
            {
                if (!isMagRequired2)
                {
                    magCapacity2 = newMagCapacity;
                }
                #if UNITY_EDITOR
                else { Debug.LogWarning("ERROR: SetMagCapacity on (" + name + ") only applies when Is Mag Required is false on the secondary firing button."); }
                #endif
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: SetMagCapacity on (" + name + ") only applies to primary (1) or secondary (2) firing button mechanisms."); }
            #endif
        }

        /// <summary>
        /// Get or set the number of additional magazines available for the primary (1) or secondary (2) firing mechanism.
        /// -1 = Unlimited.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="newMagsInReserveValue"></param>
        public void SetMagsInReserve (int weaponButtonNumber, int newMagsInReserveValue)
        {
            if (weaponButtonNumber == 1)
            {
                // Check if should be unlimited
                if (newMagsInReserveValue < 0) { magsInReserve1 = -1;  }
                else { magsInReserve1 = newMagsInReserveValue; }
            }
            else if (weaponButtonNumber == 2)
            {
                // Check if should be unlimited
                if (newMagsInReserveValue < 0) { magsInReserve2 = -1; }
                else { magsInReserve2 = newMagsInReserveValue; }
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: SetMagsInReserve weaponButtonNumber must be 1 (primary fire button) or 2 (secondary)."); }
            #endif
        }

        /// <summary>
        /// Set the Sound FX for the primary (1) or secondary (2) fire button mechanism used when the weapon equips a new mag during reloading.
        /// The EffectsType must be Sound FX.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="newReloadEquipSoundFX"></param>
        public void SetReloadEquipSoundFX (int weaponButtonNumber, StickyEffectsModule newReloadEquipSoundFX)
        {
            if (newReloadEquipSoundFX != null && newReloadEquipSoundFX.ModuleEffectsType != StickyEffectsModule.EffectsType.SoundFX)
            {
                if (weaponButtonNumber == 1) { reloadEquipSoundFX1 = null; }
                else if (weaponButtonNumber == 2) { reloadEquipSoundFX2 = null; }

                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SetReloadEquipSoundFX " + newReloadEquipSoundFX.name + " EffectsType must be SoundFX");
                #endif
            }
            else if (weaponButtonNumber == 1)
            {
                reloadEquipSoundFX1 = newReloadEquipSoundFX;
            }
            else if (weaponButtonNumber == 2)
            {
                reloadEquipSoundFX2 = newReloadEquipSoundFX;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: SetReloadEquipSoundFX weaponButtonNumber must be 1 (primary fire button) or 2 (secondary)."); }
            #endif
        }

        /// <summary>
        /// Set the reload SoundFX Effects Module for the primary (1) or secondary (2) fire button mechanism.
        /// The EffectsType must be Sound FX.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="newReloadSoundFX"></param>
        public void SetReloadSoundFX (int weaponButtonNumber, StickyEffectsModule newReloadSoundFX)
        {
            if (newReloadSoundFX != null && newReloadSoundFX.ModuleEffectsType != StickyEffectsModule.EffectsType.SoundFX)
            {
                if (weaponButtonNumber == 1) { reloadSoundFX1 = null; }
                else if (weaponButtonNumber == 2) { reloadSoundFX2 = null; }

                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SetReloadSoundFX " + newReloadSoundFX.name + " EffectsType must be SoundFX");
                #endif
            }
            else if (weaponButtonNumber == 1)
            {
                reloadSoundFX1 = newReloadSoundFX;
            }
            else if (weaponButtonNumber == 2)
            {
                reloadSoundFX2 = newReloadSoundFX;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: SetReloadSoundFX weaponButtonNumber must be 1 (primary fire button) or 2 (secondary)."); }
            #endif
        }

        /// <summary>
        /// Set the method used for reloading the primary (1) or secondary (2) firing mechanism
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        /// <param name="newReloadType"></param>
        public void SetReloadType (int weaponButtonNumber, ReloadType newReloadType)
        {
            if (weaponButtonNumber == 1)
            {
                reloadType1 = newReloadType;
                reloadType1Int = (int)reloadType1;
            }
            else if (weaponButtonNumber == 2)
            {
                reloadType2 = newReloadType;
                reloadType2Int = (int)reloadType2;
            }
            #if UNITY_EDITOR
            else { Debug.LogWarning("ERROR: SetReloadType weaponButtonNumber must be 1 (primary fire button) or 2 (secondary)."); }
            #endif
        }

        /// <summary>
        /// Validate the ammo types for this weapon
        /// </summary>
        public void ValidateAmmo()
        {
            // Ensure we have at least 1 compatible ammo type
            if (compatibleAmmo1 == null || compatibleAmmo1.Length == 0)
            {
                // Default to index 0 which is Type A
                compatibleAmmo1 = new int[1];
            }

            if (compatibleAmmo2 == null || compatibleAmmo2.Length == 0)
            {
                // Default to index 0 which is Type A
                compatibleAmmo2 = new int[1];
            }

            if (currentAmmoType1Int < 0) { currentAmmoType1Int = compatibleAmmo1[0]; }
            if (currentAmmoType2Int < 0) { currentAmmoType2Int = compatibleAmmo2[0]; }
        }

        #endregion

        #region Public API Methods - Animate

        /// <summary>
        /// When the held weapon is dropped, check to see if there is a weapon anim set Animate Action
        /// that should trigger a quick exit from the held or aiming animation.
        /// </summary>
        public void AnimCheckStateExitDrop()
        {
            AnimateCharacterStateExit(true, false, false, false);
        }

        /// <summary>
        /// When the held weapon is equipped, check to see if there is a weapon anim set Animate Action
        /// that should trigger a quick exit from the held or aiming animation.
        /// </summary>
        public void AnimateCheckStateExitEquip()
        {
            AnimateCharacterStateExit(false, true, false, false);
        }

        /// <summary>
        /// When the held weapon is socketed, check to see if there is a weapon anim set Animate Action
        /// that should trigger a quick exit from the held or aiming animation.
        /// </summary>
        public void AnimateCheckStateExitSocket()
        {
            AnimateCharacterStateExit(false, false, true, false);
        }

        /// <summary>
        /// When the held weapon is dropped, check to see if there is a weapon anim set Animate Action
        /// that should trigger a quick exit from the held or aiming animation.
        /// </summary>
        public void AnimateCheckStateExitStash()
        {
            AnimateCharacterStateExit(false, false, false, true);
        }

        /// <summary>
        /// Apply any matching weapon anim sets for the character holding the weapon
        /// </summary>
        public void ApplyWeaponAnimSets ()
        {
            ApplyOrRevertWeaponAnimSets(true);
        }

        /// <summary>
        /// Blend in an Animator layer over time using the Layer Weight.
        /// See also SetAnimLayerBlendInDuration(..).
        /// </summary>
        /// <param name="layerIndex">Range 0 to layerCount</param>
        public void BlendInAnimLayer (int layerIndex)
        {
            if (isAnimateEnabled && layerIndex >= 0 && layerIndex < numAnimLayers)
            {
                animlayerDataList[layerIndex].blendWeight = 0f;
                animlayerDataList[layerIndex].isBlendingIn = true;
                animlayerDataList[layerIndex].isBlendingOut = false;
            }
        }

        /// <summary>
        /// Blend out an Animator layer over time using the Layer Weight.
        /// See also SetAnimLayerBlendOutDuration(..).
        /// </summary>
        /// <param name="layerIndex">Range 0 to layerCount</param>
        public void BlendOutAnimLayer (int layerIndex)
        {
            if (isAnimateEnabled && layerIndex >= 0 && layerIndex < numAnimLayers)
            {
                animlayerDataList[layerIndex].blendWeight = 1f;
                animlayerDataList[layerIndex].isBlendingIn = false;
                animlayerDataList[layerIndex].isBlendingOut = true;
            }
        }

        /// <summary>
        /// Disable the sending of animation data to the weapon animation controller
        /// </summary>
        public void DisableAnimate()
        {
            EnableOrDisableAnimate(false);
        }

        /// <summary>
        /// Enable the sending of animation data to the weapon animation controller.
        /// This will fall back to Disabled if the current setup does not support it.
        /// </summary>
        public void EnableAnimate()
        {
            EnableOrDisableAnimate(true);
        }

        /// <summary>
        /// Get an S3DAnimAction class instance using the unique identifier (guidHash) of AnimAction from the list
        /// displayed in editor on the Animate tab.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public S3DAnimAction GetAnimAction (int guidHash)
        {
            S3DAnimAction s3DAnimAction = null;

            if (guidHash != 0)
            {
                int _numAnimateActions = isWeaponInitialised ? numAnimateActions : s3dAnimActionList == null ? 0 : s3dAnimActionList.Count;

                for (int aaIdx = 0; aaIdx < _numAnimateActions; aaIdx++)
                {
                    S3DAnimAction _tempAnimAction = s3dAnimActionList[aaIdx];
                    if (_tempAnimAction != null && _tempAnimAction.guidHash == guidHash)
                    {
                        s3DAnimAction = _tempAnimAction;
                        break;
                    }
                }
            }

            return s3DAnimAction;
        }

        /// <summary>
        /// Get an S3DAnimAction class instance using the zero-based index or sequence number in the list
        /// which is seen in the editor on the Animate tab.
        /// </summary>
        /// <param name="animActionIndex"></param>
        /// <returns></returns>
        public S3DAnimAction GetAnimActionByIndex (int animActionIndex)
        {
            int _numAnimateActions = isWeaponInitialised ? numAnimateActions : s3dAnimActionList == null ? 0 : s3dAnimActionList.Count;

            if (animActionIndex >= 0 && animActionIndex < _numAnimateActions)
            {
                return s3dAnimActionList[animActionIndex];
            }
            else { return null; }
        }

        /// <summary>
        /// Get the unique guidHash of an S3DAnimAction using the zero-based index or sequence number in the list
        /// which is seen in the editor on the Animate tab.
        /// </summary>
        /// <param name="animActionIndex"></param>
        /// <returns></returns>
        public int GetAnimActionHashByIndex (int animActionIndex)
        {
            int _numAnimateActions = isWeaponInitialised ? numAnimateActions : s3dAnimActionList == null ? 0 : s3dAnimActionList.Count;

            if (animActionIndex >= 0 && animActionIndex < _numAnimateActions)
            {
                S3DAnimAction _tempAnimAction = s3dAnimActionList[animActionIndex];
                if (_tempAnimAction != null) { return _tempAnimAction.guidHash; }
                else { return 0; }
            }
            else { return 0; }
        }

        /// <summary>
        /// Get the zero-based index of AnimAction from the list seen in the editor on the Animate tab,
        /// using the unique identifier (guidHash) of an AnimAction. Returns -1 if not found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public int GetAnimActionIndex (int guidHash)
        {
            int aaIndex = -1;

            if (guidHash != 0)
            {
                int _numAnimateActions = isWeaponInitialised ? numAnimateActions : s3dAnimActionList == null ? 0 : s3dAnimActionList.Count;

                for (int aaIdx = 0; aaIdx < _numAnimateActions; aaIdx++)
                {
                    S3DAnimAction _tempAnimAction = s3dAnimActionList[aaIdx];
                    if (_tempAnimAction != null && _tempAnimAction.guidHash == guidHash)
                    {
                        aaIndex = aaIdx;
                        break;
                    }
                }
            }

            return aaIndex;
        }

        /// <summary>
        /// Return the state Id (hash) given a state in an Animation Controller.
        /// NOTE: This does not test if the state exists in any of the layers.
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public int GetAnimationStateId (string stateName)
        {
            return Animator.StringToHash(stateName);
        }

        /// <summary>
        /// Get the S3D internal data being stored for the Animator zero-based Layer.
        /// </summary>
        /// <param name="layerIndex">Range 0 to number of Animator Layers</param>
        /// <returns></returns>
        public S3DAnimLayerData GetAnimLayerData (int layerIndex)
        {
            if (layerIndex >= 0 && layerIndex < numAnimLayers)
            {
                return animlayerDataList[layerIndex];
            }
            else { return null; }
        }

        /// <summary>
        /// Play the Animation State in the layer within the Animator Controller. Set the 
        /// layerIndex to -1 if you want to play the first matching state (on any layer).
        /// If the zero-based layerIndex is set, it will also check if the state exists.
        /// For no transition, set transitionDuration to 0. For a smoother, but slower
        /// transition, increase transitionDuration.
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="layerIndex"></param>
        /// <param name="transitionDuration">Transition time in seconds</param>
        public void PlayAnimationState (int stateId, int layerIndex, float transitionDuration = 0.2f)
        {
            if (isAnimateEnabled && (layerIndex == -1 || defaultAnimator.HasState(layerIndex, stateId)))
            {
                if (transitionDuration > 0f) { defaultAnimator.CrossFadeInFixedTime(stateId, transitionDuration, layerIndex); }
                else { defaultAnimator.Play(stateId, layerIndex); }
            }
        }

        /// <summary>
        /// Play the Animation State in the layer within the Animator Controller starting at a normalised
        /// offset from the start of the clip. Set the layerIndex to -1 if you want to play the first
        /// matching state (on any layer). If the zero-based layerIndex is set, it will also check if the
        /// state exists. An offset of 0.0 starts at the beginning, while 0.9 starts near the end of the
        /// clip for the animation state.
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="layerIndex"></param>
        /// <param name="transitionNormalised">Range 0.0 to 1.0</param>
        /// <param name="offsetNormalised">0.0 starts at the beginning, 0.9 starts near the end of the clip</param>
        public void PlayAnimationStateWithOffset (int stateId, int layerIndex, float transitionNormalised, float offsetNormalised)
        {
            if (isAnimateEnabled && (layerIndex == -1 || defaultAnimator.HasState(layerIndex, stateId)))
            {
                if (offsetNormalised <= 0f || offsetNormalised >= 1f)
                {
                    if (transitionNormalised > 0f && transitionNormalised <= 1f) { defaultAnimator.CrossFade(stateId, transitionNormalised, layerIndex); }
                    else { defaultAnimator.Play(stateId, layerIndex); }
                }
                else if (transitionNormalised > 0f && transitionNormalised <= 1f)
                {
                    defaultAnimator.CrossFade(stateId, transitionNormalised, layerIndex, offsetNormalised);
                }
                else
                {
                    defaultAnimator.Play(stateId, layerIndex, offsetNormalised);
                }
            }
        }

        /// <summary>
        /// Refresh and validate Animate settings. Call this after changing any Animate settings.
        /// </summary>
        public void RefreshAnimateSettings()
        {
            RefreshAnimateSettings(false);
        }

        /// <summary>
        /// Revert any matching weapon anim sets for the character holding the weapon back to original settings.
        /// </summary>
        public void RevertWeaponAnimSets()
        {
            ApplyOrRevertWeaponAnimSets(false);
        }

        /// <summary>
        /// Set the list of animation actions for this weapon
        /// </summary>
        /// <param name="newAnimActionsList"></param>
        public void SetAnimActionsList (List<S3DAnimAction> newAnimActionsList)
        {
            if (newAnimActionsList == null)
            {
                s3dAnimActionList.Clear();
            }
            else
            {
                s3dAnimActionList = newAnimActionsList;
            }
        }

        /// <summary>
        /// Set the duration, in seconds, it takes for an animator layer weight to reach 1.0.
        /// See also BlendInAnimLayer (..).
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="blendInDuration">Min value is 0.001 seconds</param>
        public void SetAnimLayerBlendInDuration (int layerIndex, float blendInDuration)
        {
            if (isAnimateEnabled && layerIndex >= 0 && layerIndex < numAnimLayers)
            {
                animlayerDataList[layerIndex].blendInDuration = blendInDuration < 0.001f ? 0.001f : blendInDuration;
            }
        }

        /// <summary>
        /// Set the duration, in seconds, it takes for an animator layer weight to reach 0.0.
        /// See also BlendOutAnimLayer (..).
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="blendOutDuration">Min value is 0.001 second</param>
        public void SetAnimLayerBlendOutDuration (int layerIndex, float blendOutDuration)
        {
            if (isAnimateEnabled && layerIndex >= 0 && layerIndex < numAnimLayers)
            {
                animlayerDataList[layerIndex].blendOutDuration = blendOutDuration < 0.001f ? 0.001f : blendOutDuration;
            }
        }

        /// <summary>
        /// Given the guidHash of a custom S3DAnimAction set its boolean value, assuming it has a matching ParamaterType.
        /// If Toggle is enabled, attempt to toggle the current value in the animation controller.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <param name="value"></param>
        public void SetCustomAnimActionBoolValue (int guidHash, bool value)
        {
            SetCustomAnimActionBoolValue(GetAnimAction(guidHash), value);
        }

        /// <summary>
        /// Given a custom S3DAnimAction set its boolean value, assuming it has a matching ParamaterType
        /// and the weaponAction is a custom action.
        /// If Toggle is enabled, attempt to toggle the current value in the animation controller.
        /// </summary>
        /// <param name="s3DAnimAction"></param>
        /// <param name="value"></param>
        public void SetCustomAnimActionBoolValue (S3DAnimAction s3DAnimAction, bool value)
        {
            if (s3DAnimAction != null && (int)s3DAnimAction.weaponAction == S3DAnimAction.WeaponActionCustomInt && s3DAnimAction.parameterType == S3DAnimAction.ParameterType.Bool)
            {
                // If required, attempt to toggle the current value in the animator.
                if (s3DAnimAction.isToggle && isAnimateEnabled && s3DAnimAction.paramHashCode != 0)
                {
                    // Query the animation controller for the current value, then toggle it.
                    s3DAnimAction.customActionBoolValue = !defaultAnimator.GetBool(s3DAnimAction.paramHashCode);
                }
                else
                {
                    s3DAnimAction.customActionBoolValue = value;
                }
            }
        }

        /// <summary>
        /// Given the guidHash of a custom S3DAnimAction set its float value, assuming it has a matching ParamaterType
        /// and the weaponAction is a custom action.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <param name="value"></param>
        public void SetCustomAnimActionFloatValue (int guidHash, float value)
        {
            SetCustomAnimActionFloatValue(GetAnimAction(guidHash), value);
        }

        /// <summary>
        /// Given a custom S3DAnimAction set its float value, assuming it has a matching ParamaterType
        /// and the weaponAction is a custom action.
        /// </summary>
        /// <param name="s3DAnimAction"></param>
        /// <param name="value"></param>
        public void SetCustomAnimActionFloatValue (S3DAnimAction s3DAnimAction, float value)
        {
            if (s3DAnimAction != null && (int)s3DAnimAction.weaponAction == S3DAnimAction.WeaponActionCustomInt && s3DAnimAction.parameterType == S3DAnimAction.ParameterType.Float)
            {
                s3DAnimAction.customActionFloatValue = value;
            }
        }

        /// <summary>
        /// Given the guidHash of a custom S3DAnimAction set its integer value, assuming it has a matching ParamaterType
        /// and the weaponAction is a custom action.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <param name="value"></param>
        public void SetCustomAnimActionIntegerValue (int guidHash, int value)
        {
            SetCustomAnimActionIntegerValue(GetAnimAction(guidHash), value);
        }

        /// <summary>
        /// Given a custom S3DAnimAction set its integer value, assuming it has a matching ParamaterType
        /// and the weaponAction is a custom action.
        /// </summary>
        /// <param name="s3DAnimAction"></param>
        /// <param name="value"></param>
        public void SetCustomAnimActionIntegerValue (S3DAnimAction s3DAnimAction, int value)
        {
            if (s3DAnimAction != null && (int)s3DAnimAction.weaponAction == S3DAnimAction.WeaponActionCustomInt && s3DAnimAction.parameterType == S3DAnimAction.ParameterType.Integer)
            {
                s3DAnimAction.customActionIntegerValue = value;
            }
        }

        /// <summary>
        /// Given the guidHash of a custom S3DAnimAction set its trigger (bool) value, assuming it has a matching ParamaterType
        /// and the weaponAction is a custom action.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <param name="value"></param>
        public void SetCustomAnimActionTriggerValue (int guidHash, bool value)
        {
            SetCustomAnimActionTriggerValue(GetAnimAction(guidHash), value);
        }

        /// <summary>
        /// Given a custom S3DAnimAction set its trigger (bool) value, assuming it has a matching ParamaterType
        /// and the weaponAction is a custom action.
        /// </summary>
        /// <param name="s3DAnimAction"></param>
        /// <param name="value"></param>
        public void SetCustomAnimActionTriggerValue (S3DAnimAction s3DAnimAction, bool value)
        {
            if (s3DAnimAction != null && (int)s3DAnimAction.weaponAction == S3DAnimAction.WeaponActionCustomInt && s3DAnimAction.parameterType == S3DAnimAction.ParameterType.Trigger)
            {
                s3DAnimAction.customActionTriggerValue = value;
            }
        }

        /// <summary>
        /// Set the weapon animator. This should be attached to, or a child of, the weapon gameobject.
        /// </summary>
        /// <param name="newAnimator"></param>
        public void SetDefaultAnimator (Animator newAnimator)
        {
            // The Animator is an interface, so cannot check if it is a child of the weapon

            defaultAnimator = newAnimator;

            if (isAnimateEnabled && defaultAnimator == null)
            {
                EnableOrDisableAnimate(false);
            }
        }

        /// <summary>
        /// Set the list of weapon anim sets for this weapon
        /// </summary>
        /// <param name="newWeaponAnimSetList"></param>
        public void SetWeaponAnimSetList (List<S3DWeaponAnimSet> newWeaponAnimSetList)
        {
            if (newWeaponAnimSetList == null)
            {
                s3dWeaponAnimSetList.Clear();
            }
            else
            {
                s3dWeaponAnimSetList = newWeaponAnimSetList;
            }
        }

        /// <summary>
        /// At runtime, verify if a parameter of the given name and type exists on the animator controller.
        /// Returns 0 if no matching parameter is found, else returns the parameter hashcode.
        /// WARNING: This impacts GC and should never be called in an update loop. Use sparingly.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        public int VerifyAnimParameter (string parameterName, S3DAnimAction.ParameterType parameterType)
        {
            int hashCode = 0;

            if (isAnimateEnabled && defaultAnimator.parameterCount > 0)
            {
                foreach(AnimatorControllerParameter parm in defaultAnimator.parameters)
                {
                    if (parm.name == parameterName)
                    {
                        if (parm.type == AnimatorControllerParameterType.Bool && parameterType == S3DAnimAction.ParameterType.Bool ||
                            parm.type == AnimatorControllerParameterType.Float && parameterType == S3DAnimAction.ParameterType.Float ||
                            parm.type == AnimatorControllerParameterType.Int && parameterType == S3DAnimAction.ParameterType.Integer ||
                            parm.type == AnimatorControllerParameterType.Trigger && parameterType == S3DAnimAction.ParameterType.Trigger
                        )
                        {
                            hashCode = parm.nameHash;
                            break;
                        }
                    }
                }

                #if UNITY_EDITOR
                if (hashCode == 0)
                {
                    Debug.LogWarning("StickyWeapon.VerifyAnimParameter " + parameterName + " does not exist in animator controller.");
                }
                #endif
            }

            return hashCode;
        }

        #endregion

        #region Public API Methods - Health

        /// <summary>
        /// Get the heat level of the weapon.
        /// 0.0 is no heat, 100 is overheated.
        /// </summary>
        /// <returns></returns>
        public float GetHeatLevel ()
        {
            return heatLevel;
        }

        /// <summary>
        /// Set the new heat level on this weapon.
        /// Range 0.0 (min) to 100.0 (max).
        /// </summary>
        /// <param name="newHeatLevel"></param>
        public void SetHeatLevel (float newHeatLevel)
        {
            if (newHeatLevel < 0f) { newHeatLevel = 0f; }
            else if (newHeatLevel > 100f) { newHeatLevel = 100f; }

            // Only update the weapon performance if we need to
            // Update when:
            // a) heatLevel will be equal or above the overheat threshold
            // b) heatLevel will fallen below the overheat threshold
            // c) AND it has changed
            if (newHeatLevel != heatLevel && (newHeatLevel >= overHeatThreshold || (newHeatLevel < overHeatThreshold && heatLevel >= overHeatThreshold)))
            {
                heatLevel = newHeatLevel;
                UpdateWeaponPerformance();
            }
            else
            {
                heatLevel = newHeatLevel;
            }
        }

        /// <summary>
        /// Set if the weapon firing mechanisms are paused. This will prevent the weapon
        /// from firing, but it can still do other operations like reloading etc.
        /// </summary>
        /// <param name="newValue"></param>
        public void SetIsFiringPaused (bool newValue)
        {
            isFiringPaused = newValue;

            if (isFiringPaused)
            {
                hasFired1 = false;
                hasFired2 = false;

                hasEmptyFired1 = false;
                hasEmptyFired2 = false;

                ResetInput();
            }
        }

        #endregion

        #region Public API Methods - Smoke

        /// <summary>
        /// Reinitialise items related to smoke effects. Call this if making any changes
        /// to smokeEffectsSet1 or 2.
        /// </summary>
        /// <param name="weaponButtonNumber"></param>
        public void ReinitialiseSmoke (int weaponButtonNumber)
        {
            if ((weaponButtonNumber == 1 || weaponButtonNumber == 2) && smokeItemKeyList == null)
            {
                ConfigureEffectsItemKeys(ref smokeItemKeyList, maxActiveSmokeEffects1);
            }

            if (weaponButtonNumber == 1)
            {
                numSmokeEffects1 = smokeEffectsSet1 == null ? 0 : smokeEffectsSet1.NumberOfEffects;

                if (numSmokeEffects1 > 0)
                {
                    smokeEffects1PrefabIDs = new int[numSmokeEffects1];
                    stickyManager.CreateEffectsPools(smokeEffectsSet1, smokeEffects1PrefabIDs);
                }
                else
                {
                    smokeEffects1PrefabIDs = null;
                }
            }
            else if (weaponButtonNumber == 2)
            {
                numSmokeEffects2 = smokeEffectsSet2 == null ? 0 : smokeEffectsSet2.NumberOfEffects;

                if (numSmokeEffects2 > 0)
                {
                    smokeEffects2PrefabIDs = new int[numSmokeEffects2];
                    stickyManager.CreateEffectsPools(smokeEffectsSet2, smokeEffects2PrefabIDs);
                }
                else
                {
                    smokeEffects2PrefabIDs = null;
                }
            }
        }

        /// <summary>
        /// Set the maximum number of smoke effects that can be active at any time on this weapon.
        /// </summary>
        /// <param name="newMax"></param>
        public void SetMaxActiveSmokeEffects (int newMax)
        {
            int prevMax = maxActiveSmokeEffects1;

            if (newMax >= 0)
            {
                maxActiveSmokeEffects1 = newMax;
            }
        }

        #endregion

        #region Public API Methods - Targeting

        /// <summary>
        /// Assign a character that the weapon should attempt to target. This is typically used
        /// when an autofire weapon, held by an NPC, is determining if the weapon is aiming
        /// or pointing toward a Sticky3D character.
        /// It is NOT used to change where the weapon is aiming.
        /// </summary>
        /// <param name="newTarget"></param>
        public void AssignTargetCharacter (StickyControlModule newTarget)
        {
            // We never want a transform and a character to be assigned at the same time
            if (isTargetTransformAssigned)
            {
                UnAssignTargetTransform();
            }

            if (newTarget == null)
            {
                // If there is currently a target, unassign it.
                if (targetSticky3D != null)
                {
                    UnAssignTargetCharacter();
                }

                isTargetSticky3DAssigned = false;
            }
            else
            {
                targetSticky3D = newTarget;
                isTargetSticky3DAssigned = true;
            }
        }

        /// <summary>
        /// Assign a transform that the weapon should attempt to target. This is typically used
        /// when an autofire weapon, held by an NPC, is determining if the weapon is aiming
        /// or pointing toward an object.
        /// It is NOT used to change where the weapon is aiming.
        /// </summary>
        /// <param name="newTarget"></param>
        public void AssignTargetTransform (Transform newTarget)
        {
            // We never want a transform and a character to be assigned at the same time
            if (isTargetSticky3DAssigned)
            {
                UnAssignTargetCharacter();
            }

            if (newTarget == null)
            {
                // If there is currently a target, unassign it.
                if (targetTransform != null)
                {
                    UnAssignTargetTransform();
                }

                isTargetTransformAssigned = false;
            }
            else
            {
                targetTransform = newTarget;
                isTargetTransformAssigned = true;
            }
        }

        /// <summary>
        /// Weapon no longer has line of sight to the target
        /// </summary>
        public void ResetLineOfSight (int weaponButtonNumber)
        {
            if (weaponButtonNumber == 1)
            {
                isLockedOnTarget1 = false;
                HasLineOfSight1 = false;
            }
            else if (weaponButtonNumber == 2)
            {
                isLockedOnTarget2 = false;
                HasLineOfSight2 = false;
            }
        }

        /// <summary>
        /// Stop targeting a Sticky3D character
        /// </summary>
        public void UnAssignTargetCharacter()
        {
            if (isTargetSticky3DAssigned)
            {
                targetSticky3D = null;
                isTargetSticky3DAssigned = false;

                ResetLineOfSight(1);
                ResetLineOfSight(2);
            }
        }

        /// <summary>
        /// Stop targeting a transform
        /// </summary>
        public void UnAssignTargetTransform()
        {
            if (isTargetTransformAssigned)
            {
                targetTransform = null;
                isTargetTransformAssigned = false;

                ResetLineOfSight(1);
                ResetLineOfSight(2);
            }
        }

        #endregion
    }
}