using UnityEngine;
namespace ForYou.GamePlay
{
    public class CameraController : MonoBehaviour
    {
        public Transform FollowTarget;
        [SerializeField] float FollowSnapping = 5;
        [SerializeField] float CameraZPosition = -10.0f;

        private void FixedUpdate()
        {
            var nowPosition = transform.position;
            var targetPosition = FollowTarget.position;

            var newPosition = Vector3.Lerp(nowPosition, targetPosition, FollowSnapping * Time.fixedDeltaTime);
            newPosition.z = CameraZPosition;

            transform.position = newPosition;
        }
    }
}