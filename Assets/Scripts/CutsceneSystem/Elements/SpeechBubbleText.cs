using UnityEngine;

namespace ForYou.Cutscene
{
    [System.Serializable]
    public class SpeechBubbleText : CutsceneElement
    {
        public ExtendedSentence Sentence;

        public GameObject SpeechBubblePrefab;
        public GameObject Position;
    }
}