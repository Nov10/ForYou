using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForYou.Cutscene
{
    /// <summary>
    /// ���� ī�޶�(�Ǵ� ���� Transform)�� ���� ����ũ�� �����ϴ� ����.
    /// - �帮��Ʈ ����: �� ������ '���� ������ ������'�� ���� �� �������� ���ϴ� ���
    /// - ���� ����ũ �ռ�: ���ÿ� ���� ��û ����
    /// - Perlin ��� �ε巯�� ���� + ���� Ŀ��
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
            public float rotationMultiplier; // ����(Z) ��鸲 ����(�� ����)
            public Vector2 axisMask;        // (1,1)=xy ���, (1,0)=x��, (0,1)=y��
            public int seed;                // �並�� ������ �õ�
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
        /// ���� ��û(���� ����). �⺻������ Camera.main.transform�� ����
        /// </summary>
        public static void Shake(float intensity, float duration, float frequency = 25f,
                                 AnimationCurve damping = null, bool unscaledTime = false,
                                 bool affectRotation = false, float rotationMultiplier = 8f,
                                 Vector2? axisMask = null, Transform target = null)
        {
            if (target == null)
            {
                var cam = Camera.main;
                if (cam == null) { Debug.LogWarning("[CameraShaker] Camera.main �� �����ϴ�."); return; }
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
            // 1) ���� Ʈ���������� '������ ������ ������' ���� �� �帮��Ʈ ����
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

            // 2) ��� ��û�� �ջ�
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

                // Perlin ��� XY ������
                float tt = (r.unscaledTime ? Time.unscaledTime : Time.time) * r.frequency;
                float nx = (Mathf.PerlinNoise(r.seed * 0.13f, tt) * 2f - 1f) * r.axisMask.x;
                float ny = (Mathf.PerlinNoise(tt, r.seed * 0.31f) * 2f - 1f) * r.axisMask.y;

                Vector3 offset = new Vector3(nx, ny, 0f) * (r.intensity * damp);
                totalOffset += offset;

                if (r.affectRotation)
                {
                    // ȸ���� ��¦ ���� (Z��)
                    float nr = (Mathf.PerlinNoise(r.seed * 0.57f, tt * 0.8f) * 2f - 1f);
                    totalRotZ += nr * r.rotationMultiplier * damp;
                }
            }

            // 3) �� ������ ���� & ���
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
            // ������Ʈ ��Ȱ�� �� ����ġ ����
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
