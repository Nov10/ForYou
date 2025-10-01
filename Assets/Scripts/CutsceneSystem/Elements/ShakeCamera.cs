using UnityEngine;

namespace ForYou.Cutscene
{
    [System.Serializable]
    public class ShakeCamera : CutsceneElement
    {
        [Min(0f)] public float Intensity = 0.5f;         // 시작 진폭
        [Min(0f)] public float Duration = 0.4f;          // 지속 시간(초)
        [Min(0.01f)] public float Frequency = 25f;       // 떨림 빈도
        public bool UnscaledTime = false;                 // 일시정지 시에도 흔들려야 하면 true
        public bool AffectRotation = false;               // Z 회전도 살짝 흔들기
        public float RotationAmount = 8f;                 // 각도 흔들림 크기(도)
        public Vector2 AxisMask = new Vector2(1, 1);      // (1,1)=xy 모두, (1,0)=x만, (0,1)=y만

        // 0초에서 1로 시작해 1초에서 0으로 감쇠되는 기본 커브(감쇠 커브는 필요 시 인스펙터에서 바꾸세요)
        public AnimationCurve Damping = AnimationCurve.EaseInOut(0, 1, 1, 0);

        /// <summary>
        /// 컷신에서 호출할 함수. Camera.main 대상으로 감쇠 쉐이크 실행.
        /// </summary>
        public void Shake()
        {
            CameraShaker.Shake(
                intensity: Intensity,
                duration: Duration,
                frequency: Frequency,
                damping: Damping,
                unscaledTime: UnscaledTime,
                affectRotation: AffectRotation,
                rotationMultiplier: RotationAmount,
                axisMask: AxisMask,
                target: null // null이면 Camera.main에 적용
            );
        }
    }
}
