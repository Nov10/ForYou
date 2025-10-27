namespace ForYou.Cutscene
{
    [System.Serializable]
    public class SetActiveCameraFollowTarget : CutsceneElement
    {
        public bool IsActive;
        public float FollowSnapping = 1.0f;
    }
}