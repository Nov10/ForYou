using UnityEngine.AI;
using UnityEngine;

public class NavigationHelper
{
    public const float AllowDistance_Plankton = 0.3f;
    public const float AllowDistance_NormalEnemy = 0.3f;
    public const float AllowDistance_BacurdaDash = 0.5f;
    public const float AllowDistance_MorayReturn2Hide = 0.5f;


    public const float AllowDistance_BlowFish = 0.3f;
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

    public static bool RandomPointEllipse2D(Vector2 center, Vector2 radii, out Vector3 result,
                                        int attempts = 30, float sampleMaxDistance = 1.0f, int areaMask = NavMesh.AllAreas)
    {
        if (radii.x <= 0f || radii.y <= 0f)
        {
            result = new Vector3(center.x, center.y, 0f);
            return false;
        }

        for (int i = 0; i < attempts; i++)
        {
            // 단위원 내부(균일) → 타원으로 스케일
            Vector2 u = Random.insideUnitCircle;               // [-1..1] 원 내부
            Vector2 p = new Vector2(u.x * radii.x, u.y * radii.y);

            var query = new Vector3(center.x + p.x, center.y + p.y, 0f);
            if (NavMesh.SamplePosition(query, out var hit, sampleMaxDistance, areaMask))
            {
                result = hit.position;
                return true;
            }
        }

        result = new Vector3(center.x, center.y, 0f);
        return false;
    }
}