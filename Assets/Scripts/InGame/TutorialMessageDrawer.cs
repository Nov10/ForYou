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
            IsEnd = false;
        }

        private void Update()
        {
            if (IsEnd == true)
                return;
            if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
            {
                Background.SetActive(false);
                Destroy(Message);
                IsEnd = true;
            }
        }
    }
}