using Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ForYou.Lobby
{
    public class LobbyUIController : MonoBehaviour
    {
        public static LobbyUIController Instance { get; private set; }


        [SerializeField] GridLayoutGroup ButtonContainer;
        [SerializeField] Button StartButton;
        [SerializeField] Button RankingButton;
        [SerializeField] Button SettingButton;
        [SerializeField] Button CreditButton;


        [SerializeField] SettingPanel SettingPanel;

        [SerializeField] CanvasGroup CreditPanel;

        private void Awake()
        {
            Instance = this;
            StartButton.onClick.AddListener(OnStartButtonPressed);
            RankingButton.onClick.AddListener(OnRankingButtonPressed);
            SettingButton.onClick.AddListener(OnSettingButtonPressed);
            CreditButton.onClick.AddListener(OnCreditButtonPressed);

            SetCreditPanelOffButton.onClick.AddListener(() =>
            {
                ObjectMoveHelper.ChangeAlpha(CreditPanel, 0.0f, 0.2f);
                DelayedFunctionHelper.InvokeDelayed(0.2f, () =>
                {
                    CreditPanel.gameObject.SetActive(false);
                });
            });
        }

        [SerializeField] Button SetCreditPanelOffButton;
        public void ShowStartSceen()
        {
            ButtonContainer.gameObject.SetActive(true);
            SettingPanel.gameObject.SetActive(false);
        }

        void OnStartButtonPressed()
        {
            SceneLoader.LoadScene(ConstValue.SCENE_INDEX_Tutorial);
        }
        void OnRankingButtonPressed()
        {
            RankingUI.ShoudSetName = false;
            SceneLoader.LoadScene(ConstValue.SCENE_INDEX_Ranking);
        }
        void OnSettingButtonPressed()
        {
            SettingPanel.gameObject.SetActive(true);
        }
        void OnCreditButtonPressed()
        {
            CreditPanel.gameObject.SetActive(true);
            CreditPanel.alpha = 0.0f;
            CreditPanel.transform.localPosition = Vector3.zero;
            ObjectMoveHelper.ChangeAlpha(CreditPanel, 1.0f, 0.2f);
        }
    }
}