using Helpers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ForYou.GamePlay
{
    public class SquidFX : MonoBehaviour
    {
        [SerializeField] Sprite[] Images;
        [SerializeField] RectTransform Prefab;
        [SerializeField] Vector2 VFXShowedScreenRatio = new Vector2(1, 1);

        [SerializeField] int WidthCellCount = 3;
        [SerializeField] int HeightCellCount = 2;
        [SerializeField] float MaxRadius = 1.0f;
        [SerializeField] float Duration = 2.0f;

        public void Play()
        {
            float width = (float)(Screen.width * VFXShowedScreenRatio.x);
            float height = (float)(Screen.height * VFXShowedScreenRatio.y);


            float cellX = width / WidthCellCount;
            float cellY = height / HeightCellCount;

            float offsetX = Screen.width - width;
            float offsetY = Screen.height - height;

            for(int x = 0; x < WidthCellCount; x++)
            {
                for(int y = 0; y < HeightCellCount; y++)
                {
                    float r = MaxRadius * UnityEngine.Random.value;
                    float theta = 360.0f * UnityEngine.Random.value;
                    float rndX = (r * Mathf.Cos(theta * Mathf.Deg2Rad)) * cellX;
                    float rndY = (r * Mathf.Sin(theta * Mathf.Deg2Rad)) * cellY;

                    var position_lefttop
                        = new Vector2((x+0.5f) * cellX + rndX + offsetX, (y+0.5f) * cellY + rndY + offsetY);

                    var position_center = position_lefttop - 0.5f * new Vector2(Screen.width, Screen.height);

                    var g = CreateSingleFXObject(Images[0], position_center);
                    g.transform.localScale = Vector3.zero;
                    g.transform.rotation = Quaternion.Euler(0, 0, 360.0f * UnityEngine.Random.value);

                    ObjectMoveHelper.ScaleObject(g.transform, Vector3.one * (0.9f + 0.1f * UnityEngine.Random.value), 0.2f * (0.9f + 0.1f *UnityEngine.Random.value));
                    DelayedFunctionHelper.InvokeDelayed(Duration, () =>
                    {
                        g.GetComponent<Image>().CrossFadeAlpha(0.0f, 0.5f * (0.9f + 0.1f * UnityEngine.Random.value), false);
                    });
                }
            }

        }

        GameObject CreateSingleFXObject(Sprite sprite, Vector2 position)
        {
            var g = Instantiate(Prefab, transform);
            g.GetComponent<RectTransform>().anchoredPosition = position;
            g.GetComponent<Image>().sprite = sprite;

            return g.gameObject;
        }

#if UNITY_EDITOR
        [SerializeField] bool DrawGizmo = false;
        [SerializeField, Min(8)] int CircleSegments = 48; // 원(타원) 근사 세그먼트 수
        void OnDrawGizmos()
        {
            if (DrawGizmo == false)
                return;
            var canvas = GetComponentInParent<Canvas>();
            var cam = Camera.main;
            if (canvas == null || cam == null) return;

            int ScreenWidth = 1920;
            int ScreenHeight = 1080;
            // 원본 Play()와 동일한 좌표계/범위를 사용 (0~width, 0~height 스크린 기준)
            float width = (float)(ScreenWidth * VFXShowedScreenRatio.x);
            float height = (float)(ScreenHeight * VFXShowedScreenRatio.y);
            float cellX = width / WidthCellCount;
            float cellY = height / HeightCellCount;

            float offsetX = ScreenWidth - width;
            float offsetY = ScreenHeight - height;

            // 1) 외곽 경계(채울 전체 영역) 사각형
            DrawRect(canvas, cam, offsetX, offsetY, offsetX + width, offsetY + height);

            // 2) 셀 경계(Grid)
            for (int cx = 0; cx <= WidthCellCount; cx++)
            {
                float x = cx * cellX;
                ScreenGizmos.DrawLine(canvas, cam, new Vector2(x, 0f), new Vector2(x, height));
            }
            for (int cy = 0; cy <= HeightCellCount; cy++)
            {
                float y = cy * cellY;
                ScreenGizmos.DrawLine(canvas, cam, new Vector2(0f, y), new Vector2(width, y));
            }

            // 3) 각 셀의 "원" 범위 (실제 분포는 타원임: rndX는 cellX, rndY는 cellY로 스케일되기 때문)
            //    r == MaxRadius 일 때의 경계 타원을 그려 샘플링 최대 반경을 시각화
            float rx = MaxRadius * cellX;
            float ry = MaxRadius * cellY;

            for (int x = 0; x < WidthCellCount; x++)
            {
                for (int y = 0; y < HeightCellCount; y++)
                {
                    Vector2 center = new Vector2((x + 0.5f) * cellX + offsetX, (y + 0.5f) * cellY + offsetY);
                    DrawEllipse(canvas, cam, center, rx, ry, CircleSegments);

                    // 만약 ‘진짜 원(원형)’으로 보고 싶다면 아래 한 줄로 교체:
                    // float r = MaxRadius * Mathf.Min(cellX, cellY);
                    // DrawEllipse(canvas, cam, center, r, r, CircleSegments);
                }
            }
        }

        void DrawRect(Canvas canvas, Camera cam, float x0, float y0, float x1, float y1)
        {
            Vector2 a = new Vector2(x0, y0);
            Vector2 b = new Vector2(x1, y0);
            Vector2 c = new Vector2(x1, y1);
            Vector2 d = new Vector2(x0, y1);
            ScreenGizmos.DrawLine(canvas, cam, a, b);
            ScreenGizmos.DrawLine(canvas, cam, b, c);
            ScreenGizmos.DrawLine(canvas, cam, c, d);
            ScreenGizmos.DrawLine(canvas, cam, d, a);
        }

        void DrawEllipse(Canvas canvas, Camera cam, Vector2 center, float rx, float ry, int segments)
        {
            if (segments < 3) segments = 3;
            float step = Mathf.PI * 2f / segments;
            Vector2 prev = center + new Vector2(rx, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float ang = step * i;
                Vector2 curr = center + new Vector2(Mathf.Cos(ang) * rx, Mathf.Sin(ang) * ry);
                ScreenGizmos.DrawLine(canvas, cam, prev, curr);
                prev = curr;
            }
        }
#endif
    }
}