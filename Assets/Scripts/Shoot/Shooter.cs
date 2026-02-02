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

    [Header("Recoil")]
    [SerializeField] private float recoilKick = 4f;
    [SerializeField] private float recoilRandomYaw = 1.25f;
    [SerializeField] private float recoilReturnSpeed = 10f;
    [SerializeField] private float maxRecoil = 10f;
    [SerializeField] private float recoilAimOffset = 0.08f;

    [Header("Impact")]
    [SerializeField] private GameObject impactPrefab;

    [Header("Input")]
    [SerializeField] private InputActionProperty fireAction;

    public bool IsFiring { get; private set; }

    float nextFireTime;
    float currentRecoil;
    float currentYaw;

    void Awake()
    {
        if (!playerCamera)
            playerCamera = Camera.main;

        fireAction.action.Enable();
    }

    void Update()
    {
        UpdateRecoil();
        HandleFire();
    }

    void UpdateRecoil()
    {
        if (currentRecoil <= 0f && Mathf.Abs(currentYaw) <= 0.001f)
            return;

        currentRecoil = Mathf.MoveTowards(currentRecoil, 0f, recoilReturnSpeed * Time.deltaTime);
        currentYaw = Mathf.MoveTowards(currentYaw, 0f, recoilReturnSpeed * Time.deltaTime);
    }

    void HandleFire()
    {
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
        if (!bulletPrefab || !muzzle || !playerCamera)
            return;

        ApplyRecoil();

        // Aim from center of screen
        float recoilOffset = Mathf.Clamp01(currentRecoil / Mathf.Max(0.01f, maxRecoil));
        float aimYOffset = recoilAimOffset * recoilOffset;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f + aimYOffset, 0));
        Vector3 dir = ApplyRecoilToDirection(ray.direction);

        BulletProjectile bullet =
            Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(dir));

        bullet.damage = damage;
        bullet.impactPrefab = impactPrefab;
        bullet.Init(dir);
    }

    void ApplyRecoil()
    {
        if (recoilKick <= 0f && recoilRandomYaw <= 0f)
            return;

        currentRecoil = Mathf.Min(currentRecoil + recoilKick, maxRecoil);
        currentYaw += Random.Range(-recoilRandomYaw, recoilRandomYaw);
    }

    Vector3 ApplyRecoilToDirection(Vector3 direction)
    {
        if (currentRecoil == 0f && Mathf.Abs(currentYaw) < 0.001f)
            return direction;

        Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, currentYaw, 0f);
        return recoilRotation * direction;
    }
}
