using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class AimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private AimIKController ik;
    [SerializeField] private Health health;
    [SerializeField] private Shooter shooter;

    [Header("Orbital Settings")]
    [SerializeField] private float normalRadius = 4f;
    [SerializeField] private float aimRadius = 2.2f;
    [SerializeField] private float camLerpSpeed = 8f;

    [Header("Rotation")]
    [SerializeField] private float aimRotationSpeed = 12f;

    [Header("Input")]
    [SerializeField] private InputActionProperty aimAction;

    CinemachineOrbitalFollow orbital;
    bool isAiming;
    bool isDie;

    void Awake()
    {
        if (!cinemachineCamera)
        {
            Debug.LogError("AimController: CinemachineCamera not assigned");
            enabled = false;
            return;
        }

        orbital = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
        if (!orbital)
        {
            Debug.LogError("AimController: No CinemachineOrbitalFollow found");
            enabled = false;
            return;
        }

        if (!playerRoot)
            playerRoot = transform;

        if (!ik)
            ik = GetComponentInChildren<AimIKController>();

        if (!shooter)
            shooter = GetComponent<Shooter>();

        aimAction.action.Enable();

        if (health)
            health.isDie.Changed += IsDie_Changed;
    }

    private void IsDie_Changed(bool obj)
    {
        isDie = obj;
    }

    void Update()
    {
        if (isDie) return;

        isAiming = aimAction.action.IsPressed();

        if (ik)
            ik.SetAiming(isAiming);

        HandleCamera();

        if (isAiming || IsShooting())
            RotateBodyToAim();
    }

    bool IsShooting()
    {
        return shooter && shooter.IsFiring;
    }

    void HandleCamera()
    {
        float targetRadius = isAiming ? aimRadius : normalRadius;
        orbital.Radius = Mathf.Lerp(orbital.Radius, targetRadius, Time.deltaTime * camLerpSpeed);
    }

    void RotateBodyToAim()
    {
        if (!Camera.main) return;

        Vector3 dir = Camera.main.transform.forward;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        playerRoot.rotation =
            Quaternion.Slerp(playerRoot.rotation, targetRot, aimRotationSpeed * Time.deltaTime);
    }

    public bool IsAiming()
    {
        return isAiming;
    }
}