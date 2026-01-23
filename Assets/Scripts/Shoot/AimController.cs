using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class AimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform playerRoot;

    [Header("Orbital Settings")]
    [SerializeField] private float normalRadius = 4f;
    [SerializeField] private float aimRadius = 2.2f;
    [SerializeField] private float camLerpSpeed = 8f;

    [Header("Rotation")]
    [SerializeField] private float aimRotationSpeed = 18f;

    [Header("Input")]
    [SerializeField] private InputActionProperty aimAction;

    CinemachineOrbitalFollow orbital;
    bool isAiming;

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
            Debug.LogError("AimController: No CinemachineOrbitalFollow found on camera");
            enabled = false;
            return;
        }

        aimAction.action.Enable();
    }

    void Update()
    {
        isAiming = aimAction.action.IsPressed();

        HandleCamera();
        HandleRotation();
    }

    void HandleCamera()
    {
        float targetRadius = isAiming ? aimRadius : normalRadius;

        orbital.Radius =
            Mathf.Lerp(orbital.Radius, targetRadius, Time.deltaTime * camLerpSpeed);
    }

    void HandleRotation()
    {
        if (!isAiming) return;

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;

        if (camForward.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(camForward);
        playerRoot.rotation =
            Quaternion.Slerp(playerRoot.rotation, targetRot, aimRotationSpeed * Time.deltaTime);
    }

    public bool IsAiming()
    {
        return isAiming;
    }
}