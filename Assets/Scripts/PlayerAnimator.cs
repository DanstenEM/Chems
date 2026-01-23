using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerAnimator :  ITickable
{
    private Animator animator;
    private ThirdPersonMovement playerMovement;
    private float speedDampTime = 0.1f;
    private AimController aimController;

    PlayerInput input;//
    InputAction sprintAction;

    int speedHash;
    int isSprintingHash;

    public PlayerAnimator(Animator animator,
        ThirdPersonMovement thirdPersonMovement,
        PlayerInput playerInput,
        AimController aimController)
    {
        this.animator = animator;
        if (!playerMovement) playerMovement = thirdPersonMovement;
        
        input = playerInput;
        sprintAction = input.actions["Sprint"];

        // Cache animator parameter hashes
        speedHash = Animator.StringToHash("Speed");
        isSprintingHash = Animator.StringToHash("IsSprinting");
        this.aimController = aimController;
    }

    public void Tick()
    {
        UpdateMovementAnimation();
    }

    void UpdateMovementAnimation()
    {
        // ËÎÃÈÊÀ ÑÎÕÐÀÍÅÍÀ:
        // áåð¸ì ðåàëüíóþ ñêîðîñòü ïåðñîíàæà
        float speed = playerMovement.CurrentSpeed;

        animator.SetFloat(speedHash, speed, speedDampTime, Time.deltaTime);

        bool isSprinting =
            sprintAction.IsPressed() &&
            speed > 0.1f &&
            (aimController == null || !aimController.IsAiming());

        animator.SetBool(isSprintingHash, isSprinting);
    }
}
