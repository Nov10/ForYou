using UnityEngine;

namespace ForYou.Cutscene
{
    [System.Serializable]
    public class PlayAnimation : CutsceneElement
    {
        public Animator Target;
        public string AnimationName;
        public float Duration = 0.0f;
    }
}