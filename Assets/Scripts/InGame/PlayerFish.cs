using Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForYou.GamePlay
{
    public enum ControlMode
    {
        Self = 0,
        Cutscene
    }

    public class SpeedMultipler
    {
        public float Duration;
        public float StartTime;
        public float Value;

        public SpeedMultipler(float duration, float startTime, float value)
        {
            Duration = duration;
            StartTime = startTime;
            Value = value;
        }
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
        [SerializeField] string AnimationName_Die = "Moving";
        public enum RotationMode
        {
            Sprtie,
            Transform
        }
        public RotationMode RotateMode;

        int AnimatorNameHash_Idle;
        int AnimatorNameHash_IdleWithPlankton;
        int AnimatorNameHash_Moving;
        int AnimatorNameHash_MovingWithPlankton;
        int AnimatorNameHash_Die;

        //[SerializeField] bool FlipFlag_MovingRight;
        [SerializeField] float FlipThresholdSpeed_X = 0.5f;

        [Header("Speed Settings")]
        [SerializeField] float VelocitySnapping_Vertical = 0.1f;
        [SerializeField] float VelocitySnapping_Horizontal = 0.1f;
        [SerializeField] float NormalSpeed = 5;
        [SerializeField] float SnatchingPlanktonSpeed = 2;
        float SpeedByCutsceneControl;

        Dictionary<int, SpeedMultipler> SpeedMultipliersLastTimes = new Dictionary<int, SpeedMultipler>();
        public float GetNormalSpeed() { return NormalSpeed; }

        public void SetSpeedMultiplier(int id, SpeedMultipler data)
        {
            SpeedMultipliersLastTimes[id] = data;
        }

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
        public bool PreventDropPlankton;
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
            AnimatorNameHash_Die = Animator.StringToHash(AnimationName_Die);

            if (Detector == null)
                Detector = GetComponentInChildren<PlanktonDetector>();

            Detector.OnPlanktonDetected += OnPlanktonDetected;
        }

        private void OnDestroy()
        {
            PlayerInput.Disable();
        }

        private void Update()
        {
            if (InGameManager.Instance.IsGameOver == true)
                return;

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


            var targetVelocity = CalculateTargetVelocity();
            Quaternion targetRotation = Quaternion.identity;
            if (RotateMode == RotationMode.Sprtie)
            {
                if (targetVelocity.y > 0.1f)
                {
                    ThisAnimator.SetFloat("MoveY", 1);
                }
                else if (targetVelocity.y < -0.1f)
                {
                    ThisAnimator.SetFloat("MoveY", 0);
                }
                else
                {
                    ThisAnimator.SetFloat("MoveY", 0.5f);
                }
            }
            else
            {
                ThisAnimator.SetFloat("MoveY", 0.5f);
                if (targetVelocity.y > 0.1f)
                {
                    targetRotation = Quaternion.Euler(0, 0, -45f) * targetRotation;
                    //targetRotation.z = -45f;
                }
                else if (targetVelocity.y < -0.1f)
                {
                    targetRotation = Quaternion.Euler(0, 0, 45f) * targetRotation;
                    //targetRotation.z = 45f;
                }
                else
                {
                    targetRotation = Quaternion.Euler(0, 0, 0f) * targetRotation;
                    //targetRotation.z = 0f;
                }
            }

            //Flip Settings
            var nowVelocity = NowVelocity;
            var rot = transform.rotation;
            if (nowVelocity.x > FlipThresholdSpeed_X)
            {
                targetRotation = Quaternion.Euler(0, 180, 0) * targetRotation;
                //targetRotation.y = 180;
                //transform.rotation = Quaternion.Euler(rot.x, 180, targetAngle);
                //ThisSpriteRenderer.flipX = FlipFlag_MovingRight;
            }
            else if (nowVelocity.x < -FlipThresholdSpeed_X)
            {
                targetRotation = Quaternion.Euler(0, 0, 0) * targetRotation;
                //targetRotation.y = 0;
                //transform.rotation = Quaternion.Euler(rot.x, 0, targetAngle);
                //ThisSpriteRenderer.flipX = !FlipFlag_MovingRight;
            }
            else
            {
                targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0) * targetRotation;
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, targetRotation.eulerAngles.y, transform.eulerAngles.z);


            //이번 프레임에 눌렀으면, 감지 + 플랑크톤 Snatch 시도
            if (DoesHavePlankton == false && PlayerInput.Base.SnatchPlankton.WasPressedThisFrame())
            {
                Detector.DetectOneFrame();
            }
            //버리기
            else if (DoesHavePlankton == true && PlayerInput.Base.SnatchPlankton.IsPressed() == false && PreventDropPlankton == false)
            {
                DropPlankton();
            }
        }


        private void FixedUpdate()
        {
            if (IsForcedByAnemone == true)
                return;
            var targetVelocity = CalculateTargetVelocity();
            var nowVelocity = ThisRigidbody.linearVelocity;

            var dt = Time.fixedDeltaTime;
            var finalVelocity = Vector2.zero;
            finalVelocity.x = Mathf.Lerp(nowVelocity.x, targetVelocity.x, VelocitySnapping_Horizontal * dt);
            finalVelocity.y = Mathf.Lerp(nowVelocity.y, targetVelocity.y, VelocitySnapping_Vertical * dt);

            ThisRigidbody.linearVelocity = finalVelocity;
        }

        bool IsForcedByAnemone;
        public void AddForceByAnemone(Vector2 force, float duration)
        {
            IsForcedByAnemone = true;
            DelayedFunctionHelper.InvokeDelayed(duration, () => IsForcedByAnemone = false);

            ThisRigidbody.AddForce(force, ForceMode2D.Impulse);
        }

        Vector2 CalculateTargetVelocity()
        {
            var direction = GetInputDirection();
            float speed = NormalSpeed;
            if(DoesHavePlankton == true)
            {
                speed = (SnatchingPlanktonSpeed);
            }
            else
            {
                speed = (NormalSpeed);
            }
            return direction * speed * GetSpeedMultipler();
        }


        float GetSpeedMultipler()
        {
            float multipler = 1;
            List<int> indices2Removed = new List<int>();
            foreach(var v in SpeedMultipliersLastTimes)
            {
                var data = v.Value;
                if(Time.time - data.StartTime < data.Duration)
                {
                    multipler = multipler * data.Value;
                }
                else
                {
                    indices2Removed.Add(v.Key);
                }
            }

            foreach(var k in indices2Removed)
            {
                SpeedMultipliersLastTimes.Remove(k);
            }


            return multipler;
        }


        static PlayerFishInputActions PlayerInput;
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


        public void OnAttackedByEnemyFish(EnemyFish enemyFish, bool shouldPlayerFishDie)
        {
            if(shouldPlayerFishDie)
            {
                //return;
                DelayedFunctionHelper.InvokeDelayed(2.0f, () => InGameManager.Instance.GameOver());

                PlayerInput.Disable();
                ThisAnimator.Play(AnimatorNameHash_Die);
                DelayedFunctionHelper.InvokeDelayed(1.0f, () => gameObject.SetActive(false));
            }
            else
            {
                //...
            }
        }
    }
}
