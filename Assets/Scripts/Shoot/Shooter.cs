using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform muzzle;

    [Header("Projectile")]
    [SerializeField] private BulletProjectile bulletPrefab;

    [Header("Fire Settings")]
    [SerializeField] private float fireRate = 0.15f;
    [SerializeField] private float damage = 25f;

    [Header("Impact")]
    [SerializeField] private GameObject impactPrefab;

    [Header("Input")]
    [SerializeField] private InputActionProperty fireAction;

    [Header("Weapon Gate")]
    [SerializeField] private WeaponQuickbar weaponQuickbar;

    public bool IsFiring { get; private set; }

    float nextFireTime;

    void Awake()
    {
        if (!playerCamera)
            playerCamera = Camera.main;

        if (!weaponQuickbar)
        {
            weaponQuickbar = FindObjectOfType<WeaponQuickbar>();
        }

        fireAction.action.Enable();
    }

    void Update()
    {
        HandleFire();
    }

    void HandleFire()
    {
        if (weaponQuickbar != null && !weaponQuickbar.HasSelectedWeapon)
        {
            IsFiring = false;
            return;
        }

        if (!fireAction.action.IsPressed())
        {
            IsFiring = false;
            return;
        }

        IsFiring = true;

        if (Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + fireRate;
        Fire();
    }

    void Fire()
    {
        if (!bulletPrefab|| !muzzle || !playerCamera)
            return;

        // Aim from center of screen
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 dir = ray.direction;

        BulletProjectile bullet =
            Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(dir));

        bullet.damage = damage;
        bullet.impactPrefab = impactPrefab;
        bullet.Init(dir);
    }
}
