using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public static class Extensions
    {
        #region List
        public static void Shift<T>(this List<T> list, int amount)
        {
            List<T> tempList = list.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                int newValueIndex = (i + amount) % list.Count;
                list[i] = tempList[newValueIndex];
            }
        }
        #endregion

        #region Transform
        public static void SetLayer(this Transform transform, int layer, bool recursive = false)
        {
            transform.gameObject.layer = layer;

            if (recursive)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    transform.GetChild(i).SetLayer(layer, true);
                }
            }
        }

        public static void ClearChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }
        #endregion

        #region Rect Transform
        public static void SetSizeDeltaWidth(this RectTransform rectTransform, float width)
        {
            rectTransform.SetSizeDelta(width, rectTransform.sizeDelta.y);
        }

        public static void SetSizeDeltaHeight(this RectTransform rectTransform, float height)
        {
            rectTransform.SetSizeDelta(rectTransform.sizeDelta.x, height);
        }

        public static void SetSizeDelta(this RectTransform rectTransform, float width, float height)
        {
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        public static void SetPaddingLeft(this RectTransform rectTransform, float left)
        {
            rectTransform.offsetMin = new Vector2(left, rectTransform.offsetMin.y);
        }

        public static void SetPaddingRight(this RectTransform rectTransform, float right)
        {
            rectTransform.offsetMax = new Vector2(-right, rectTransform.offsetMax.y);
        }

        public static void SetPaddingTop(this RectTransform rectTransform, float top)
        {
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -top);
        }

        public static void SetPaddingBottom(this RectTransform rectTransform, float bottom)
        {
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, bottom);
        }

        public static void SetPadding(this RectTransform rectTransform, float horizontal, float vertical)
        {
            rectTransform.offsetMax = new Vector2(-horizontal, -vertical);
            rectTransform.offsetMin = new Vector2(horizontal, vertical);
        }

        public static void SetPadding(this RectTransform rectTransform, float left, float top, float right,
            float bottom)
        {
            rectTransform.offsetMax = new Vector2(-right, -top);
            rectTransform.offsetMin = new Vector2(left, bottom);
        }
        #endregion

        #region UI
        public static void ScrollTo(this ScrollRect scrollRect, RectTransform targetRectTransform,
            RectTransform contentRectTransform)
        {
            Canvas.ForceUpdateCanvases();
            Vector2 viewportLocalPosition = scrollRect.viewport.localPosition;
            Vector2 targetLocalPosition = targetRectTransform.localPosition;

            Vector2 newTargetLocalPosition = new Vector2(
                0 - (viewportLocalPosition.x + targetLocalPosition.x) + (scrollRect.viewport.rect.width / 2) -
                (targetRectTransform.rect.width / 2),
                0 - (viewportLocalPosition.y + targetLocalPosition.y) + (scrollRect.viewport.rect.height / 2) -
                (targetRectTransform.rect.height / 2));
            contentRectTransform.localPosition = newTargetLocalPosition;
        }

        public static void ScrollTo(this ScrollRect scrollRect, RectTransform targetRectTransform, RectOffset padding = null)
        {
            Canvas.ForceUpdateCanvases();
            Vector3 relativePosition = scrollRect.content.InverseTransformPoint(targetRectTransform.position);
            float horizontalNormalizedPosition =
                (relativePosition.x + scrollRect.content.pivot.x * scrollRect.content.rect.width) /
                scrollRect.content.rect.width;
            float verticalNormalizedPosition =
                (relativePosition.y + scrollRect.content.pivot.y * scrollRect.content.rect.height) /
                scrollRect.content.rect.height;

            // if (padding == null)
            // {
            //     padding = new RectOffset();
            // }
            if (scrollRect.horizontal)
            {
                scrollRect.horizontalNormalizedPosition = horizontalNormalizedPosition;
            }
            if (scrollRect.vertical)
            {
                // Debug.Log(verticalNormalizedPosition);
                // if (verticalNormalizedPosition > 0.5f)
                // {
                //     verticalNormalizedPosition += (-padding.top + padding.bottom) / scrollRect.content.rect.height;
                // }
                // else
                // {
                //     verticalNormalizedPosition -= (-padding.top + padding.bottom) / scrollRect.content.rect.height;
                // }
                // Debug.Log(verticalNormalizedPosition);
                
                scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
            }
        }
        #endregion
    }
}