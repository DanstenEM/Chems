using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StashMenuItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text countText;

    public string ItemId { get; private set; }
    public int Count { get; private set; }

    public Transform parentAfterDrag;

    public void SetupComponents(Image targetImage, TMP_Text targetCountText)
    {
        image = targetImage;
        countText = targetCountText;
    }

    public void Construct(string itemId, Sprite icon, Color fallbackColor, int count)
    {
        ItemId = itemId;
        Count = Mathf.Max(1, count);

        if (image != null)
        {
            if (icon != null)
            {
                image.sprite = icon;
                image.color = Color.white;
                image.preserveAspect = true;
            }
            else
            {
                image.sprite = null;
                image.color = fallbackColor;
            }
        }

        RefreshCount();
    }

    public void SetCount(int value)
    {
        Count = Mathf.Max(1, value);
        RefreshCount();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (image != null)
        {
            image.raycastTarget = false;
        }

        if (countText != null)
        {
            countText.raycastTarget = false;
        }

        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (image != null)
        {
            image.raycastTarget = true;
        }

        if (countText != null)
        {
            countText.raycastTarget = true;
        }

        transform.SetParent(parentAfterDrag);

        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    private void RefreshCount()
    {
        if (countText != null)
        {
            countText.text = Count.ToString();
        }
    }
}
