using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core3lb
{
    [CreateAssetMenu]
    public class GroBlocksDataScriptableObject : ScriptableObject
    {
        [System.Serializable]
        public struct GroBlockInfo
        {
            public string displayName;
            public string folder;
            public string prefabName;
            [TextArea]
            public string description;
        }

        [SerializeField]
        public List<GroBlockInfo> groBlockInfos;
    }
}
