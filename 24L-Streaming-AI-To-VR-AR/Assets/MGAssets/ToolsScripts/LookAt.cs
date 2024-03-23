using UnityEngine;

public class LookAt : MonoBehaviour
{
    public GameObject target;

    void LateUpdate()
    {
        transform.LookAt(target.transform);
    }

}
