using UnityEngine;

namespace Core3lb
{
    public class PrefabInfo : MonoBehaviour
    {
        public MeshRenderer meshRenderer;
        public Collider currentCollider;
        public Rigidbody rigidBody;

        Vector3 savedPos;
        Quaternion savedRot;

        public void Awake()
        {
            _SaveTransform();
        }

        public void _SaveTransform()
        {
            savedPos = transform.position;
            savedRot = transform.rotation;
        }

        public void _GotoSavedTransform()
        {
            transform.position = savedPos;
            transform.rotation = savedRot;
        }

        public void _ToggleRenderer(bool toggle)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = toggle;
            }
        }

        public void _ToggleCollider(bool toggle)
        {
            if (currentCollider != null)
            {
                currentCollider.enabled = toggle;
            }
        }

        public void _ToggleRigidbody(bool toggle)
        {
            if (rigidBody != null)
            {
                rigidBody.isKinematic = !toggle;
            }
        }

        [CoreButton("DEBUG_FillProperties",true)]
        public void DEBUG_FillProperties()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            currentCollider = GetComponent<Collider>();
            rigidBody = GetComponent<Rigidbody>();

            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<MeshRenderer>();
            }
            if (currentCollider == null)
            {
                currentCollider = GetComponentInChildren<Collider>();
            }
            if (rigidBody == null)
            {
                rigidBody = GetComponentInChildren<Rigidbody>();
            }
        }
    }
}
