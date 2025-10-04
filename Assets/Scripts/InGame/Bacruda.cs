using Helpers;
using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace ForYou.GamePlay
{
    public class Bacruda : EnemyFish
    {
        public enum State
        {
            Patrol,
            Chase,
            Dash,
            Attack,
            Eaten
        }
        State NowState;

        [Header("Speed Settings")]
        [SerializeField] float DashSpeed;

        [Header("Animation Settings")]
        [SerializeField] string AnimationName_Idle = "Idle";
        [SerializeField] string AnimationName_Attack = "Attack";
        [SerializeField] string AnimationName_Moving_Patrol = "Moving";
        [SerializeField] string AnimationName_Moving_Chase = "Moving";
        [SerializeField] string AnimationName_Dash = "Dash";
        [SerializeField] string AnimationName_Eaten = "Eaten";

        int AnimatorNameHash_Idle;
        int AnimatorNameHash_Attack;
        int AnimatorNameHash_Moving_Patrol;
        int AnimatorNameHash_Moving_Chase;
        int AnimatorNameHash_Dash;
        int AnimatorNameHash_Eaten;

        PlayerFish Target;

        [Header("Patrol Settings")]
        [SerializeField] float PatrolRadius;
        Vector3 PatrolCenterPosition;
        Vector3 NowTargetPosition;

        [Header("Dash Settings")]
        [SerializeField] float DashDistance;
        [SerializeField] float DashAllowHeight;
        Vector3 DashTargetPosition;
        float DashCoolDownTime;
        float LastDashTime;

        protected override void Awake()
        {
            base.Awake();
            AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            AnimatorNameHash_Attack = Animator.StringToHash(AnimationName_Attack);
            AnimatorNameHash_Moving_Patrol = Animator.StringToHash(AnimationName_Moving_Patrol);
            AnimatorNameHash_Moving_Chase = Animator.StringToHash(AnimationName_Moving_Chase);
            AnimatorNameHash_Dash = Animator.StringToHash(AnimationName_Dash);
            AnimatorNameHash_Eaten = Animator.StringToHash(AnimationName_Eaten);
        }
        protected override void Start()
        {
            base.Start();
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


            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, 2 * new Vector3(DashDistance, DashAllowHeight, 0.5f));

            if(NowState == State.Dash)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(DashTargetPosition, 0.5f);
            }
#endif
        }
        protected override void Update()
        {
            base.Update();
            switch (NowState)
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
                        var diff = (Target.transform.position - transform.position);
                        float diffX = Mathf.Abs(diff.x);
                        float diffY = Mathf.Abs(diff.y);
                        if(DashDistance * 0.2f < diffX && diffX < DashDistance * 1.2f
                            && diffY < DashAllowHeight)
                        {
                            if(Time.time - LastDashTime > DashCoolDownTime)
                            {
                                SetState(State.Dash);
                                return;
                            }
                        }


                        NowTargetPosition = Target.transform.position;
                        SetDestination(NowTargetPosition);
                        MoveStepToDestination();
                        PlayAnimationByNowVelocity();
                    }
                    break;
                case State.Dash:
                    {
                        if ((transform.position - DashTargetPosition).sqrMagnitude < (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_BacurdaDash) * (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_BacurdaDash))
                        {
                            if (Target != null)
                                SetState(State.Chase);
                            else
                                SetState(State.Patrol);
                            return;
                        }
                        SetDestination(DashTargetPosition);
                        MoveStepToDestination();
                        ThisAnimator.Play(AnimatorNameHash_Dash);
                    }
                    break;
            }
        }

        public override void OnAttackedByAnemone(Anemone anemone)
        {
            SetState(State.Patrol);
            EndRecognizePlayerFish();
            DelayedFunctionHelper.InvokeDelayed(3.0f, StartRecognizePlayerFish);
        }

        protected override float GetTargetSpeedByNowFishState()
        {
            switch(NowState)
            {
                case State.Patrol: return NormalSpeed;
                case State.Chase: return ChaseSpeed;
                case State.Dash: return DashSpeed;
            }
            return NormalSpeed;
        }


        void SetState(State state)
        {
            NowState = state;
            switch (NowState)
            {
                case State.Patrol:
                    {
                        NavigationHelper.RandomPoint2D(PatrolCenterPosition, PatrolRadius, out NowTargetPosition);
                        ThisAgent.destination = NowTargetPosition;
                    }
                    break;
                case State.Chase:
                    break;
                case State.Dash:
                    {
                        if (Target == null) return;
                        LastDashTime = Time.time;
                        var diff = Target.transform.position - transform.position;
                        var direction = Vector3.zero;
                        if (diff.x > 0) direction = new Vector3(1, 0, 0) * DashDistance;
                        else direction = new Vector3(-1, 0, 0) * DashDistance;
                        var startDirection = direction;
                        float Multiplier = 1.0f;
                        while(NavigationHelper.RandomPoint2D(transform.position + direction, 0.1f, out DashTargetPosition) == false)
                        {
                            direction = startDirection * Multiplier;
                            Multiplier -= 0.05f;
                            if (Multiplier < 0)
                            {
                                SetState(State.Chase);
                                return;
                            }
                        }
                        
                        NowTargetPosition = DashTargetPosition;
                        ThisAgent.destination = DashTargetPosition;
                    }
                    break;
                case State.Attack:
                    break;
                case State.Eaten:
                    break;
            }
            ThisAgent.speed = GetTargetSpeedByNowFishState();
        }
        void PlayAnimationByNowVelocity()
        {
            if (IsMoving)
            {
                if (NowState == State.Chase)
                    ThisAnimator.Play(AnimatorNameHash_Moving_Chase);
                else
                    ThisAnimator.Play(AnimatorNameHash_Moving_Patrol);
                if (NowVelocity.x < 0) transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                if (NowVelocity.x > 0) transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
            }
            else
            {
                ThisAnimator.Play(AnimatorNameHash_Idle);
            }
        }
        public override void OnRecognizePlayerFish(PlayerFish fish)
        {
            if(NowState == State.Patrol)
            {
                Target = fish;
                SetState(State.Chase);
            }
        }
    }
}