using System.Collections.Generic;
using UnityEngine;

namespace Core3lb
{

    public class ResettableObject : MonoBehaviour

    {
        [SerializeField] Vector3 position;
        [SerializeField] Quaternion rotation;
        [SerializeField] Rigidbody bodyToreset;


        List<MassResetter> resetters = new List<MassResetter>();

        //public UnityEvent ResetEvent;
        //public UnityEvent SetSpawnEvent;

        private void Start()
        {
            bodyToreset = GetComponent<Rigidbody>();
            _SetNewSpawn();
        }

        private void OnDestroy()
        {
            foreach (var resetter in resetters)
            {
                resetter.RemoveResettable(this);
            }
        }

        public void ConnectResetter(MassResetter resetter)
        {
            if (!resetters.Contains(resetter))
                resetters.Add(resetter);
        }

        public void RemoveResetter(MassResetter resetter)
        {
            if (!resetters.Contains(resetter))
                resetters.Add(resetter);
        }

        public void _ResetObject()
        {
            ActualReset();
        }

        void ActualReset()
        {
            //if(TryGetComponent(out Grabbable3lb grabber))
            //{
            //    if(grabber.isGrabbed)
            //    {
            //        grabber.ForceHandsRelease();
            //    }
            //}
            if (bodyToreset)
            {
                bodyToreset.velocity = Vector3.zero;
                bodyToreset.angularVelocity = Vector3.zero;
                //bodyToreset.isKinematic = true;
            }
            transform.position = position;
            transform.rotation = rotation;
        }

        public void _SetNewSpawn()
        {
            position = transform.position;
            rotation = transform.rotation;
        }
    }
}
