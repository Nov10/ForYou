using UnityEngine;

namespace ForYou.Cutscene
{
    [System.Serializable]
    public class SpeechBubbleText : CutsceneElement
    {
        [Header("Sentence Setting\nRich Text, Vibrate, Delay 태그를 사용할 수 있습니다.")]
        public ExtendedSentence Sentence;

        [Header("UI Setting\n어떤 말풍선으로 보여줄지, 어디에 보여줄지 설정")]
        public GameObject SpeechBubblePrefab;
        public GameObject Position;

        [Header("Delay 이후에 말풍선이 자동으로 삭제됩니다.")]
        public float DestoryDelay = 0.0f;
    }
}