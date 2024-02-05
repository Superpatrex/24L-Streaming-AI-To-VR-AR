using System.Collections.Generic;
using UnityEngine;

namespace scsmmedia
{
    /// <summary>
    /// Script to open and close LBS_Door01 in LB Enviro Pack Samples.
    /// If door is locked, it cannot be opened, however, it can
    /// be closed. The speed can be overridden at runtime.
    /// </summary>
    public class LBS_DoorAnimation01 : MonoBehaviour
    {
        #region Public variables and parameters

        public bool isLocked { get { return _isLocked; } set { _isLocked = value; } }

        /// <summary>
        /// Get or set the Opening speed of the door. Min value 0.01, Max 10. Default 1
        /// </summary>
        public float OpenSpeed { get { return _animationOpeningSpeed; } set { _animationOpeningSpeed = Mathf.Clamp(value, 0.01f, 10f); } }
        /// <summary>
        /// Get or set the Closing speed of the door. Min value 0.01, Max 10. Default 1
        /// </summary>
        public float CloseSpeed { get { return _animationClosingSpeed; } set { _animationClosingSpeed = Mathf.Clamp(value, 0.01f, 10f); } }

        /// <summary>
        /// List of Unity tags. If you change this at runtime, call RefreshTagList()
        /// </summary>
        public List<string> tagList;

        #endregion

        #region Private variables and parameters
        private Animator _animator = null;
        private int _numTags = 0;
        [SerializeField] private bool _isLocked = false;
        [SerializeField] [Range(0.01f, 10f)] private float _animationOpeningSpeed = 1f;
        [SerializeField] [Range(0.01f, 10f)] private float _animationClosingSpeed = 1f;

        #endregion

        #region Initialisation Methods

        // Use this for initialization
        void Awake()
        {
            _animator = GetComponent<Animator>();
            RefreshTagList();

            // Set the openning speed of the door
            if (_animator != null) { _animator.speed = _animationOpeningSpeed; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Call this if you have changed the tag list at runtime
        /// </summary>
        public void RefreshTagList()
        {
            _numTags = tagList == null ? 0 : tagList.Count;
        }

        #endregion

        #region Event Methods

        private void OnTriggerEnter(Collider other)
        {
            if (_animator != null && !_isLocked)
            {
                bool isOpenDoor = false;

                // If not tags in the list, always open the door
                if (_numTags == 0) { isOpenDoor = true; }
                else
                {
                    // Check all the tags
                    for (int t = 0; t < _numTags; t++)
                    {
                        if (other.gameObject.CompareTag(tagList[t]))
                        {
                            isOpenDoor = true;
                            break;
                        }
                    }
                }

                // Open the door if the object entering the door is in the tag list or there are not tags
                if (isOpenDoor)
                {
                    // Update the custom parameter in the animator, which is a multipler of the Speed property.
                    // Simply setting _animator.Speed will not work at runtime.
                    _animator.SetFloat("AnimSpeed", _animationOpeningSpeed);
                    _animator.SetBool("isOpen", true);
                    //_animator.StopPlayback();
                    //Debug.Log("opening speed: " + _animationOpeningSpeed);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_animator != null)
            {
                bool isCloseDoor = false;

                // If no tags in the list, always close the door
                if (_numTags == 0) { isCloseDoor = true; }
                else
                {
                    // Check all the tags
                    for (int t = 0; t < _numTags; t++)
                    {
                        if (other.gameObject.CompareTag(tagList[t]))
                        {
                            isCloseDoor = true;
                            break;
                        }
                    }
                }

                // Close the door if the object leaving the door is in the tag list or there are not tags
                if (isCloseDoor)
                {
                    // Update the custom parameter in the animator, which is a multipler of the Speed property
                    _animator.SetFloat("AnimSpeed", _animationClosingSpeed);
                    _animator.SetBool("isOpen", false);
                    //Debug.Log("opening speed: " + _animationOpeningSpeed);
                }
            }
        }

        #endregion
    }
}
