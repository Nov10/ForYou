using UnityEngine;

namespace ForYou.GamePlay
{
    public enum ControlMode
    {
        Self = 0,
        Cutscene
    }
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
        float SpeedByCutsceneControl;
        public float GetNormalSpeed() { return NormalSpeed; }
        public void SetSpeedByCutscene(float speed)
        {
            SpeedByCutsceneControl = speed;
        }
        public float GetTargetSpeed()
        {
            if (NowControlMode == ControlMode.Cutscene)
                return SpeedByCutsceneControl;

            if (DoesHavePlankton) return SnatchingPlanktonSpeed;
            else return NormalSpeed;
        }
        public Vector2 GetSnapping()
        {
            return new Vector2(VelocitySnapping_Horizontal, VelocitySnapping_Vertical);
        }
        public void SetSnapping(Vector2 snap)
        {
            VelocitySnapping_Horizontal = snap.x;
            VelocitySnapping_Vertical = snap.y;
        }

        [Header("Plankton Settings")]
        [SerializeField] Transform _SnatchedPlanktonPosition;
        public Transform SnatchedPlanktonPosition { get { return _SnatchedPlanktonPosition; } }


        [Header("Plankton")]
        [SerializeField] PlanktonDetector Detector;
        public Vector2 NowVelocity
        {
            get { return ThisRigidbody.linearVelocity; }
        }
        public bool IsMoving
        {
            get 
            {
                if (NowControlMode == ControlMode.Self) return GetInputDirection().magnitude > 0.1f;
                else if (NowControlMode == ControlMode.Cutscene) return NowVelocity.sqrMagnitude > 0.1f;
                return false;
            }
        }

        public bool DoesHavePlankton
        {
            get { return SnatchedPlankton != null; }
        }
        Plankton SnatchedPlankton;

        [SerializeField] ControlMode NowControlMode;
        public void ChangeControlMode(ControlMode mode)
        {
            NowControlMode = mode;
        }

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


            if (Detector == null)
                Detector = GetComponentInChildren<PlanktonDetector>();

            Detector.OnPlanktonDetected += OnPlanktonDetected;
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


            //이번 프레임에 눌렀으면, 감지 + 플랑크톤 Snatch 시도
            if(DoesHavePlankton == false && PlayerInput.Base.SnatchPlankton.WasPressedThisFrame())
            {
                Detector.DetectOneFrame();
            }
            //버리기
            else if(DoesHavePlankton == true && PlayerInput.Base.SnatchPlankton.IsPressed() == false)
            {
                DropPlankton();
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
        public Vector2 InputDirectionByCutscene;
        Vector2 GetInputDirection()
        {
            if (NowControlMode == ControlMode.Self)
                return PlayerInput.Base.Move.ReadValue<Vector2>().normalized;
            else if (NowControlMode == ControlMode.Cutscene)
                return InputDirectionByCutscene;
            return Vector2.zero;
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

        public Plankton GetPlankton()
        {
            return SnatchedPlankton;
        }
        public void DropPlankton()
        {
            SnatchedPlankton.OnDroppedByPlayerFish(this);

            SnatchedPlankton = null;
        }
    }
}
