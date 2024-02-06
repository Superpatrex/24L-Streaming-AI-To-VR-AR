#if SSC_ENTITIES
using Unity.Entities;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    public class DOTSHelper
    {
        /// <summary>
        /// Get the default World in which to instantiate Entities.
        /// USAGE: World sscWorld = DOTSHelper.GetDefaultWorld();
        /// </summary>
        /// <returns></returns>
        public static World GetDefaultWorld()
        {
            #if UNITY_2019_3_OR_NEWER || UNITY_ENTITIES_0_2_0_OR_NEWER
            // Entities 0.2.0+ in U2019.3+
            return World.DefaultGameObjectInjectionWorld;
            #else
            // Entities 0.012-preview.33 - 0.1.1
            return World.Active;
            #endif
        }

        #if SSC_PHYSICS
        /// <summary>
        /// Typically called once to get a reference to the BuildPhysicsWorld for a given World.
        /// USAGE: Unity.Physics.Systems.BuildPhysicsWorld buildPhysicsWorld;
        /// DOTSHelper.GetBuildPhysicsWorld(DOTSHelper.GetDefaultWorld(), ref buildPhysicsWorld);
        /// </summary>
        /// <param name="world"></param>
        /// <param name="buildPhysicsWorld"></param>
        /// <returns></returns>
        public static bool GetBuildPhysicsWorld(World world, ref Unity.Physics.Systems.BuildPhysicsWorld buildPhysicsWorld)
        {
            if (world == null) { return false; }
            else
            {
                buildPhysicsWorld = world.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
                return true;
            }
        }

        #endif

    }
}
#endif