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
        public bool IsSnatched { get; private set; } = false;

        private void Awake()
        {
            ThisAnimator = GetComponent<Animator>();
            AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            AnimatorNameHash_Snatched = Animator.StringToHash(AnimationName_Snatched);
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

            if (TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.simulated = false;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                rb.SetRotation(0);
            }

            ThisAnimator.Play(AnimatorNameHash_Snatched);
        }

        public void OnDroppedByPlayerFish(PlayerFish dropper)
        {
            IsSnatched = false;

            transform.SetParent(null);
            ThisAnimator.Play(AnimatorNameHash_Idle);
            if (TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.simulated = true;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }
}