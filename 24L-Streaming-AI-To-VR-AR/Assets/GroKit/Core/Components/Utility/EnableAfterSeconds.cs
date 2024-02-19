using System.Collections;
using UnityEngine;

public class EnableAfterSeconds : MonoBehaviour
{
    public float time = 1;
    public GameObject whatObject;
    public IEnumerator Start()
    {
        yield return new WaitForSeconds(time);
        whatObject.SetActive(true);
    }
}
