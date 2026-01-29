using UnityEngine;
using Zenject.SpaceFighter;

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

    bool dead;

    [SerializeField] bool isPlayer;
    public ReactiveProperty<bool> isDie = new ReactiveProperty<bool>();

    public float CurrentHealth { get; private set; }

    void Awake()
    {
        CurrentHealth = maxHealth;
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
    }

    void Die()
    {
        if (dead) return;
        dead = true;

        Debug.Log($"{name} died");

        if (isPlayer)
        {
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
            Vector3 spawnPosition = dropSpawnPoint
                ? dropSpawnPoint.position
                : transform.position + dropOffset;
            Instantiate(deathDropPrefab, spawnPosition, Quaternion.identity);
        }

        Destroy(gameObject);
    }
    void Respawn()
    {
        Debug.Log("RESPAWN");

        dead = false;
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
