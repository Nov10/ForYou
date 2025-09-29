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
        while (true)
        {
            timer += Time.deltaTime;
            string text = sentence.Evaluate(timer);
            Text.text = text;
            yield return null;
        }
    }
}
