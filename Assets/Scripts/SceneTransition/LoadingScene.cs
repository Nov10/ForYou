using Helpers;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] Image FadeImage;
    [SerializeField] TMP_Text LoadingText;

    private void Awake()
    {
        LoadingText.gameObject.SetActive(false);
        FadeImage.CrossFadeAlpha(0.0f, 0.0f, true);
    }
    private void Start()
    {
        FadeImage.CrossFadeAlpha(1.0f, 1.0f, true);
        LoadingText.gameObject.SetActive(false);
        c = StartCoroutine(_SetActiveText(true));
    }
    Coroutine c;

    IEnumerator _SetActiveText(bool active)
    {
        yield return new WaitForSeconds(1.0f);
        LoadingText.gameObject.SetActive(active);
    }

    public void Hide()
    {
        LoadingText.gameObject.SetActive(false);
        FadeImage.CrossFadeAlpha(0.0f, 1.5f, true);
        //c = StartCoroutine(_SetActiveText(false));
    }
}
