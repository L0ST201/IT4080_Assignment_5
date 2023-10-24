using UnityEngine;

public class PlayerAnimationHandler
{
    private Animator animator;

    public PlayerAnimationHandler(Animator animator)
    {
        this.animator = animator ?? throw new System.ArgumentNullException(nameof(animator));
    }

    public void UpdateMovementAnimations(float xMove, float zMove)
    {
        animator.SetFloat("X", xMove);
        animator.SetFloat("Y", zMove);
        animator.SetFloat("Speed", Mathf.Sqrt(xMove * xMove + zMove * zMove));
    }

    public void SetAiming(bool isAiming)
    {
        animator.SetBool("Aiming", isAiming);
    }

    public void TriggerShootAnimation()
    {
        animator.SetTrigger("Shoot");
    }

    public void TriggerReloadAnimation()
    {
        animator.SetTrigger("Reloading");
    }

    public void TriggerRollAnimation()
    {
        animator.SetTrigger("Roll");
    }

/*     public void TriggerPickupAnimation()
    {
        animator.SetTrigger("Pickup");
    } */

    public void SetJumping(float jumpFactor)
    {
        animator.SetFloat("Jump", jumpFactor);
    }

    public void FootStep()
    {
        // place holder
    }

    public void RollSound()
    {
         // place holder
    }

    public void CantRotate()
    {
         // place holder
    }

    public void EndRoll()
    {
         // place holder
    }
}
