using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonWithText : MonoBehaviour
{
    TMP_Text Text;
    private void Awake()
    {
        Text = GetComponentInChildren<TMP_Text>();
    }

    public void SetTextUp()
    {
        var p = Text.rectTransform;
        Text.rectTransform.anchoredPosition = new Vector2(p.anchoredPosition.x, 8.200001f);
    }
    public void SetTextDown()
    {
        var p = Text.rectTransform;
        Text.rectTransform.anchoredPosition = new Vector2(p.anchoredPosition.x, 2f);
    }
}
