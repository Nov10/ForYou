using UnityEngine;
using UnityEngine.UI;

namespace ForYou.Lobby
{
    public class BackgroundAnimation : MonoBehaviour
    {
        [SerializeField] float MovementRange1 = 5f;
        [SerializeField] float MovementRange2 = 5f;
        [SerializeField] Image Image1;
        [SerializeField] Image Image2;
        private void Update()
        {
            var cursorPosition = Input.mousePosition;
            var normalizedCursorPosition = new Vector2(
                (cursorPosition.x / Screen.width) - 0.5f,
                (cursorPosition.y / Screen.height) - 0.5f
            );
            normalizedCursorPosition.x = Mathf.Clamp(normalizedCursorPosition.x, -0.5f, 0.5f);
            normalizedCursorPosition.y = Mathf.Clamp(normalizedCursorPosition.y, -0.5f, 0.5f);
            //Debug.Log(normalizedCursorPosition);
            //normalizedCursorPosition.x = Mathf.Sign(normalizedCursorPosition.x) * normalizedCursorPosition.x * normalizedCursorPosition.x;
            //normalizedCursorPosition.y = Mathf.Sign(normalizedCursorPosition.y) * normalizedCursorPosition.y * normalizedCursorPosition.y;


            var targetPosition1 = normalizedCursorPosition * MovementRange1;
            var targetPosition2 = normalizedCursorPosition * MovementRange2;

            Image1.transform.localPosition = Vector2.Lerp(Image1.transform.localPosition, targetPosition1, Time.deltaTime * 5.0f);
            Image2.transform.localPosition = Vector2.Lerp(Image2.transform.localPosition, targetPosition2, Time.deltaTime * 5.0f);
        }
    }
}