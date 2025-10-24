using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

namespace ForYou.GamePlay
{
    public class Plankton : MonoBehaviour
    {

        [Header("Animation Settings")]
        [SerializeField] string AnimationName_Idle = "Idle";
        [SerializeField] string AnimationName_Snatched = "Snatched";

        [Header("Patrol Radius")]
        [SerializeField] float PatrolRadius = 1;

        Vector3 StartPosition;
        Vector3 NowTargetPosition;

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

            ThisAgent.speed = Speed;
        }
        public void Start()
        {
            StartPosition = transform.position;
            ThisAnimator.Play(AnimatorNameHash_Idle);


            NavigationHelper.RandomPoint2D(StartPosition, PatrolRadius, out NowTargetPosition);
            ThisAgent.destination = NowTargetPosition;
        }

        private void OnEnable()
        {

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
            if (((Vector2)(transform.position - NowTargetPosition)).sqrMagnitude < (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_Plankton) * (ThisAgent.stoppingDistance + NavigationHelper.AllowDistance_Plankton))
            {
                NavigationHelper.RandomPoint2D(StartPosition, PatrolRadius, out NowTargetPosition);
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