using UnityEngine;

namespace ForYou.GamePlay
{
    [RequireComponent(typeof(Collider2D))]
    public class PlanktonDetector : MonoBehaviour
    {
        PlayerFish Parent;
        private void Awake()
        {
            Parent = GetComponentInParent<PlayerFish>();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.transform.TryGetComponent<Plankton>(out var plankton))
            {
                Parent.OnPlanktonDetected(plankton);
            }
        }
    }
}