using Helpers;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

namespace ForYou.GamePlay
{
    public class Moray : EnemyFish
    {
        public enum State
        {
            Hide,
            NearHole,
            Chase,
            Attack,
            Return2Hide
        }
        State NowState;

        Vector3 NowTargetPosition;
        Vector3 HidePosition;
        [SerializeField] float Hide2ChaseDelay = 1.0f;

        public bool IsHoleOnLeftWall = true;

        [Header("Animation Settings")]
        [SerializeField] string AnimationName_Idle = "Idle";
        [SerializeField] string AnimationName_Hide = "Hide";
        [SerializeField] string AnimationName_NearHole = "Hide";
        [SerializeField] string AnimationName_Attack = "Attack";
        [SerializeField] string AnimationName_Moving_Chase = "Moving";

        int AnimatorNameHash_Idle;
        int AnimatorNameHash_Hide;
        int AnimatorNameHash_NearHole;
        int AnimatorNameHash_Attack;
        int AnimatorNameHash_Moving_Chase;
        PlayerFish Target;
        protected override void Awake()
        {
            base.Awake();
            AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            AnimatorNameHash_Hide = Animator.StringToHash(AnimationName_Hide);
            AnimatorNameHash_NearHole = Animator.StringToHash(AnimationName_NearHole);
            AnimatorNameHash_Attack = Animator.StringToHash(AnimationName_Attack);
            AnimatorNameHash_Moving_Chase = Animator.StringToHash(AnimationName_Moving_Chase);
        }
        protected override void Start()
        {
            base.Start();
            HidePosition = transform.position;
            SetState(State.Hide);
        }

        protected override void Update()
        {
            base.Update();
            switch (NowState)
            {
                case State.Chase:
                    {
                        NowTargetPosition = Target.transform.position;
                        SetDestination(NowTargetPosition);
                        MoveStepToDestination();
                        PlayAnimationByNowVelocity();
                    }
                    break;
                case State.Return2Hide:
                    {
                        if (((Vector2)(transform.position - NowTargetPosition)).sqrMagnitude < (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_MorayReturn2Hide) * (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_MorayReturn2Hide))
                        {
                            SetState(State.Hide);
                            return;
                        }
                        if (((Vector2)(transform.position - NowTargetPosition)).sqrMagnitude < (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_MorayReturn2Hide * 5) * (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_MorayReturn2Hide * 5))
                        {
                            GetComponent<Collider2D>().enabled = false;
                        }
                        NowTargetPosition = HidePosition;
                        SetDestination(NowTargetPosition);
                        MoveStepToDestination();
                        PlayAnimationByNowVelocity();
                    }
                    break;
            }
        }
        void SetState(State state)
        {
            NowState = state;
            switch (NowState)
            {
                case State.Hide:
                    {
                        EndDetectAttackRange();
                        StartRecognizePlayerFish();

                        GetComponent<NavMeshAgent>().enabled = false;
                        GetComponent<Collider2D>().enabled = false;
                        if (IsHoleOnLeftWall)
                            transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 188, 0) : Quaternion.Euler(0, 0, 0);
                        else
                            transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);

                        transform.position = HidePosition;
                        ThisAnimator.Play(AnimatorNameHash_Hide);
                        ThisRigidbody.linearVelocity = Vector2.zero;
                    }
                    break;
                case State.Chase:
                    {
                        GetComponent<SpriteRenderer>().sortingLayerName = "Fish";
                        StartDetectAttackRange();
                        GetComponent<NavMeshAgent>().enabled = true;
                        GetComponent<Collider2D>().enabled = true;
                    }
                    break;
                case State.NearHole:
                    {
                        GetComponent<SpriteRenderer>().sortingLayerName = "Background";
                        ThisAnimator.Play(AnimatorNameHash_NearHole);
                        DelayedFunctionHelper.InvokeDelayed(Hide2ChaseDelay, () => SetState(State.Chase));
                        ThisRigidbody.linearVelocity = Vector2.zero;

                        GetComponent<Collider2D>().enabled = false;
                        if (IsHoleOnLeftWall)
                            transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
                        else
                            transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                    }
                    break;
                case State.Attack:
                    GetComponent<NavMeshAgent>().enabled = true;
                    GetComponent<Collider2D>().enabled = true;
                    EndDetectAttackRange();
                    ThisAnimator.Play(AnimatorNameHash_Attack);
                    Target.OnAttackedByEnemyFish(this, true);

                    DelayedFunctionHelper.InvokeDelayed(1.0f, () => SetState(State.Chase));
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
                if (NowVelocity.x < 0) transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                if (NowVelocity.x > 0) transform.rotation = IsSpriteLookLeft ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
            }
            else
            {
                ThisAnimator.Play(AnimatorNameHash_Idle);
            }
        }
        public override void OnAttackedByAnemone(Anemone anemone)
        {
            SetState(State.Return2Hide);
            EndRecognizePlayerFish();
            DelayedFunctionHelper.InvokeDelayed(3.0f, StartRecognizePlayerFish);
        }

        public override void OnPlayerFishInAttackRange(PlayerFish fish)
        {
            if (!(NowState == State.Chase))
                return;

            Target = fish;
            SetState(State.Attack);
        }

        public override void OnRecognizePlayerFish(PlayerFish fish)
        {
            if (NowState == State.Hide)
            {
                Target = fish;
                SetState(State.NearHole);
            }
        }

        protected override float GetTargetSpeedByNowFishState()
        {
            switch (NowState)
            {
                case State.Chase: return ChaseSpeed;
            }
            return NormalSpeed;
        }
    }
}