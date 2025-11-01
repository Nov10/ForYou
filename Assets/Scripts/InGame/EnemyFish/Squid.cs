using Helpers;
using Unity.VisualScripting;
using UnityEngine;

namespace ForYou.GamePlay
{
    public class Squid : EnemyFish
    {
        State NowState;
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

        [SerializeField] PlayerFishDetector PlayerDetector_Attack;

        [SerializeField] float BlackFXAttackCoolDownTime;
        float LastBlackFXAttackUsedTime = 0;
        protected override void OnEnable()
        {
            base.OnEnable();
            PatrolCenterPosition = transform.position;
            SetState(State.Patrol);
            LastBlackFXAttackUsedTime = Time.time - BlackFXAttackCoolDownTime;

            PlayerDetector_Attack.OnPlayerFishDetected += OnPlayerFishInAttackRange_Attack;
            PlayerDetector_Attack.StartDetect();
        }
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
            if (state == State.Attack || state == State.Chase || state == State.Attack_BlackFX)
            {
                InGameManager.Instance.AddAsChasingEnemy(transform.GetInstanceID());
            }
            else
            {
                InGameManager.Instance.RemoveChasingEnemy(transform.GetInstanceID());
            }
            switch (NowState)
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
                case State.Attack_BlackFX:
                    {
                        //Target.OnAttackedByEnemyFish(this, true);
                        if (Time.time - LastBlackFXAttackUsedTime > BlackFXAttackCoolDownTime)
                        {
                            LastBlackFXAttackUsedTime = Time.time;
                            EndDetectAttackRange();
                            ThisAnimator.Play(AnimatorNameHash_Attack);
                            InGameManager.Instance.PlaySquidBlackFX();
                        }
                        DelayedFunctionHelper.InvokeDelayed(1.0f, () => SetState(State.Chase));
                    }
                    break;
                case State.Attack:
                    {
                        EndDetectAttackRange();
                        PlayerDetector_Attack.EndDetect();
                        ThisAnimator.Play(AnimatorNameHash_Attack);
                        Target.OnAttackedByEnemyFish(this, true);
                        DelayedFunctionHelper.InvokeDelayed(1.0f, () => SetState(State.Patrol));
                    }
                    break;
            }
            ThisAgent.speed = GetTargetSpeedByNowFishState();
        }

        public override void OnAttackedByAnemone(Anemone anemone)
        {
            SetState(State.Patrol);
            EndRecognizePlayerFish();
            DelayedFunctionHelper.InvokeDelayed(ConstValue.Delay_NotRecognizeAttackedByAnemone, StartRecognizePlayerFish);
            EndDetectAttackRange();
            DelayedFunctionHelper.InvokeDelayed(ConstValue.Delay_NotRecognizeAttackedByAnemone, StartDetectAttackRange);
        }

        public override void OnPlayerFishInAttackRange(PlayerFish fish)
        {
            if (NowState != State.Chase)
                return;

            Target = fish;
            SetState(State.Attack_BlackFX);
        }

        public void OnPlayerFishInAttackRange_Attack(PlayerFish fish)
        {
            if (NowState != State.Chase && NowState != State.Attack_BlackFX)
                return;

            Target = fish;
            SetState(State.Attack);
        }

        public override void OnRecognizePlayerFish(PlayerFish fish)
        {
            if (NowState == State.Patrol)
            {
                Target = fish;
                SetState(State.Chase);
            }
        }

        protected override float GetTargetSpeedByNowFishState()
        {
            switch (NowState)
            {
                case State.Patrol:
                    return NormalSpeed;
                case State.Chase:
                    return ChaseSpeed;
                case State.Attack_BlackFX:
                    return ChaseSpeed;
            }
            return 0;
        }

        protected override void Update()
        {
            base.Update();
            switch (NowState)
            {
                case State.Patrol:
                    {
                        if (((Vector2)(transform.position - NowTargetPosition)).sqrMagnitude < (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_NormalEnemy) * (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_NormalEnemy))
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

                        if (Vector2.Distance(transform.position, Target.transform.position) > ConstValue.DistanceThreshold_Return2Patrol)
                        {
                            SetState(State.Patrol);
                            EndRecognizePlayerFish();
                            DelayedFunctionHelper.InvokeDelayed(ConstValue.Delay_NotRecognizeAttackedByAnemone, StartRecognizePlayerFish);
                            EndDetectAttackRange();
                            DelayedFunctionHelper.InvokeDelayed(ConstValue.Delay_NotRecognizeAttackedByAnemone, StartDetectAttackRange);
                        }
                    }
                    break;
                case State.Attack_BlackFX:
                    {
                        //공격 직후에 이동에 대한 딜레이를 부여
                        //if(Time.time - LastBlackFXAttackUsedTime > BlackFXAttackCoolDownTime * 0.5f)
                        //{
                        //    NowTargetPosition = Target.transform.position;
                        //    SetDestination(NowTargetPosition);
                        //    MoveStepToDestination();
                        //    PlayAnimationByNowVelocity();
                        //    ThisAnimator.Play(AnimatorNameHash_Idle);
                        //}
                        //else
                        //{
                        //    ThisRigidbody.linearVelocity = Vector2.zero;
                        //}


                        //if (Time.time - LastBlackFXAttackUsedTime > BlackFXAttackCoolDownTime)
                        //{
                        //    LastBlackFXAttackUsedTime = Time.time;
                        //    EndDetectAttackRange();
                        //    ThisAnimator.Play(AnimatorNameHash_Attack);
                        //    InGameManager.Instance.PlaySquidBlackFX();
                        //}
                    }
                    break;
            }
        }

        void PlayAnimationByNowVelocity()
        {
            if (IsMoving)
            {
                if (NowState == State.Chase)
                    ThisAnimator.Play(AnimatorNameHash_Moving_Chase);
                else if (NowState == State.Patrol)
                    ThisAnimator.Play(AnimatorNameHash_Moving_Patrol);
                if (NowVelocity.x < 0) transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                if (NowVelocity.x > 0) transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
            }
            else
            {
                ThisAnimator.Play(AnimatorNameHash_Idle);
            }
        }

        public enum State
        {
            Patrol,
            Chase,
            Attack_BlackFX,
            Attack,
            Eaten
        }
    }
}