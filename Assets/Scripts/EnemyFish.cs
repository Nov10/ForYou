using UnityEngine;
using UnityEngine.InputSystem;

namespace ForYou.GamePlay
{
    public enum EnemyFishState
    {
        Patrol,
        Chase,
        Attack
    }
    public class EnemyFish : MonoBehaviour
    {
        Animator ThisAnimator;
        Rigidbody2D ThisRigidbody;
        SpriteRenderer ThisSpriteRenderer;

        [Header("Animation Settings")]
        [SerializeField] string AnimationName_Idle = "Idle";
        [SerializeField] string AnimationName_Moving = "Moving";
        [SerializeField] string AnimationName_Attack = "Attack";

        int AnimatorNameHash_Idle;
        int AnimatorNameHash_Moving;
        int AnimatorNameHash_Attack;

        EnemyFishState NowState;
        [SerializeField] PlayerFishDetector AttackRangeDetector;

        public Vector2 NowVelocity
        {
            get { return ThisRigidbody.linearVelocity; }
        }
        public bool IsMoving
        {
             get { return NowVelocity.magnitude > 0.1f; } //Velocity ±â¹Ý
        }

        private void Awake()
        {
            ThisRigidbody = GetComponent<Rigidbody2D>();

            ThisAnimator = GetComponent<Animator>();
            ThisSpriteRenderer = GetComponent<SpriteRenderer>();

            AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            AnimatorNameHash_Moving = Animator.StringToHash(AnimationName_Moving);
            AnimatorNameHash_Attack = Animator.StringToHash(AnimationName_Attack);
        }

        private void Update()
        {
            
        }

        public void OnPlayerFishDetected(PlayerFish fish)
        {

        }
    }
}