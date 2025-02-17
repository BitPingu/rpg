using UnityEngine;

public class PlayerDeadState : PlayerState
{
    // pass in any parameters you need in the constructors
    public PlayerDeadState(Player player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine)
    {
        // this.player = player;
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        Debug.Log(player.name + " is dead.");

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
        player.Movement = Vector3.zero;
        player.Anim.SetFloat("Movement", player.Movement.magnitude);

        player.Input.enabled = false;
        player.Controller.enabled = false;

        if (player.Anim.GetLayerWeight(1) == 1)
            player.Anim.SetLayerWeight(1, 0);

        // Animate death
        player.Anim.SetTrigger("Die");

        // game over screen/respawn
    }

}

