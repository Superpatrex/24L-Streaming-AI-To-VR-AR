using UnityEngine;

// Copyright (c) 2015-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// [FUTURE - WIP]
    /// Objects with the IFloatingTransform interface are able to translate when the FloatingOrigin updates.
    /// The IFloatingTransform must be registered with the scenes FloatingOrigin Object and removed before being destroyed.
    /// </summary>
    public interface IFloatingTransform
    {
        /// <summary>
        /// Adds positionDelta to all world-space Vector3s.
        /// </summary>
        /// <param name="positionDelta"></param>
        void FloatingTransformShift(Vector3 positionDelta);
    }
}
