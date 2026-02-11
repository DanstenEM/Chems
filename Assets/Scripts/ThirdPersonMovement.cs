using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour, IInitializable
{
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float rotationSpeed = 12f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] LayerMask groundMask = ~0;
    [Header("Stamina")]
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDrainRate = 10f;
    [SerializeField] float staminaRegenRate = 10f;
    [SerializeField] float staminaRegenDelay = 2f;
    [SerializeField] float jumpStaminaCost = 10f;

    public float CurrentSpeed { get; private set; }
    public float StaminaNormalized => maxStamina > 0f ? stamina / maxStamina : 0f;


    CharacterController controller;
    [Inject] public PlayerInput input;
    Transform cam;
    AimController aimController;
    Shooter shooter;


    InputAction moveAction;
    InputAction sprintAction;
    InputAction jumpAction;

    Vector3 velocity;
    float stamina;
    float lastSprintTime = float.NegativeInfinity;

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleGravity();
    }

    void HandleMovement()
    {
        if (LootCrateUI.IsAnyLootMenuOpen)
        {
            CurrentSpeed = 0f;
            UpdateStamina(false);
            return;
        }

        Vector2 inputVector = moveAction.ReadValue<Vector2>();

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * inputVector.y + camRight * inputVector.x;

        bool canSprint = sprintAction.IsPressed()
            && (aimController == null || !aimController.IsAiming())
            && (shooter == null || !shooter.IsFiring)
            && stamina > 0f;
        bool isSprinting = canSprint && moveDir.sqrMagnitude > 0.01f;
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        controller.Move(moveDir * speed * Time.deltaTime);
        CurrentSpeed = controller.velocity.magnitude;

        UpdateStamina(isSprinting);

        if (moveDir.sqrMagnitude > 0.01f
            && (aimController == null || !aimController.IsAiming())
            && (shooter == null || !shooter.IsFiring))
        {
            Vector3 lookDir = moveDir;
            lookDir.y = 0f;

            Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized);
            transform.rotation =
                Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

    }

    void HandleGravity()
    {
        if (IsGrounded() && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJump()
    {
        if (LootCrateUI.IsAnyLootMenuOpen)
        {
            return;
        }

        if (IsGrounded() && jumpAction.triggered && stamina >= jumpStaminaCost)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            stamina = Mathf.Max(0f, stamina - jumpStaminaCost);
            lastSprintTime = Time.time;
        }
    }

    bool IsGrounded()
    {
        if (controller.isGrounded)
        {
            return true;
        }

        Vector3 origin = controller.bounds.center;
        float radius = Mathf.Max(0.01f, controller.radius * 0.9f);
        float castDistance = (controller.height * 0.5f) - controller.radius + groundCheckDistance;

        return Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out _,
            castDistance,
            groundMask,
            QueryTriggerInteraction.Ignore);
    }

    void UpdateStamina(bool isSprinting)
    {
        if (maxStamina <= 0f)
        {
            stamina = 0f;
            return;
        }

        if (isSprinting)
        {
            stamina = Mathf.Max(0f, stamina - staminaDrainRate * Time.deltaTime);
            lastSprintTime = Time.time;
        }
        else if (Time.time - lastSprintTime >= staminaRegenDelay)
        {
            stamina = Mathf.Min(maxStamina, stamina + staminaRegenRate * Time.deltaTime);
        }
    }

    public void Initialize()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        aimController = GetComponent<AimController>();
        shooter = GetComponent<Shooter>();


        moveAction = input.actions["Move"];
        sprintAction = input.actions["Sprint"];
        jumpAction = input.actions["Jump"];
        stamina = maxStamina;
    }
}
