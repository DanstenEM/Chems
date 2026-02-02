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

    public float CurrentSpeed { get; private set; }


    CharacterController controller;
    [Inject] public PlayerInput input;
    [SerializeField] private Transform cam;
    AimController aimController;


    InputAction moveAction;
    InputAction sprintAction;

    Vector3 velocity;

    void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }

    void Update()
    {
        if (controller == null || cam == null || moveAction == null || sprintAction == null)
        {
            return;
        }

        HandleMovement();
        HandleGravity();
    }

    void HandleMovement()
    {
        Vector2 inputVector = moveAction.ReadValue<Vector2>();

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * inputVector.y + camRight * inputVector.x;

        bool canSprint = sprintAction.IsPressed() && (aimController == null || !aimController.IsAiming());
        float speed = canSprint ? sprintSpeed : walkSpeed;

        controller.Move(moveDir * speed * Time.deltaTime);
        CurrentSpeed = controller.velocity.magnitude;

        if (moveDir.sqrMagnitude > 0.01f && (aimController == null || !aimController.IsAiming()))
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
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void Initialize()
    {
        controller = GetComponent<CharacterController>();
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }
        aimController = GetComponent<AimController>();


        if (input != null)
        {
            moveAction = input.actions["Move"];
            sprintAction = input.actions["Sprint"];
        }
    }
}
