using Unity.VisualScripting;
using UnityEngine;

namespace ForYou.GamePlay
{
    public class JellyFish : EnemyFish
    {
        [SerializeField] float AttackDuration;
        [SerializeField] bool MoveWhileAttack;
        [SerializeField] float AttackCoolDownTime;
        [SerializeField] float AdjustSpeedDuration;
        [SerializeField] float AdjustSpeedValue;
        float LastTime_Attack = 0f;
        [SerializeField] string AnimationName_Idle = "Idle";
        [SerializeField] string AnimationName_Attack = "Attack";
        [SerializeField] string AnimationName_Moving_Patrol = "Moving";
        int AnimatorNameHash_Attack;
        int AnimatorNameHash_Idle;
        int AnimatorNameHash_Moving_Patrol;

        [Header("Patrol Settings")]
        [SerializeField] Vector2 PatrolRadius;
        Vector3 PatrolCenterPosition;
        Vector3 NowTargetPosition;

        bool IsAttacking
        {
            get
            {
                if (Time.time - LastTime_Attack < AttackDuration) return true;
                return false;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            AnimatorNameHash_Attack = Animator.StringToHash(AnimationName_Attack);
            AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            AnimatorNameHash_Moving_Patrol = Animator.StringToHash(AnimationName_Moving_Patrol);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            PatrolCenterPosition = transform.position;
            NowTargetPosition = PatrolCenterPosition;

            LastTime_Attack = Time.time - AttackCoolDownTime;
            StartDetectAttackRange();
        }
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying == true)
                DrawEllipse(PatrolCenterPosition, PatrolRadius);
            else
                DrawEllipse(transform.position, PatrolRadius);
#endif
        }
        public static void DrawEllipse(Vector2 center, Vector2 radii, float angleDeg = 0f, int segments = 64, float z = 0f)
        {
            if (radii.x <= 0f || radii.y <= 0f) return;
            segments = Mathf.Max(8, segments);

            // 기존 행렬 보관 후: (이동 * 회전 * 스케일)로 단위원을 타원으로 변환
            Matrix4x4 prev = Gizmos.matrix;
            Gizmos.matrix =
                Matrix4x4.TRS(new Vector3(center.x, center.y, z),
                              Quaternion.Euler(0f, 0f, angleDeg),
                              new Vector3(radii.x, radii.y, 1f));

            // 단위원(반지름=1) 상의 점들을 폴리라인으로 연결 → 행렬에 의해 타원으로 렌더됨
            float step = Mathf.PI * 2f / segments;
            Vector3 prevPt = new Vector3(Mathf.Cos(0f), Mathf.Sin(0f), 0f);

            for (int i = 1; i <= segments; i++)
            {
                float t = step * i;
                Vector3 currPt = new Vector3(Mathf.Cos(t), Mathf.Sin(t), 0f);
                Gizmos.DrawLine(prevPt, currPt);
                prevPt = currPt;
            }

            // 행렬 복원
            Gizmos.matrix = prev;
        }
        public override void OnAttackedByAnemone(Anemone anemone)
        {
            //공격받지 않음
        }

        public override void OnPlayerFishInAttackRange(PlayerFish fish)
        {
            if (Time.time - LastTime_Attack < AttackCoolDownTime)
                return;

            ThisAnimator.Play(AnimatorNameHash_Attack);
            LastTime_Attack = Time.time;
            InGameManager.Instance.PlayJellyFishBlurFX(AdjustSpeedDuration);
            fish.SetSpeedMultiplier(ConstValue.SpeedAdjustID_JellyFish, new SpeedMultipler(AdjustSpeedDuration, Time.time, AdjustSpeedValue));
        }
        protected override void Update()
        {
            base.Update();
            if (IsAttacking == false || (IsAttacking == true && MoveWhileAttack == true))
            {
                if (((Vector2)(transform.position - NowTargetPosition)).sqrMagnitude < (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_BlowFish) * (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_BlowFish))
                {
                    NavigationHelper.RandomPointEllipse2D(PatrolCenterPosition, PatrolRadius, out NowTargetPosition);
                    ThisAgent.destination = NowTargetPosition;
                }
                SetDestination(NowTargetPosition);
                MoveStepToDestination();
                if (IsAttacking == false)
                    PlayAnimationByNowVelocity();
            }
        }
        void PlayAnimationByNowVelocity()
        {
            if (IsMoving)
            {
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
        }

        protected override float GetTargetSpeedByNowFishState()
        {
            return NormalSpeed;
        }
    }
}