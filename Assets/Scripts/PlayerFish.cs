using UnityEngine;

namespace ForYou.GamePlay
{
    public class PlayerFish : MonoBehaviour
    {
        Rigidbody2D ThisRigidbody;
        Animator ThisAnimator;
        SpriteRenderer ThisSpriteRenderer;
        [Header("Animation Settings")]
        [SerializeField] string AnimationName_Idle = "Idle";
        [SerializeField] string AnimationName_IdleWithPlankton = "IdleWithPlankton";
        [SerializeField] string AnimationName_Moving = "Moving";
        [SerializeField] string AnimationName_MovingWithPlankton = "MovingWithPlankton";

        int AnimatorNameHash_Idle;
        int AnimatorNameHash_IdleWithPlankton;
        int AnimatorNameHash_Moving;
        int AnimatorNameHash_MovingWithPlankton;

        //[SerializeField] bool FlipFlag_MovingRight;
        [SerializeField] float FlipThresholdSpeed_X = 0.5f;

        [Header("Speed Settings")]
        [SerializeField] float VelocitySnapping_Vertical = 0.1f;
        [SerializeField] float VelocitySnapping_Horizontal = 0.1f;
        [SerializeField] float NormalSpeed = 5;
        [SerializeField] float SnatchingPlanktonSpeed = 2;

        [Header("Plankton Settings")]
        [SerializeField] Transform _SnatchedPlanktonPosition;
        public Transform SnatchedPlanktonPosition { get { return _SnatchedPlanktonPosition; } }
        public Vector2 NowVelocity
        {
            get { return ThisRigidbody.linearVelocity; }
        }
        public bool IsMoving
        {
            // get { return NowVelocity.magnitude > 0.1f; } //Velocity 기반
            get { return GetInputDirection().magnitude > 0.1f; } //Input 기반
        }

        public bool DoesHavePlankton
        {
            get { return SnatchedPlankton != null; }
        }
        Plankton SnatchedPlankton;


        private void Awake()
        {
            PlayerInput = new PlayerFishInputActions();
            PlayerInput.Base.Enable();

            ThisRigidbody = GetComponent<Rigidbody2D>();

            ThisAnimator = GetComponent<Animator>();
            ThisSpriteRenderer = GetComponent<SpriteRenderer>();

            AnimatorNameHash_Idle = Animator.StringToHash(AnimationName_Idle);
            AnimatorNameHash_IdleWithPlankton = Animator.StringToHash(AnimationName_IdleWithPlankton);
            AnimatorNameHash_Moving = Animator.StringToHash(AnimationName_Moving);
            AnimatorNameHash_MovingWithPlankton = Animator.StringToHash(AnimationName_MovingWithPlankton);
        }


        private void Update()
        {
            //에니메이션
            if(IsMoving == true && DoesHavePlankton == true)
            {
                //Moving With Plankton
                ThisAnimator.Play(AnimatorNameHash_MovingWithPlankton);
            }
            else if (IsMoving == true && DoesHavePlankton == false)
            {
                //Moving
                ThisAnimator.Play(AnimatorNameHash_Moving);
            }
            else if (IsMoving == false && DoesHavePlankton == true)
            {
                //Idle With Plankton
                ThisAnimator.Play(AnimatorNameHash_IdleWithPlankton);
            }
            else if (IsMoving == false && DoesHavePlankton == false)
            {
                //Idle
                ThisAnimator.Play(AnimatorNameHash_Idle);
            }

            //Flip Settings
            var nowVelocity = NowVelocity;
            if(nowVelocity.x > FlipThresholdSpeed_X)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
                //ThisSpriteRenderer.flipX = FlipFlag_MovingRight;
            }
            else if (nowVelocity.x < -FlipThresholdSpeed_X)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
                //ThisSpriteRenderer.flipX = !FlipFlag_MovingRight;
            }



            /////////////////////////////////////////////
            ///시험용 : 플랑크톤 버리기
            if(DoesHavePlankton == true && Input.GetKeyDown(KeyCode.F))
            {
                SnatchedPlankton.OnDroppedByPlayerFish(this);

                SnatchedPlankton = null;
            }
        }


        private void FixedUpdate()
        {
            var targetVelocity = CalculateTargetVelocity();
            var nowVelocity = ThisRigidbody.linearVelocity;

            var dt = Time.fixedDeltaTime;
            var finalVelocity = Vector2.zero;
            finalVelocity.x = Mathf.Lerp(nowVelocity.x, targetVelocity.x, VelocitySnapping_Horizontal * dt);
            finalVelocity.y = Mathf.Lerp(nowVelocity.y, targetVelocity.y, VelocitySnapping_Vertical * dt);

            ThisRigidbody.linearVelocity = finalVelocity;
        }



        Vector2 CalculateTargetVelocity()
        {
            var direction = GetInputDirection();
            float speed = NormalSpeed;
            if(DoesHavePlankton == true)
            {
                speed = SnatchingPlanktonSpeed;
            }
            else
            {
                speed = NormalSpeed;
            }
            return direction * speed;
        }


        public static PlayerFishInputActions PlayerInput;
        Vector2 GetInputDirection()
        {
            return PlayerInput.Base.Move.ReadValue<Vector2>().normalized;
        }



        public void OnPlanktonDetected(Plankton plankton)
        {
            if (DoesHavePlankton)
                return;

            OnSnatchingPlankton(plankton);
            plankton.OnSnatchedByPlayerFish(this);
        }


        void OnSnatchingPlankton(Plankton plankton)
        {
            SnatchedPlankton = plankton;
        }
    }
}
