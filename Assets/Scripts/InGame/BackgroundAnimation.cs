using UnityEngine;

namespace ForYou.GamePlay
{
    public class BackgroundAnimation : MonoBehaviour
    {
        [SerializeField] SpriteRenderer Image;
        [SerializeField] float WorldWidth = 100;
        [SerializeField] float WorldHeight = 100;
        [SerializeField] float MovementRange = 10.0f;
        private void Update()
        {
            var playerfishPosition = InGameManager.Instance.GetPlayerFish().transform.position;
            var normalizedPosition = new Vector2(
                Mathf.Clamp(playerfishPosition.x / WorldWidth, -0.5f, 0.5f),
                Mathf.Clamp(playerfishPosition.y / WorldHeight, -0.5f, 0.5f)
            );

            var targetPosition = normalizedPosition * MovementRange;
            Image.transform.localPosition = Vector2.Lerp(Image.transform.localPosition, targetPosition, Time.deltaTime * 5.0f);
        }
    }
}