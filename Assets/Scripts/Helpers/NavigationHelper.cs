using UnityEngine.AI;
using UnityEngine;

public class NavigationHelper
{
    public const float AllowDistance_Plankton = 0.3f;
    public const float AllowDistance_NormalEnemy = 0.3f;
    public const float AllowDistance_BacurdaDash = 0.5f;
    public static bool RandomPoint2D(Vector2 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector2 rnd = center + Random.insideUnitCircle * radius;
            var query = new Vector3(rnd.x, rnd.y, 0f);
            if (NavMesh.SamplePosition(query, out var hit, 1.0f, NavMesh.AllAreas))
            { result = hit.position; return true; }
        }
        result = center;
        return false;
    }
}