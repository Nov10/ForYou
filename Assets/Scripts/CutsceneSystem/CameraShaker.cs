using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForYou.Cutscene
{
    /// <summary>
    /// 메인 카메라(또는 임의 Transform)에 감쇠 쉐이크를 적용하는 러너.
    /// - 드리프트 방지: 매 프레임 '직전 적용한 오프셋'을 빼고 새 오프셋을 더하는 방식
    /// - 다중 쉐이크 합성: 동시에 여러 요청 가능
    /// - Perlin 기반 부드러운 떨림 + 감쇠 커브
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class CameraShaker : MonoBehaviour
    {
        [Serializable]
        private struct ShakeReq
        {
            public float startTime;
            public float duration;
            public float intensity;
            public float frequency;
            public AnimationCurve dampingCurve;
            public bool unscaledTime;
            public bool affectRotation;
            public float rotationMultiplier; // 각도(Z) 흔들림 배율(도 단위)
            public Vector2 axisMask;        // (1,1)=xy 모두, (1,0)=x만, (0,1)=y만
            public int seed;                // 페를린 노이즈 시드
        }

        private readonly List<ShakeReq> _reqs = new();
        private Vector3 _lastAppliedOffset = Vector3.zero;
        private float _lastAppliedRotZ = 0f;

        public static CameraShaker GetOrAdd(Transform target)
        {
            if (target == null) return null;
            var s = target.GetComponent<CameraShaker>();
            if (s == null) s = target.gameObject.AddComponent<CameraShaker>();
            return s;
        }

        /// <summary>
        /// 흔들기 요청(정적 헬퍼). 기본적으로 Camera.main.transform에 적용
        /// </summary>
        public static void Shake(float intensity, float duration, float frequency = 25f,
                                 AnimationCurve damping = null, bool unscaledTime = false,
                                 bool affectRotation = false, float rotationMultiplier = 8f,
                                 Vector2? axisMask = null, Transform target = null)
        {
            if (target == null)
            {
                var cam = Camera.main;
                if (cam == null) { Debug.LogWarning("[CameraShaker] Camera.main 이 없습니다."); return; }
                target = cam.transform;
            }

            var shaker = GetOrAdd(target);
            shaker.Enqueue(intensity, duration, frequency, damping, unscaledTime, affectRotation, rotationMultiplier, axisMask ?? new Vector2(1, 1));
        }

        public void Enqueue(float intensity, float duration, float frequency = 25f,
                            AnimationCurve damping = null, bool unscaledTime = false,
                            bool affectRotation = false, float rotationMultiplier = 8f,
                            Vector2? axisMask = null)
        {
            if (damping == null) damping = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            if (duration <= 0f || intensity <= 0f) return;

            _reqs.Add(new ShakeReq
            {
                startTime = TimeNow(unscaledTime),
                duration = duration,
                intensity = intensity,
                frequency = Mathf.Max(0.01f, frequency),
                dampingCurve = damping,
                unscaledTime = unscaledTime,
                affectRotation = affectRotation,
                rotationMultiplier = rotationMultiplier,
                axisMask = axisMask ?? new Vector2(1, 1),
                seed = UnityEngine.Random.Range(0, 999999)
            });
        }

        private void LateUpdate()
        {
            // 1) 현재 트랜스폼에서 '직전에 적용한 오프셋' 제거 → 드리프트 방지
            if (_lastAppliedOffset != Vector3.zero)
            {
                transform.localPosition -= _lastAppliedOffset;
                _lastAppliedOffset = Vector3.zero;
            }
            if (Mathf.Abs(_lastAppliedRotZ) > 0.0001f)
            {
                var e = transform.localEulerAngles;
                e.z -= _lastAppliedRotZ;
                transform.localEulerAngles = e;
                _lastAppliedRotZ = 0f;
            }

            if (_reqs.Count == 0) return;

            // 2) 모든 요청을 합산
            Vector3 totalOffset = Vector3.zero;
            float totalRotZ = 0f;

            float nowScaled = Time.time;
            float nowUnscaled = Time.unscaledTime;

            for (int i = _reqs.Count - 1; i >= 0; --i)
            {
                var r = _reqs[i];
                float now = r.unscaledTime ? nowUnscaled : nowScaled;
                float t = now - r.startTime;
                float p = Mathf.Clamp01(t / r.duration);

                if (p >= 1f)
                {
                    _reqs.RemoveAt(i);
                    continue;
                }

                float damp = Mathf.Max(0f, r.dampingCurve.Evaluate(p));
                if (damp <= 0f) continue;

                // Perlin 기반 XY 오프셋
                float tt = (r.unscaledTime ? Time.unscaledTime : Time.time) * r.frequency;
                float nx = (Mathf.PerlinNoise(r.seed * 0.13f, tt) * 2f - 1f) * r.axisMask.x;
                float ny = (Mathf.PerlinNoise(tt, r.seed * 0.31f) * 2f - 1f) * r.axisMask.y;

                Vector3 offset = new Vector3(nx, ny, 0f) * (r.intensity * damp);
                totalOffset += offset;

                if (r.affectRotation)
                {
                    // 회전도 살짝 흔들기 (Z축)
                    float nr = (Mathf.PerlinNoise(r.seed * 0.57f, tt * 0.8f) * 2f - 1f);
                    totalRotZ += nr * r.rotationMultiplier * damp;
                }
            }

            // 3) 새 오프셋 적용 & 기록
            if (totalOffset != Vector3.zero)
            {
                transform.localPosition += totalOffset;
                _lastAppliedOffset = totalOffset;
            }
            if (Mathf.Abs(totalRotZ) > 0.0001f)
            {
                var e = transform.localEulerAngles;
                e.z += totalRotZ;
                transform.localEulerAngles = e;
                _lastAppliedRotZ = totalRotZ;
            }
        }

        private float TimeNow(bool unscaled) => unscaled ? Time.unscaledTime : Time.time;

        private void OnDisable()
        {
            // 컴포넌트 비활성 시 원위치 복원
            if (_lastAppliedOffset != Vector3.zero)
            {
                transform.localPosition -= _lastAppliedOffset;
                _lastAppliedOffset = Vector3.zero;
            }
            if (Mathf.Abs(_lastAppliedRotZ) > 0.0001f)
            {
                var e = transform.localEulerAngles;
                e.z -= _lastAppliedRotZ;
                transform.localEulerAngles = e;
                _lastAppliedRotZ = 0f;
            }
        }
    }
}
