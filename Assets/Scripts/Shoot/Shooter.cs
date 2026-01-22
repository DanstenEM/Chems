using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AimController aimController;

    [Header("Fire Settings")]
    [SerializeField] private float fireRate = 0.15f;
    [SerializeField] private float fireDistance = 100f;
    [SerializeField] private LayerMask hitMask;

    [Header("Input")]
    [SerializeField] private InputActionProperty fireAction;

    [Header("Tracer")]
    [SerializeField] private BulletTracer tracerPrefab;
    [SerializeField] private Transform muzzle;

    float nextFireTime;

    void Awake()
    {
        if (!playerCamera)
            playerCamera = Camera.main;

        if (!aimController)
            aimController = GetComponent<AimController>();

        fireAction.action.Enable();
    }

    void Update()
    {
        HandleFire();
    }

    void HandleFire()
    {

        if (Time.time < nextFireTime)
            return;

        if (!fireAction.action.IsPressed())
            return;

        nextFireTime = Time.time + fireRate;
        Fire();
    }

    void Fire()
    {
        Debug.Log("Fire method");
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 startPoint = muzzle ? muzzle.position : ray.origin;
        Vector3 endPoint = ray.origin + ray.direction * fireDistance;

        if (Physics.Raycast(ray, out hit, fireDistance, hitMask))
        {
            endPoint = hit.point;
            Debug.Log("Hit: " + hit.collider.name);
        }

        SpawnTracer(startPoint, endPoint);
    }
    void SpawnTracer(Vector3 start, Vector3 end)
    {
        if (!tracerPrefab) return;

        BulletTracer tracer = Instantiate(tracerPrefab);
        tracer.Init(start, end);
    }
}