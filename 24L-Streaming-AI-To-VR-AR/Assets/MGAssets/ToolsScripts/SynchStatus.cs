using UnityEngine;

public class SynchStatus : MonoBehaviour
{
    public bool isActive = true;

    [Space]
    public bool status;
    public int index;
    public SynchStatus masterParent;

    [Space]
    public bool isMaster = false;

    SynchStatus[] synchAll = new SynchStatus[0];
    bool synchAllIsDone = false;


    ////////////////////////////// First Inicialization
    void Awake()
    {
        if (isMaster)
        {
            if (!synchAllIsDone)
            {
                synchAllIsDone = true;
                synchAll = gameObject.GetComponentsInChildren<SynchStatus>(true);
            }
        }
        else
        {
            if(masterParent == null)
            {
                SynchStatus[] searchMasterParent = gameObject.GetComponentsInParent<SynchStatus>(true);
                foreach (SynchStatus synch in searchMasterParent) if (synch.isMaster && synch.isActive) { masterParent = synch; break; }

                if (masterParent == null)
                {
                    print("GameObject " + name + " missing SynchStatus MasterParent!");
                    isActive = false;
                }
            }
        }
    }
    ////////////////////////////// First Inicialization



    //////////////////////////////////////////////////////////// Transition of Status
    void OnEnable()
    {
        if (isMaster) return;
        if (gameObject.activeSelf && !status) { status = true; updateStatusAll(); }
    }
    //
    void OnDisable()
    {
        if (isMaster) return;
        if (!gameObject.activeSelf && status) { status = false; updateStatusAll(); }  
    }
    //////////////////////////////////////////////////////////// Transition of Status




    //////////////////////////////////////////////////////////// Status Update
    public void setStatus(int indexToUpdate, bool active)
    {
        if (indexToUpdate != index || isMaster) return;

        status = active;
        gameObject.SetActive(status);
    }
    //
    public void updateStatusAll()
    {
        if (masterParent == null || isMaster) return;

        foreach (SynchStatus synch in masterParent.synchAll) if (synch != null && synch.index == index && synch.status != status) synch.setStatus(index, status);
    }
    //////////////////////////////////////////////////////////// Status Update

}
