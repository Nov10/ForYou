using Helpers;
using System.Collections;
using UnityEngine;
namespace ForYou.GamePlay
{
    public class CameraController : MonoBehaviour
    {
        public Transform FollowTarget;
        [SerializeField] Transform OffsetObject;
        int OffsetID;
        [SerializeField] public float FollowSnapping = 5;
        [SerializeField] float CameraZPosition = -10.0f;

        public bool Follow = true;

        [SerializeField] bool StartZoomEffect = false;
        [SerializeField] float CameraSize = 7.0f;
        private void Awake()
        {
            if (StartZoomEffect == true)
            {
                Follow = false;
                DelayedFunctionHelper.InvokeDelayed(3.0f, () =>
                {
                    StartCoroutine(_ChangeCameraSize(CameraSize, 2.0f));
                    Follow = true;
                    FollowSnapping = 2.0f;
                    DelayedFunctionHelper.InvokeDelayed(2.0f, () =>
                    {
                        FollowSnapping = 5.0f;
                    });
                });
            }
            else
            {
                GetComponentInChildren<Camera>().orthographicSize = CameraSize;
            }
        }

        IEnumerator _ChangeCameraSize(float targetSize, float duration)
        {
            var camera = GetComponentInChildren<Camera>();
            var nowSize = camera.orthographicSize;
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                //var newSize = Mathf.Lerp(nowSize, targetSize, elapsed / duration);

                //그냥 lerp는 좀 단순하니까...
                var t = elapsed / duration;
                t = t * t * (3f - 2f * t); //smoothstep
                var newSize = Mathf.Lerp(nowSize, targetSize, t);
                camera.orthographicSize = newSize;
                yield return new WaitForFixedUpdate();
            }
            camera.orthographicSize = targetSize;
        }

        private void FixedUpdate()
        {
            if (Follow == false)
                return;
            var nowPosition = transform.position;
            var targetPosition = FollowTarget.position;

            var newPosition = Vector3.Lerp(nowPosition, targetPosition, FollowSnapping * Time.fixedDeltaTime);
            newPosition.z = CameraZPosition;

            transform.position = newPosition;
        }

        public Vector2 GetOffset()
        {
            return OffsetObject.position;
        }
        public void SetOffset(Vector2 offset, float duration)
        {
            ObjectMoveHelper.TryStop(OffsetID);

            if(duration == 0.0f)
            {
                OffsetObject.localPosition = offset;
                return;
            }

            OffsetID = ObjectMoveHelper.MoveObjectSmooth_FixedUpdate(OffsetObject, offset, duration, ePosition.Local);
        }
    }
}