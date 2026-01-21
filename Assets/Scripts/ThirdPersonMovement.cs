using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float rotationSpeed = 12f;
    public float gravity = -20f;

    public float CurrentSpeed { get; private set; }


    CharacterController controller;
    PlayerInput input;
    Transform cam;
    AimController aimController;


    InputAction moveAction;
    InputAction sprintAction;

    Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        cam = Camera.main.transform;
        aimController = GetComponent<AimController>();


        moveAction = input.actions["Move"];
        sprintAction = input.actions["Sprint"];
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
        Debug.Log(CurrentSpeed);

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
}
