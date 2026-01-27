using UnityEngine;

public class AimIKController : MonoBehaviour
{
    [Header("Targets")]
    public Transform aimTarget;
    public Transform rightHandTarget;

    [Header("Weights")]
    [Range(0, 1)] public float bodyWeight = 0.15f;
    [Range(0, 1)] public float headWeight = 0.8f;
    [Range(0, 1)] public float eyesWeight = 1f;
    [Range(0, 1)] public float clampWeight = 0.3f;

    [Header("Blend")]
    public float ikBlendSpeed = 6f;

    Animator animator;
    bool aiming;
    float currentWeight;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetAiming(bool value)
    {
        aiming = value;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!animator) return;
        if (!aimTarget)
        {
            animator.SetLookAtWeight(0f);
            return;
        }

        float target = aiming ? 1f : 0f;
        currentWeight = Mathf.Lerp(currentWeight, target, Time.deltaTime * ikBlendSpeed);

        // LOOK AT (spine + head)
        animator.SetLookAtWeight(currentWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
        animator.SetLookAtPosition(aimTarget.position);

        // RIGHT HAND IK
        if (rightHandTarget)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, currentWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, currentWeight);

            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }
    }
}