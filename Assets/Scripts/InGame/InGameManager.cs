using UnityEngine;

namespace ForYou.GamePlay
{
    public class InGameManager : MonoBehaviour
    {
        public static InGameManager Instance { get; private set; }
        private void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }    
            Instance = this;
        }
        [SerializeField] PlayerFish Player;

        public bool IsGameOver { get; private set; } = false;


        public void GameOver()
        {
            IsGameOver = true;
        }

        [SerializeField] SquidFX SquidBlackFX;
        [SerializeField] JellyFishFX JellyFishBlurFX;
        public void PlaySquidBlackFX()
        {
            SquidBlackFX.Play();
        }

        public void PlayJellyFishBlurFX(float duration)
        {
            JellyFishBlurFX.Play(duration);
        }
    }
}