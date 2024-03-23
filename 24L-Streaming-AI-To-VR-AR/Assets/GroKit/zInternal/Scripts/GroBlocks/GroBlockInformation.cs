using Core3lb;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Core3lbClass]
public class GroBlockInformation : MonoBehaviour
{
    public string displayName;
    [TextArea(1, 10)]
    public string description;
    //Group name is from Folder!
    [HideInInspector]
    public string prefabName;
    [HideInInspector]
    public string folderName;
}
