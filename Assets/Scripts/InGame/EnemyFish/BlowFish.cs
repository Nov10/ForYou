using Helpers;
using UnityEngine;

namespace ForYou.GamePlay
{
    public class BlowFish : EnemyFish
    {
        State NowState;
        [Header("Animation Settings")]
        [SerializeField] string AnimationName_Idle = "Idle";
        [SerializeField] string AnimationName_Moving_Patrol = "Moving";
        [SerializeField] string AnimationName_Eaten = "Eaten";
        [SerializeField] string AnimationName_Expand = "Expand";
        [SerializeField] string AnimationName_Contract = "Contract";

        int AnimatorNameHash_Idle;
        int AnimatorNameHash_Attack;
        int AnimatorNameHash_Moving_Patrol;
        int AnimatorNameHash_Eaten;
        int AnimatorNameHash_Expand;
        int AnimatorNameHash_Contract;

        [Header("Patrol Settings")]
        [SerializeField] float PatrolRadius;
        Vector3 PatrolCenterPosition;
        Vector3 NowTargetPosition;

        [Header("Expand And Contract Settings")]
        [SerializeField] float ExpandDuration = 1.0f;
        [SerializeField] float DelayDuration = 1.0f;
        [SerializeField] float ContractDuration = 1.0f;

        [SerializeField] float ExpandAndContract_CoolDown = 3.0f;
        float LastExpandAndContractTime;
        [SerializeField] bool MoveWhileAttack = false;

        [SerializeField] float SpeedAdjustDuration = 3.0f;
        [SerializeField] float SpeedAdjustValue = 0.25f;

        protected override void Awake()
        {
            base.Awake();
            AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            AnimatorNameHash_Moving_Patrol = Animator.StringToHash(AnimationName_Moving_Patrol);
            AnimatorNameHash_Eaten = Animator.StringToHash(AnimationName_Eaten);
            AnimatorNameHash_Expand = Animator.StringToHash(AnimationName_Expand);
            AnimatorNameHash_Contract = Animator.StringToHash(AnimationName_Contract);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            PatrolCenterPosition = transform.position;
            SetState(State.Patrol);
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

        public void SetState(State state)
        {
            NowState = state;
            switch (NowState)
            {
                case State.Patrol:
                    {
                        EndDetectAttackRange();
                        NavigationHelper.RandomPoint2D(PatrolCenterPosition, PatrolRadius, out NowTargetPosition);
                        ThisAgent.destination = NowTargetPosition;
                    }
                    break;
                case State.Attack:
                    {
                        LastExpandAndContractTime = Time.time;
                        StartExpandAndContract(() =>
                        {
                            SetState(State.Patrol);
                        });
                    }
                    break;
            }
            ThisAgent.speed = GetTargetSpeedByNowFishState();
        }

        public override void OnAttackedByAnemone(Anemone anemone)
        {
            SetState(State.Patrol);
            EndRecognizePlayerFish();
            DelayedFunctionHelper.InvokeDelayed(3.0f, StartRecognizePlayerFish);
        }

        public override void OnPlayerFishInAttackRange(PlayerFish fish)
        {
            if (NowState == State.Attack)
            {
                //물고기에게 속도 딜레이 부여
                fish.SetSpeedMultiplier(ConstValue.SpeedAdjustID_BlowFish,
                    new SpeedMultipler(SpeedAdjustDuration, Time.time, SpeedAdjustValue));
            }
        }

        public override void OnRecognizePlayerFish(PlayerFish fish)
        {
            //복어는 인식 해도 할게 없음
        }

        protected override float GetTargetSpeedByNowFishState()
        {
            switch (NowState)
            {
                case State.Patrol:
                    return NormalSpeed;
                case State.Attack:
                    return NormalSpeed;
            }
            return 0;
        }

        void StartExpandAndContract(System.Action onEnd)
        {
            StartExpand(() =>
            {
                DelayedFunctionHelper.InvokeDelayed(DelayDuration, () =>
                {
                    StartContract(onEnd);
                });
            });
        }

        void StartExpand(System.Action onEnd)
        {
            StartDetectAttackRange();
            ThisAnimator.Play(AnimatorNameHash_Expand);
            DelayedFunctionHelper.InvokeDelayed(ExpandDuration, onEnd);
        }

        void StartContract(System.Action onEnd)
        {
            EndDetectAttackRange();
            ThisAnimator.Play(AnimatorNameHash_Contract);
            DelayedFunctionHelper.InvokeDelayed(ContractDuration, onEnd);
        }

        protected override void Update()
        {
            base.Update();
            switch (NowState)
            {
                case State.Patrol:
                    {
                        if ((transform.position - NowTargetPosition).sqrMagnitude < (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_BlowFish) * (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_BlowFish))
                        {
                            NavigationHelper.RandomPoint2D(PatrolCenterPosition, PatrolRadius, out NowTargetPosition);
                            ThisAgent.destination = NowTargetPosition;
                        }

                        SetDestination(NowTargetPosition);
                        MoveStepToDestination();
                        PlayAnimationByNowVelocity();
                    }
                    break;
                case State.Attack:
                    {
                        if (MoveWhileAttack == true)
                        {
                            if ((transform.position - NowTargetPosition).sqrMagnitude < (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_BlowFish) * (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_BlowFish))
                            {
                                NavigationHelper.RandomPoint2D(PatrolCenterPosition, PatrolRadius, out NowTargetPosition);
                                ThisAgent.destination = NowTargetPosition;
                            }

                            SetDestination(NowTargetPosition);
                            MoveStepToDestination();
                            //PlayAnimationByNowVelocity();
                        }
                        else
                        {
                            ThisRigidbody.linearVelocity = Vector2.zero;
                        }
                    }
                    break;
            }

            if (Time.time - (LastExpandAndContractTime + ExpandDuration + DelayDuration + ContractDuration) > ExpandAndContract_CoolDown && NowState != State.Attack)
            {
                SetState(State.Attack);
            }
        }

        void PlayAnimationByNowVelocity()
        {
            if (IsMoving)
            {
                if (NowState == State.Patrol)
                    ThisAnimator.Play(AnimatorNameHash_Moving_Patrol);
                if (NowVelocity.x < 0) transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                if (NowVelocity.x > 0) transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
            }
            else
            {
                ThisAnimator.Play(AnimatorNameHash_Idle);
            }
        }
    }

    public enum State
    {
        Patrol,
        Attack
    }
}