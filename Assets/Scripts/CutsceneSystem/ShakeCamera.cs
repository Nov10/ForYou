using UnityEngine;

namespace ForYou.Cutscene
{
    [System.Serializable]
    public class ShakeCamera : CutsceneElement
    {
        [Min(0f)] public float Intensity = 0.5f;         // ���� ����
        [Min(0f)] public float Duration = 0.4f;          // ���� �ð�(��)
        [Min(0.01f)] public float Frequency = 25f;       // ���� ��
        public bool UnscaledTime = false;                 // �Ͻ����� �ÿ��� ������ �ϸ� true
        public bool AffectRotation = false;               // Z ȸ���� ��¦ ����
        public float RotationAmount = 8f;                 // ���� ��鸲 ũ��(��)
        public Vector2 AxisMask = new Vector2(1, 1);      // (1,1)=xy ���, (1,0)=x��, (0,1)=y��

        // 0�ʿ��� 1�� ������ 1�ʿ��� 0���� ����Ǵ� �⺻ Ŀ��(���� Ŀ��� �ʿ� �� �ν����Ϳ��� �ٲټ���)
        public AnimationCurve Damping = AnimationCurve.EaseInOut(0, 1, 1, 0);

        /// <summary>
        /// �ƽſ��� ȣ���� �Լ�. Camera.main ������� ���� ����ũ ����.
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
                target: null // null�̸� Camera.main�� ����
            );
        }
    }
}
