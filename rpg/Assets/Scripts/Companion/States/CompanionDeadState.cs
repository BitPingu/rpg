using UnityEngine;

public class CompanionDeadState : CompanionState
{
    // pass in any parameters you need in the constructors
    public CompanionDeadState(Companion companion, CompanionStateMachine companionStateMachine) : base(companion, companionStateMachine)
    {
        // this.companion = companion;
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        Debug.Log(companion.name + " is dead.");

        Die();
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    private void Die()
    {
        companion.Movement = Vector3.zero;
        companion.Anim.SetFloat("Movement", companion.Movement.magnitude);

        companion.Controller.enabled = false;
        companion.Collider.enabled = false;

        if (companion.Anim.GetLayerWeight(1) == 1)
            companion.Anim.SetLayerWeight(1, 0);

        // Animate death
        companion.Anim.SetTrigger("Die");

        // game over screen/respawn
    }

}

