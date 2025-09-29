using UnityEngine;

namespace ForYou.GamePlay
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerFishDetector : MonoBehaviour
    {
        Anemone Parent;
        private void Awake()
        {
            Parent = GetComponentInParent<Anemone>();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform.TryGetComponent<PlayerFish>(out var fish))
            {
                Parent.OnPlayerFishDetected(fish);
            }
        }
    }
}