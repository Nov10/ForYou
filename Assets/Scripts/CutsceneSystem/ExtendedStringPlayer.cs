using System.Collections;
using TMPro;
using UnityEngine;

public class ExtendedStringPlayer : MonoBehaviour
{
    [SerializeField] ExtendedSentence Sentence;

    private void Start()
    {
        if (Sentence != null)
            Play(Sentence, transform);
    }

    [SerializeField] TMP_Text Text;
    public void Play(ExtendedSentence sentence, Transform position)
    {
        StartCoroutine(_Play(sentence, position));
    }

    IEnumerator _Play(ExtendedSentence sentence, Transform position)
    {
        float timer = 0f;
            Text.GetComponent<TMPVibrateAnimator>().Sentence = sentence;
            Text.GetComponent<TMPVibrateAnimator>().ResetTimer();
        while (true)
        {
            timer += Time.deltaTime;
            // text = sentence.Evaluate(timer, out var vibrates);
            //Text.text = text;
            yield return null;
        }
    }
}
