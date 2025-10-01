using Helpers;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ForYou.Cutscene
{
    public class CutscenePlayer : MonoBehaviour
    {
        [SerializeField] CutsceneData Data;

        private void Start()
        {
            Play();
        }

        int NowIndex = 0;

        public void Play()
        {
            NowIndex = -1;
            PlayNext();
        }

        void PlayNext()
        {
            NowIndex++;
            if (NowIndex >= Data.Elements.Length)
                return;
            StartCoroutine(_PlaySingleElement(Data.Elements[NowIndex], PlayNext));
        }

        public ExtendedStringPlayer PlayerPrefab;
        IEnumerator _PlaySingleElement(CutsceneElement element, System.Action onEnd)
        {
            var type = element.GetType();
            if(type == typeof(Delay))
            {
                yield return new WaitForSeconds(((Delay)element).Duration);
                onEnd();
            }
            else if(type == typeof(SpeechBubbleText))
            {
                var text = (SpeechBubbleText)element;
                var bubble = Instantiate(text.SpeechBubblePrefab.gameObject, text.Position.transform);
                bubble.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                var player = Instantiate(PlayerPrefab.gameObject, transform).GetComponent<ExtendedStringPlayer>();
                yield return null;
                player.Play(text.Sentence, bubble.GetComponentInChildren<TMP_Text>());

                while(player.IsEnd == false)
                {
                    yield return null;
                }
                DelayedFunctionHelper.InvokeDelayed(text.DestoryDelay, onEnd);
                Destroy(bubble.gameObject, text.DestoryDelay);
                Destroy(player.gameObject, text.DestoryDelay);
            }
        }
    }
}