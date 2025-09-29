using UnityEngine;

namespace ForYou.GamePlay
{
    public class Plankton : MonoBehaviour
    {
        [SerializeField] float Level = 1;
        public float GetLevel() { return Level; }

        [Header("Animation Settings")]
        [SerializeField] string AnimationName_Idle = "Idle";
        [SerializeField] string AnimationName_Snatched = "Snatched";

        int AnimatorNameHash_Idle;
        int AnimatorNameHash_Snatched;
        Animator ThisAnimator;
        Rigidbody2D ThisRigidbody;
        Collider2D ThisCollider;
        public bool IsSnatched { get; private set; } = false;

        private void Awake()
        {
            ThisAnimator = GetComponent<Animator>();
            AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            AnimatorNameHash_Snatched = Animator.StringToHash(AnimationName_Snatched);

            ThisRigidbody = GetComponent<Rigidbody2D>();
            ThisCollider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            ThisAnimator.Play(AnimatorNameHash_Idle);
        }

        public void OnSnatchedByPlayerFish(PlayerFish snatcher)
        {
            IsSnatched = true;

            var newParent = snatcher.SnatchedPlanktonPosition;
            transform.SetParent(newParent);
            transform.SetLocalPositionAndRotation(Vector2.zero, Quaternion.identity);

            ThisRigidbody.simulated = false;
            ThisRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            ThisRigidbody.SetRotation(0);
            ThisCollider.enabled = false;

            ThisAnimator.Play(AnimatorNameHash_Snatched);
        }

        public void OnDroppedByPlayerFish(PlayerFish dropper)
        {
            IsSnatched = false;

            transform.SetParent(null);

            ThisRigidbody.simulated = true;
            ThisRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            ThisCollider.enabled = true;

            ThisAnimator.Play(AnimatorNameHash_Idle);
        }
    }
}