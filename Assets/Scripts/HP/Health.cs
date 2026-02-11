using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private Transform respawnPoint;
    [Header("Drops")]
    [SerializeField] private GameObject deathDropPrefab;
    [SerializeField] private Transform dropSpawnPoint;
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, 0.5f, 0f);
    [Header("Player Death Loot")]
    [SerializeField] private GameObject playerDeathLootCratePrefab;
    [SerializeField] private bool clearPlayerInventoryOnDeath = true;
    private ColorAjusmentComponent colorAjusment;

    bool dead;
    private bool playerLootTransferredThisDeath;

    [SerializeField] bool isPlayer;
    public ReactiveProperty<bool> isDie = new ReactiveProperty<bool>();

    public float CurrentHealth { get; private set; }

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    [Inject]
    public void Construct(ColorAjusmentComponent colorAjusment)
    {
        this.colorAjusment = colorAjusment;
    }

    public void TakeDamage(float amount)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth -= amount;

        Debug.Log($"{name} took {amount} damage. HP: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
        if(isPlayer)
            colorAjusment?.Execute();
    }

    void Die()
    {
        if (dead) return;
        dead = true;

        Debug.Log($"{name} died");

        if (isPlayer)
        {
            HandlePlayerDeathLootTransfer();

            var move = GetComponent<ThirdPersonMovement>();
            if (move) move.enabled = false;

            var shooter = GetComponent<Shooter>();
            if (shooter) shooter.enabled = false;

            var aim = GetComponent<AimController>();
            if (aim) aim.enabled = false;

            var controller = GetComponent<CharacterController>();
            if (controller) controller.enabled = false;

            var swap = GetComponentInChildren<CMChangeView>();
            if (swap) swap.enabled = false;

            RagdollController ragdoll = GetComponentInChildren<RagdollController>();
            if (ragdoll)
                ragdoll.SetRagdoll(true);

            isDie.Value = true;

            Invoke(nameof(Respawn), respawnDelay);
            return;
        }

        if (deathDropPrefab)
        {
            Vector3 spawnPosition = GetDropSpawnPosition();
            Instantiate(deathDropPrefab, spawnPosition, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void HandlePlayerDeathLootTransfer()
    {
        if (playerLootTransferredThisDeath)
        {
            return;
        }

        var gameplayInventory = InventorySystem.GameplayInventory;
        var snapshot = InventorySnapshotMapper.BuildSnapshot(gameplayInventory);

        bool hasLoot = snapshot != null && snapshot.stacks != null && snapshot.stacks.Count > 0;
        if (hasLoot)
        {
            SpawnPlayerDeathLootCrate(snapshot);
        }

        if (clearPlayerInventoryOnDeath)
        {
            InventorySnapshotMapper.ClearInventoryContents(gameplayInventory);
        }

        playerLootTransferredThisDeath = true;
    }

    private void SpawnPlayerDeathLootCrate(SavedInventory snapshot)
    {
        var cratePrefab = playerDeathLootCratePrefab != null ? playerDeathLootCratePrefab : deathDropPrefab;
        if (cratePrefab == null)
        {
            Debug.LogWarning("No player death loot crate prefab configured.");
            return;
        }

        Vector3 spawnPosition = GetDropSpawnPosition();
        var crate = Instantiate(cratePrefab, spawnPosition, Quaternion.identity);
        var interactable = crate.GetComponentInChildren<InteractableCube>();
        if (interactable == null)
        {
            Debug.LogWarning("Spawned player death crate has no InteractableCube for loot transfer.");
            return;
        }

        IReadOnlyDictionary<string, InventoryItemObj> lookup = InventorySnapshotMapper.BuildLookupFromResources();
        List<InventoryItemObj> lootItems = InventorySnapshotMapper.BuildItemList(snapshot, lookup);
        interactable.SetLootItems(lootItems);
    }

    private Vector3 GetDropSpawnPosition()
    {
        return dropSpawnPoint
            ? dropSpawnPoint.position
            : transform.position + dropOffset;
    }

    void Respawn()
    {
        Debug.Log("RESPAWN");

        dead = false;
        playerLootTransferredThisDeath = false;
        CurrentHealth = maxHealth;
        isDie.Value = false;

        if (respawnPoint)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }

        RagdollController ragdoll = GetComponentInChildren<RagdollController>();
        if (ragdoll)
        {
            ragdoll.SetRagdoll(false);
            ragdoll.transform.localPosition = Vector3.zero;
            ragdoll.transform.localRotation = Quaternion.identity;
        }

        var controller = GetComponent<CharacterController>();
        if (controller) controller.enabled = true;

        var move = GetComponent<ThirdPersonMovement>();
        if (move) move.enabled = true;

        var shooter = GetComponent<Shooter>();
        if (shooter) shooter.enabled = true;

        var aim = GetComponent<AimController>();
        if (aim) aim.enabled = true;

        var swap = GetComponentInChildren<CMChangeView>();
        if (swap) swap.enabled = true;
    }
}
