using UnityEngine;

namespace ForYou.GamePlay
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerFishDetector : MonoBehaviour
    {
        Anemone Parent;
        EnemyFish Parent2;
        private void Awake()
        {
            Parent = GetComponentInParent<Anemone>();
            Parent2 = GetComponentInParent<EnemyFish>();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform.TryGetComponent<PlayerFish>(out var fish))
            {
                Parent?.OnPlayerFishDetected(fish);
                Parent2?.OnPlayerFishDetected(fish);
            }
        }
    }
}