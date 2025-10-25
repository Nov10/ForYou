using ForYou.GamePlay;
using Helpers;
using TMPro;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankingUI : MonoBehaviour
{
    public static bool ShoudSetName;
    [SerializeField] TMP_Text[] NameTexts;
    [SerializeField] TMP_Text[] ScoreTexts;
    [SerializeField] TMP_Text SelfScoreText;
    [SerializeField] Button CloseButton;

    private void Start()
    {
        CloseButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(ConstValue.SCENE_INDEX_Title);
        });
        StartRankingUI(ShoudSetName, InGameManager.LastScore);
    }

    [SerializeField] Image HidePanel;
    [SerializeField] RectTransform NameSetPanel;
    [SerializeField] TMP_InputField NameInputField;
    [SerializeField] Button DetermineNameButton;
    [SerializeField] TMP_Text WarningText;

    public void StartRankingUI(bool setName, int myScore)
    {
        HidePanel.transform.localPosition = Vector3.zero;
        HidePanel.gameObject.SetActive(true);
        if (setName == true)
        {
            NameSetPanel.anchoredPosition = Vector2.zero;
            NameSetPanel.gameObject.SetActive(true);

            DetermineNameButton.onClick.RemoveAllListeners();
            DetermineNameButton.onClick.AddListener(() =>
            {
                var name = NameInputField.text;

                if(name.Length < 2)
                {
                    WarningText.text = "2±ÛÀÚ ÀÌ»óÀÌ¿©¾ß ÇÕ´Ï´Ù.";
                    return;
                }
                if(name.Length > 6)
                {
                    WarningText.text = "6±ÛÀÚ ÀÌÇÏ¿©¾ß ÇÕ´Ï´Ù.";
                    return;
                }
                //ÇÑ±Û, ¿µ¾î¸¸ ÀÔ·Â °¡´É
                foreach (char c in name)
                {
                    if (!((c >= '°¡' && c <= 'ÆR') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                    {
                        WarningText.text = "ÇÑ±Û°ú ¿µ¾î¸¸ ÀÔ·Â °¡´ÉÇÕ´Ï´Ù.";
                        return;
                    }
                }

                ObjectMoveHelper.ChangeAlpha(NameSetPanel.gameObject.GetComponent<CanvasGroup>(), 0.0f, 2.0f);
                DelayedFunctionHelper.InvokeDelayed(2.0f, () =>
                {
                    NameSetPanel.gameObject.SetActive(false);
                });

                SelfScoreText.text = $"{name} : {myScore}";
                string myId = SystemInfo.deviceUniqueIdentifier;
                string timeStamp = System.DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                FindFirstObjectByType<LeaderboardClient>().SubmitScore($"{myId}_at_{timeStamp}", name, myScore, (success) =>
                {
                    ShowRanking();
                });
            });
        }
        else
        {
            ShowRanking();
        }
    }

    void ShowRanking()
    {
        ClaerAllTexts();
        FindFirstObjectByType<LeaderboardClient>().GetTop(7, (data) =>
        {
            ObjectMoveHelper.ChangeAlpha(HidePanel, 0.0f, 2.0f);
            DelayedFunctionHelper.InvokeDelayed(2.0f, () =>
            {
                HidePanel.gameObject.SetActive(false);
            });
            for (int i = 0; i < NameTexts.Length; i++)
            {
                NameTexts[i].text = string.Empty;
                ScoreTexts[i].text = string.Empty;
            }
            for (int i = 0; i < data.Length; i++)
            {
                NameTexts[i].text = data[i].name;
                ScoreTexts[i].text = data[i].score.ToString();
            }
        });
    }
    void ClaerAllTexts()
    {
        for (int i = 0; i < NameTexts.Length; i++)
        {
            NameTexts[i].text = string.Empty;
            ScoreTexts[i].text = string.Empty;
        }
    }
}
