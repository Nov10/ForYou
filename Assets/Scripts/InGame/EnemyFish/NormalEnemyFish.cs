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
        [SerializeField] string AnimationName_Moving_Patrol = "Moving";
        [SerializeField] string AnimationName_Moving_Chase = "Moving";
        [SerializeField] string AnimationName_Eaten = "Eaten";

        int AnimatorNameHash_Idle;
        int AnimatorNameHash_Attack;
        int AnimatorNameHash_Moving_Patrol;
        int AnimatorNameHash_Moving_Chase;
        int AnimatorNameHash_Eaten;

        [Header("Patrol Settings")]
        [SerializeField] float PatrolRadius;
        Vector3 PatrolCenterPosition;
        Vector3 NowTargetPosition;

        protected override void Awake()
        {
            base.Awake();
            AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            AnimatorNameHash_Attack = Animator.StringToHash(AnimationName_Attack);
            AnimatorNameHash_Moving_Patrol = Animator.StringToHash(AnimationName_Moving_Patrol);
            AnimatorNameHash_Moving_Chase = Animator.StringToHash(AnimationName_Moving_Chase);
            AnimatorNameHash_Eaten = Animator.StringToHash(AnimationName_Eaten);
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

        PlayerFish Target;

        public void SetState(State state)
        {
            NowState = state;
            switch(NowState)
            {
                case State.Patrol:
                    {
                        EndDetectAttackRange();
                        NavigationHelper.RandomPoint2D(PatrolCenterPosition, PatrolRadius, out NowTargetPosition);
                        ThisAgent.destination = NowTargetPosition;
                    }
                    break;
                case State.Chase:
                    {
                        StartDetectAttackRange();
                    }
                    break;
                case State.Attack:
                    {
                        EndDetectAttackRange();
                        ThisAnimator.Play(AnimatorNameHash_Attack);
                        Target.OnAttackedByEnemyFish(this, true);
                        DelayedFunctionHelper.InvokeDelayed(1.0f, () => SetState(State.Patrol));
                    }
                    break;
            }
            ThisAgent.speed = GetTargetSpeedByNowFishState();
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
                if(NowState == State.Chase)
                    ThisAnimator.Play(AnimatorNameHash_Moving_Chase);
                else if(NowState == State.Patrol)
                    ThisAnimator.Play(AnimatorNameHash_Moving_Patrol);
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

        public override void OnRecognizePlayerFish(PlayerFish fish)
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
            EndRecognizePlayerFish();
            DelayedFunctionHelper.InvokeDelayed(3.0f, StartRecognizePlayerFish);
        }

        public override void OnPlayerFishInAttackRange(PlayerFish fish)
        {
            if (NowState != State.Chase)
                return;

            Target = fish;
            SetState(State.Attack);
        }
    }
}