using System.Globalization;
using System.Collections.Generic;
using Assets.Scripts.Interactions.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider))]
public class InteractableCube : MonoBehaviour, IInteractable
{
    private static InteractableCube activeHintOwner;
    private const string DefaultHintFormat = "PRESS {0} INTERACT";

    [field: SerializeField] public KeyActiveType keyType { get; set; } = KeyActiveType.Tap;

    [Header("Hint")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Vector3 hintOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private string hintFormat = DefaultHintFormat;

    [Header("Loot Crate")]
    [SerializeField] private LootCrateUI lootCrateUI;
    [SerializeField] private InventoryItemObj[] lootItems;
    [SerializeField] private InventoryItemObj[] defaultLootPool;
    [SerializeField] private InventoryItemObj[] chemicalLootPool;
    [SerializeField] private InventoryItemObj[] weaponLootPool;
    [SerializeField] private Vector2Int defaultLootCountRange = new Vector2Int(2, 6);
    [SerializeField] private Vector2Int chemicalLootCountRange = new Vector2Int(0, 2);
    [SerializeField] private Vector2Int weaponLootCountRange = new Vector2Int(0, 1);
    [FormerlySerializedAs("generateRandomLootOnAwake")]
    [SerializeField] private bool generateRandomLootOnStart = true;
    private bool hasGeneratedLoot;

    private void Awake()
    {
        EnsureHint();

        if (lootCrateUI == null)
        {
            lootCrateUI = GetComponent<LootCrateUI>();
        }

        hasGeneratedLoot = lootItems != null && lootItems.Length > 0;
    }

    private void Start()
    {
        if (generateRandomLootOnStart && !hasGeneratedLoot && (lootItems == null || lootItems.Length == 0))
        {
            lootItems = GenerateRandomLoot();
            hasGeneratedLoot = true;
        }
    }

    private void LateUpdate()
    {
        if (hintText == null || !hintText.gameObject.activeSelf)
        {
            return;
        }

        var cameraTarget = Camera.main;
        if (cameraTarget == null)
        {
            return;
        }

        var direction = hintText.transform.position - cameraTarget.transform.position;
        hintText.transform.rotation = Quaternion.LookRotation(direction);
    }

    public void Interact()
    {
        Debug.Log($"Interactable cube triggered: {name}", this);

        if (lootCrateUI != null)
        {
            lootCrateUI.Toggle(lootItems);
        }
    }

    public void Active(InputBinding input)
    {
        if (hintText == null)
        {
            return;
        }

        if (activeHintOwner != null && activeHintOwner != this)
        {
            activeHintOwner.Deactive();
        }

        activeHintOwner = this;

        var keyLabel = InputControlPath.ToHumanReadableString(
            input.path,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
        if (string.IsNullOrWhiteSpace(keyLabel))
        {
            keyLabel = input.path;
        }

        hintText.text = string.Format(CultureInfo.InvariantCulture, hintFormat, keyLabel.ToUpperInvariant());
        hintText.gameObject.SetActive(true);
    }

    public void Deactive()
    {
        if (hintText == null)
        {
            return;
        }

        hintText.gameObject.SetActive(false);

        if (activeHintOwner == this)
        {
            activeHintOwner = null;
        }
    }

    private void EnsureHint()
    {
        if (hintText != null)
        {
            hintText.gameObject.SetActive(false);
            return;
        }

        var hintObject = new GameObject("InteractHint");
        hintObject.transform.SetParent(transform, false);
        hintObject.transform.localPosition = hintOffset;

        hintText = hintObject.AddComponent<TextMeshPro>();
        hintText.text = string.Format(CultureInfo.InvariantCulture, hintFormat, "E");
        hintText.fontSize = 3.5f;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.color = Color.white;
        hintText.gameObject.SetActive(false);
    }

    private InventoryItemObj[] GenerateRandomLoot()
    {
        int defaultCount = GetCountFromRange(defaultLootCountRange);
        int chemicalCount = GetCountFromRange(chemicalLootCountRange);
        int weaponCount = GetCountFromRange(weaponLootCountRange);

        var results = new List<InventoryItemObj>(defaultCount + chemicalCount + weaponCount);
        AddLootFromPool(results, defaultLootPool, defaultCount);
        AddLootFromPool(results, chemicalLootPool, chemicalCount);
        AddLootFromPool(results, weaponLootPool, weaponCount);

        if (results.Count == 0)
        {
            var fallback = GetFallbackLootItem();
            if (fallback != null)
            {
                results.Add(fallback);
            }
        }

        Shuffle(results);

        return results.ToArray();
    }

    private void AddLootFromPool(List<InventoryItemObj> results, InventoryItemObj[] pool, int count)
    {
        if (results == null || count <= 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var item = GetRandomFromPool(pool) ?? GetFallbackLootItem();
            if (item != null)
            {
                results.Add(item);
            }
        }
    }

    private static void Shuffle(List<InventoryItemObj> items)
    {
        if (items == null || items.Count < 2)
        {
            return;
        }

        for (int i = items.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (items[i], items[swapIndex]) = (items[swapIndex], items[i]);
        }
    }

    private static int GetCountFromRange(Vector2Int range)
    {
        int min = Mathf.Min(range.x, range.y);
        int max = Mathf.Max(range.x, range.y);

        return Random.Range(min, max + 1);
    }

    private InventoryItemObj GetRandomFromPool(InventoryItemObj[] pool)
    {
        if (pool == null || pool.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, pool.Length);
        return pool[index];
    }

    private InventoryItemObj GetFallbackLootItem()
    {
        var fallback = GetRandomFromPool(defaultLootPool);
        if (fallback != null)
        {
            return fallback;
        }

        fallback = GetRandomFromPool(chemicalLootPool);
        if (fallback != null)
        {
            return fallback;
        }

        return GetRandomFromPool(weaponLootPool);
    }
}
