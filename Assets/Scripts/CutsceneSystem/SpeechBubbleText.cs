using UnityEngine;

namespace ForYou.Cutscene
{
    [System.Serializable]
    public class SpeechBubbleText : CutsceneElement
    {
        [Header("Sentence Setting\nRich Text, Vibrate, Delay �±׸� ����� �� �ֽ��ϴ�.")]
        public ExtendedSentence Sentence;

        [Header("UI Setting\n� ��ǳ������ ��������, ��� �������� ����")]
        public GameObject SpeechBubblePrefab;
        public GameObject Position;

        [Header("Delay ���Ŀ� ��ǳ���� �ڵ����� �����˴ϴ�.")]
        public float DestoryDelay = 0.0f;
    }
}