using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

namespace ForYou.GamePlay
{
    public class Plankton : MonoBehaviour
    {
        [SerializeField] float Level = 1;
        public float GetLevel() { return Level; }

        [Header("Animation Settings")]
        [SerializeField] string AnimationName_Idle = "Idle";
        [SerializeField] string AnimationName_Snatched = "Snatched";

        [Header("Patrol Radius")]
        [SerializeField] float PatrolRadius = 1;

        Vector3 StartPosition;
        Vector3 NowTargetPosition;
        const float AllowDistance = 0.3f;

        [SerializeField] float Speed = 1;
        [SerializeField] float Snapping = 5;

        [SerializeField] bool IsSpriteLookLeft = false;

        NavMeshAgent ThisAgent;


        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying == true)
                Gizmos.DrawWireSphere(StartPosition, PatrolRadius);
            else
                Gizmos.DrawWireSphere(transform.position, PatrolRadius);
#endif
        }


        int AnimatorNameHash_Idle;
        int AnimatorNameHash_Snatched;
        Animator ThisAnimator;
        Rigidbody2D ThisRigidbody;
        Collider2D ThisCollider;
        public bool IsSnatched { get; private set; } = false;

        private void Awake()
        {
            ThisAnimator = GetComponent<Animator>();
            AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            AnimatorNameHash_Snatched = Animator.StringToHash(AnimationName_Snatched);

            ThisRigidbody = GetComponent<Rigidbody2D>();
            ThisCollider = GetComponent<Collider2D>();

            ThisAgent = GetComponent<NavMeshAgent>();
            ThisAgent.updateRotation = false;
            ThisAgent.updateUpAxis = false;
            ThisAgent.updatePosition = false;

            StartPosition = transform.position;
        }

        bool RandomPoint2D(Vector2 center, float radius, out Vector3 result)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector2 rnd = center + Random.insideUnitCircle * radius;
                var query = new Vector3(rnd.x, rnd.y, 0f);
                if (NavMesh.SamplePosition(query, out var hit, 1.0f, NavMesh.AllAreas))
                { result = hit.position; return true; }
            }
            result = center;
            return false;
        }


        private void OnEnable()
        {
            ThisAnimator.Play(AnimatorNameHash_Idle);

             
            RandomPoint2D(StartPosition, PatrolRadius, out NowTargetPosition);
            ThisAgent.destination = NowTargetPosition;
        }

        private void Update()
        {
            if(IsSnatched == false)
            {
                if (ThisRigidbody.linearVelocityX < 0)
                {
                    transform.rotation = IsSpriteLookLeft == true ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    transform.rotation = IsSpriteLookLeft == true ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
                }
            }
        }

        private void FixedUpdate()
        {
            if ((transform.position - NowTargetPosition).sqrMagnitude < (ThisAgent.stoppingDistance + AllowDistance) * (ThisAgent.stoppingDistance + AllowDistance))
            {
                RandomPoint2D(StartPosition, PatrolRadius, out NowTargetPosition);
                ThisAgent.destination = NowTargetPosition;
            }

            
            Vector2 desired = ThisAgent.desiredVelocity;
            float desiredX = Mathf.Clamp(desired.x, -Speed, Speed);
            float desiredY = Mathf.Clamp(desired.y, -Speed, Speed);

            var newVelocityX = Mathf.MoveTowards(ThisRigidbody.linearVelocityX, desiredX, Snapping * Time.fixedDeltaTime);
            var newVelocityY = Mathf.MoveTowards(ThisRigidbody.linearVelocityY, desiredY, Snapping * Time.fixedDeltaTime);
            ThisRigidbody.linearVelocity = new Vector2(newVelocityX, newVelocityY);

            ThisAgent.nextPosition = ThisRigidbody.position;
        }

        public void OnSnatchedByPlayerFish(PlayerFish snatcher)
        {
            IsSnatched = true;

            var newParent = snatcher.SnatchedPlanktonPosition;
            transform.SetParent(newParent);
            transform.SetLocalPositionAndRotation(Vector2.zero, Quaternion.identity);

            ThisRigidbody.simulated = false;
            ThisRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            ThisRigidbody.SetRotation(0);
            ThisCollider.enabled = false;

            ThisAnimator.Play(AnimatorNameHash_Snatched);
        }

        public void OnDroppedByPlayerFish(PlayerFish dropper)
        {
            IsSnatched = false;

            transform.SetParent(null);

            ThisRigidbody.simulated = true;
            ThisRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            ThisCollider.enabled = true;

            ThisAnimator.Play(AnimatorNameHash_Idle);
        }
    }
}