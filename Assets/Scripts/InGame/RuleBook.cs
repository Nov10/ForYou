using UnityEngine;
using UnityEngine.UI;

namespace ForYou.GamePlay
{
    public class RuleBook : MonoBehaviour
    {
        [SerializeField] RectTransform[] Pages;
        [SerializeField] Button NextPage;
        [SerializeField] Button PreviousPage;
        [SerializeField] Button Close;

        int CurrentPageIndex = 0;
        private void Start()
        {
            UpdatePageVisibility();
            NextPage.onClick.AddListener(OnNextPage);
            PreviousPage.onClick.AddListener(OnPreviousPage);
            Close.onClick.AddListener(() => { gameObject.SetActive(false); });
        }
        void OnNextPage()
        {
            if (CurrentPageIndex < Pages.Length - 1)
            {
                CurrentPageIndex++;
                UpdatePageVisibility();
            }
        }
        void OnPreviousPage()
        {
            if (CurrentPageIndex > 0)
            {
                CurrentPageIndex--;
                UpdatePageVisibility();
            }
        }
        void UpdatePageVisibility()
        {
            for (int i = 0; i < Pages.Length; i++)
            {
                Pages[i].gameObject.SetActive(i == CurrentPageIndex);
            }
            PreviousPage.gameObject.SetActive(CurrentPageIndex > 0);
            NextPage.gameObject.SetActive(CurrentPageIndex < Pages.Length - 1);
        }
    }
}