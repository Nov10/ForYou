using UnityEngine;

namespace ForYou.Cutscene
{
    public class PositionTracker : MonoBehaviour
    {
        [SerializeField] GameObject Target;
        RectTransform ThisRectTransform;

        private void Awake()
        {
            ThisRectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            var pos = Camera.main.WorldToScreenPoint(Target.transform.position);
            ThisRectTransform.position = pos;
        }
    }
}