using UnityEngine;

namespace ForYou.Cutscene
{
    [System.Serializable]
    public class MovePlayerFish : CutsceneElement
    {
        public Transform TargetPosition;
        [Header("이 거리만큼 가까워져야 정상적으로 종료됩니다. 충돌 범위를 잘 고려하여 설정하세요.")]
        public float AllowDistance = 1;
        [Header("이 거리만큼 가까워져야 정상적으로 종료됩니다. 충돌 범위를 잘 고려하여 설정하세요.")]
        public float SlowDistance = 1;
        [Header("자동으로 플레이어 조작이 가능한 모드로 바꿀까요?")]
        public bool AutoReturnToPlayerControlMode = true;
    }
}