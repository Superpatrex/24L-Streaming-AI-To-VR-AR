using UnityEngine;


namespace Core3lb
{
    public class DEBUG_GenerateGizmoNow : MonoBehaviour
    {
        public Color myColor = Color.red;
        public float radius;
        public bool useCuboidGizmo;
        public Vector3 boxExtends;

        public static void MakeGizmo(Vector3 postion, float radius = .3f)
        {
            GameObject holder = new GameObject("DEBUG_GizmoObject");
            holder.AddComponent<DEBUG_GenerateGizmoNow>().radius = radius;
        }

        public static void MakeCuboidGizmo(Vector3 postion, Vector3 sizeOfBox)
        {
            GameObject holder = new GameObject("DEBUG_GizmoObject");
            holder.AddComponent<DEBUG_GenerateGizmoNow>().useCuboidGizmo = true;
            holder.AddComponent<DEBUG_GenerateGizmoNow>().boxExtends = sizeOfBox;
        }

        public static void MakeSphere(Vector3 postion, float radius = .3f)
        {
            GameObject holder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            holder.name = "DEBUG_GizmoObject";
            holder.transform.position = postion;
            holder.transform.localScale = Vector3.one * radius;
        }

        public static void MakeCube(Vector3 postion, Quaternion rotation, float radius = .3f)
        {
            GameObject holder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            holder.name = "DEBUG_GizmoObject";
            holder.transform.position = postion;
            holder.transform.rotation = rotation;
            holder.transform.localScale = Vector3.one * radius;
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = myColor;
            if (useCuboidGizmo)
            {
                Gizmos.DrawWireCube(transform.position, boxExtends);
                return;
            }
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}