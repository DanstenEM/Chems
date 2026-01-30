using System.Collections.Generic;
using Assets.Scripts.Interactions.Abstract;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class LootCrate : TriggerInteractable
{
    [Header("UI")]
    [SerializeField] private LootCrateOverlay lootOverlay;
    [SerializeField] private TMP_Text interactionHint;
    [SerializeField] private string interactionHintText = "OPEN CRATE";

    [Header("Loot Items")]
    [SerializeField] private List<InventoryItemObj> regularLoot = new List<InventoryItemObj>();
    [SerializeField] private List<InventoryItemObj> chemicalLoot = new List<InventoryItemObj>();
    [SerializeField] private List<InventoryItemObj> weaponLoot = new List<InventoryItemObj>();

    [Header("Loot Generation")]
    [SerializeField] private int minItems = 2;
    [SerializeField] private int maxItems = 8;

    private readonly List<InventoryItemObj> generatedLoot = new List<InventoryItemObj>();
    private bool hasGeneratedLoot;

    private const float RegularChance = 0.6f;
    private const float ChemicalChance = 0.3f;
    private const float WeaponChance = 0.1f;

    private void Awake()
    {
        lootOverlay = GetOrCreateOverlay();
    }

    public override void Active(InputBinding input)
    {
        base.Active(input);
        if (interactionHint != null)
        {
            interactionHint.text = interactionHintText;
            interactionHint.gameObject.SetActive(true);
        }
    }

    public override void Deactive()
    {
        base.Deactive();
        if (interactionHint != null)
        {
            interactionHint.gameObject.SetActive(false);
        }
        if (lootOverlay != null)
        {
            lootOverlay.Close();
        }
    }

    public override void Interact()
    {
        lootOverlay = GetOrCreateOverlay();
        if (lootOverlay != null)
        {
            lootOverlay.Open(this);
        }
    }

    public void GenerateLootIfNeeded()
    {
        if (hasGeneratedLoot)
        {
            return;
        }

        generatedLoot.Clear();
        int targetCount = Random.Range(minItems, maxItems + 1);

        for (int i = 0; i < targetCount; i++)
        {
            var item = GetRandomLootItem();
            if (item != null)
            {
                generatedLoot.Add(item);
            }
        }

        hasGeneratedLoot = true;
    }

    public IReadOnlyList<InventoryItemObj> GetLootItems()
    {
        return generatedLoot;
    }

    public void SyncLootFromSlots(IReadOnlyList<InventorySlot> slots)
    {
        if (slots == null)
        {
            return;
        }

        generatedLoot.Clear();

        foreach (var slot in slots)
        {
            if (slot == null)
            {
                continue;
            }

            var item = slot.GetComponentInChildren<InventoryItem>();
            if (item == null || item.itemObj == null)
            {
                continue;
            }

            int count = Mathf.Max(1, item.count);
            for (int i = 0; i < count; i++)
            {
                generatedLoot.Add(item.itemObj);
            }
        }

        hasGeneratedLoot = true;
    }

    private InventoryItemObj GetRandomLootItem()
    {
        float roll = Random.value;
        InventoryItemObj item = roll switch
        {
            <= RegularChance => GetRandomFromList(regularLoot),
            <= RegularChance + ChemicalChance => GetRandomFromList(chemicalLoot),
            _ => GetRandomFromList(weaponLoot)
        };

        if (item != null)
        {
            return item;
        }

        var fallbackPool = new List<InventoryItemObj>();
        fallbackPool.AddRange(regularLoot);
        fallbackPool.AddRange(chemicalLoot);
        fallbackPool.AddRange(weaponLoot);

        return GetRandomFromList(fallbackPool);
    }

    private static InventoryItemObj GetRandomFromList(List<InventoryItemObj> items)
    {
        if (items == null || items.Count == 0)
        {
            return null;
        }

        int index = Random.Range(0, items.Count);
        return items[index];
    }

    private LootCrateOverlay GetOrCreateOverlay()
    {
        if (lootOverlay != null)
        {
            return lootOverlay;
        }

        lootOverlay = FindObjectOfType<LootCrateOverlay>();
        if (lootOverlay != null)
        {
            return lootOverlay;
        }

        var overlayObject = new GameObject("LootCrateOverlaySystem");
        lootOverlay = overlayObject.AddComponent<LootCrateOverlay>();
        return lootOverlay;
    }
}
