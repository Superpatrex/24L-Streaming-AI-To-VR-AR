using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This component, can be used to attach or snap interactive-enabled objects to a gameobject.
    /// It requires a trigger collider.
    /// To attach objects to a character use Equip Points, Stash or Sticky Parts.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Objects/Sticky Socket")]
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickySocket : MonoBehaviour
    {
        #region Public Variables - General

        /// <summary>
        /// If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are
        /// instantiating the component through code.
        /// </summary>
        public bool initialiseOnStart = false;

        /// <summary>
        /// The maximum number of items to attach to this socket
        /// </summary>
        [Range(1, 10)] public int maxItems = 1;

        /// <summary>
        /// When adding, non-trigger colliders on interactive-enabled objects will be disabled.
        /// When removed, they will be re-enabled.
        /// </summary>
        public bool isDisableRegularColOnAdd = true;

        /// <summary>
        /// When adding, trigger colliders on interactive-enabled objects will be disabled.
        /// When removed, they will be re-enabled.
        /// </summary>
        public bool isDisableTriggerColOnAdd = false;

        #endregion

        #region Public Variables - Events

        /// <summary>
        /// These are triggered by a S3D character when they start looking at this socket.
        /// </summary>
        public S3DSocketEvt1 onHoverEnter = null;

        /// <summary>
        /// These are triggered by a S3D character when they stop looking at this socket. 
        /// </summary>
        public S3DSocketEvt1 onHoverExit = null;

        /// <summary>
        /// These are triggered near the start of the AddItem process for a StickyInteractive object
        /// </summary>
        public S3DSocketEvt2 onPreAdd = null;

        /// <summary>
        /// These are triggered immediately after a StickyInteractive object has been added to the socket.
        /// </summary>
        public S3DSocketEvt2 onPostAdd = null;

        #endregion

        #region Public Variables - Editor

        /// <summary>
        /// [INTERNAL ONLY]
        /// Remember which tabs etc were shown in the editor
        /// </summary>
        [HideInInspector] public int selectedTabInt = 0;

        /// <summary>
        /// [INTERNAL ONLY]
        /// </summary>
        [HideInInspector] public bool allowRepaint = false;

        #endregion

        #region Public Variables - Popup

        /// <summary>
        /// The default local space offset a StickyPopupModule appears relative to the socket.
        /// </summary>
        public Vector3 defaultPopupOffset = Vector3.zero;

        /// <summary>
        /// The local space offset a StickyPopupModule appears relative to the socket when the socket is 
        /// empty or has no interactive objects attached.
        /// </summary>
        public Vector3 emptyPopupOffset = Vector3.zero;

        #endregion

        #region Public Properties - General

        /// <summary>
        /// The prefabID for the pooled generic module assigned by StickyManager.
        /// </summary>
        public int DefaultPopupPrefabID { get { return defaultPopupPrefabID; } }

        /// <summary>
        /// The prefabID for the pooled generic module assigned by StickyManager
        /// when the socket is empty or has no interactive objects attached.
        /// </summary>
        public int EmptyPopupPrefabID { get { return emptyPopupPrefabID; } }

        /// <summary>
        /// [READONLY]
        /// Has the module been initialised?
        /// </summary>
        public bool IsInitialised { get { return isInitialised; } }

        /// <summary>
        /// [READONLY]
        /// The number of interactive objects attached to this socket.
        /// </summary>
        public int NumberOfSocketedItems { get { return numSocketedItems; } }

        /// <summary>
        /// [READONLY]
        /// The identifier for the StickySocket. Only available after it has been initialised.
        /// </summary>
        public int SocketID { get { return guidHash; } } 

        #endregion

        #region Public Static Variables
        /// <summary>
        /// Used to denote that a socket reference is not set.
        /// </summary>
        public static int NoID = 0;
        #endregion

        #region Protected Variables - Serialized General

        /// <summary>
        /// The list of interactive-enabled objects attached to this socket.
        /// </summary>
        [SerializeField] protected List<S3DStoreItem> storeItemList = new List<S3DStoreItem>(2);

        /// <summary>
        /// The common interactive tags scriptableobject used to determine which interactive objects
        /// can be added to this socket. Typically only one would be used for all interactive objects
        /// as the text is only used to help you remember what each number from 1-32 represent.
        /// </summary>
        [SerializeField] protected S3DInteractiveTags interactiveTags = null;

        /// <summary>
        /// The interactive-enabled objects that are permitted to be attached to the socket.
        /// See the S3DInteractiveTags scriptableobject.
        /// </summary>
        [SerializeField] protected int permittedTags = ~0;

        #endregion

        #region Protected Variables - Serialized Popup

        /// <summary>
        /// The default StickyPopupModule to display when a character enagages with this socket.
        /// </summary>
        [SerializeField] protected StickyPopupModule defaultPopupPrefab = null;

        /// <summary>
        /// The StickyPopupModule to display when a character engages with the socket and the
        /// socket is empty or has no interactive objects attached.
        /// </summary>
        [SerializeField] protected StickyPopupModule emptyPopupPrefab = null;

        #endregion

        #region Protected and Private Variables

        /// <summary>
        /// Unique identifier for the socket
        /// </summary>
        [SerializeField] protected int guidHash = 0;

        protected bool isInitialised = false;

        protected int numSocketedItems = 0;

        [System.NonSerialized] protected Collider socketCollider;
        [System.NonSerialized] protected MeshRenderer highlighterRenderer;

        /// <summary>
        /// The prefabID for the pooled generic module assigned by StickyManager.
        /// Will return StickyManager.NoPrefabID (-1) if no pool has been created.
        /// </summary>
        protected int defaultPopupPrefabID = -1;

        /// <summary>
        /// The prefabID for the pooled generic module assigned by StickyManager
        /// when the socket is empty or has no interactive objects attached.
        /// Will return StickyManager.NoPrefabID (-1) if no pool has been created.
        /// </summary>
        protected int emptyPopupPrefabID = -1;

        /// <summary>
        /// The currently Active StickyPopupModule.
        /// i.e. being displayed near the object.
        /// 0 = none
        /// </summary>
        protected int popupActiveInstanceID = 0;

        [System.NonSerialized] protected Camera popupCamera = null;

        [System.NonSerialized] private static List<StickyInteractive> tempStickyInteractiveList = null;

        [System.NonSerialized] protected StickyManager stickyManager = null;

        #endregion

        #region Public Delegates

        public delegate void CallbackOnPopupItemClicked(StickySocket stickySocket, int popupPrefabID, int itemNumberClicked, int numberOfItems);

        /// <summary>
        /// The name of your custom method that is called immediately after a popup item is clicked and the popup is closed.
        /// </summary>
        [System.NonSerialized] public CallbackOnPopupItemClicked callbackOnPopupClicked = null;

        #endregion

        #region Private Initialise Methods

        // Start is called before the first frame update
        private void Start()
        {
            if (initialiseOnStart && !isInitialised) { Initialise(); }
        }

        #endregion

        #region Update Methods

        //void Update()
        //{
            
        //}

        #endregion

        #region Protected, Private and Internal Methods - General

        /// <summary>
        /// Add an interactive-enabled object to the socket.
        /// Assumes it in non-null and Socketable.
        /// Returns the storeItemID for the item attached to the socket.
        /// </summary>
        /// <param name="itemToAdd"></param>
        /// <returns></returns>
        protected int AddInteractiveBegin (StickyInteractive itemToAdd)
        {
            int storeItemID = S3DStoreItem.NoStoreItem;

            // Get the unique ID of the interactive-enabled object we want to equip 
            int interactiveID = itemToAdd.StickyInteractiveID;

            // Perform any actions required when the interactive-enabled object added to a socket.
            // This includes performing actions configured in the StickyInteractive editor.
            // Calls any onPreAdd methods.
            if (itemToAdd.SocketObject(this))
            {
                S3DStoreItem storeItem = new S3DStoreItem()
                {
                    stickyInteractive = itemToAdd,
                    stickyInteractiveID = interactiveID
                };

                // Add it to the "inventory" of this socket
                storeItemList.Add(storeItem);
                storeItemID = storeItem.StoreItemID;

                numSocketedItems++;

                itemToAdd.SetStickySocket(this);
                itemToAdd.IsSocketed = true;

                AddInteractiveFinalise(itemToAdd);
            }

            return storeItemID;
        }

        /// <summary>
        /// Complete the adding of an interactive object to the socket.
        /// This includes parenting the item to the socket.
        /// </summary>
        /// <param name="itemToAdd"></param>
        protected void AddInteractiveFinalise (StickyInteractive itemToAdd)
        {
            if (ParentInteractive(itemToAdd))
            {
                // As AddInteractiveFinalise may be delayed, we start the lasso operation here rather
                // than in say stickyCharacterController.EquipToSocket(..), SocketFromLeftHand(..),
                // or SocketFromRightHand(..)
                if (itemToAdd.IsLassoEnabled)
                {
                    StickyControlModule fromCharacter = itemToAdd.LassoCharacter;

                    if (fromCharacter != null)
                    {
                        if (itemToAdd.LassoEquipPoint != null)
                        {
                            // Lasso from a character Equip Point to the socket
                            fromCharacter.StartCoroutine(fromCharacter.LassoInteractiveToSocket(itemToAdd, this, itemToAdd.LassoEquipPoint));
                        }
                        else
                        {
                            // Lasso from a left or right hand to the socket
                            fromCharacter.StartCoroutine(fromCharacter.LassoInteractiveToSocket(itemToAdd, this));
                        }
                    }
                }

                // Fire any StickySocket onPostAdd event methods
                if (onPostAdd != null) { onPostAdd.Invoke(guidHash, itemToAdd.Sticky3DCharacterID, itemToAdd.StickyInteractiveID); }
            }
            // If parenting fails, for whatever reason, and lasso is enabled, turn it off.
            else if (itemToAdd.IsLassoEnabled)
            {
                // Re-enable weapon firing after the add to socket has completed
                if (itemToAdd.LassoCharacter != null)
                {
                    itemToAdd.LassoCharacter.UnpauseWeaponsFiring(0.1f);
                }

                itemToAdd.ClearLasso();
            }
        }

        /// <summary>
        /// Attempt to parent the item to the socket.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        /// <returns></returns>
        protected bool ParentInteractive (StickyInteractive stickyInteractive)
        {
            bool isParented = false;

            // Set the world space position and rotation before parenting to avoid issues with socket scaling.
            Vector3 wsPosition = transform.position;
            Quaternion wsRotation = transform.rotation;

            if (stickyInteractive.IsLassoEnabled)
            {
                stickyInteractive.LassoSourcePositionLS = Quaternion.Inverse(wsRotation) * (stickyInteractive.transform.position - wsPosition);
                stickyInteractive.LassoSourceRotationLS = Quaternion.Inverse(wsRotation) * stickyInteractive.transform.rotation;
            }

            // We want the object's socket attach point rotation to point towards the socket point.
            Quaternion socketLocalRot = Quaternion.Euler(stickyInteractive.socketRotation);

            // Rotate around world axis and subtract the local rotation.
            wsRotation *= Quaternion.Inverse(socketLocalRot);

            stickyInteractive.transform.SetPositionAndRotation(wsPosition, wsRotation);

            //GameObject go = S3DUtils.CreateSphere(wsPosition, 0.1f, false);
            //go.transform.SetParent(transform);

            if (stickyInteractive.isReparentOnDrop)
            {
                // Is the object already held by a character?
                if (stickyInteractive.PreGrabParentTfrm != null)
                {
                    stickyInteractive.PreSocketParentTfrm = stickyInteractive.PreGrabParentTfrm;
                    stickyInteractive.PreGrabParentTfrm = null;
                }
                // Is the object already equipped by a character?
                else if (stickyInteractive.PreEquippedParentTfrm != null)
                {
                    stickyInteractive.PreSocketParentTfrm = stickyInteractive.PreEquippedParentTfrm;
                    stickyInteractive.PreEquippedParentTfrm = null;
                }
                // Is the object already stashed by a character?
                else if (stickyInteractive.PreStashParentTfrm != null)
                {
                    stickyInteractive.PreSocketParentTfrm = stickyInteractive.PreStashParentTfrm;
                    stickyInteractive.PreStashParentTfrm = null;
                }
                else
                {
                    // Always remember the previous parent transform
                    stickyInteractive.PreSocketParentTfrm = stickyInteractive.transform.parent;
                }
            }
            else
            {
                // Always remember the previous parent transform
                stickyInteractive.PreSocketParentTfrm = stickyInteractive.transform.parent;
            }

            // Always parent it to the socket
            stickyInteractive.transform.SetParent(transform);
            // Subtract the local offset
            stickyInteractive.transform.localPosition = stickyInteractive.transform.localRotation * -stickyInteractive.socketOffset;

            // Not sure why this would ever be false but just in case
            // we need to fail it for some reason in the future.
            isParented = true;

            return isParented;
        }

        /// <summary>
        /// Show the default or empty socket popup with the basic configuration
        /// </summary>
        /// <returns></returns>
        protected StickyPopupModule ShowPopupModule()
        {
            StickyPopupModule stickyPopupModule = null;

            if (isInitialised)
            {
                int popupPrefabID = StickyManager.NoPrefabID;
                Vector3 _position = Vector3.zero;

                // Check if the socket is empty and if it has a pooled popup prefab.
                if (numSocketedItems == 0 && emptyPopupPrefabID != StickyManager.NoPrefabID)
                {
                    _position = GetEmptyPopupPosition();
                    popupPrefabID = emptyPopupPrefabID;
                }
                // Check if there is a default pooled popup prefab.
                else if (defaultPopupPrefabID != StickyManager.NoPrefabID)
                {
                    _position = GetDefaultPopupPosition();
                    popupPrefabID = defaultPopupPrefabID;
                }

                if (popupPrefabID != StickyManager.NoPrefabID)
                {
                    // Setup the generic parameters
                    S3DInstantiateGenericObjectParameters igParms = new S3DInstantiateGenericObjectParameters
                    {
                        position = _position,
                        rotation = transform.rotation,
                        genericModulePrefabID = popupPrefabID
                    };

                    StickyGenericModule stickyGenericModule = stickyManager.InstantiateGenericObject(ref igParms);

                    if (stickyGenericModule != null)
                    {
                        // Configure our extended functionality
                        stickyPopupModule = (StickyPopupModule)stickyGenericModule;

                        // Tell the popup which socket triggered it
                        stickyPopupModule.SetSocket(this);
                    }
                }
            }

            return stickyPopupModule;
        }


        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Attempt to configure the child mesh object that will
        /// be enabled when the socket is highlighted, typically
        /// by a player character looking at it.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ConfigureHighlighter()
        {
            bool isConfigured = false;
            Collider tempCollider;

            if (TryGetComponent(out socketCollider) && socketCollider.isTrigger)
            {
                GameObject socketHighlighterGO = null;
                Transform socketHighlighterTfrm = null;

                if (socketCollider is BoxCollider)
                {
                    socketHighlighterGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    socketHighlighterTfrm = socketHighlighterGO.transform;
                    socketHighlighterTfrm.localScale = (socketCollider as BoxCollider).size;
                }
                else if (socketCollider is SphereCollider)
                {
                    socketHighlighterGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    socketHighlighterTfrm = socketHighlighterGO.transform;
                    float radius = (socketCollider as SphereCollider).radius;
                    socketHighlighterTfrm.localScale = new Vector3(radius, radius, radius);
                }
                else if (socketCollider is CapsuleCollider)
                {
                    socketHighlighterGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    socketHighlighterTfrm = socketHighlighterGO.transform;
                    float radius = (socketCollider as CapsuleCollider).radius;
                    float height = (socketCollider as CapsuleCollider).height;
                    socketHighlighterTfrm.localScale = new Vector3(radius, height, radius);
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("[ERROR] StickySocket.ConfigureHighLighter did not find a box, sphere or capsule trigger collider on " + name);
                }
                #endif

                if (socketHighlighterGO != null)
                {
                    if(socketHighlighterGO.TryGetComponent(out tempCollider))
                    {
                        // Immediately disable the collider so it doesn't cause issues
                        tempCollider.enabled = false;
                        // Remove collider - which may not happen immediately
                        #if UNITY_EDITOR
                        DestroyImmediate(tempCollider);
                        #else
                        Destroy(tempCollider);
                        #endif
                    }
                    if (socketHighlighterGO.TryGetComponent(out highlighterRenderer))
                    {
                        // NOTE: The material is set remotely from the player character
                        highlighterRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        highlighterRenderer.receiveShadows = false;

                        // Turn off rendering by default
                        highlighterRenderer.enabled = false;

                        isConfigured = true;
                    }

                    socketHighlighterTfrm.SetParent(transform);
                    // Collider bounds are in world space
                    socketHighlighterTfrm.position = socketCollider.bounds.center;
                    socketHighlighterTfrm.localRotation = Quaternion.identity;

                }
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning("[ERROR] StickySocket.ConfigureHighLighter did not find a valid trigger collider on " + name);
            }
            #endif

            return isConfigured;
        }

        #endregion

        #region Events

        private void OnDestroy()
        {
            RemoveListeners();
        }

        #endregion

        #region Public API Methods - General

        /// <summary>
        /// Return the Instance ID of the active StickyPopupModule.
        /// If there isn't one, return 0. See also SetActivePopupID(..).
        /// </summary>
        /// <returns></returns>
        public int GetActivePopupID()
        {
            return popupActiveInstanceID;
        }

        /// <summary>
        /// Get the world space position for the default popup module
        /// </summary>
        /// <returns></returns>
        public Vector3 GetDefaultPopupPosition()
        {
            return transform.position + (transform.rotation * defaultPopupOffset);
        }

        /// <summary>
        /// Get the world space position for the popup module when the socket is empty
        /// </summary>
        /// <returns></returns>
        public Vector3 GetEmptyPopupPosition()
        {
            return transform.position + (transform.rotation * emptyPopupOffset);
        }

        /// <summary>
        /// Get the S3DStoreItem of the last item that was added.
        /// Ignores items that have already been removed.
        /// </summary>
        /// <returns></returns>
        public S3DStoreItem GetLastItem()
        {
            if (isInitialised && numSocketedItems > 0)
            {
                return storeItemList[numSocketedItems - 1];
            }
            else { return null; }
        }

        /// <summary>
        /// Get the StoreItemID of the last item that was added.
        /// Ignores items that have already been removed.
        /// </summary>
        /// <returns></returns>
        public int GetLastItemID()
        {
            if (isInitialised && numSocketedItems > 0)
            {
                return storeItemList[numSocketedItems - 1].StoreItemID;
            }
            else { return S3DStoreItem.NoStoreItem; }
        }

        /// <summary>
        /// Get the last StickyInteractive object that was added.
        /// Ignores items that have already been removed.
        /// </summary>
        /// <returns></returns>
        public StickyInteractive GetLastInteractive()
        {
            if (isInitialised && numSocketedItems > 0)
            {
                return storeItemList[numSocketedItems - 1].stickyInteractive;
            }
            else { return null; }
        }

        /// <summary>
        /// Get the StickyInteractiveID of the last item that was added.
        /// Ignores items that have already been removed.
        /// </summary>
        /// <returns></returns>
        public int GetLastInteractiveID()
        {
            if (isInitialised && numSocketedItems > 0)
            {
                return storeItemList[numSocketedItems - 1].stickyInteractiveID;
            }
            else { return StickyInteractive.NoID; }
        }

        /// <summary>
        /// Get the store item given a S3DStoreItem.StoreItemId.
        /// </summary>
        /// <param name="storeItemID"></param>
        /// <returns></returns>
        public S3DStoreItem GetItem (int storeItemID)
        {
            S3DStoreItem storeItem = null;

            if (isInitialised && numSocketedItems > 0)
            {
                for (int sIdx = 0; sIdx < numSocketedItems; sIdx++)
                {
                    if (storeItemList[sIdx].StoreItemID == storeItemID)
                    {
                        storeItem = storeItemList[sIdx];
                        break;
                    }
                }
            }

            return storeItem;
        }

        /// <summary>
        /// Get the store item given a stickyInteractiveID
        /// </summary>
        /// <param name="stickyInteractiveID"></param>
        /// <returns></returns>
        public S3DStoreItem GetItemByInteractiveID (int stickyInteractiveID)
        {
            S3DStoreItem storeItem = null;

            if (isInitialised && numSocketedItems > 0)
            {
                for (int sIdx = 0; sIdx < numSocketedItems; sIdx++)
                {
                    if (storeItemList[sIdx].stickyInteractiveID == stickyInteractiveID)
                    {
                        storeItem = storeItemList[sIdx];
                        break;
                    }
                }
            }

            return storeItem;
        }

        /// <summary>
        /// Get the StoreItemID of an interactive-enabled object using the StickyInteractiveID.
        /// Returns the StoreItemID or S3DStoreItem.NoStoreItem.
        /// </summary>
        /// <param name="stickyInteractiveID"></param>
        /// <returns></returns>
        public int GetItemID (int stickyInteractiveID)
        {
            int storeItemID = S3DStoreItem.NoStoreItem;

            if (isInitialised && stickyInteractiveID != StickyManager.NoPrefabID)
            {
                for (int sIdx = 0; sIdx < numSocketedItems; sIdx++)
                {
                    if (storeItemList[sIdx].stickyInteractiveID == stickyInteractiveID)
                    {
                        storeItemID = storeItemList[sIdx].StoreItemID;
                        break;
                    }
                }
            }

            return storeItemID;
        }

        /// <summary>
        /// Get the StoreItemID of an interactive-enabled object using a StickyInteractive item.
        /// Returns the StoreItemID or S3DStoreItem.NoStoreItem.
        /// </summary>
        /// <param name="stashItem"></param>
        /// <returns></returns>
        public int GetItemID (StickyInteractive stashItem)
        {
            if (stashItem == null) { return S3DStoreItem.NoStoreItem; }
            else { return GetItemID(stashItem.StickyInteractiveID); }
        }

        /// <summary>
        /// Get the 32-bit mask use to detemine which interactive-enabled objects are permitted to be attached to the socket.
        /// See the S3DInteractiveTags scriptableobject.
        /// </summary>
        /// <param name="bitMask"></param>
        public int GetPermittedTags()
        {
            return permittedTags;
        }

        /// <summary>
        /// Get the world space position and rotation of an interactive object if it were to be attached to the socket.
        /// NOTE: For performance reasons, does NOT check if the stickyInteractive is null.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        /// <param name="wsPosition"></param>
        /// <param name="wsRotation"></param>
        public void GetSocketedLocation (StickyInteractive stickyInteractive, ref Vector3 wsPosition, ref Quaternion wsRotation)
        {
            // We want the object's socket attach point rotation to point towards the socket point.
            // We need to calculate the rotation first.
            Quaternion socketLocalRot = Quaternion.Euler(stickyInteractive.socketRotation);
            // Rotate around world axis and subtract the local rotation.
            wsRotation = transform.rotation * Quaternion.Inverse(socketLocalRot);
            // Subtract the interactive socket offset
            wsPosition = transform.position + (wsRotation * -stickyInteractive.socketOffset);
        }

        /// <summary>
        /// This method is called automatically by StickyPopupModule when an item is clicked. 
        /// </summary>
        /// <param name="stickyPopupModule"></param>
        /// <param name="itemNumber"></param>
        /// <param name="sourceSocket"></param>
        public void PopupItemClicked (StickyPopupModule stickyPopupModule, int itemNumber, StickySocket sourceSocket)
        {
            //string socketName = sourceSocket == null ? "unknown socket" : sourceSocket.name;
            //Debug.Log("[DEBUG] item " + itemNumber + " clicked on " + socketName + " T:" + Time.time);

            int popupPrefabID = stickyPopupModule.popupPrefabID;
            int numberOfPopupItems = stickyPopupModule.NumberOfItems;

            // Close the popup after use - i.e. return it to the pool.
            if (stickyPopupModule != null) { stickyPopupModule.DestroyGenericObject(); }

            if (callbackOnPopupClicked != null) { callbackOnPopupClicked.Invoke(this, popupPrefabID, itemNumber, numberOfPopupItems); }
        }

        /// <summary>
        /// Attempt to remove the StickyInteractive object from the socket.
        /// If isRestoreColliders is true, colliders will be re-enabled based on the socket settings.
        /// </summary>
        /// <param name="stickyInteractiveID"></param>
        /// <param name="isRestoreColliders"></param>
        /// <param name="isRestoreGravity"></param>
        public void RemoveInteractive (int stickyInteractiveID, bool isRestoreColliders, bool isRestoreGravity)
        {
            S3DStoreItem storeItem = GetItemByInteractiveID(stickyInteractiveID);

            if (storeItem != null)
            {
                StickyInteractive stickyInteractive = storeItem.stickyInteractive;

                stickyInteractive.IsSocketed = false;

                RemoveItem(storeItem.StoreItemID);

                if (isRestoreColliders) { RestoreColliders(stickyInteractive); }
                if (isRestoreGravity) { stickyInteractive.RestoreRigidbodySettings(); }
            }
        }

        /// <summary>
        /// Call this when you wish to remove any custom event listeners, like
        /// after creating them in code and then destroying the object.
        /// You could add this to your game play OnDestroy code.
        /// </summary>
        public void RemoveListeners()
        {
            if (isInitialised)
            {
                if (onHoverEnter != null) { onHoverEnter.RemoveAllListeners(); }
                if (onHoverExit != null) { onHoverExit.RemoveAllListeners(); }
                if (onPreAdd != null) { onPreAdd.RemoveAllListeners(); }
                if (onPostAdd != null) { onPostAdd.RemoveAllListeners(); }
            }
        }

        /// <summary>
        /// If required, enable colliders.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        public void RestoreColliders (StickyInteractive stickyInteractive)
        {
            if (isDisableRegularColOnAdd) { stickyInteractive.EnableNonTriggerColliders(); }
            if (isDisableTriggerColOnAdd) { stickyInteractive.EnableTriggerColliders(); }
        }

        /// <summary>
        /// Set the instance ID of the active StickyPopupModule currently being displayed.
        /// See also GetActivePopupID().
        /// </summary>
        /// <param name="instanceID"></param>
        public void SetActivePopupID (int instanceID)
        {
            popupActiveInstanceID = instanceID;
        }

        /// <summary>
        /// Set default StickyPopupModule to display when a character enagages with this socket.
        /// </summary>
        /// <param name="stickyPopupModulePrefab"></param>
        public void SetDefaultPopupPrefab (StickyPopupModule stickyPopupModulePrefab)
        {
            defaultPopupPrefabID = StickyManager.NoPrefabID;

            defaultPopupPrefab = stickyPopupModulePrefab;

            if (isInitialised && stickyPopupModulePrefab != null)
            {
                if (stickyManager == null) { stickyManager = StickyManager.GetOrCreateManager(gameObject.scene.handle); }
                defaultPopupPrefabID = stickyManager.GetOrCreateGenericPool(stickyPopupModulePrefab);
            }
        }

        /// <summary>
        /// Set StickyPopupModule to display when a character enagages with this socket
        /// when empty or has no iteractive objects attached.
        /// </summary>
        /// <param name="stickyPopupModulePrefab"></param>
        public void SetEmptyPopupPrefab (StickyPopupModule stickyPopupModulePrefab)
        {
            emptyPopupPrefabID = StickyManager.NoPrefabID;

            emptyPopupPrefab = stickyPopupModulePrefab;

            if (isInitialised && stickyPopupModulePrefab != null)
            {
                if (stickyManager == null) { stickyManager = StickyManager.GetOrCreateManager(gameObject.scene.handle); }
                emptyPopupPrefabID = stickyManager.GetOrCreateGenericPool(stickyPopupModulePrefab);
            }
        }

        /// <summary>
        /// The common interactive tags scriptableobject used to determine which interactive objects can be added to this socket
        /// </summary>
        /// <param name="newInteractiveTags"></param>
        public void SetInteractiveTags (S3DInteractiveTags newInteractiveTags)
        {
            interactiveTags = newInteractiveTags;
        }

        /// <summary>
        /// The 32-bit mask use to detemine which interactive-enabled objects are permitted to be attached to the socket.
        /// Defaults to ~0 (Everything). See the S3DInteractiveTags scriptableobject.
        /// </summary>
        /// <param name="bitMask"></param>
        public void SetPermittedTags (int bitMask)
        {
            permittedTags = bitMask;
        }

        /// <summary>
        /// Sets which camera, if any the current popup will face
        /// </summary>
        /// <param name="camera"></param>
        public void SetPopupCamera (Camera camera)
        {
            popupCamera = camera;
        }

        /// <summary>
        /// Set the world space position and rotation of an interactive object at the attach point.
        /// NOTE: For performance reasons, does NOT check if the stickyInteractive is null.
        /// </summary>
        /// <param name="stickyInteractive"></param>
        public void SetSocketedLocation (StickyInteractive stickyInteractive)
        {
            // We want the object's socket attach point rotation to point towards the socket point.
            // We need to calculate the rotation first.
            Quaternion socketLocalRot = Quaternion.Euler(stickyInteractive.socketRotation);
            // Rotate around world axis and subtract the local rotation.
            Quaternion wsRotation = transform.rotation * Quaternion.Inverse(socketLocalRot);
            // Subtract the interactive socket offset
            Vector3 wsPosition = transform.position + (wsRotation * -stickyInteractive.socketOffset);
            stickyInteractive.transform.SetPositionAndRotation(wsPosition, wsRotation);
        }

        /// <summary>
        /// Show the default or empty socket popup
        /// </summary>
        public void ShowPopup()
        {
            if (isInitialised)
            {
                StickyPopupModule stickyPopupModule = ShowPopupModule();

                // Configure our extended functionality
                if (stickyPopupModule != null)
                {
                    stickyPopupModule.SetCamera(popupCamera);

                    // Call the same method when any of the items are clicked
                    stickyPopupModule.SetItemAction2(PopupItemClicked);
                }
            }
        }

        /// <summary>
        /// Show the default or empty socket popup, indirectly triggered by a (player) character.
        /// Typically used when a character is looking at a socket, and brings up a popup menu.
        /// The popup will be parented to the socket but the popup will know the character
        /// indirectly triggered it.
        /// </summary>
        /// <param name="indirectCharacter"></param>
        public void ShowPopupFromCharacter (StickyControlModule indirectCharacter)
        {
            if (isInitialised)
            {
                StickyPopupModule stickyPopupModule = ShowPopupModule();

                // Configure our extended functionality
                if (stickyPopupModule != null)
                {
                    stickyPopupModule.SetCamera(popupCamera);

                    stickyPopupModule.SetCharacterIndirect(indirectCharacter);

                    // Call the same method when any of the items are clicked
                    stickyPopupModule.SetItemAction2(PopupItemClicked);
                }
            }
        }

        #endregion

        #region Public API Virtual Methods - General

        /// <summary>
        /// Attempt to add an interactive-enabled object to the socket.
        /// Returns the storeItemID for the item attached to the socket.
        /// If it is not added, S3DStoreItem.NoStoreItem is returned.
        /// </summary>
        /// <param name="itemToAdd"></param>
        /// <returns></returns>
        public virtual int AddItem (StickyInteractive itemToAdd)
        {
            int storeItemID = S3DStoreItem.NoStoreItem;

            if (CanAddItem(itemToAdd))
            {
                storeItemID = AddInteractiveBegin(itemToAdd);
            }

            return storeItemID;
        }

        /// <summary>
        /// Check to see if the interactive-enabled object is eligible to be added to the socket.
        /// NOTE: To add the item, see AddItem(..).
        /// </summary>
        /// <param name="itemToCheck"></param>
        /// <returns></returns>
        public virtual bool CanAddItem (StickyInteractive itemToCheck)
        {
            bool isEligible = false;

            if (itemToCheck == null)
            { 
                #if UNITY_EDITOR
                Debug.LogWarning("StickySocket.CanAddItem " + name + " cannot add the item because it is null");
                #endif
            }
            else if (!isInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("StickySocket.CanAddItem " + name + " cannot add " + itemToCheck.name + " because the socket is not initialised");
                #endif
            }
            else if (!itemToCheck.IsSocketable)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("StickySocket.CanAddItem " + name + " cannot add " + itemToCheck.name + " because it is not Socketable");
                #endif
            }
            else if (numSocketedItems >= maxItems)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("StickySocket.CanAddItem " + name + " cannot add " + itemToCheck.name + " because there are no available slots");
                #endif
            }
            else if ((itemToCheck.GetInteractiveTag() & permittedTags) == 0)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("StickySocket.CanAddItem " + name + " cannot add " + itemToCheck.name + " because the interactive tag is not permitted on this socket");
                #endif
            }
            else
            {
                isEligible = true;
            }

            return isEligible;
        }

        /// <summary>
        /// Destroy the StoreItem.
        /// </summary>
        /// <param name="storeItem"></param>
        public virtual void DestroyItem (S3DStoreItem storeItem)
        {
            if (storeItem != null)
            {
                #if UNITY_EDITOR
                DestroyImmediate(storeItem.stickyInteractive.gameObject);
                #else
                Destroy(storeItem.stickyInteractive.gameObject);
                #endif

                storeItemList.Remove(storeItem);

                numSocketedItems = storeItemList.Count;
            }
        }

        /// <summary>
        /// Destroy the store item given a S3DStoreItem.StoreItemId.
        /// </summary>
        /// <param name="storeItemID"></param>
        /// <returns></returns>
        public virtual void DestroyItem (int storeItemID)
        {
            DestroyItem(GetItem(storeItemID));
        }

        /// <summary>
        /// Safely destroy the StickySocket
        /// 1. Return active popup (if any) to the pool
        /// 2. Destroy the actual gameobject
        /// </summary>
        public virtual void DestroySocket()
        {
            if (isInitialised)
            {
                // Return the active popup to the pool (if there is one)
                if (popupActiveInstanceID != 0)
                {
                    /// TODO - Fix potential Garbage created here.
                    StickyPopupModule activePopup = GetComponentInChildren<StickyPopupModule>(true);

                    // If there is a popup, call the overridden version of DestroyGenericObject.
                    if (activePopup != null) { activePopup.DestroyGenericObject(); }
                }
            }

            #if UNITY_EDITOR
            DestroyImmediate(gameObject);
            #else
            Destroy(gameObject);
            #endif
        }

        /// <summary>
        /// Hide the socket
        /// </summary>
        public virtual void HideSocket()
        {
            if (isInitialised) { highlighterRenderer.enabled = false; }
        }

        /// <summary>
        /// Initialise the StickyInteractive component at runtime. Has no effect if already initialised.
        /// If you wish to override this in a child (inherited) class you almost always will want to
        /// call the base method first.
        /// public override void Initialise()
        /// {
        ///    base.Initialise();
        ///    // Do stuff here
        /// }
        /// </summary>
        public virtual void Initialise()
        {
            if (!isInitialised)
            {
                // If this component doesn't have a unique code, create one now.
                if (guidHash == 0) { guidHash = S3DMath.GetHashCodeFromGuid(); }

                isInitialised = ConfigureHighlighter();

                SetDefaultPopupPrefab(defaultPopupPrefab);
                SetEmptyPopupPrefab(emptyPopupPrefab);

                #region Add and initialise any child items

                if (tempStickyInteractiveList == null) { tempStickyInteractiveList = new List<StickyInteractive>(5); }
                else { tempStickyInteractiveList.Clear(); }

                transform.GetComponentsInChildren(true, tempStickyInteractiveList);

                int numItemsFound = tempStickyInteractiveList.Count;

                // Add any child items onto the socket
                for (int sIdx = 0; sIdx < numItemsFound; sIdx++)
                {
                    StickyInteractive _stickyInteractive = tempStickyInteractiveList[sIdx];

                    // If the item isn't socketable, make it so.
                    if (!_stickyInteractive.IsSocketable) { _stickyInteractive.SetIsSocketable(true); }

                    // If the item is not initialised, attempt to do that now
                    if (!_stickyInteractive.IsInitialised) { _stickyInteractive.Initialise(); }

                    AddItem(_stickyInteractive);
                }

                tempStickyInteractiveList.Clear();
                #endregion

                // FUTURE
                //if (onPostInitialised != null)
                //{
                //    if (onPostInitialisedEvtDelay > 0f) { Invoke("DelayOnPostInitialiseEvents", onPostInitialisedEvtDelay); }
                //    else { onPostInitialised.Invoke(guidHash, 0); }
                //}
            }
        }

        /// <summary>
        /// [INCOMPLETE]
        /// Remove an interactive-enabled object from the StickySocket given the StoreItem ID.
        /// </summary>
        /// <param name="storeItemID"></param>
        public virtual void RemoveItem (int storeItemID)
        {
            if (isInitialised && storeItemID != S3DStoreItem.NoStoreItem)
            {
                S3DStoreItem storeItem = GetItem(storeItemID);
                if (storeItem != null)
                {
                    StickyInteractive stickyInteractive = storeItem.stickyInteractive;

                    if (storeItemList.Remove(storeItem))
                    {
                        numSocketedItems--;
                        stickyInteractive.SetStickySocket(null);
                        stickyInteractive.IsSocketed = false;
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to show or highlight the socket
        /// </summary>
        public virtual void ShowSocket (Material socketMaterial)
        {
            if (isInitialised)
            {
                highlighterRenderer.material = socketMaterial;
                highlighterRenderer.enabled = true;
            }
        }

        #endregion

    }
}