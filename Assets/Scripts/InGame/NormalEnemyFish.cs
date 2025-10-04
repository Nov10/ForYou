using Helpers;
using UnityEngine;

namespace ForYou.GamePlay
{
    public class NormalEnemyFish : EnemyFish
    {
        public enum State
        {
            Patrol,
            Chase,
            Attack,
            Eaten
        }
        [SerializeField] State NowState;

        [Header("Animation Settings")]
        [SerializeField] string AnimationName_Idle = "Idle";
        [SerializeField] string AnimationName_Attack = "Attack";
        [SerializeField] string AnimationName_Moving = "Moving";
        [SerializeField] string AnimationName_Eaten = "Eaten";

        int AnimatorNameHash_Idle;
        int AnimatorNameHash_Attack;
        int AnimatorNameHash_Moving;
        int AnimatorNameHash_Eaten;

        [Header("Patrol Settings")]
        [SerializeField] float PatrolRadius;
        Vector3 PatrolCenterPosition;
        Vector3 NowTargetPosition;

        protected override void Awake()
        {
            base.Awake();
            Patrol2ChaseRange.OnPlayerFishDetected += OnPlayerFishDetected_Patrol2ChaseRange;

            AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            AnimatorNameHash_Attack = Animator.StringToHash(AnimationName_Attack);
            AnimatorNameHash_Moving = Animator.StringToHash(AnimationName_Moving);
            AnimatorNameHash_Eaten = Animator.StringToHash(AnimationName_Eaten);
        }

        private void OnEnable()
        {
            Patrol2ChaseRange.StartDetect();
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying == true)
                Gizmos.DrawWireSphere(PatrolCenterPosition, PatrolRadius);
            else
                Gizmos.DrawWireSphere(transform.position, PatrolRadius);
#endif
        }

        [SerializeField] PlayerFishDetector Patrol2ChaseRange;

        PlayerFish Target;

        public void SetState(State state)
        {
            switch(NowState)
            {
                case State.Patrol:
                    break;
            }
            NowState = state;
            switch(NowState)
            {
                case State.Patrol:
                    {
                        NavigationHelper.RandomPoint2D(PatrolCenterPosition, PatrolRadius, out NowTargetPosition);
                        ThisAgent.destination = NowTargetPosition;
                    }
                    break;
            }
        }
        protected override float GetTargetSpeedByNowFishState()
        {
            switch(NowState)
            {
                case State.Patrol:
                    return NormalSpeed;
                case State.Chase:
                    return ChaseSpeed;
            }
            return 0;
        }
        protected override void Update()
        {
            base.Update();
            switch(NowState)
            {
                case State.Patrol:
                    {
                        if ((transform.position - NowTargetPosition).sqrMagnitude < (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_NormalEnemy) * (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_NormalEnemy))
                        {
                            NavigationHelper.RandomPoint2D(PatrolCenterPosition, PatrolRadius, out NowTargetPosition);
                            ThisAgent.destination = NowTargetPosition;
                        }

                        SetDestination(NowTargetPosition);
                        MoveStepToDestination();
                        PlayAnimationByNowVelocity();
                    }
                    break;
                case State.Chase:
                    {
                        NowTargetPosition = Target.transform.position;
                        SetDestination(NowTargetPosition);
                        MoveStepToDestination();
                        PlayAnimationByNowVelocity();
                    }
                    break;
            }
        }

        void PlayAnimationByNowVelocity()
        {
            if (IsMoving)
            {
                ThisAnimator.Play(AnimatorNameHash_Moving);
                if (NowVelocity.x < 0) transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                if (NowVelocity.x > 0) transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
            }
            else
            {
                ThisAnimator.Play(AnimatorNameHash_Idle);
            }
        }

        protected override void Start()
        {
            base.Start();
            PatrolCenterPosition = transform.position;
            SetState(State.Patrol);
        }
        void OnPlayerFishDetected_Patrol2ChaseRange(PlayerFish fish)
        {
            if (NowState == State.Patrol)
            {
                SetState(State.Chase);
                Target = fish;
            }
        }
        public override void OnAttackedByAnemone(Anemone anemone)
        {
            SetState(State.Patrol);
            Patrol2ChaseRange.EndDetect();
            DelayedFunctionHelper.InvokeDelayed(3.0f, () => Patrol2ChaseRange.StartDetect());
        }
    }
}