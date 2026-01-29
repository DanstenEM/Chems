using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [field: SerializeField] public InventoryItemObj itemObj { get; private set; }
    [field: SerializeField] public int count { get; set; } = 1;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text counter;
    private InventorySystem inventorySystem;
    
    public Transform parentAfterDrag;

    public void Construct(InventorySystem inventorySystem,InventoryItemObj inventoryItemObj)
    {
        itemObj = inventoryItemObj;
        this.inventorySystem = inventorySystem;
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }
        if (image != null)
        {
            image.color = itemObj != null ? GetCategoryColor(itemObj.category) : new Color(1f, 0.85f, 0.2f, 1f);
            if (itemObj != null && itemObj.icon != null)
            {
                image.sprite = itemObj.icon;
                image.preserveAspect = true;
            }
        }
        RefrashCount();
    }

    public void RefrashCount()
    {
        counter.text = count.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }

        //if( parentAfterDrag.TryGetComponent(out InventorySlot component))
        //    component.inventoryItem = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = inventorySystem.mousePosition;
    } 

    private static Color GetCategoryColor(InventoryItemObj.ItemCategory category)
    {
        return category switch
        {
            InventoryItemObj.ItemCategory.Chemical => new Color(0.2f, 0.9f, 0.2f, 1f),
            InventoryItemObj.ItemCategory.Weapon => new Color(0.95f, 0.2f, 0.2f, 1f),
            _ => new Color(1f, 0.85f, 0.2f, 1f)
        };
    }
}
