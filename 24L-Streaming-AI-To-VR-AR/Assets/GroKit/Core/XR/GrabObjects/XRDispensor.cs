namespace Core3lb
{
    public class XRDispenser : XRInteract
    {
        [CoreHeader("Dispenser Settings")]
        [CoreRequired]
        public GroKitXRGrabObject objectToDispense;

        public override void Interact()
        {
            if (wasUsed)
            {
                return;
            }
            wasUsed = true;
            interact.Invoke();
            SpawnAndForceGrabObject();
        }

        public virtual void SpawnAndForceGrabObject()
        {
            GroKitXRGrabObject currentObject = Instantiate(objectToDispense, transform.position, transform.rotation);
            currentObject.ForceGrab(currentHand);
        }

    }
}
