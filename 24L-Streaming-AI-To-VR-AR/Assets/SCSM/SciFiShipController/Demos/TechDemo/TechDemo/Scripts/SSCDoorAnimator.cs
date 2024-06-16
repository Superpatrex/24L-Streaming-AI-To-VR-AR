using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Component to open and close a door (or doors) using a Unity Animator, controller,
    /// and animation clips. It can also be used to lower/raise landing gear etc.
    /// Optionally add populate an array of bool parameter names. This can be used to independency control
    /// more than one door or sets of doors. e.g. isDoor1Open, isDoor2Open, isSlidingDoorsOpen etc.
    /// If none are provided, it assumes there is one called isOpenDoor in the animator.
    /// SETUP
    /// 1. Create animations (clips) for closed, closing and opening for each door or sets of doors
    /// 2. For each door or sets of doors you need a Layer in the Animator Controller
    /// 3. Each door or sets of doors needs to have a bool parameter in the Animator Controller
    /// 4. Ensure the layer settings have a weight > 0, else nothing will happen.
    /// 5. If you want audio, add an Audio Source to this gameobject
    /// WARNING: The animator controller must have an isOpen (or similar) boolean parameter. It needs
    /// to be a transistion condition to move to the open, close or closed states.
    /// AUDIO NOTE:
    /// For audio, make sure the transition durations from closed->Opening and Opening->Closing are 0 seconds.
    /// If audioSource Loop is enabled, need to disable the audioSource in Animation.
    /// See SSCLiftHeadAirLockDoors.anim for an example.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SSCDoorAnimator : MonoBehaviour
    {
        #region Enumerations
        public enum ParmeterType
        {
            Bool = 0,
            Trigger = 1
        }

        #endregion

        #region Public Variables and Properties

        /// <summary>
        /// Animation parameter types include boolean or triggers
        /// </summary>
        public ParmeterType parameterType = ParmeterType.Bool;

        //[Tooltip("Array of bool Animation Parameters to control one or more doors")]
        /// <summary>
        /// Array of bool or trigger Animimation Parameters to control one or more doors. There should be one for each door or sets of doors.
        /// </summary>
        public string[] openParamNames;

        /// <summary>
        /// Array of trigger Animimation Parameters to control one or more doors These must have the word Close in their names
        /// </summary>
        public string[] closeParamNames;

        /// <summary>
        /// Array of isLocked statuses. There should be one for each door or sets of doors. 
        /// </summary>
        public bool[] isLockedStatuses;

        /// Get or set the Opening speed of the door. Min value 0.01, Max 10. Default 1
        /// </summary>
        public float OpenSpeed { get { return openingSpeed; } set { openingSpeed = Mathf.Clamp(value, 0.01f, 10f); } }
        /// <summary>
        /// Get or set the Closing speed of the door. Min value 0.01, Max 10. Default 1
        /// </summary>
        public float CloseSpeed { get { return closingSpeed; } set { closingSpeed = Mathf.Clamp(value, 0.01f, 10f); } }

        /// <summary>
        /// The audio clip that is played when the doors are opening.
        /// To stop the clip when doors have opened, in the animation controller, disable the AudioSource when
        /// animation has finished.
        /// </summary>
        public AudioClip openingAudioClip = null;

        /// <summary>
        /// The relative volume of the Opening Audio Clip compared to the initial volume of the Audio Source
        /// </summary>
        [Range(0f, 1f)] public float openingClipVolume = 1f;

        /// <summary>
        /// The audio clip that is played when the doors are closing.
        /// To stop the clip when doors have closed, in the animation controller, disable the AudioSource when
        /// animation has finished.
        /// </summary>
        public AudioClip closingAudioClip = null;

        /// <summary>
        /// The relative volume of the Closing Audio Clip compared to the initial volume of the Audio Source
        /// </summary>
        [Range(0f, 1f)] public float closingClipVolume = 1f;

        /// <summary>
        /// The audio clip that is played when the an attempt is made to open a door but the door is locked
        /// </summary>
        public AudioClip isLockedAudioClip = null;

        /// <summary>
        /// The relative volume of the Is Locked Audio Clip compared to the initial volume of the Audio Source
        /// </summary>
        [Range(0f, 1f)] public float isLockedClipVolume = 1f;

        /// <summary>
        /// These are triggered by a door when it starts to open
        /// </summary>
        public SSCDoorAnimEvt1 onOpening = null;

        /// <summary>
        /// These are triggered by a door when it starts to close
        /// </summary>
        public SSCDoorAnimEvt1 onClosing = null;

        /// <summary>
        /// Has the door animator been initialised at runtime?
        /// </summary>
        public bool IsInitialised { get; private set; }

        #endregion

        #region Private Variables
        private Animator _animator = null;
        [SerializeField] [Range(0.01f, 10f)] private float openingSpeed = 1f;
        [SerializeField] [Range(0.01f, 10f)] private float closingSpeed = 1f;
        private int[] isOpenHash = null;
        private int[] isCloseHash = null; // Only applies to triggers
        private int numOpenParms = 0;
        private int numCloseParms = 0;  // Only applies to triggers
        // By default we will use Bool for backward compatibility
        private bool useTriggers = false;
        // used for triggers
        private bool[] isDoorOpenArray;
        private AudioSource doorAudio = null;
        private bool isAudioAvailable = false;
        private float maxAudioVolume = 1f;
        private int scriptInstanceID = 0;
        #endregion

        #region Initialisation Methods

        void Awake()
        {
            _animator = GetComponent<Animator>();

            scriptInstanceID = GetInstanceID();

            // Set the openning speed of the door
            if (_animator != null)
            {
                _animator.speed = openingSpeed;

                numOpenParms = openParamNames == null ? 0 : openParamNames.Length;
                numCloseParms = closeParamNames == null ? 0 : closeParamNames.Length;

                // Maintain backward compatibility. Default to a single isOpen bool animation parameter.
                if (numOpenParms < 1)
                {
                    openParamNames = new string[] { "isOpen" };
                    // Get the hash to avoid Garbage Collection
                    isOpenHash = new int[] { Animator.StringToHash("isOpen") };
                    // Start in the closed state
                    _animator.SetBool(isOpenHash[0], false);
                    numOpenParms = 1;
                }
                else
                {
                    isOpenHash = new int[numOpenParms];

                    useTriggers = parameterType == ParmeterType.Trigger;

                    // Process all the user-defined conditional or open trigger parameters
                    for (int pIdx = 0; pIdx < numOpenParms; pIdx++)
                    {
                        // Get the hash to avoid Garbage Collection
                        isOpenHash[pIdx] = Animator.StringToHash(openParamNames[pIdx]);

                        // Start in the closed state
                        if (useTriggers) { _animator.ResetTrigger(isOpenHash[pIdx]); }
                        else { _animator.SetBool(isOpenHash[pIdx], false); }
                    }

                    if (useTriggers)
                    {
                        // For triggers we need to keep track of the status as currently no simple way to get
                        // this data from Unity.
                        isDoorOpenArray = new bool[numOpenParms];

                        isCloseHash = new int[numCloseParms];

                        // Process all the user-defined trigger parameters
                        for (int pIdx = 0; pIdx < numCloseParms; pIdx++)
                        {
                            // Get the hash to avoid Garbage Collection
                            isCloseHash[pIdx] = Animator.StringToHash(closeParamNames[pIdx]);

                            //Debug.Log("[DEBUG] closeParamNames idx: " + pIdx + " " + closeParamNames[pIdx]);

                            // Start in the closed (idle) state
                            _animator.ResetTrigger(isCloseHash[pIdx]);
                        }
                    }
                }

                VerifyLockArray();

                ResetAudioSettings();

                IsInitialised = true;
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] SSCDoorAnimator could not find Animator component. Did you attach one to this gameobject?");
            }
            #endif
        }

        #endregion

        #region Private Methods

        private void PlayOpeningAudioClip()
        {
            if (isAudioAvailable && openingAudioClip != null)
            {
                doorAudio.clip = openingAudioClip;
                doorAudio.volume = maxAudioVolume * openingClipVolume;
                if (!doorAudio.isActiveAndEnabled) { doorAudio.enabled = true; }

                doorAudio.Play();
            }
        }

        private void PlayClosingAudioClip()
        {
            if (isAudioAvailable && closingAudioClip != null)
            {
                doorAudio.clip = closingAudioClip;
                doorAudio.volume = maxAudioVolume * closingClipVolume;
                if (!doorAudio.isActiveAndEnabled) { doorAudio.enabled = true; }

                //if (doorAudio.isActiveAndEnabled && doorAudio.isPlaying) { doorAudio.Stop(); }

                doorAudio.Play();

                //Invoke("PlayDelayedClosingClip", 0.05f);

                //Debug.Log("[DEBUG] play door closing: " + Time.time);
            }
        }

        private void PlayDelayedClosingClip()
        {
            //Debug.Log("[DEBUG] play door closing delyed audio play: " + Time.time);
            doorAudio.Play();
        }

        // Play the IsLocked or deny clip just once (don't loop)
        private void PlayIsLockedAudioClip()
        {
            if (isAudioAvailable && isLockedAudioClip != null)
            {
                doorAudio.volume = maxAudioVolume * isLockedClipVolume;
                if (!doorAudio.isActiveAndEnabled) { doorAudio.enabled = true; }

                // Ignore Loop setting on AudioSource
                doorAudio.PlayOneShot(isLockedAudioClip);
            }
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Get the door ID that can be used to discover if a door is open or closed.
        /// If the doorIndex is invalid, 0 will be returned.
        /// </summary>
        /// <param name="doorIndex"></param>
        /// <returns></returns>
        public int GetDoorId(int doorIndex)
        {
            if (IsInitialised && doorIndex >= 0 && doorIndex < numOpenParms)
            {
                return isOpenHash[doorIndex];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the door ID that can be used to discover if a door is open or closed.
        /// Where possible use GetDoorId(doorIndex) which doesn't use strings.
        /// if it is not found, 0 will be returned.
        /// </summary>
        /// <param name="doorParameterName"></param>
        /// <returns></returns>
        public int GetDoorId(string doorParameterName)
        {
            if (IsInitialised && !string.IsNullOrEmpty(doorParameterName) && numOpenParms > 0)
            {
                int doorId = 0;
                for (int pIdx = 0; pIdx < numOpenParms; pIdx++)
                {
                    string _openParm = openParamNames[pIdx];
                    if (!string.IsNullOrEmpty(_openParm) && _openParm == doorParameterName)
                    {
                        doorId = isOpenHash[pIdx];
                        break;
                    }
                }
                return doorId;
            }
            else { return 0; }
        }

        /// <summary>
        /// Get the door index given the name of the open door paramater.
        /// Returns -1 if not found.
        /// </summary>
        /// <param name="doorParameterName"></param>
        /// <returns></returns>
        public int GetDoorIndex(string doorParameterName)
        {
            if (IsInitialised && !string.IsNullOrEmpty(doorParameterName) && numOpenParms > 0)
            {
                int doorIndex = -1;
                for (int pIdx = 0; pIdx < numOpenParms; pIdx++)
                {
                    string _openParm = openParamNames[pIdx];
                    if (!string.IsNullOrEmpty(_openParm) && _openParm == doorParameterName)
                    {
                        doorIndex = pIdx;
                        break;
                    }
                }
                return doorIndex;
            }
            else { return -1; }
        }

        /// <summary>
        /// Is any of the doors in the locked state?
        /// </summary>
        /// <returns></returns>
        public bool IsAnyDoorLocked()
        {
            bool isLocked = false;

            if (IsInitialised)
            {
                for (int doorIndex = 0; doorIndex < numOpenParms; doorIndex++)
                {
                    if (isLockedStatuses[doorIndex])
                    {
                        isLocked = true;
                        break;
                    }
                }
            }

            return isLocked;
        }

        /// <summary>
        /// Is any of the doors in the unlocked state?
        /// </summary>
        /// <returns></returns>
        public bool IsAnyDoorUnlocked()
        {
            bool isUnlocked = false;

            if (IsInitialised)
            {
                for (int doorIndex = 0; doorIndex < numOpenParms; doorIndex++)
                {
                    if(!isLockedStatuses[doorIndex])
                    {
                        isUnlocked = true;
                        break;
                    }
                }
            }

            return isUnlocked;
        }

        /// <summary>
        /// Discover if the door is open by passing in the Door Id.
        /// See also GetDoorId(..). Always returns false if ParamaterType is Trigger.
        /// </summary>
        /// <param name="doorId"></param>
        /// <returns></returns>
        public bool IsDoorOpenById(int doorId)
        {
            return IsInitialised && useTriggers ? false : _animator.GetBool(doorId);
        }

        /// <summary>
        /// Discover if a the door is open by passing in the zero-based index of the door in the openParamNames array.
        /// </summary>
        /// <param name="doorIndex"></param>
        /// <returns></returns>
        public bool IsDoorOpenByIndex(int doorIndex)
        {
            if (IsInitialised && doorIndex >= 0 && doorIndex < numOpenParms)
            {
                if (useTriggers)
                {
                    return isDoorOpenArray == null || isDoorOpenArray.Length < doorIndex + 1 ? false : isDoorOpenArray[doorIndex];
                }
                else { return _animator.GetBool(isOpenHash[doorIndex]); }
            }
            else { return false; }
        }

        /// <summary>
        /// Discover if a the door is locked by passing in the zero-based index of the door in the isLockedStatuses array.
        /// </summary>
        /// <param name="doorIndex"></param>
        /// <returns></returns>
        public bool IsDoorLockedByIndex(int doorIndex)
        {
            if (!IsInitialised) { VerifyLockArray(); }

            if (doorIndex >= 0 && doorIndex < numOpenParms)
            {
                return isLockedStatuses[doorIndex];
            }
            else { return false; }
        }

        /// <summary>
        /// Lock a door by passing in the zero-based index of the door in the isLockedStatuses array.
        /// </summary>
        /// <param name="doorIndex"></param>
        public void LockDoor(int doorIndex)
        {
            if (!IsInitialised) { VerifyLockArray(); }

            if (doorIndex >= 0 && doorIndex < numOpenParms)
            {
                isLockedStatuses[doorIndex] = true;
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] SSCDoorAnimator.LockDoor() - the doorIndex of " + doorIndex + " is invalid. Check the number of openParamNames.");
            }
            #endif
        }

        /// <summary>
        /// Attempt to lock all the doors.
        /// </summary>
        public void LockDoors()
        {
            if (!IsInitialised) { VerifyLockArray(); }

            for (int doorIndex = 0; doorIndex < numOpenParms; doorIndex++)
            {
                isLockedStatuses[doorIndex] = true;
            }
        }

        /// <summary>
        /// Unlock a door by passing in the zero-based index of the door in the isLockedStatuses array.
        /// </summary>
        /// <param name="doorIndex"></param>
        public void UnlockDoor(int doorIndex)
        {
            if (!IsInitialised) { VerifyLockArray(); }

            if (doorIndex >= 0 && doorIndex < numOpenParms)
            {
                isLockedStatuses[doorIndex] = false;
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] SSCDoorAnimator.UnlockDoor() - the doorIndex of " + doorIndex + " is invalid. Check the number of openParamNames.");
            }
            #endif
        }

        /// <summary>
        /// Attempt to unlock all the doors.
        /// </summary>
        public void UnlockDoors()
        {
            if (!IsInitialised) { VerifyLockArray(); }

            for (int doorIndex = 0; doorIndex < numOpenParms; doorIndex++)
            {
                isLockedStatuses[doorIndex] = false;
            }
        }

        /// <summary>
        /// Open the (first) door(s) at the current OpenSpeed
        /// </summary>
        public void OpenDoors()
        {
            if (_animator != null && numOpenParms > 0 && (!useTriggers || numCloseParms > 0))
            {
                if (!IsDoorLockedByIndex(0))
                {
                    _animator.speed = openingSpeed;

                    if (useTriggers)
                    {
                        if (!IsDoorOpenByIndex(0))
                        {
                            _animator.ResetTrigger(isCloseHash[0]);
                            _animator.SetTrigger(isOpenHash[0]);
                            isDoorOpenArray[0] = true;
                        }
                    }
                    else { _animator.SetBool(isOpenHash[0], true); }

                    PlayOpeningAudioClip();

                    if (onOpening != null) { onOpening.Invoke(scriptInstanceID, 0, Vector3.zero); }
                }
                else
                {
                    PlayIsLockedAudioClip();
                }
            }
        }

        /// <summary>
        /// Open a specific door or set of doors given the zero-based index in the array of open param names.
        /// We use the doorIndex rather than the parameter name to avoid GC.
        /// </summary>
        /// <param name="doorIndex"></param>
        public void OpenDoors(int doorIndex)
        {
            if (_animator != null)
            {
                if (doorIndex >= 0 && doorIndex < numOpenParms && (!useTriggers || doorIndex < numCloseParms))
                {
                    if (!IsDoorLockedByIndex(doorIndex))
                    {
                        _animator.speed = openingSpeed;

                        if (useTriggers)
                        {
                            if (!IsDoorOpenByIndex(doorIndex))
                            {
                                _animator.ResetTrigger(isCloseHash[doorIndex]);
                                _animator.SetTrigger(isOpenHash[doorIndex]);
                                isDoorOpenArray[doorIndex] = true;
                            }
                        }
                        else { _animator.SetBool(isOpenHash[doorIndex], true); }

                        PlayOpeningAudioClip();

                        if (onOpening != null) { onOpening.Invoke(scriptInstanceID, doorIndex, Vector3.zero); }
                    }
                    else
                    {
                        PlayIsLockedAudioClip();
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("[ERROR] SSCDoorAnimator.OpenDoors() - the doorIndex of " + doorIndex + " is invalid. Check the number of openParamNames" + (useTriggers ? " or closeParamNames." : "."));
                }
                #endif
            }
        }

        /// <summary>
        /// Open a specific door or set of doors given the zero-based index in the array of open param names.
        /// We use the doorIndex rather than the parameter name to avoid GC.
        /// openSpeed must be greater than 0 to change the current OpenSpeed.
        /// </summary>
        /// <param name="doorIndex"></param>
        /// <param name="openSpeed"></param>
        public void OpenDoors(int doorIndex, float openSpeed)
        {
            if (_animator != null)
            {
                if (doorIndex >= 0 && doorIndex < numOpenParms && (!useTriggers || doorIndex < numCloseParms))
                {
                    if (!IsDoorLockedByIndex(doorIndex))
                    {
                        openingSpeed = openSpeed > 0f ? openSpeed : openingSpeed;
                        _animator.speed = openingSpeed;

                        if (useTriggers)
                        {
                            if (!IsDoorOpenByIndex(doorIndex))
                            {
                                _animator.ResetTrigger(isCloseHash[doorIndex]);
                                _animator.SetTrigger(isOpenHash[doorIndex]);
                                isDoorOpenArray[doorIndex] = true;
                            }
                        }
                        else { _animator.SetBool(isOpenHash[doorIndex], true); }

                        PlayOpeningAudioClip();

                        if (onOpening != null) { onOpening.Invoke(scriptInstanceID, doorIndex, Vector3.zero); }
                    }
                    else
                    {
                        PlayIsLockedAudioClip();
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("[ERROR] SSCDoorAnimator.OpenDoors() - the doorIndex of " + doorIndex + " is invalid. Check the number of openParamNames" + (useTriggers ? " or closeParamNames." : "."));
                }
                #endif
            }
        }

        /// <summary>
        /// Attempt to open all doors
        /// </summary>
        public void OpenDoorsAll()
        {
            for (int drIdx = 0; drIdx < numOpenParms; drIdx++)
            {
                OpenDoors(drIdx);
            }
        }

        /// <summary>
        /// Open a specific door or set of doors given the zero-based index in the array of open param names.
        /// Open them instantly rather than playing the animation. Do not play any audio. Ignore lock status.
        /// Do not invoke any opening events.
        /// To get the the stateId call SSCUtils.GetAnimationStateID(stateName). Cache the stateId to avoid GC.
        /// e.g., int stateId = SSCUtils.GetAnimationStateID("RearRamp_Lower");
        /// </summary>
        /// <param name="doorIndex"></param>
        public void OpenDoorsInstantly (int doorIndex, int stateId)
        {
            if (_animator != null)
            {
                if (doorIndex >= 0 && doorIndex < numOpenParms && (!useTriggers || doorIndex < numCloseParms))
                {
                    _animator.Play(stateId, -1, 0.99f);

                    if (useTriggers)
                    {
                        if (!IsDoorOpenByIndex(doorIndex))
                        {
                            isDoorOpenArray[doorIndex] = true;
                        }
                    }
                    else { _animator.SetBool(isOpenHash[doorIndex], true); }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("[ERROR] SSCDoorAnimator.OpenDoorsInstantly() - the doorIndex of " + doorIndex + " is invalid. Check the number of openParamNames" + (useTriggers ? " or closeParamNames." : "."));
                }
                #endif
            }
        }

        /// <summary>
        /// Close the (first) door(s) at the current CloseSpeed
        /// </summary>
        public void CloseDoors()
        {
            if (_animator != null && numOpenParms > 0 && (!useTriggers || numCloseParms > 0) && !IsDoorLockedByIndex(0))
            {
                _animator.speed = closingSpeed;

                if (useTriggers)
                {
                    if (IsDoorOpenByIndex(0))
                    {
                        _animator.ResetTrigger(isOpenHash[0]);
                        _animator.SetTrigger(isCloseHash[0]);
                        isDoorOpenArray[0] = false;
                    }
                }
                else { _animator.SetBool(isOpenHash[0], false); }

                PlayClosingAudioClip();

                if (onClosing != null) { onClosing.Invoke(scriptInstanceID, 0, Vector3.zero); }
            }
        }

        /// <summary>
        /// Close a specific door or set of doors given the zero-based index in the array of open param names.
        /// We use the doorIndex rather than the parameter name to avoid GC.
        /// </summary>
        /// <param name="doorIndex"></param>
        public void CloseDoors(int doorIndex)
        {
            if (_animator != null)
            {
                if (doorIndex >= 0 && doorIndex < numOpenParms && (!useTriggers || doorIndex < numCloseParms))
                {
                    if (!IsDoorLockedByIndex(doorIndex))
                    {
                        _animator.speed = closingSpeed;

                        if (useTriggers)
                        {
                            if (IsDoorOpenByIndex(doorIndex))
                            {
                                _animator.ResetTrigger(isOpenHash[doorIndex]);
                                _animator.SetTrigger(isCloseHash[doorIndex]);
                                isDoorOpenArray[doorIndex] = false;
                            }
                        }
                        else { _animator.SetBool(isOpenHash[doorIndex], false); }

                        PlayClosingAudioClip();

                        if (onClosing != null) { onClosing.Invoke(scriptInstanceID, doorIndex, Vector3.zero); }
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("[ERROR] SSCDoorAnimator.CloseDoors() - the doorIndex of " + doorIndex + " is invalid. Check the number of openParamNames" + (useTriggers ? " or closeParamNames." : "."));
                }
                #endif
            }
        }

        /// <summary>
        /// Close a specific door or set of doors given the zero-based index in the array of open param names.
        /// We use the doorIndex rather than the parameter name to avoid GC.
        /// closeSpeed must be greater than 0 to change the current CloseSpeed.
        /// </summary>
        /// <param name="doorIndex"></param>
        /// <param name="closeSpeed"></param>
        public void CloseDoors(int doorIndex, float closeSpeed)
        {
            if (_animator != null)
            {
                if (doorIndex >= 0 && doorIndex < numOpenParms && (!useTriggers || doorIndex < numCloseParms))
                {
                    if (!IsDoorLockedByIndex(doorIndex))
                    {
                        closingSpeed = closeSpeed > 0f ? closeSpeed : closingSpeed;
                        _animator.speed = closingSpeed;

                        if (useTriggers)
                        {
                            if (IsDoorOpenByIndex(doorIndex))
                            {
                                _animator.ResetTrigger(isOpenHash[doorIndex]);
                                _animator.SetTrigger(isCloseHash[doorIndex]);
                                isDoorOpenArray[doorIndex] = false;
                            }
                        }
                        else { _animator.SetBool(isOpenHash[doorIndex], false); }

                        PlayClosingAudioClip();

                        if (onClosing != null) { onClosing.Invoke(scriptInstanceID, doorIndex, Vector3.zero); }
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("[ERROR] SSCDoorAnimator.CloseDoors() - the doorIndex of " + doorIndex + " is invalid. Check the number of openParamNames." + (useTriggers ? " or closeParamNames." : "."));
                }
                #endif
            }
        }

        /// <summary>
        /// Attempt to close all doors
        /// </summary>
        public void CloseDoorsAll()
        {
            for (int drIdx = 0; drIdx < numOpenParms; drIdx++)
            {
                CloseDoors(drIdx);
            }
        }

        /// <summary>
        /// Call this when you wish to remove any custom event listeners, like
        /// after creating them in code and then destroying the object.
        /// You could add this to your game play OnDestroy code.
        /// </summary>
        public void RemoveListeners()
        {
            if (IsInitialised)
            {
                if (onClosing != null) { onClosing.RemoveAllListeners(); }
                if (onOpening != null) { onOpening.RemoveAllListeners(); }
            }
        }

        /// <summary>
        /// Open or close the specific door or sets of doors given the zero-based index in the array of open param names.
        /// The door(s) will open or close based on the current state of the door(s).
        /// We use the doorIndex rather than the parameter name to avoid GC. 
        /// </summary>
        /// <param name="doorIndex"></param>
        public void ToggleDoors(int doorIndex)
        {
            if (_animator != null)
            {
                if (doorIndex >= 0 && doorIndex < numOpenParms && (!useTriggers || doorIndex < numCloseParms))
                {
                    if (!IsDoorLockedByIndex(doorIndex))
                    {
                        if (useTriggers)
                        {
                            // Is door(s) already open?
                            if (IsDoorOpenByIndex(doorIndex))
                            {
                                CloseDoors(doorIndex, closingSpeed);
                            }
                            else
                            {
                                OpenDoors(doorIndex, openingSpeed);
                            }
                        }
                        else
                        {
                            int isOpenHashValue = isOpenHash[doorIndex];
                            bool isOpen = _animator.GetBool(isOpenHashValue);

                            // Is door(s) already open?
                            if (isOpen)
                            {
                                CloseDoors(doorIndex, closingSpeed);
                            }
                            else
                            {
                                OpenDoors(doorIndex, openingSpeed);
                            }

                            // Toggle the state
                            // isOpen = !isOpen;
                            //_animator.speed = isOpen ? openingSpeed : closingSpeed;
                            //_animator.SetBool(isOpenHashValue, isOpen);
                        }
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("[ERROR] SSCDoorAnimator.ToggleDoors() - the doorIndex of " + doorIndex + " is invalid. Check the number of openParamNames" + (useTriggers ? " or closeParamNames." : "."));
                }
                #endif
            }
        }

        /// <summary>
        /// Open or close the specific door or sets of doors given the zero-based index in the array of open param names.
        /// The door(s) will open or close based on the current state of the door(s).
        /// We use the doorIndex rather than the parameter name to avoid GC.
        /// openSpeed and closeSpeed must be greater than 0 to change the current OpenSpeed and CloseSpeed.
        /// </summary>
        /// <param name="doorIndex"></param>
        /// <param name="openSpeed"></param>
        /// <param name="closeSpeed"></param>
        public void ToggleDoors(int doorIndex, float openSpeed, float closeSpeed)
        {
            if (_animator != null)
            {
                if (doorIndex >= 0 && doorIndex < numOpenParms && (!useTriggers || doorIndex < numCloseParms))
                {
                    if (!IsDoorLockedByIndex(doorIndex))
                    {
                        openingSpeed = openSpeed > 0f ? openSpeed : openingSpeed;
                        closingSpeed = closeSpeed > 0f ? closeSpeed : closingSpeed;

                        if (useTriggers)
                        {
                            // Is door(s) already open?
                            if (IsDoorOpenByIndex(doorIndex))
                            {
                                CloseDoors(doorIndex, closingSpeed);
                            }
                            else
                            {
                                OpenDoors(doorIndex, openingSpeed);
                            }
                        }
                        else
                        {
                            int isOpenHashValue = isOpenHash[doorIndex];
                            bool isOpen = _animator.GetBool(isOpenHashValue);

                            // Toggle the state
                            isOpen = !isOpen;

                            _animator.speed = isOpen ? openingSpeed : closingSpeed;
                            _animator.SetBool(isOpenHashValue, isOpen);
                        }
                    }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("[ERROR] SSCDoorAnimator.ToggleDoors() - the doorIndex of " + doorIndex + " is invalid. Check the number of openParamNames" + (useTriggers ? " or closeParamNames." : "."));
                }
                #endif
            }
        }

        /// <summary>
        /// Call after changing audio source
        /// </summary>
        public void ResetAudioSettings()
        {
            isAudioAvailable = false;

            doorAudio = GetComponent<AudioSource>();

            if (doorAudio != null)
            {
                maxAudioVolume = doorAudio.volume;
                isAudioAvailable = true;
            }
            else if (openingAudioClip != null || closingAudioClip != null || isLockedAudioClip != null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SSCDoorAnimator - there is no AudioSource attached to " + name + ", so the audio clips will not play.");
                #endif
            }
        }

        /// <summary>
        /// Make sure there is the correct number of locks to match the number of doors, which is determined
        /// by the number of Open Param Names.
        /// </summary>
        public void VerifyLockArray()
        {
            int numDoors = IsInitialised ? numOpenParms : openParamNames == null ? 0 : openParamNames.Length;
            int numLocks = isLockedStatuses == null ? 0 : isLockedStatuses.Length;

            if (numLocks != numDoors)
            {
                System.Array.Resize(ref isLockedStatuses, numDoors);
            }
        }

        #endregion
    }
}