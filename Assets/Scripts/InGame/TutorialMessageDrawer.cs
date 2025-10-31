using Helpers;
using UnityEngine;
using UnityEngine.UIElements;

namespace ForYou.GamePlay
{
    public class TutorialMessageDrawer : MonoBehaviour
    {
        [SerializeField] RectTransform Container;
        [SerializeField] GameObject Background;
        GameObject Message;
        public bool IsEnd { get; private set; }

        private void Awake()
        {
            Background.SetActive(false);
        }
        private void OnEnable()
        {
            Background.SetActive(false);
        }
        public void Play(GameObject prefab)
        {
            Background.SetActive(true);
            Message = Instantiate(prefab, Container);
            var cg = Message.GetComponent<CanvasGroup>();
            cg.alpha = 0.0f;
            ObjectMoveHelper.ChangeAlpha(cg, 1.0f, 0.2f);
            IsEnd = false;
        }

        private void Update()
        {
            if (IsEnd == true)
                return;
            if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
            {
                ObjectMoveHelper.ChangeAlpha(Message.GetComponent<CanvasGroup>(), 1.0f, 0.2f);
                DelayedFunctionHelper.InvokeDelayed(0.2f, () =>
                {
                    Background.SetActive(false);
                    Destroy(Message);
                    IsEnd = true;
                });
            }
        }
    }
}