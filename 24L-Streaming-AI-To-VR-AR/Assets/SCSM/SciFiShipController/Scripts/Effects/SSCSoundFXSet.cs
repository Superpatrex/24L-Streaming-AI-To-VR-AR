using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// A scriptable object that stores a list of EffectsModules used exclusively to play audio clips.
    /// Typically they would be Pooled.
    /// </summary>
    [CreateAssetMenu(fileName = "SSC Sound FX Set", menuName = "Sci-Fi Ship Controller/Sound FX Set")]
    [HelpURL("https://scsmmedia.com/ssc-documentation")]
    public class SSCSoundFXSet : ScriptableObject
    {
        #region Public Variables

        public List<EffectsModule> effectsModuleList;

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the number of EffectsModule slots in the list. NOTE: Some may be null
        /// </summary>
        public int NumberOfEffects { get { return effectsModuleList == null ? 0 : effectsModuleList.Count; } }

        #endregion
    }
}