using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace ForYou.GamePlay
{
    public abstract class EnemyFish : MonoBehaviour
    {
        protected Animator ThisAnimator { get; private set; }
        protected Rigidbody2D ThisRigidbody { get; private set; }
        SpriteRenderer ThisSpriteRenderer;
        protected NavMeshAgent ThisAgent { get; private set; }

        //[Header("Animation Settings")]
        //[SerializeField] string AnimationName_Idle = "Idle";
        //[SerializeField] string AnimationName_Moving = "Moving";
        //[SerializeField] string AnimationName_Attack = "Attack";

        //int AnimatorNameHash_Idle;
        //int AnimatorNameHash_Moving;
        //int AnimatorNameHash_Attack;

        //EnemyFishState NowState;


        [Header("Movement Settings")]
        [SerializeField] protected float NormalSpeed = 0.5f;
        [SerializeField] protected float ChaseSpeed = 0.6f;
        [SerializeField] float Snapping = 5.0f;

        [SerializeField] bool _IsSpriteLookLeft = false;
        public bool IsSpriteLookLeft => _IsSpriteLookLeft;

        public Vector2 NowVelocity
        {
            get { return ThisRigidbody.linearVelocity; }
        }
        public bool IsMoving
        {
             get { return NowVelocity.magnitude > 0.1f; } //Velocity ���
        }

        protected virtual void Awake()
        {
            ThisRigidbody = GetComponent<Rigidbody2D>();

            ThisAnimator = GetComponent<Animator>();
            ThisSpriteRenderer = GetComponent<SpriteRenderer>();

            //AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            //AnimatorNameHash_Moving = Animator.StringToHash(AnimationName_Moving);
            //AnimatorNameHash_Attack = Animator.StringToHash(AnimationName_Attack);

            ThisAgent = GetComponent<NavMeshAgent>();
            ThisAgent.updateRotation = false;
            ThisAgent.updateUpAxis = false;
            ThisAgent.updatePosition = false;
        }
        protected virtual void Start()
        {

        }
        protected virtual void Update()
        {

        }
        protected void MoveStepToDestination()
        {
            float speed = GetTargetSpeedByNowFishState();
            Vector2 desired = ThisAgent.desiredVelocity;
            float desiredX = Mathf.Clamp(desired.x, -speed, speed);
            float desiredY = Mathf.Clamp(desired.y, -speed, speed);

            var newVelocityX = Mathf.MoveTowards(ThisRigidbody.linearVelocityX, desiredX, Snapping * Time.fixedDeltaTime);
            var newVelocityY = Mathf.MoveTowards(ThisRigidbody.linearVelocityY, desiredY, Snapping * Time.fixedDeltaTime);
            ThisRigidbody.linearVelocity = new Vector2(newVelocityX, newVelocityY);

            ThisAgent.nextPosition = ThisRigidbody.position;
        }
        protected void SetDestination(Vector3 position)
        {
            ThisAgent.SetDestination(position);
        }

        protected abstract float GetTargetSpeedByNowFishState();

        public abstract void OnAttackedByAnemone(Anemone anemone);
    }
}