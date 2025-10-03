using UnityEngine;

namespace ForYou.Cutscene
{
    [System.Serializable]
    public class MovePlayerFish : CutsceneElement
    {
        public Transform TargetPosition;
        public float AllowDistance = 0.5f;
        public float Speed = -1.0f;
        public bool AutoReturnToPlayerControlMode = false;
    }
}