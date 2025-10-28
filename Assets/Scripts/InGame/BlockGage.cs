using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ForYou.GamePlay
{
    public class BlockGage : MonoBehaviour
    {
        GridLayoutGroup Layout;
        [SerializeField] Image BackgroundImage;
        [SerializeField] Image BlockImagePrefab;
        [SerializeField] float MinWidth;
        [SerializeField] float MaxWidth; // <- 2Ä­ ±âÁØ Width
        [SerializeField] float Space;
        [SerializeField] Color FilledColor;
        [SerializeField] Color NormalColor;
        [SerializeField] int Current;
        [SerializeField] int Max;
        [SerializeField] int MinBlock = 2;
        [SerializeField] int MaxBlock = 15;
        private void Awake()
        {
            Layout = GetComponent<GridLayoutGroup>();
            //StartCoroutine(_S());
        }
        IEnumerator _S()
        {
            while(true)
            {

                SetGage(Current, Max);
                yield return new WaitForSeconds(0.5f);
            }
        }
        [SerializeField] float Offset;
        public void SetGage(int current, int max)
        {
            float width = ((float)(max - MinBlock) / (MaxBlock - MinBlock)) * (MaxWidth - MinWidth) + MinWidth;
            //Width == (max - 1) * Space + max * SingleBlockWidth
            float singleBlockWidth = (width - (max - 1) * Space) / max;

            BackgroundImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width + Offset);

            Layout.cellSize = new Vector2(singleBlockWidth, Layout.cellSize.y);
            Layout.spacing = new Vector2(Space, Layout.spacing.y);

            var images = Layout.GetComponentsInChildren<Image>();
            for(int i =0; i < images.Length; i++)
            {
                Destroy(images[i].gameObject);
            }

            for (int i = 0; i < max; i++)
            {
                var g = Instantiate(BlockImagePrefab.gameObject, Layout.transform).GetComponent<Image>();
                if (i < current)
                    g.color = FilledColor;
                else
                    g.color = NormalColor;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(Layout.GetComponent<RectTransform>());
        }
    }
}