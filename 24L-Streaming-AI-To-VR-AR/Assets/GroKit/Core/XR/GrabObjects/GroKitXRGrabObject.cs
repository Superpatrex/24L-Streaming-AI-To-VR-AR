using UnityEngine;
using System.Collections.Generic;

namespace Core3lb
{
    public class GroKitXRGrabObject : BaseXRGrabObject
    {
        //This is a Simplfied Generic XRGrab Object it does not require a rigidbody If you are looking for something with more advanced features use the XRI or Oculus GrabSystems
        //It uses a fake parenting system to attach the object to hand setting it to Kinemantic on Grab

        [Header("GroKit Settings")]
        [Tooltip("Throwing Requires Rigidbody disable throwing by setting these to zero")]
        public float throwPower = 1;
        public float angularPower = 1;

        //Parenting Calcuations
        Vector3 startParentPosition;
        Quaternion startParentRotationQ;
        Vector3 startParentScale;

        Vector3 startChildPosition;
        Quaternion startChildRotationQ;
        Vector3 startChildScale;

        Matrix4x4 parentMatrix;

        public void Awake()
        {
            //if no Body it will ignore throwing
            body = GetComponent<Rigidbody>();
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out XRHand curHand))
            {
                currentHand = curHand;
                EnterEvent();
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (!isGrabbed)
            {
                if (other.TryGetComponent(out XRHand curHand))
                {
                    if (currentHand == curHand)
                    {
                        currentHand = null;
                        ExitEvent();
                    }
                }
            }
        }

        public virtual bool GetGrabInputDown()
        {
            return currentHand.GrabProcessor.isDown;
        }

        public virtual bool GetGrabInput()
        {
            return currentHand.GrabProcessor.getState;
        }

        public virtual void Update()
        {

            if (currentHand)
            {   
                if (GetGrabInputDown())
                {

                    Grab();
                    ExitEvent(); //Enter and Exit are used for Highlights so 
                }
            }
            if (isGrabbed)
            {
                if (GetGrabInput() == false)
                {
                    Drop();
                    ExitEvent();
                    return;
                }
            }
            if (isGrabbed)
            {
                TrackMovement();
                FollowHand();
            }
        }

        //This is used to ensure that if the object is disabled while grabbed it will drop
        public void OnDisable()
        {
            if(isGrabbed)
            {
                ForceDrop();
            }
        }

        public void FollowHand()
        {
            //Fake Parenting Calcuation using Matrix4x4
            parentMatrix = Matrix4x4.TRS(currentHand.transform.position, currentHand.transform.rotation, currentHand.transform.lossyScale);
            transform.position = parentMatrix.MultiplyPoint3x4(startChildPosition);
            transform.rotation = (currentHand.transform.rotation * Quaternion.Inverse(startParentRotationQ)) * startChildRotationQ;
        }

        public override void Drop()
        {
            isGrabbed = false;
            currentHand.isGrabbing = false;
            currentHand.currentHeldObject = null;
            //Only deal with rigidbody if exists
            if (body)
            {
                if (!kinematicOnDrop)
                {
                    body.isKinematic = false;
                }
                DoThrow();
            }
            base.Drop();
        }

        public override void ForceDrop()
        {
            Drop();
            currentHand = null;
        }

        public override void ForceGrab(XRHand selectedHand)
        {
            currentHand = selectedHand;
            Grab();
        }

        public override void Grab()
        {
            if(isLocked)
            {
                return;
            }
            isGrabbed = true;
            currentHand.isGrabbing = true;
            //Store values for Matrix
            startParentPosition = currentHand.transform.position;
            startParentRotationQ = currentHand.transform.rotation;
            startParentScale = currentHand.transform.lossyScale;
            startChildPosition = transform.position;
            startChildRotationQ = transform.rotation;
            startChildScale = transform.lossyScale;

            // Keeps child position from being modified at the start by the parent's initial transform
            startChildPosition = DivideVectors(Quaternion.Inverse(currentHand.transform.rotation) * (startChildPosition - startParentPosition), startParentScale);
            if (body)
                body.isKinematic = true;
            base.Grab();
        }

        #region Throwing Calcuations
        //#######################
        //Throw Calcuations

        private Queue<Vector3> positionQueue = new Queue<Vector3>();
        private Queue<Quaternion> rotationQueue = new Queue<Quaternion>();
        private int maxFrameCount = 10;


        protected virtual void TrackMovement()
        {
            // Add the current position and rotation to the queues
            positionQueue.Enqueue(transform.position);
            rotationQueue.Enqueue(transform.rotation);

            // Ensure queues only contain the last 10 frames of data
            if (positionQueue.Count > maxFrameCount)
            {
                positionQueue.Dequeue();
                rotationQueue.Dequeue();
            }
        }


        protected virtual Vector3 CalculateAverageChange(Queue<Vector3> queue)
        {
            Vector3 sumOfChanges = Vector3.zero;
            Vector3 previous = queue.Peek();

            foreach (Vector3 current in queue)
            {
                sumOfChanges += current - previous;
                previous = current;
            }

            return sumOfChanges / queue.Count;
        }

        protected virtual Vector3 CalculateAngularVelocity()
        {
            Quaternion sumOfDeltaRotations = Quaternion.identity;
            Quaternion previousRotation = rotationQueue.Peek();

            foreach (Quaternion currentRotation in rotationQueue)
            {
                Quaternion deltaRotation = currentRotation * Quaternion.Inverse(previousRotation);
                sumOfDeltaRotations *= deltaRotation;
                previousRotation = currentRotation;
            }

            // Convert the summed rotation to angular velocity
            return new Vector3(sumOfDeltaRotations.x, sumOfDeltaRotations.y, sumOfDeltaRotations.z) * (2.0f / Time.fixedDeltaTime);
        }
        public virtual void DoThrow()
        {
            if (!body)
            {
                //No RigidBody you cannot be thrown;
                return;
            }
            Vector3 initialVelocity = CalculateAverageChange(positionQueue) / Time.fixedDeltaTime;
            Vector3 angularVelocity = CalculateAngularVelocity();

            //This mag is to ensure you can drop things softly
            if (initialVelocity.magnitude > .15f)
            {
                body.velocity = initialVelocity * throwPower;
                body.angularVelocity = angularVelocity * angularPower;
            }
        }

        Vector3 DivideVectors(Vector3 num, Vector3 den) { return new Vector3(num.x / den.x, num.y / den.y, num.z / den.z); }
        #endregion
    }
}
