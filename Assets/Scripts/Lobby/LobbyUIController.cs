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

        private void Awake()
        {
            Instance = this;
            StartButton.onClick.AddListener(OnStartButtonPressed);
            RankingButton.onClick.AddListener(OnRankingButtonPressed);
            SettingButton.onClick.AddListener(OnSettingButtonPressed);
            CreditButton.onClick.AddListener(OnCreditButtonPressed);
        }

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

        }
    }
}