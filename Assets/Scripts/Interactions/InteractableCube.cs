using System.Globalization;
using System.Collections.Generic;
using Assets.Scripts.Interactions.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private bool generateRandomLootOnAwake = true;

    private void Awake()
    {
        EnsureHint();

        if (lootCrateUI == null)
        {
            lootCrateUI = GetComponent<LootCrateUI>();
        }

        if (generateRandomLootOnAwake && (lootItems == null || lootItems.Length == 0))
        {
            lootItems = GenerateRandomLoot();
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
        int amount = Random.Range(2, 11);
        var results = new List<InventoryItemObj>(amount);

        for (int i = 0; i < amount; i++)
        {
            var item = GetRandomLootItem();
            if (item != null)
            {
                results.Add(item);
            }
        }

        return results.ToArray();
    }

    private InventoryItemObj GetRandomLootItem()
    {
        float roll = Random.value;

        if (roll < 0.75f)
        {
            return GetRandomFromPool(defaultLootPool) ?? GetFallbackLootItem();
        }

        if (roll < 0.95f)
        {
            return GetRandomFromPool(chemicalLootPool) ?? GetFallbackLootItem();
        }

        return GetRandomFromPool(weaponLootPool) ?? GetFallbackLootItem();
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
