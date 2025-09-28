

using UnityEngine;

namespace Helpers
{
    public class TransformHelper
    {
        public static void RemoveAllChildren(Transform parent)
        {
            int childCount = parent.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject.Destroy(parent.GetChild(i).gameObject);
            }
        }

        public static int FindNearestTransform(Transform[] positions, Vector3 from)
        {
            float minDistance = float.MaxValue;
            int nearestIndex = -1;
            for (int i = 0; i < positions.Length; i++)
            {
                float distance = (positions[i].position-  from).sqrMagnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }
            return nearestIndex;
        }
    }
}